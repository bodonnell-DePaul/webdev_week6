# 📘 CI/CD — From Pipeline to Enterprise Delivery System

---

## 🎯 Learning Objectives

By the end of this lecture, students should be able to:

1. **Explain** the CI/CD pipeline and distinguish between Continuous Integration, Continuous Delivery, and Continuous Deployment.
2. **Author** GitHub Actions workflows that lint, test, build, and promote a web application.
3. **Describe** how enterprise teams protect the main branch, govern pull requests, and enforce quality gates.
4. **Compare** blue/green, red/black, canary, and feature flag deployment strategies.
5. **Explain** how Infrastructure as Code differs from application deployment in lifecycle, risk, and review model.
6. **Use AI tools** to generate and validate CI/CD configuration, while recognizing common pitfalls.
7. **Deploy** a web application to Azure App Service and Azure Static Web Apps from a GitHub Actions pipeline.
8. **Evaluate** a CI/CD workflow as a socio-technical system: people, process, permissions, evidence, and automation.

---

## Part 1: Foundations

### 1.1 What Is CI/CD?

**Continuous Integration (CI)** means every developer merges code to a shared branch frequently — at least once a day — and every merge triggers an automated build and test run. The key idea: catch problems early.

**Continuous Delivery** means software is always in a deployable state. Every change that passes CI _could_ be deployed, but a human approves the release.

**Continuous Deployment** means every change that passes CI is _automatically_ deployed to production with no manual gate.

| | Continuous Delivery | Continuous Deployment |
|---|---|---|
| **Human approval** | Yes | No |
| **Risk level** | Lower | Higher — requires excellent coverage |
| **Common in** | Enterprise, regulated industries | SaaS startups, mature DevOps teams |

> In this course we practice Continuous Delivery: the pipeline automates quality gates, but a GitHub environment protection rule approves the production deploy.

**The pipeline** is the sequence of automated steps from commit to production:

```
commit → install → lint → test → build → deploy staging → approve → deploy production
```

Each step is a gate: if it fails, the pipeline stops and the developer is notified.

#### Why CI/CD?

| With CI/CD | Without CI/CD |
|---|---|
| Fast feedback — broken code found in minutes | Bugs found days or weeks later |
| Repeatable, boring deployments | Manual, stressful releases |
| Frequent small changes | "Big bang" releases |
| Rollback in seconds | Manual rollback procedures |

---

### 1.2 CI/CD as an Enterprise Capability

CI/CD in a mature organization is not "a YAML file that runs tests." It is the **delivery control plane** for software change.

It answers:
- What changed?
- Who reviewed it?
- What evidence proves it was tested?
- Which artifact was built?
- Which environment received it?
- Who approved production?
- How do we know it is healthy?
- How do we roll back safely?

**Vocabulary students often conflate:**

| Term | Meaning |
|---|---|
| Continuous Integration | Merging validated changes to a shared branch frequently |
| Continuous Delivery | Keeping software always deployable |
| Continuous Deployment | Auto-deploying validated changes to production |
| Release Management | Deciding when and how users receive capabilities |
| Progressive Delivery | Gradually exposing changes via traffic routing, rings, canaries, or flags |

**Instructor emphasis:** Students reduce CI/CD to "tests run on GitHub." Push them toward release engineering thinking: identity, approvals, auditability, rollback, database safety, observability, and blast-radius control matter as much as the build command.

---

## Part 2: GitHub Actions

### 2.1 Core Concepts

| Concept | What It Is |
|---|---|
| **Workflow** | A YAML file in `.github/workflows/` — defines an automated process |
| **Trigger / Event** | What causes the workflow to run |
| **Job** | A set of steps that run on the same runner VM |
| **Step** | A single task — either a shell command or a reusable action |
| **Action** | Pre-built, reusable step (`actions/checkout@v4`) |
| **Runner** | The virtual machine that executes your workflow |

Jobs run in parallel by default unless you declare `needs:` dependencies.

### 2.2 Workflow YAML Structure

```yaml
name: CI Pipeline

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

permissions:
  contents: read

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
      - run: npm ci
      - run: npm run lint
      - run: npm test
      - run: npm run build
```

