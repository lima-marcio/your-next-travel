# Your Next Travel — Project Status

Living summary of the decisions taken, what has been built so far, and what
comes next. For day-to-day standards see `.ia/00-project.md`,
`.ia/10-backend.md`, `.ia/20-frontend.md`, `.ia/30-conventions.md`; for the
full implementation plan see
`docs/superpowers/specs/2026-07-28-frontend-design.md` (the source of truth
this document summarizes). Roadmap phases: `docs/roadmap.md`.

## 1. What the product is

A travel-planning app, not a booking engine (no cart/payment flow), built
around two flows:

- **Destination Guide** (renamed from "Dossier" — see §2): given a
  destination and trip length, assembles weather/best-time-to-visit, currency
  exchange rate, a real lodging price estimate, visa/health requirements, and
  an estimated budget adjusted by traveler profile (student / tourist /
  business).
- **Interest-based discovery**: users register interests (motorsport,
  football, auctions, concerts, arts, cultural festivals, etc.) and the app
  surfaces destinations/dates with matching events across three horizons
  (within a week / next month / next semester), plus a "Random Outing"
  surprise mode.
- **Trip Plans** (new, planned — §2.9): a persisted, editable itinerary per
  user (destination, dates, self-reported lodging/car rental, selected
  events), separate from the disposable Destination Guide result.

Prices/data come from real external APIs (Amadeus, football-data.org,
Ticketmaster, Google OAuth) so budget estimates stay grounded in reality.

**Stack:** .NET 10 / ASP.NET Core / EF Core / SQLite (dev) / SQL Server
(prod) / JWT + Google OAuth on the backend; React 19 / TypeScript / Vite /
Tailwind / Axios / React Router / TanStack Query / React Hook Form / Zod on
the frontend (not started yet — see §4).

## 2. Key decisions taken

### Product / naming
- **"Dossier" → "Destination Guide."** The word "dossier" reads like a
  police file in Portuguese and is off-putting for a consumer app. Executed
  in code (see §3) — controller, service, DTOs, route, config keys, README
  and roadmap all updated.
- **No booking engine.** Lodging/car rental data in Trip Plans is
  self-reported by the user (typed in), not selected against live inventory.
  Rationale: building real inventory selection isn't worth it for an MVP;
  revisiting after collecting feedback is tracked in `docs/roadmap.md`.
- **Price disclaimer required** wherever estimates are shown or entered
  ("Valor estimado — o valor definitivo será confirmado diretamente com a
  empresa selecionada"), both on Destination Guide and Trip Plan screens.

### Domain modeling
- **`Domain/Geography` (Country/Region/City for user home location) kept
  separate from `Domain/Destinations` (Country/City for travel-destination
  search)**, despite overlapping real-world concepts — different lifecycles
  (curated reference data vs. on-demand geocoding). Flagged in the roadmap as
  a "revisit if duplication becomes painful" item.
- **"Region" chosen over "estado"/"state"/"província"** as the
  internationalized term for the sub-national administrative level (ISO
  3166-2-aligned), since no single national term generalizes across
  countries. Same reasoning for **"City"** over "município."
- **Region seed data covers Brazil only** (27 states/DF) for this pass —
  seeding the whole world needs an external reference dataset the agent
  can't fetch under the no-external-calls constraint (§2, Agent operating
  constraints). `Country` is fully seeded (~195 rows); `City` is
  get-or-create on demand, not pre-seeded.
- **Interest taxonomy** finalized as: Motorsport, Motorcycling (new),
  ExtremeSports (new), Football, Auctions, ConcertsShows, Arts (new,
  cinema/theatre/performing arts), CulturalFestivals. `ConcertsShows` and
  `Arts` deliberately kept separate; revisit post-launch if redundant.
- **Trip Plan lodging/car rental fields are free-text category strings**
  ("3 estrelas", "econômico"), not enums — categories vary too widely across
  lodging/rental types to enumerate usefully.

### Auth & security
- **Refresh tokens with rotation**: new `RefreshToken` entity, hash stored
  (never the raw token); `POST /api/auth/refresh` rotates on use. Proposed
  lifetime **14 days** — flagged as an assumption to confirm, not
  user-specified.
