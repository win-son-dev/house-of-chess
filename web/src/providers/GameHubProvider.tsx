import { useEffect, useState, type ReactNode } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { HubConnectionState, type HubConnection } from '@microsoft/signalr';
import { buildGameHubConnection } from '@/lib/signalr';
import { GameHubContext } from '@/lib/gameHubContext';

export function GameHubProvider({ children }: { children: ReactNode }) {
  const { getAccessTokenSilently, isAuthenticated } = useAuth0();
  const [hub, setHub] = useState<HubConnection | null>(null);
  const [state, setState] = useState<HubConnectionState>(HubConnectionState.Disconnected);

  useEffect(() => {
    if (!isAuthenticated) return;

    const conn = buildGameHubConnection(() => getAccessTokenSilently());
    conn.onclose(() => setState(HubConnectionState.Disconnected));
    conn.onreconnecting(() => setState(HubConnectionState.Reconnecting));
    conn.onreconnected(() => setState(HubConnectionState.Connected));

    conn.start()
      .then(() => {
        setHub(conn);
        setState(HubConnectionState.Connected);
      })
      .catch(err => {
        console.error('SignalR start failed', err);
        setState(HubConnectionState.Disconnected);
      });

    return () => {
      void conn.stop();
      setHub(null);
    };
  }, [isAuthenticated, getAccessTokenSilently]);

  return (
    <GameHubContext.Provider value={{ hub, state }}>
      {children}
    </GameHubContext.Provider>
  );
}
