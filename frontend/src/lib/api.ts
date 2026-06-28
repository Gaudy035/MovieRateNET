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

  // Obsluga refresh
  if (response.status === 401) {
    // Nie fetchuje jesli refresh sie nie powiodl
    if (endpoint === '/auth/refresh') {
      window.location.href = '/Login';
      throw new Error('Session expired');
    }

    const refreshResponse = await fetch(`${BASE_URL}/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
    });

    if (!refreshResponse.ok) {
      window.location.href = '/Login';
      throw new Error('Session expired');
    }

    const retryResponse = await fetch(`${BASE_URL}${endpoint}`, {
      ...options,
      credentials: 'include',
      headers: { ...defaultHeaders, ...options.headers },
    });

    const retryData = await retryResponse.json();

    if (!retryResponse.ok) {
      throw new Error(retryData?.message || 'API request failed');
    }

    return retryData;
  }
  //

  const data = await response.json();

  if (!response.ok) {
    throw new Error(data?.message || 'API request failed');
  }

  return data;
}
