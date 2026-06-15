const isServer = typeof window === 'undefined';

const BASE_URL = isServer
  ? process.env.API_URL_SERVER
  : process.env.NEXT_PUBLIC_API_URL_CLIENT;

export async function apiFetch(endpoint: string, options: RequestInit = {}) {
  const defaultHeaders: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  const response = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    credentials: 'include',
    headers: { ...defaultHeaders, ...options.headers },
  });

  const data = await response.json();

  if (!response.ok) {
    throw new Error(data?.message || 'API request failed');
  }

  return data;
}