### 2.3 Triggers

| Trigger | When It Fires | Common Use |
|---|---|---|
| `push` | Code pushed to branch | Run CI on main |
| `pull_request` | PR opened or updated | Run checks before merge |
| `schedule` | Cron schedule | Nightly security scans |
| `workflow_dispatch` | Manual run button | On-demand deploys |
| `release` | GitHub release published | Publish release artifacts |

**Best practice:** Use `pull_request` for quality checks, `push` to `main` for deployments.

### 2.4 Secrets and Environment Variables

**Never hard-code passwords, tokens, or API keys.**

- Store secrets under **Settings → Secrets and variables → Actions**
- Reference: `${{ secrets.SECRET_NAME }}`
- Secrets are masked in logs, but don't split or encode them
- Environment variables at workflow, job, or step level via `env:`

**OIDC (OpenID Connect)** is the modern replacement for long-lived secrets in cloud deployments. Instead of storing a static service principal password, GitHub exchanges a short-lived token with Azure/AWS/GCP:

```yaml
- uses: azure/login@v2
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

No long-lived credential stored. The token expires after the job.

### 2.5 Matrix Builds and Caching

**Matrix builds** run the same job multiple times in parallel:

```yaml
strategy:
  matrix:
    node-version: [18, 20, 22]
```

**Caching** reduces install time from ~60s to ~5s:

```yaml
- uses: actions/setup-node@v4
  with:
    node-version: '20'
    cache: 'npm'   # Built-in cache
