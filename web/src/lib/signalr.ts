import * as signalR from '@microsoft/signalr';

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export function buildGameHubConnection(
  getAccessToken: () => Promise<string>,
): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/game`, {
      accessTokenFactory: getAccessToken,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
}
