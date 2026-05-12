import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ReleaseBoardApp } from './App.jsx';

const sample = {
  summary: {
    releaseCount: 3,
    environmentCount: 3,
    deploymentCount: 4,
    successfulDeployments: 3,
    failedDeployments: 1,
    enabledFlags: 2,
  },
  version: {
    service: 'ReleaseBoard.Api',
    environment: 'Development',
    gitSha: 'abc123',
    buildNumber: '42',
    buildTime: 'test',
  },
  releases: [
    {
      id: 1,
      name: 'Release 2026.05',
      artifactVersion: 'releaseboard-api-2026.05.1',
      gitSha: 'abc123',
      status: 'Production',
      createdAt: new Date().toISOString(),
      deployments: [],
    },
  ],
  environments: [
    { id: 1, name: 'staging', url: 'https://staging.example', requiresApproval: false, sortOrder: 1 },
  ],
  deployments: [
    {
      id: 1,
      releaseId: 1,
      releaseName: 'Release 2026.05',
      environmentName: 'staging',
      status: 'Succeeded',
      gitSha: 'abc123',
      artifactVersion: 'releaseboard-api-2026.05.1',
      strategy: 'blue/green',
      startedAt: new Date().toISOString(),
      completedAt: new Date().toISOString(),
    },
  ],
  flags: [
    {
      id: 1,
      key: 'new-dashboard-layout',
      description: 'Canary dashboard',
      isEnabled: true,
      rolloutPercentage: 25,
      updatedAt: new Date().toISOString(),
    },
  ],
  auditEvents: [
    {
      id: 1,
      title: 'Production approval recorded',
      details: 'Approved after health checks.',
      actor: 'releaseboard-system',
      occurredAt: new Date().toISOString(),
    },
  ],
};

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn(async (url) => {
    const path = new URL(url).pathname;
    const map = {
      '/api/summary': sample.summary,
      '/api/version': sample.version,
      '/api/releases': sample.releases,
      '/api/environments': sample.environments,
      '/api/deployments': sample.deployments,
      '/api/feature-flags': sample.flags,
      '/api/audit-events': sample.auditEvents,
    };

    return {
      ok: true,
      json: async () => map[path],
    };
  }));
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('ReleaseBoardApp', () => {
  it('renders dashboard metrics and artifact identity', async () => {
    render(
      <MemoryRouter>
        <ReleaseBoardApp />
      </MemoryRouter>,
    );

    await waitFor(() => expect(screen.getByText('Artifact identity')).toBeInTheDocument());

    expect(screen.getByText('abc123')).toBeInTheDocument();
    expect(screen.getAllByText('Releases').length).toBeGreaterThan(0);
    expect(screen.getAllByText('3').length).toBeGreaterThan(0);
  });

  it('shows an operational error when the API cannot be reached', async () => {
    fetch.mockResolvedValueOnce({ ok: false, text: async () => 'API down' });

    render(
      <MemoryRouter>
        <ReleaseBoardApp />
      </MemoryRouter>,
    );

    expect(await screen.findByText('ReleaseBoard API is unavailable')).toBeInTheDocument();
    expect(screen.getByText(/dotnet run --project/)).toBeInTheDocument();
  });
});