```

### 2.6 Complete Production CI Workflow

A real workflow has three sequential jobs:

1. **lint-and-typecheck** — fast, runs first
2. **test** — depends on lint passing, matrix across Node versions
3. **build-and-deploy** — depends on tests passing, only on `main`

The `needs:` keyword chains jobs so a failure halts the downstream chain. Use `if: github.ref == 'refs/heads/main'` to prevent PR runs from triggering deployment.

---

## Part 3: AI-Assisted Workflow Generation

### 3.1 Using AI for CI/CD

AI tools like GitHub Copilot and ChatGPT can generate complete workflow YAML quickly, but prompt quality matters.

| Bad Prompt | Good Prompt |
|---|---|
| "Make me a GitHub Actions file" | "Generate a GitHub Actions CI/CD workflow for a Node.js 20 Express API that runs ESLint, Jest tests with coverage, and deploys to Azure App Service on push to main. Use npm ci and cache dependencies." |

AI gets you ~80% of the way there. Your job is the last 20%.

### 3.2 What to Verify in AI-Generated Workflows

| Check | What to Look For |
|---|---|
| Action versions | Use `@v4`, not `@v2` or `@v3` |
| Node version | Match `.nvmrc` or `engines` field |
| Secrets | Are names correct? Do they exist in repo settings? |
| Working directory | Monorepo? Need `working-directory:`? |
| Permissions | Minimum necessary — not default `write-all` |
| Build command | Does `npm run build` exist in package.json? |

### 3.3 Common AI Mistakes

**Security:**
- Echoing secrets in logs: `echo "${{ secrets.TOKEN }}"` — secrets can leak even when masked
- Unpinned actions: `uses: some-action@main` — supply chain attack vector

**Correctness:**
- Outdated action versions (`@v2`, `@v3`)
- EOL Node versions (14, 16)
- Missing `working-directory:` for monorepos
- Forgetting service containers for database tests
- Wrong build output path (`./build` vs `./dist`)
- Missing `npm run build` before deploy

**Always test the workflow on a branch first, not main.**

---

## Part 4: Branch Protection and PR Governance

### 4.1 Main Branch as Production Contract

The main branch should represent the version of the system that is releasable, supportable, and trusted. Protecting it creates a reliable integration point — not bureaucracy.

**Enterprise branch protection capabilities:**

- Pull requests required before merge
- Required status checks (CI must pass)
- Required code owner review
- Required minimum number of approving reviewers
- Stale approval dismissal when new commits are pushed
- Linear history / squash merge for cleaner audit trails
- Restrictions on force pushes and deletions
- Admin bypass controls that are visible and logged

**Failure mode if skipped:** A single accidental push can bypass tests, skip review, modify deployment credentials, introduce vulnerable dependencies, or deploy unreviewed infrastructure changes.

### 4.2 CODEOWNERS

CODEOWNERS maps repository paths to accountable reviewers. Tied to branch protection, it becomes an enforcement mechanism — not just documentation.

Examples:
- Database migrations → data owner review required
- Auth code → security owner review required
- Infrastructure changes → platform owner review required
- Workflow changes → DevOps/platform owner review required

```
# .github/CODEOWNERS
/src/auth/              @security-team
/infrastructure/        @platform-team
/.github/workflows/     @devops-team
/migrations/            @backend-team @data-team
```

### 4.3 The Pull Request as the Unit of Change

A PR is where human judgment and automated evidence meet. A strong PR answers:
- What problem does this solve?
- How was it tested?
- What risks remain?
- Does it affect infrastructure, data, security, or compliance?

**Automated PR gates enterprises commonly require:**
- Frontend lint, build, unit tests, accessibility checks
- Backend restore, build, unit/API tests
- Dependency review and vulnerability scanning
- Secret scanning
- SAST (static application security testing)
- IaC formatting and plan generation
- Preview environment creation

**Human review focus:**
- Correctness and edge cases
- Security and authorization
- Maintainability
- Operational impact (deployment and rollback risk)
- Test adequacy
- Whether the change belongs in this PR

**Key insight:** Automated checks without human review miss context. Human review without automation becomes slow and inconsistent. Mature teams use both.

---

## Part 5: Artifact Promotion

### 5.1 Build Once, Promote Many

A production deployment should promote the **same artifact** that passed CI. Never rebuild separately for staging and production — the thing that was tested must be the thing that ships.

This creates confidence that production receives the exact tested package.

**Enterprise artifact pipeline produces:**
- Backend package or container image
- Frontend static asset bundle
- Test reports
- SBOM (Software Bill of Materials) in CycloneDX/SPDX format
- Artifact signature and provenance metadata
- Build number, git SHA, and source branch
- Release notes generated from merged PRs

### 5.2 Supply Chain Security

Supply chain security asks: "Can we prove where this artifact came from and that it was not tampered with?"

**Practices:**
- Pin third-party actions by SHA (not version tag) for high-security workflows
- Use scoped workflow permissions (`permissions: contents: read`)
- Generate SBOMs and sign artifacts
- Use OIDC for short-lived cloud credentials (no stored secrets)
- Use Dependabot or Renovate to update dependencies intentionally
- Track provenance: which git commit, which workflow run, which actor

---

## Part 6: Azure Deployment

### 6.1 Deployment Options

**Azure App Service** — best for Node.js/Express APIs, .NET backends:
- PaaS — no server management
- Auto-scaling, SSL, custom domains
- Deployment slots for blue/green deploys
- Built-in App Insights integration
- Free tier (F1): 60 min/day compute

**Azure Static Web Apps** — best for React/Vue/Angular SPAs:
- Free hosting with global CDN
- Automatic HTTPS
- PR preview environments built-in
- Integrated serverless API (Azure Functions)
- Auto-generates GitHub Actions workflow on setup
- Free tier: 2 custom domains, 100 GB bandwidth/month

**Rule of thumb:** Static Web Apps for the React frontend, App Service for the Express/dotnet API.

### 6.2 Deploying with OIDC (Recommended for Production)

```yaml
- uses: azure/login@v2
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

- uses: azure/webapps-deploy@v3
  with:
    app-name: ${{ secrets.AZURE_APP_NAME }}
    package: .
```

### 6.3 Deploying with Publish Profile (Simpler for Learning)

Download publish profile from Azure Portal → store entire XML as `AZURE_PUBLISH_PROFILE` secret:

```yaml
- uses: azure/webapps-deploy@v3
  with:
    app-name: ${{ secrets.AZURE_APP_NAME }}
    publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
    package: ./dist
