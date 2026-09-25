<!--
SYNC IMPACT REPORT
==================
Version change: [none] → 1.0.0 (initial ratification)
Bump rationale: First formal adoption of the project constitution; establishes the
  full governance baseline. Under semantic versioning, the initial published version
  is 1.0.0.

Modified principles: N/A (initial creation)
Added principles:
  - I. Domain-Driven Bounded Contexts
  - II. Layered Architecture & Dependency Direction
  - III. Test-First Quality (NON-NEGOTIABLE)
  - IV. Contract-First REST APIs
  - V. Independent Deployability & Observability
Added sections:
  - Security & Compliance Requirements (Section 2)
  - Development Workflow & Quality Gates (Section 3)
  - Governance

Templates / references requiring review:
  - .specify/templates/* : re-check "Constitution Check" gates align with the five
    principles above (no automated verification performed here).

Deferred TODOs:
  - RATIFICATION_DATE set to 2026-09-24 (date of this initial adoption). If the team
    considers an earlier project-start date authoritative, amend accordingly.

NOTE: This comment block is scratch material for reviewing the amendment and SHOULD be
  removed before the constitution is committed.
-->

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
request that changes domain or application logic without accompanying tests MUST be
rejected or explicitly justified in review.

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
- Every pull request MUST: build successfully, pass the full automated test suite, and be
  reviewed by at least one other contributor before merge.
- Reviewers MUST verify compliance with the Core Principles — bounded-context isolation,
  layering/dependency direction, test coverage, contract/Swagger accuracy, and
  observability — and MUST block merges that violate them without documented justification.
- Any added complexity (extra abstraction, cross-context coupling, new infrastructure)
  MUST be justified in the pull request against the simpler alternative it replaces.
- API changes MUST update the corresponding Swagger/OpenAPI documentation in the same
  change set.

## Governance

This constitution supersedes other conflicting practices for the BuildTruck backend.
Amendments MUST be proposed via pull request, MUST document the rationale and any required
migration, and MUST be approved by the team before taking effect.

Versioning of this constitution follows semantic versioning:
- MAJOR: backward-incompatible governance changes or removal/redefinition of a principle.
- MINOR: a new principle or section, or materially expanded guidance.
- PATCH: clarifications, wording, and non-semantic refinements.

Compliance is enforced at code review: every pull request MUST be checked against the
Core Principles and the Quality Gates above, and violations MUST be resolved or explicitly
justified before merge. Runtime development guidance that elaborates on these principles
belongs in project templates and service-level documentation and MUST remain consistent
with this constitution.

**Version**: 1.0.0 | **Ratified**: 2026-09-24 | **Last Amended**: 2026-09-24
