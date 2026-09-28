# BuildTruck Backend Constitution

## Core Principles

### I. Domain-Driven Bounded Contexts

Each business capability MUST live in its own bounded context, and each context MUST
be self-contained with a clear, single business responsibility (e.g. Auth, Users,
Projects, Materials, Machinery, Incidents, Documentation, Notifications, Stats,
Configuration). A context MUST NOT reach into another context's internal domain model,
persistence, or database tables. Cross-context communication MUST go through an
Anti-Corruption Layer (ACL) or a published REST contract; foreign concepts MUST be
translated at the boundary, never leaked inward.

Rationale: Bounded contexts with explicit ACLs are what let each service be documented,
deployed, and validated independently. Direct coupling between domains reintroduces the
distributed-monolith failure mode the microservice split exists to prevent.

### II. Layered Architecture & Dependency Direction

Every context MUST preserve the four-layer structure already established across services:
`Interfaces` (REST/ACL entry points), `Application` (use cases, command/query handlers),
`Domain` (entities, value objects, domain services, repository abstractions), and
`Infrastructure` (persistence, external services, token providers). Dependencies MUST
point inward: Domain depends on nothing outward; Application depends only on Domain
abstractions; Infrastructure and Interfaces depend inward and are wired via dependency
injection. Business rules MUST live in Domain/Application, never in controllers or
persistence code. Repository access MUST go through Domain-defined interfaces, not
concrete DbContext usage from outer layers.

Rationale: Consistent, inward-pointing layering keeps business logic testable in
isolation and prevents framework or database concerns from contaminating the domain.

### III. Test-First Quality (NON-NEGOTIABLE)

Automated tests are mandatory for every new behavior and every bug fix. New or changed
business logic MUST be covered by tests in `BuildTruckBackend.Tests` before the change is
considered complete, and those tests MUST have been observed to fail (or a bug reproduced)
before the implementation is accepted. The full test suite MUST pass before merge. A pull
request that changes domain or application logic without accompanying tests MUST NOT be
merged unless the omission is explicitly justified in the PR.

Rationale: Tests written against a red state prove the code does what is claimed and
guard the independent-deployment guarantee; skipping them silently erodes both.

### IV. Contract-First REST APIs

Every service MUST expose its capabilities as versioned REST endpoints documented via
Swagger/OpenAPI, and that documentation MUST stay in sync with the implemented contract.
Protected endpoints MUST enforce JWT-based authentication and authorization at the
Interfaces layer. Request and response payloads MUST use explicit DTOs — domain entities
MUST NOT be serialized directly onto the wire. Breaking changes to a published contract
MUST be introduced through a new version or a documented, communicated migration path,
never by silently altering existing request/response shapes or status codes.

Rationale: The frontend and other services integrate through these contracts and their
Swagger docs; unversioned breaking changes silently break consumers in production.

### V. Independent Deployability & Observability

Each service MUST build, run, and deploy independently via its container image and the
shared `docker-compose` / reverse-proxy topology, without requiring another service's
source at build time. Services MUST NOT share a database schema across contexts; each
context owns its data. Every service MUST emit structured logs for authentication
attempts, endpoint execution, database access, and runtime errors so behavior is
reviewable through cloud monitoring (e.g. CloudWatch). Configuration and secrets MUST be
supplied through environment/configuration, never hardcoded, so the same image runs in
every environment.

Rationale: Independent deployment plus consistent observability is the operational
payoff of the microservice split; shared schemas or missing logs collapse it back into a
coupled, undebuggable system.

### VI. Specification-First Development

Every new feature or change in observable behavior MUST start from a specification
at `specs/<feature>/spec.md` that is ready for implementation before implementation code
is written. The specification MUST define user scenarios and uniquely identified acceptance
criteria (e.g. `AC-01`, `AC-02`). Existing behavior that is being modified without a prior
spec MUST first be documented through a reverse-engineering specification. When scope
changes during implementation, the specification MUST be updated before the corresponding
code is merged. A spec is considered ready for implementation when the responsible
developer has completed and verified its user stories, its acceptance criteria, and its
consistency with the agreed scope. Senior review of the specification MAY take place
afterwards and does not block implementation.

Rationale: A written specification is the shared source of truth for what is being built;
writing code first turns requirements into guesswork and makes later review subjective.

### VII. Acceptance Criteria Test Traceability

