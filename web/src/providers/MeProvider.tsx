import { useEffect, useState, type ReactNode } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { authedFetch } from '@/lib/api';
import { deriveUsername } from '@/lib/onboarding';
import { MeContext, type Me } from '@/lib/meContext';

export function MeProvider({ children }: { children: ReactNode }) {
  const { user, getAccessTokenSilently, isAuthenticated } = useAuth0();
  const [me, setMe] = useState<Me | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;
    let cancelled = false;
    (async () => {
      try {
        const token = await getAccessTokenSilently();
        const username = deriveUsername(user?.email);
        const res = await authedFetch('/api/account/onboarding', token, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ username }),
        });
        if (!res.ok) throw new Error(`Onboarding failed: ${res.status}`);
        const body = await res.json();
        if (!cancelled) setMe({ userId: body.userId, username: body.username, isNew: body.isNew });
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Onboarding failed');
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [isAuthenticated, user?.email, getAccessTokenSilently]);

  return (
    <MeContext.Provider value={{ me, isLoading, error }}>
      {children}
    </MeContext.Provider>
  );
}