```

---

## Part 7: Environments and Deployment Gates

### 7.1 Environment Management

Most real apps run in multiple environments:

| Environment | Purpose | Risk |
|---|---|---|
| Development / local | Feature development | None |
| PR preview | Review changes before merge | Low |
| Staging | Pre-production validation | Medium |
| Production | Live user-facing application | High |

Each environment has its own database, API keys, secrets, URL, and logging settings.

**GitHub Environments** let you define protection rules per environment:

```yaml
jobs:
  deploy-prod:
    environment:
      name: production
      url: https://myapp.com
    # pauses here until environment protection rules are satisfied
```

Protection rules:
- Required reviewers — someone must approve before deploy
- Wait timer — delay deployment by N minutes
- Branch restrictions — only `main` can deploy to production
- Environment secrets — different secrets per environment

### 7.2 Deployment Gates

Gates can be automated, manual, or both:

**Automated gates:**
- Tests must pass
- Artifact must be built by a trusted workflow
- Security scan must not exceed threshold
- Staging health checks must pass

**Manual gates:**
- Production deployment requires environment approval
- Change ticket or release record must be linked
- Database migration plan must be reviewed
- Deployment windows or freeze periods apply

**Separation of duties:** In regulated environments, the person who writes the change may not be the same person who approves and deploys it. CI/CD systems enforce this through required reviews, environment approvals, and audit logs.

**Failure mode if skipped:** Without gates, production becomes just another target. The organization loses evidence, accountability, and the ability to reason about release risk.

---

## Part 8: Infrastructure as Code

### 8.1 What Is IaC?

Infrastructure as Code defines cloud resources in code files instead of portal clicks — networks, databases, identity, permissions, hosting, queues, DNS, monitoring, and secrets configuration.

**Azure IaC options:**
- **Bicep** — Azure-native, clean syntax, compiles to ARM
- **Terraform** — multi-cloud, large ecosystem
- **ARM templates** — JSON, verbose, still supported

**Benefits:** Version-controlled, reviewable, repeatable. Recreate entire environments from scratch. Prevent configuration drift.

### 8.2 IaC vs. Application Deployment — They Are Different

| Dimension | Application Deployment | Infrastructure as Code |
|---|---|---|
| Primary artifact | App package, container, static bundle | Desired state definition |
| Typical change | Code behavior | Environment capabilities and permissions |
| Feedback | Tests, health checks, logs | Plan/apply, drift detection, policy checks |
| Rollback | Redeploy previous artifact | Reconcile state; rollback may not be simple |
| Blast radius | Usually one app or service | Can affect networking, data, identity, cost |
| Review focus | Correctness and user behavior | Security, cost, permissions, availability |
| Approval model | Release owner or environment approver | Platform, security, data, or finance owner |

**Key principle:** IaC changes should be gated separately from application changes. They have different reviewers, different risk profiles, and different approval models.

### 8.3 IaC Pipeline Pattern

1. Validate formatting and syntax
2. Generate a **plan** (what will change)
3. Post the plan to the PR for review
4. Review security, cost, and blast radius
5. **Apply** only after merge and environment approval
6. Detect drift between declared state and real state

```yaml
# Separate workflows:
# iac-plan.yml  — runs on PR, shows what will change
# iac-apply.yml — runs on merge to main, with environment approval gate
```

**GitOps note:** In a GitOps model, a controller (Flux, ArgoCD) reconciles the runtime environment to match the desired state in Git. The environment _pulls_ desired state rather than a workflow _pushing_ changes.

---

## Part 9: Progressive Delivery

### 9.1 Blue/Green Deployment

Two production-like environments: one serves live traffic (blue), the other receives the new release (green). After validation, traffic switches.

**Good for:** Fast rollback, lower downtime, full-environment validation before exposure.

**Watch out for:** Database compatibility between versions, cost of duplicate infrastructure, long-running sessions, background jobs in both environments.

**In Azure App Service:** Deployment slots provide built-in blue/green with instant swap:

```bash
az webapp deployment slot swap --name myapp --slot staging
# If broken, swap back — zero downtime
```

### 9.2 Red/Black Deployment

Common variant of blue/green terminology. The mechanics are similar: keep one environment live while preparing the replacement.

### 9.3 Canary Deployment

Deploy new version to a small percentage of traffic (5–10%), measure impact, then gradually expand.

**Good for:** Real user measurement, reduced blast radius, high-traffic systems with strong telemetry.

**Watch out for:** Requires traffic routing infrastructure, clear success metrics, and fast automated rollback.

### 9.4 Feature Flags

Decouple **deployment** from **release**. Code ships but features are hidden until explicitly enabled for a user, tenant, ring, or environment.

**Good for:** Dark launches, gradual enablement, kill switches, A/B testing.

**Watch out for:** Flag debt accumulation, complex flag combinations, and the mistake of treating "flag is off" as "feature is secure" — hidden UI does not mean protected backend access.

### 9.5 Rollback vs. Roll-Forward

**Rollback:** Return to a previous known-good version. Good for simple stateless application regressions.

**Roll-forward:** Fix the issue in a new release and deploy it. Often preferred for data changes because the database may have already been migrated.

**Key insight:** Every deployment should have a rollback plan. The question is not _if_ a deploy will fail, but _when_.

---

## Part 10: Data — The Hard Part

Application code can often roll back quickly. Databases are different because data persists and may have been modified by the new version.

### 10.1 Expand and Contract Pattern

A safe migration strategy for production databases:

1. **Expand:** Add backward-compatible schema changes (new column, new table)
2. Deploy code that writes both old and new formats
3. Backfill existing data
4. Switch reads to the new structure
5. **Contract:** Remove old columns/tables after the old version is fully retired

### 10.2 Why Database Gates Are Separate

Database changes can:
- Lock large tables
- Destroy data irreversibly
- Break older application versions still running during deployment
- Fail under production data volume in ways tests miss
- Be difficult or impossible to roll back

**Pipeline implication:** Keep `db-migrate.yml` as a **separate workflow** with its own approval gate. Do not embed migrations in application startup as a side effect.

---

## Part 11: Secrets, Identity, and Least Privilege

Pipelines are powerful identities. A workflow that can deploy to production is part of the production security boundary.

### Enterprise Practices

- Store secrets outside the repository (GitHub Secrets, Azure Key Vault)
- Scope secrets by environment — staging and production secrets are separate
- Use OIDC for short-lived cloud credentials where possible
- Give each job the **minimum required permissions** (`permissions:` block)
- Separate read-only PR workflows from deploy workflows
- Never expose secrets to untrusted forked PRs
- Scan commits for accidental secrets (GitHub secret scanning, truffleHog)
- Rotate secrets and audit usage

**Failure mode if skipped:** A compromised workflow token or leaked cloud secret can allow attackers to modify production infrastructure — potentially worse than a vulnerable endpoint.

---

## Part 12: Observability and Post-Deployment Validation

Deployment is not complete when the workflow turns green. Deployment is complete when the system is healthy and the team can observe the impact.

### Signals to Watch

- Liveness and readiness health checks (`/health/live`, `/health/ready`)
- Error rate — trending up after deploy?
- Latency — p50, p95, p99 response times
- Saturation — CPU, memory, connection pool
- Logs with correlation IDs
- Business metrics — did conversion drop?
- Deployment event recorded in observability platform

### Azure Application Insights

Add with a few lines of code for automatic telemetry:

```javascript
const appInsights = require('applicationinsights');
appInsights.setup(process.env.APPINSIGHTS_CONNECTION_STRING)
  .setAutoCollectRequests(true)
  .setAutoCollectExceptions(true)
  .start();
