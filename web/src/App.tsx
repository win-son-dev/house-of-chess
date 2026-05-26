import { useEffect, type ReactNode } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { Lobby } from '@/pages/Lobby';
import { Game } from '@/pages/Game';
import { MeProvider } from '@/providers/MeProvider';
import { GameHubProvider } from '@/providers/GameHubProvider';

function RequireAuth({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading, loginWithRedirect } = useAuth0();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      void loginWithRedirect({
        appState: { returnTo: window.location.pathname },
      });
    }
  }, [isLoading, isAuthenticated, loginWithRedirect]);

  if (isLoading || !isAuthenticated) {
    return (
      <div className="min-h-full flex items-center justify-center text-muted-foreground">
        Signing you in…
      </div>
    );
  }

  return (
    <MeProvider>
      <GameHubProvider>{children}</GameHubProvider>
    </MeProvider>
  );
}

function App() {
  return (
    <Routes>
      <Route path="/"          element={<RequireAuth><Lobby /></RequireAuth>} />
      <Route path="/game/:id"  element={<RequireAuth><Game /></RequireAuth>} />
      <Route path="*"          element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
