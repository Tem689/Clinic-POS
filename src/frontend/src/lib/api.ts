const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8081';

export async function apiFetch(endpoint: string, options: RequestInit = {}) {
  const token = typeof window !== 'undefined' ? localStorage.getItem('clinic_token') : null;

  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    // Handle unauthorized - maybe redirect to login
    if (typeof window !== 'undefined') {
      localStorage.removeItem('clinic_token');
      window.location.href = '/login';
    }
  }

  return response;
}