- **Access token lifetime reduced from 24h to 2h**, matching the project
  baseline rule now in `.ia/10-backend.md`. This is the specific change
  currently uncommitted in the working tree (`JwtOptions.ExpiryMinutes`
  1440 → 120, `appsettings.json` to match).
- **Registration no longer returns an access token** — matches the new
  frontend rule that sign-up redirects to sign-in instead of auto-login.
- **Soft delete for users** (`DeletedAtUtc`), not hard delete — reusing a
  soft-deleted email is explicitly out of scope for this pass.
- **`FirstLoginAtUtc` / `LastAccessAtUtc` drive the post-login redirect**:
  first successful login ever → `/profile` (onboarding), otherwise →
  `/dashboard`. `LastAccessAtUtc` updates on login/Google login/refresh only,
  not on every API call (an MVP cost scoping choice, flagged as such).
- **Passwords**: irreversible hashes only (ASP.NET Core Identity
  `PasswordHasher<TUser>`, salted PBKDF2), minimum 8 characters, must combine
  upper/lower/digit/special character. Email format validated with the
  built-in `[EmailAddress]` attribute — custom regex explicitly disallowed.
- **Builds are blocked on any NuGet vulnerability**, any severity (not just
  high/critical), via `Directory.Build.props`. Vulnerable transitive
  packages get pinned via a direct `PackageReference`, not a lowered audit
  threshold.

### Backend architecture
- Feature-based folders (`Domain/`, `Features/`, `Infrastructure/`,
  `BackgroundServices/`); controllers orchestrate only, services hold
  business rules; manual mapping (no AutoMapper).
- **`Program.cs` never references a project service directly** — every
  registration flows through one `ApplicationServicesExtensions` that calls
  per-feature/per-infrastructure extension methods.
- **External integrations (weather/currency/lodging/events) sit behind one
  interface per source**, so providers are swappable; data is cached and
  refreshed by a `BackgroundService` on a `PeriodicTimer`, never fetched
  live per request.
- **Agent operating constraints** (process decisions, not product ones):
  EF migrations are applied manually by a human, never by the agent; calls
  to real external APIs only happen during the user's manual testing phase;
  data seeds never run automatically on startup.

### Frontend architecture (planned, not yet built)
- Feature-sliced modules (`api/ components/ features/ hooks/ pages/ routes/
  stores/ types/`); server state via TanStack Query, client auth-session
  state via a small Zustand store persisted to `localStorage`.
- Axios instance with a request interceptor for the bearer token and a
  response interceptor that attempts one silent refresh on 401 before
  clearing the session and redirecting to `/signin`.
- Mandatory baseline pages (project-wide rule, not specific to this app):
  landing, sign in, sign up, profile, dashboard; dashboard = collapsible
  left sidebar + top navbar with right-aligned user menu (profile/logout).
- Visual direction: warm/editorial travel-guide feel (strong imagery,
  editorial typography). An **adaptive theme that shifts tone by the user's
  selected interests** was considered and deliberately deferred to end of
  project (`docs/roadmap.md`).

## 3. What has been done so far

**Backend — scaffolded and partially hardened:**
- Initial scaffold (`07a2fee`): Auth (register/login/Google/`me`), Dossier
  (destination guide, pre-rename), Discovery, external integrations
  (weather, currency, lodging, events providers), background refresh
  service. This is the only EF Core migration applied so far
  (`20260727214444_InitialCreate`).
- `da2bcc1`: centralized service registration through
  `ApplicationServicesExtensions`; build now gated on any NuGet
  vulnerability, any severity.
- `0a862e1`: password complexity enforcement on register; removed an unused
  NuGet package.