```

Tracks: request performance, errors, dependencies, exceptions, custom events.

### Alert Hygiene

- Start with essential alerts only: error rate > 5%, response time > 3s, availability < 99%
- Each alert must be actionable — if you can't act on it, don't alert on it
- Alert fatigue leads to ignored alerts, which defeats the purpose

### Post-Deployment Validation Steps

1. Smoke test the deployed URL
2. Verify `/health/ready`
3. Confirm database connectivity
4. Run a minimal user journey
5. Check telemetry for elevated errors
6. Record a deployment audit event

---

## Part 13: Enterprise Operating Models

### Trunk-Based Development

Small changes merge frequently to main behind feature flags. Requires strong automated tests and disciplined flag management. Supports fast integration and high deployment frequency.

### GitFlow-Style Branching

Long-lived release branches support scheduled releases and maintenance streams, but increase merge complexity and delay integration.

### Platform Teams and Paved Roads

Many organizations create a **platform team** that provides:
- Reusable, versioned workflow templates
- Secure golden deployment paths
- Observability defaults
- Infrastructure modules
- Developer experience tooling

Product teams still own their applications; the platform reduces duplicated DevOps work across the organization.

### DORA Metrics

Four metrics that measure delivery system health:

| Metric | What It Measures |
|---|---|
| Deployment frequency | How often you successfully ship to production |
| Lead time for changes | Time from code commit to production |
| Change failure rate | What percentage of deployments cause incidents |
| Mean time to recovery (MTTR) | How long to restore service after an incident |

**The goal is not to chase metrics blindly.** Use them to diagnose whether the delivery system is fast, safe, and recoverable. A team with a high change failure rate should focus on test coverage and gating before optimizing deployment frequency.

---

## Part 14: ReleaseBoard Demo

### What to Show

**ReleaseBoard** is a small release management app that demonstrates enterprise CI/CD patterns in a working codebase.

1. Open the React dashboard — point out visible build/version metadata (`/api/version`)
2. Show environments and deployment history
3. Toggle a feature flag — discuss deployment vs. release as separate decisions
4. Open audit events — connect to approvals and deployment traceability
5. Call `/api/health/live`, `/api/health/ready`, and `/api/version` directly
6. Walk through the GitHub Actions workflow files:
   - `pr-validate.yml` — PR quality gate (lint, test, build, security scan)
   - `ci-build-artifact.yml` — build once, attach SHA, upload artifact
   - `cd-deploy-staging.yml` — promote artifact to staging
   - `cd-deploy-prod.yml` — gated production deployment with environment approval
   - `iac-plan.yml` / `iac-apply.yml` — separate infrastructure pipeline
   - `db-migrate.yml` — explicit migration gate
   - `nightly-security.yml` — scheduled dependency and SAST scan

### Framing for Students

Tell students: the YAML exists to make the repo complete, but the lesson is the **design** behind each workflow:

- What evidence does this workflow create?
- What risk does each gate reduce?
- What is automated vs. what still requires human judgment?
- What would change in a larger organization?
- Who would own each workflow?

---

## Discussion Prompts

1. What should block a merge to main?
2. What should block a production deployment but not a staging deployment?
3. Who should approve infrastructure changes?
4. When would you choose feature flags instead of blue/green deployment?
5. Why is a database migration riskier than a frontend deployment?
6. What evidence would an auditor want after a production incident?
7. Which DORA metric would you improve first for a team that deploys rarely and fears releases?
8. What are the risks of giving a CI workflow `write-all` permissions?

---

## Key Takeaways

- CI/CD is the delivery system for code, evidence, and organizational trust.
- Main branch protection creates a reliable production contract.
- Pull requests combine human judgment with automated quality evidence.
- Build once: the thing that was tested must be the thing that ships.
- IaC changes environment state and requires a different review and rollback mindset than app deployment.
- Progressive delivery (blue/green, canary, flags) reduces blast radius but depends on observability.
- Database migrations need explicit strategy and gates — never hide them in startup.
- The best pipelines balance velocity, safety, accountability, and recoverability.
- **The goal: make deployment boring.** If shipping is stressful, the pipeline needs work.

---

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure App Service Docs](https://learn.microsoft.com/en-us/azure/app-service/)
- [Azure Static Web Apps Docs](https://learn.microsoft.com/en-us/azure/static-web-apps/)
- [Azure Bicep Documentation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [OIDC in GitHub Actions](https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/about-security-hardening-with-openid-connect)
- [DORA Research](https://dora.dev/research/)
- [OWASP Top 10 CI/CD Security Risks](https://owasp.org/www-project-top-10-ci-cd-security-risks/)
- [Supply Chain Levels for Software Artifacts (SLSA)](https://slsa.dev/)
