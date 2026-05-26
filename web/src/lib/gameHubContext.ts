import { createContext, useContext } from 'react';
import { HubConnectionState, type HubConnection } from '@microsoft/signalr';

export interface GameHubContextValue {
  hub: HubConnection | null;
  state: HubConnectionState;
}

export const GameHubContext = createContext<GameHubContextValue | null>(null);

export function useGameHub() {
  const ctx = useContext(GameHubContext);
  if (!ctx) throw new Error('useGameHub must be used within GameHubProvider');
  return ctx;
}