- `ec09d52`: email format validation on register/login requests.
- **Uncommitted right now:**
  - `JwtOptions.ExpiryMinutes` and `appsettings.json`'s `Jwt:ExpiryMinutes`
    changed 1440 → 120, implementing spec item 1.5 (access token lifetime).
  - Spec item **1.1** (Dossier → Destination Guide rename) fully applied:
    `Features/Dossier/*` moved to `Features/DestinationGuide/*`
    (`DossierController`/`DossierService`/`IDossierService`/`DossierDtos`/
    `DossierFeatureExtensions` → `DestinationGuide*` equivalents, namespace
    updated), route `api/dossier` → `api/destination-guide`, config section
    `Dossier` → `DestinationGuide` in `appsettings.json`, config keys in
    `EventMatchingService` updated to match, plus wording fixes in
    `LodgingPriceEstimate.cs`, `README.md` and `docs/roadmap.md`. No schema
    change involved (no migration needed) — `dotnet build` verified clean.
    Not yet committed.

**Design work:**
- `e07431b` / `3966436`: frontend design spec drafted and then revised
  (`docs/superpowers/specs/2026-07-28-frontend-design.md`) after new
  baseline rules were added to `.ia/`. Status: **approved, ready for
  implementation** — this is the plan being executed against, not yet
  built.

**Not started yet:**
- No frontend project exists (`frontend/` directory hasn't been created).
- Beyond 1.1 (rename, done) and 1.5 (JWT expiry, done), none of the other
  Part 1 backend revisions are implemented in code yet: the interest
  taxonomy change, event pricing fields, refresh tokens, soft delete,
  `FirstLoginAtUtc`/`LastAccessAtUtc`, the `Domain/Geography` entities, and
  `Domain/TripPlanning` all still reflect the pre-revision state — only one
  migration exists, no `RefreshToken`/`Geography`/`TripPlan` entities in the
  codebase yet.

## 4. Next steps

In spec order (`docs/superpowers/specs/2026-07-28-frontend-design.md`, Part
1 — backend revisions to finish before frontend work starts):

1. **1.1** ✅ done in code (uncommitted) — `Dossier` → `Destination Guide`
   rename throughout (controller, service, DTOs, extensions, route
   `api/dossier` → `api/destination-guide`, config keys, README, roadmap).
2. **1.2** Apply the interest taxonomy revision (new enum values,
   Ticketmaster/OpenF1 provider tagging changes) — requires a migration and
   a manual drop/reseed of the dev SQLite DB (human-run, not agent-run).
3. **1.3** Add event pricing fields (`MinPriceAmount`/`MaxPriceAmount`/
   `PriceCurrency`) and the `sort=date|price` query param on discovery
   endpoints — new migration.
4. **1.4** Auth completeness: `RefreshToken` entity + rotation, `POST
   /api/auth/logout`, `DELETE /api/auth/me` (soft delete), registration
   response no longer includes a token — new migration.
5. **1.5** ✅ done in code (uncommitted) — access token lifetime 2h.
6. **1.6** No separate action — this item just documents how 1.2–1.9 get
   delivered (migrations stop short of being applied; live external-API
   verification deferred to manual testing).
7. **1.7** Add `FirstLoginAtUtc`/`LastAccessAtUtc` to `User`, wire the
   first-login redirect signal (`IsFirstLogin`) into the auth response —
   new migration.
8. **1.8** Add `Domain/Geography` (Country/Region/City), `User.CountryId`/
   `RegionId`/`CityId`, seed ~195 countries + Brazil's regions, new
   `GeographyController` — new migration.
9. **1.9** Add `Domain/TripPlanning` (`TripPlan`, `TripPlanEvent`) and
   `TripPlanController` (`api/trip-plans`) — new migration.

Then **Part 2 — frontend build**, from scratch: project setup
(`frontend/`), `DashboardShell` (sidebar + navbar), public pages (Landing,
SignIn, SignUp), protected pages (Dashboard, Destination Guide
search/result, Discovery feed, Profile with cascading Country/Region/City
selects and interests), Axios instance with silent-refresh interceptor,
TanStack Query hooks per feature, Vitest + RTL + MSW test coverage for the
flows listed in the spec's Testing section. Google OAuth on the frontend
stays explicitly deferred (email/password only for now).

**Open assumptions still pending confirmation** (called out in the spec,
not yet decided by the user): refresh token lifetime (14 days proposed);
`LastAccessAtUtc` update scope (session events only, not every API call);
`Region` seed coverage limited to Brazil for this pass.
