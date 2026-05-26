export function deriveUsername(email: string | undefined): string {
  const prefix = (email ?? 'guest').split('@')[0] ?? 'guest';
  const sanitized = prefix.replace(/[^a-zA-Z0-9_]/g, '_').slice(0, 20);
  return sanitized.length >= 3 ? sanitized : sanitized.padEnd(3, '_');
}
