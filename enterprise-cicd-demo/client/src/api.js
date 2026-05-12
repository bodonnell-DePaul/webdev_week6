const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5144';
const DEMO_API_KEY = import.meta.env.VITE_DEMO_API_KEY ?? 'local-demo-key';

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.demoKey ? { 'X-ReleaseBoard-Demo-Key': DEMO_API_KEY } : {}),
      ...options.headers,
    },
    ...options,
  });

  if (!response.ok) {
    const detail = await response.text();
    throw new Error(detail || `Request failed with ${response.status}`);
  }

  return response.json();
}

export const api = {
  summary: () => request('/api/summary'),
  version: () => request('/api/version'),
  releases: () => request('/api/releases'),
  environments: () => request('/api/environments'),
  deployments: () => request('/api/deployments'),
  flags: () => request('/api/feature-flags'),
  auditEvents: () => request('/api/audit-events'),
  updateFlag: (key, payload) =>
    request(`/api/feature-flags/${key}`, {
      method: 'PATCH',
      body: JSON.stringify(payload),
      demoKey: true,
    }),
};
