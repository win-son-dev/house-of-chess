import { useEffect, useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { useNavigate } from 'react-router-dom';
import { HubConnectionState } from '@microsoft/signalr';
import { Button } from '@/components/ui/button';
import { useGameHub } from '@/lib/gameHubContext';
import { useMe } from '@/lib/meContext';

// Matches TimeControlCategory enum on the server: Bullet=0, Blitz=1, Rapid=2.
const TIME_CONTROLS: { label: string; category: number }[] = [
  { label: 'Bullet 1+0',  category: 0 },
  { label: 'Bullet 2+1',  category: 0 },
  { label: 'Blitz 3+0',   category: 1 },
  { label: 'Blitz 3+2',   category: 1 },
  { label: 'Blitz 5+0',   category: 1 },
  { label: 'Rapid 10+0',  category: 2 },
  { label: 'Rapid 15+10', category: 2 },
];

interface MatchFound {
  matched: boolean;
  gameId: string | null;
  whiteUserId: string | null;
  blackUserId: string | null;
  timeControl: string | null;
}

export function Lobby() {
  const { user, logout } = useAuth0();
  const { hub, state } = useGameHub();
  const { me, isLoading: meLoading, error: meError } = useMe();
  const navigate = useNavigate();
  const [searching, setSearching] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  useEffect(() => {
    if (!hub) return;
    const handler = (result: MatchFound) => {
      if (result?.gameId) navigate(`/game/${result.gameId}`);
    };
    hub.on('matchFound', handler);
    return () => hub.off('matchFound', handler);
  }, [hub, navigate]);

  const hubReady = hub?.state === HubConnectionState.Connected;
  const canPlay = hubReady && !!me && !searching;

  async function handleEnqueue(category: number) {
    if (!hub) return;
    setErrorMsg(null);
    setSearching(true);
    try {
      const result = await hub.invoke<MatchFound>('EnqueueMatch', category);
      if (result?.matched && result.gameId) {
        navigate(`/game/${result.gameId}`);
      }
    } catch (e) {
      console.error('EnqueueMatch failed', e);
      setErrorMsg(e instanceof Error ? e.message : 'Failed to join queue');
      setSearching(false);
    }
  }

  async function handleCancel() {
    if (!hub) return;
    try { await hub.invoke('CancelMatch'); } catch (e) { console.error(e); }
    setSearching(false);
  }

  return (
    <div className="min-h-full p-8">
      <header className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-semibold">House of Chess</h1>
        <div className="flex items-center gap-4">
          <span className="text-sm text-muted-foreground">
            {meLoading ? 'Setting up…' : me ? `@${me.username}` : user?.email}
          </span>
          <Button
            variant="secondary"
            onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
          >
            Log out
          </Button>
        </div>
      </header>

      {meError && (
        <p className="mb-4 text-sm text-red-400">Account setup error: {meError}</p>
      )}
      {errorMsg && (
        <p className="mb-4 text-sm text-red-400">{errorMsg}</p>
      )}

      <section>
        <h2 className="text-xl mb-4">
          {searching ? 'Searching for an opponent…' : 'Pick a time control'}
        </h2>

        {searching ? (
          <Button variant="secondary" onClick={handleCancel}>Cancel search</Button>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3 max-w-3xl">
            {TIME_CONTROLS.map(tc => (
              <Button
                key={tc.label}
                variant="outline"
                disabled={!canPlay}
                className="h-20 flex-col items-start justify-center text-left"
                onClick={() => handleEnqueue(tc.category)}
              >
                <div className="font-medium">{tc.label}</div>
                <div className="text-xs text-muted-foreground">
                  {hubReady ? 'Ready' : state}
                </div>
              </Button>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
