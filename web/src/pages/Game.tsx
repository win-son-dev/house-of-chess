import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Chess, type Square } from 'chess.js';
import { Chessground } from 'chessground';
import type { Api as ChessgroundApi } from 'chessground/api';
import type { Color, Key, Dests } from 'chessground/types';
import 'chessground/assets/chessground.base.css';
import 'chessground/assets/chessground.brown.css';
import 'chessground/assets/chessground.cburnett.css';
import { useAuth0 } from '@auth0/auth0-react';
import { authedFetch } from '@/lib/api';
import { useGameHub } from '@/lib/gameHubContext';
import { useMe } from '@/lib/meContext';

interface GameMoveEntry { ply: number; san: string; uci: string }
interface GameSnapshot {
  id: string;
  whiteUserId: string;
  blackUserId: string;
  timeControl: string;
  result: string | null;
  moves: GameMoveEntry[];
}
interface MoveResultDto {
  accepted: boolean;
  rejectionReason: string | null;
  newFen: string | null;
  san: string | null;
  uci: string | null;
  finalResult: number | null;
}

function uciToMove(uci: string) {
  return {
    from: uci.slice(0, 2),
    to: uci.slice(2, 4),
    promotion: uci.length === 5 ? uci[4] : undefined,
  };
}

function computeDests(chess: Chess): Dests {
  const dests: Dests = new Map();
  for (const m of chess.moves({ verbose: true })) {
    const arr = dests.get(m.from as Key) ?? [];
    arr.push(m.to as Key);
    dests.set(m.from as Key, arr);
  }
  return dests;
}

export function Game() {
  const { id } = useParams<{ id: string }>();
  const { getAccessTokenSilently } = useAuth0();
  const { hub } = useGameHub();
  const { me } = useMe();

  const boardRef = useRef<HTMLDivElement | null>(null);
  const groundRef = useRef<ChessgroundApi | null>(null);
  const chessRef = useRef<Chess>(new Chess());

  const [snapshot, setSnapshot] = useState<GameSnapshot | null>(null);
  const [error, setError] = useState<string | null>(null);

  const myColor: Color | null = useMemo(() => {
    if (!snapshot || !me) return null;
    if (snapshot.whiteUserId === me.userId) return 'white';
    if (snapshot.blackUserId === me.userId) return 'black';
    return null;
  }, [snapshot, me]);

  const applyChessToGround = useCallback(() => {
    const g = groundRef.current;
    const chess = chessRef.current;
    if (!g) return;
    const turn: Color = chess.turn() === 'w' ? 'white' : 'black';
    g.set({
      fen: chess.fen(),
      turnColor: turn,
      movable: {
        color: myColor ?? undefined,
        dests: myColor && turn === myColor ? computeDests(chess) : new Map(),
      },
    });
  }, [myColor]);

  const submitMove = useCallback(async (uci: string) => {
    if (!hub || !id) return;
    try {
      const result = await hub.invoke<MoveResultDto>('SubmitMove', id, { uci });
      if (!result.accepted) {
        console.warn('Move rejected:', result.rejectionReason);
        const chess = new Chess();
        for (const m of (snapshot?.moves ?? [])) chess.move(uciToMove(m.uci));
        chessRef.current = chess;
        applyChessToGround();
      }
    } catch (e) {
      console.error('SubmitMove failed', e);
    }
  }, [hub, id, snapshot, applyChessToGround]);

  // Fetch snapshot once.
  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    (async () => {
      try {
        const token = await getAccessTokenSilently();
        const res = await authedFetch(`/api/games/${id}`, token);
        if (!res.ok) throw new Error(`Failed to load game: ${res.status}`);
        const body: GameSnapshot = await res.json();
        if (cancelled) return;

        const chess = new Chess();
        for (const m of body.moves) chess.move(uciToMove(m.uci));
        chessRef.current = chess;
        setSnapshot(body);
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Failed to load game');
      }
    })();
    return () => { cancelled = true; };
  }, [id, getAccessTokenSilently]);

  // Mount chessground once snapshot is ready.
  useEffect(() => {
    if (!boardRef.current || !snapshot) return;
    if (groundRef.current) return;

    const chess = chessRef.current;
    const turn: Color = chess.turn() === 'w' ? 'white' : 'black';

    const g = Chessground(boardRef.current, {
      fen: chess.fen(),
      orientation: myColor ?? 'white',
      turnColor: turn,
      movable: {
        color: myColor ?? undefined,
        free: false,
        dests: myColor && turn === myColor ? computeDests(chess) : new Map(),
        events: {
          after: (orig, dest) => {
            const piece = chess.get(orig as Square);
            const isPromotion =
              piece?.type === 'p' &&
              ((piece.color === 'w' && dest[1] === '8') ||
               (piece.color === 'b' && dest[1] === '1'));
            const uci = `${orig}${dest}${isPromotion ? 'q' : ''}`;
            const applied = chess.move(uciToMove(uci));
            if (!applied) return;
            applyChessToGround();
            void submitMove(uci);
          },
        },
      },
    });
    groundRef.current = g;

    return () => {
      g.destroy();
      groundRef.current = null;
    };
  }, [snapshot, myColor, submitMove, applyChessToGround]);

  // Join SignalR game group + subscribe to moveApplied.
  useEffect(() => {
    if (!hub || !id) return;
    void hub.invoke('JoinGame', id);

    const handler = (incoming: MoveResultDto) => {
      if (!incoming.accepted || !incoming.uci) return;
      const applied = chessRef.current.move(uciToMove(incoming.uci));
      if (applied) applyChessToGround();
    };
    hub.on('moveApplied', handler);

    return () => {
      hub.off('moveApplied', handler);
      void hub.invoke('LeaveGame', id).catch(() => {});
    };
  }, [hub, id, applyChessToGround]);

  return (
    <div className="min-h-full p-8">
      <h1 className="text-2xl mb-4">Game {id?.slice(0, 8)}…</h1>
      {error && <p className="mb-4 text-sm text-red-400">{error}</p>}
      <div className="grid grid-cols-1 lg:grid-cols-[480px_1fr] gap-6">
        <div
          ref={boardRef}
          className="aspect-square w-full max-w-[480px] rounded-lg overflow-hidden"
        />
        <aside className="space-y-3">
          <PlayerCard label="White" userId={snapshot?.whiteUserId} mine={myColor === 'white'} />
          <PlayerCard label="Black" userId={snapshot?.blackUserId} mine={myColor === 'black'} />
          <div className="rounded-lg bg-white/5 p-4 text-sm">
            <div className="text-xs opacity-60 mb-1">Result</div>
            <div>{snapshot?.result ?? 'In progress'}</div>
          </div>
        </aside>
      </div>
    </div>
  );
}

function PlayerCard({ label, userId, mine }: { label: string; userId?: string; mine: boolean }) {
  return (
    <div className="rounded-lg bg-white/5 p-4">
      <div className="text-xs opacity-60">{label}{mine && ' (you)'}</div>
      <div className="font-medium font-mono text-sm">
        {userId ? `${userId.slice(0, 8)}…` : '—'}
      </div>
    </div>
  );
}
