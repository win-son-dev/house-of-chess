import { createContext, useContext } from 'react';

export interface Me {
  userId: string;
  username: string;
  isNew: boolean;
}

export interface MeContextValue {
  me: Me | null;
  isLoading: boolean;
  error: string | null;
}

export const MeContext = createContext<MeContextValue | null>(null);

export function useMe() {
  const ctx = useContext(MeContext);
  if (!ctx) throw new Error('useMe must be used within MeProvider');
  return ctx;
}