Every acceptance criterion in a ready specification MUST be covered by at least one
automated test in `BuildTruckBackend.Tests`. Each such test MUST reference the acceptance
criterion it verifies by its identifier (in the test name or as a test trait/category,
e.g. `[Trait("AC", "AC-01")]`). Each specification MUST maintain a traceability matrix
mapping every acceptance criterion to its test(s). An acceptance criterion without an
associated automated test MUST block the merge; manual verification alone does not satisfy
this principle. This principle complements, and does not replace, Principle III.

Rationale: Tests traced to acceptance criteria make "done" objectively verifiable and
prevent requirements from silently going untested.

### VIII. Responsible AI-Assisted Development

AI tools MAY assist in producing specifications, code, tests, and documentation, but a
human developer remains accountable for every change. Every pull request MUST include an
"AI Note" section stating what the AI generated and what the developer reviewed, corrected,
or validated; if no AI assistance was used, the section MUST say so. AI-generated output
MUST pass the same build, test, and review gates as human-written code, and any AI-suggested
dependency, API, or endpoint MUST be verified to exist before merge. Secrets, credentials,
connection strings, and real personal data MUST NOT be supplied to AI tools. AI assistance
MUST NOT be used as the approving reviewer of a pull request.

Rationale: Transparent disclosure and human validation keep AI a productivity aid rather
than a source of unreviewed, unverifiable, or insecure changes.

## Security & Compliance Requirements

- Authentication across the platform MUST use JWT tokens issued by the User Service;
  downstream services MUST validate tokens and MUST NOT trust unauthenticated identity
  claims.
- Secrets (database credentials, signing keys, third-party keys) MUST be provided via
  configuration/environment and MUST NOT be committed to the repository.
- Passwords and other credentials MUST be stored only in hashed/encrypted form; plaintext
  storage is prohibited.
- All externally exposed traffic MUST be routed through the reverse proxy and served over
  HTTPS in deployed environments.
- Input from external callers MUST be validated at the Interfaces layer before reaching
  Application or Domain logic.

## Development Workflow & Quality Gates

- Work is tracked in GitHub; changes MUST be delivered through commits/pull requests with
  a reviewable history, aligned to the team's Sprint cadence.
- Every pull request MUST build successfully and pass the full automated test suite. Before
  merging, the responsible developer MUST personally verify that all Quality Gates in this
  section are met, including their own review and understanding of any AI-generated code.
  Senior review MAY take place after merge; its observations MUST be addressed when
  applicable, through follow-up changes where needed.
- Before merge, the responsible developer MUST verify compliance with all Core Principles —
  including bounded-context isolation, layering/dependency direction, test coverage,
  contract/Swagger accuracy, observability, specification readiness, AC → test
  traceability, and responsible AI use — and MUST NOT merge changes that violate them
  without documented justification. Subsequent senior review observations MUST be resolved
  through follow-up changes when applicable.
- Any added complexity (extra abstraction, cross-context coupling, new infrastructure)
  MUST be justified in the pull request against the simpler alternative it replaces.
- API changes MUST update the corresponding Swagger/OpenAPI documentation in the same
  change set.
- Every pull request that introduces or changes behavior MUST link its
  ready-for-implementation specification under `specs/<feature>/` (Principle VI).
- Every pull request MUST demonstrate AC → test traceability: each acceptance criterion in
  the linked specification maps to at least one passing automated test (Principle VII).
- Every pull request MUST include an "AI Note" section (Principle VIII).
- Where the Spec Kit workflow applies (after `tasks.md` is generated and before
  implementation), `/speckit-analyze` MUST be run and any CRITICAL findings resolved; it is
  not required for changes outside that workflow stage.

## Governance

This constitution supersedes other conflicting practices for the BuildTruck backend.
Amendments MUST be proposed via pull request, MUST document the rationale and any required
migration, and MUST be approved by the team before taking effect.

Versioning of this constitution follows semantic versioning:
- MAJOR: backward-incompatible governance changes or removal/redefinition of a principle.
- MINOR: a new principle or section, or materially expanded guidance.
- PATCH: clarifications, wording, and non-semantic refinements.

Compliance is verified before merge by the responsible developer: every pull request MUST
be checked against the Core Principles and the Quality Gates above, and violations MUST be
resolved or explicitly justified before merge. Observations from subsequent senior review
MUST be addressed when applicable. Runtime development guidance that elaborates on these principles
belongs in project templates and service-level documentation and MUST remain consistent
with this constitution.

**Version**: 1.1.0 | **Ratified**: 2026-09-24 | **Last Amended**: 2026-09-28
