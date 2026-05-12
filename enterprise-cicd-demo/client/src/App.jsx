import { useEffect, useMemo, useState } from 'react';
import { Link, NavLink, Route, Routes } from 'react-router-dom';
import { api } from './api.js';

const navItems = [
  { to: '/', label: 'Dashboard' },
  { to: '/releases', label: 'Releases' },
  { to: '/environments', label: 'Environments' },
  { to: '/flags', label: 'Feature flags' },
  { to: '/audit', label: 'Audit' },
];

export function ReleaseBoardApp() {
  const [data, setData] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  async function loadData() {
    setLoading(true);
    setError('');

    try {
      const [summary, version, releases, environments, deployments, flags, auditEvents] = await Promise.all([
        api.summary(),
        api.version(),
        api.releases(),
        api.environments(),
        api.deployments(),
        api.flags(),
        api.auditEvents(),
      ]);

      setData({ summary, version, releases, environments, deployments, flags, auditEvents });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to load ReleaseBoard data.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
  }, []);

  if (loading) {
    return <Shell version={null}><div className="panel">Loading release data...</div></Shell>;
  }

  if (error) {
    return (
      <Shell version={null}>
        <div className="panel error">
          <h2>ReleaseBoard API is unavailable</h2>
          <p>{error}</p>
          <p>Start the backend with <code>dotnet run --project ReleaseBoard.Api</code>.</p>
        </div>
      </Shell>
    );
  }

  return (
    <Shell version={data.version}>
      <Routes>
        <Route path="/" element={<Dashboard data={data} />} />
        <Route path="/releases" element={<Releases releases={data.releases} deployments={data.deployments} />} />
        <Route path="/environments" element={<Environments environments={data.environments} deployments={data.deployments} />} />
        <Route path="/flags" element={<FeatureFlags flags={data.flags} onChanged={loadData} />} />
        <Route path="/audit" element={<Audit events={data.auditEvents} />} />
      </Routes>
    </Shell>
  );
}

function Shell({ version, children }) {
  return (
    <div className="app-shell">
      <header className="hero">
        <div>
          <p className="eyebrow">Enterprise delivery demo</p>
          <h1>ReleaseBoard</h1>
          <p>Visualize release evidence, deployment strategy, feature flags, and audit events.</p>
        </div>
        <div className="version-card" aria-label="Build identity">
          <span>Artifact identity</span>
          <strong>{version?.gitSha ?? 'not loaded'}</strong>
          <small>Build {version?.buildNumber ?? '-'} | {version?.environment ?? 'local'}</small>
        </div>
      </header>

      <nav className="nav" aria-label="ReleaseBoard sections">
        {navItems.map((item) => (
          <NavLink key={item.to} to={item.to} className={({ isActive }) => (isActive ? 'active' : undefined)} end={item.to === '/'}>
            {item.label}
          </NavLink>
        ))}
      </nav>

      <main>{children}</main>
    </div>
  );
}

function Dashboard({ data }) {
  const successfulRate = useMemo(() => {
    const total = data.summary.deploymentCount || 1;
    return Math.round((data.summary.successfulDeployments / total) * 100);
  }, [data.summary]);

  return (
    <section>
      <div className="stats-grid">
        <StatCard label="Releases" value={data.summary.releaseCount} hint="Immutable artifact candidates" />
        <StatCard label="Deployments" value={data.summary.deploymentCount} hint={`${successfulRate}% succeeded`} />
        <StatCard label="Environments" value={data.summary.environmentCount} hint="Staging plus blue/green prod" />
        <StatCard label="Flags enabled" value={data.summary.enabledFlags} hint="Deployment decoupled from release" />
      </div>

      <div className="panel">
        <h2>Teaching path</h2>
        <div className="timeline">
          <span>PR evidence</span>
          <span>Build artifact</span>
          <span>Staging gate</span>
          <span>Production approval</span>
          <span>Health validation</span>
        </div>
        <p>
          This demo is intentionally small, but the surfaces map to enterprise release concerns:
          artifact identity, environment gates, progressive delivery, database migration, and audit trails.
        </p>
      </div>
    </section>
  );
}

function Releases({ releases, deployments }) {
  return (
    <section className="grid-two">
      <div className="panel">
        <h2>Release candidates</h2>
        {releases.map((release) => (
          <article className="list-card" key={release.id}>
            <div>
              <h3>{release.name}</h3>
              <p>{release.artifactVersion}</p>
              <small>Git SHA {release.gitSha}</small>
            </div>
            <StatusBadge status={release.status} />
          </article>
        ))}
      </div>
      <div className="panel">
        <h2>Deployment history</h2>
        {deployments.map((deployment) => (
          <article className="list-card" key={deployment.id}>
            <div>
              <h3>{deployment.releaseName}</h3>
              <p>{deployment.environmentName} | {deployment.strategy}</p>
              <small>{deployment.artifactVersion}</small>
            </div>
            <StatusBadge status={deployment.status} />
          </article>
        ))}
      </div>
    </section>
  );
}

function Environments({ environments, deployments }) {
  return (
    <section className="grid-three">
      {environments.map((environment) => {
        const lastDeployment = deployments.find((deployment) => deployment.environmentName === environment.name);
        return (
          <article className="panel environment" key={environment.id}>
            <h2>{environment.name}</h2>
            <p>{environment.url}</p>
            <strong>{environment.requiresApproval ? 'Approval required' : 'Automated promotion'}</strong>
            <small>Latest: {lastDeployment?.releaseName ?? 'No deployment yet'}</small>
          </article>
        );
      })}
    </section>
  );
}

function FeatureFlags({ flags, onChanged }) {
  const [savingKey, setSavingKey] = useState('');

  async function toggle(flag) {
    setSavingKey(flag.key);
    await api.updateFlag(flag.key, {
      isEnabled: !flag.isEnabled,
      rolloutPercentage: flag.isEnabled ? 0 : Math.max(flag.rolloutPercentage, 10),
    });
    setSavingKey('');
    await onChanged();
  }

  return (
    <section className="panel">
      <h2>Feature flags</h2>
      <p>Flags show that deployment and release are related but separate decisions.</p>
      {flags.map((flag) => (
        <article className="flag-row" key={flag.key}>
          <div>
            <h3>{flag.key}</h3>
            <p>{flag.description}</p>
            <small>{flag.rolloutPercentage}% rollout</small>
          </div>
          <button onClick={() => toggle(flag)} disabled={savingKey === flag.key}>
            {flag.isEnabled ? 'Disable' : 'Enable'}
          </button>
        </article>
      ))}
    </section>
  );
}

function Audit({ events }) {
  return (
    <section className="panel">
      <h2>Audit events</h2>
      <p>Audit events provide release evidence for operations, incident review, and compliance.</p>
      {events.map((event) => (
        <article className="audit-event" key={event.id}>
          <strong>{event.title}</strong>
          <p>{event.details}</p>
          <small>{event.actor} | {new Date(event.occurredAt).toLocaleString()}</small>
        </article>
      ))}
    </section>
  );
}

function StatCard({ label, value, hint }) {
  return (
    <article className="stat-card">
      <span>{label}</span>
      <strong>{value}</strong>
      <small>{hint}</small>
    </article>
  );
}

function StatusBadge({ status }) {
  return <span className={`status ${status.toLowerCase()}`}>{status}</span>;
}

export function EmptyState() {
  return (
    <div className="panel">
      <h2>No release data yet</h2>
      <p>Create a release candidate after the first build artifact is available.</p>
      <Link to="/releases">Review release workflow</Link>
    </div>
  );
}
