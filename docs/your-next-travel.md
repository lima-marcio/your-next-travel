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
the frontend (public + protected routes built — see §3/§4).

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

### Frontend architecture
- Feature-sliced modules (`api/ components/ features/ hooks/ pages/ routes/
  stores/ types/`); server state via TanStack Query, client auth-session
  state via a small Zustand store persisted to `localStorage`.
- Axios instance with a request interceptor for the bearer token. The
  response interceptor currently only clears the session on a 401 from an
  authenticated request — the spec's silent-*refresh* step is still deferred
  until backend 1.4 ships `POST /api/auth/refresh` (see §3).
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
- **1.1** (Dossier → Destination Guide rename, `8c53c0e`): `Features/Dossier/*`
  moved to `Features/DestinationGuide/*` (`DossierController`/`DossierService`/
  `IDossierService`/`DossierDtos`/`DossierFeatureExtensions` →
  `DestinationGuide*` equivalents, namespace updated), route `api/dossier` →
  `api/destination-guide`, config section `Dossier` → `DestinationGuide` in
  `appsettings.json`, config keys in `EventMatchingService` updated to match,
  plus wording fixes in `LodgingPriceEstimate.cs`, `README.md` and
  `docs/roadmap.md`. No schema change needed (no migration).
- **1.5** (`15c6b50`): access token lifetime 24h → 2h (`JwtOptions.ExpiryMinutes`
  / `appsettings.json`).

**Design work:**
- `e07431b` / `3966436`: frontend design spec drafted and then revised
  (`docs/superpowers/specs/2026-07-28-frontend-design.md`) after new
  baseline rules were added to `.ia/`. Status: **approved, ready for
  implementation** — this is the plan being executed against.

**Frontend — Part 2, Public routes (`67d175d`, pushed):**
- `frontend/` scaffolded: Vite + React 19 + TypeScript, per `.ia/20-frontend.md`
  (Tailwind v4, Axios, React Router, TanStack Query, React Hook Form, Zod,
  plus Zustand for the auth store as called for in the design spec).
  `react-router-dom` is pinned to the latest release (7.18.2); its one open
  advisory is RSC-mode-specific and doesn't apply to this client-only SPA —
  downgrading to a version outside the advisory range would trade it for over
  a dozen *unpatched* CVEs, so latest is the safer choice.
- Design direction: warm/editorial travel-guide aesthetic per the spec,
  built with the `design-taste-frontend` skill — Playfair Display (display) +
  Inter Tight (body/UI), terracotta + slate palette (deliberately not the
  beige/brass/oxblood combo that's the generic default for this kind of
  brief), light/dark mode via `prefers-color-scheme`.
- Built: `LandingPage` (`/`), `SignInPage` (`/signin`), `SignUpPage`
  (`/signup`), plus a `NotFoundPage` catch-all. Shared infra: Axios client
  with bearer-token attach and a 401 handler, Zustand auth store persisted
  to `localStorage`, Zod schemas mirroring the backend's validation rules
  exactly, ProblemDetails-aware error parsing.
- Verified against the real backend (not just mocks): `dotnet build` +
  `npm run build` both clean, `npm run lint` (oxlint) clean, 4 Vitest/RTL/MSW
  tests covering the spec's auth-flow scope (sign up → redirect to sign in
  with confirmation → sign in → invalid-credentials error → client-side
  validation) all passing. Manually smoke-tested the real `/api/auth/register`
  and `/api/auth/login` endpoints via curl to confirm the response contract
  (this caught and fixed a real bug: a bare 401 from login actually carries an
  auto-generated `title: "Unauthorized"` from ASP.NET's ProblemDetails
  middleware, not an empty body as first assumed — the frontend's error
  parsing was adjusted accordingly).

**Frontend — Part 2, Protected routes (uncommitted):**
- `AuthGuard` (redirects to `/signin` when there's no session) +
  `DashboardShell` (collapsible `Sidebar` — Dashboard/Guide/Discovery/Profile
  — persisted to `localStorage`, and a `Navbar` with an avatar dropdown for
  Profile/Sair) wrap five new routes: `DashboardPage` (`/dashboard`),
  `DestinationGuideSearchPage` (`/guide`) + `DestinationGuideResultPage`
  (`/guide/:searchId`), `DiscoveryFeedPage` (`/discovery`), `ProfilePage`
  (`/profile`). New shared `ErrorState` (retry) and `EmptyState` components.
- Every new page/hook is built against the backend's **current, real**
  DTOs (read directly from the C# source, not assumed) — this surfaced a
  real gap between the design spec (which assumes all of Part 1 is done)
  and what's actually implemented, so several spec'd pieces are
  deliberately scoped down or stubbed for now, each flagged in the UI or a
  code comment rather than built against a contract that doesn't exist:
  - **Profile → Location section**: the spec's cascading Country/Region/City
    selects need `GeographyController` (backend 1.8, not implemented). Shown
    as a labeled "coming soon" placeholder instead of a broken form.
  - **Profile → Delete account**: needs `DELETE /api/auth/me` (backend 1.4,
    not implemented). Same "coming soon" treatment, low-key per the spec.
  - **Discovery → price sort toggle**: needs the `sort` query param and
    price fields on `DiscoverySuggestion` (backend 1.3, not implemented).
    Feed renders date-ordered groups only; no sort control shown.
  - **Interests**: chips use the *current* 8-value `InterestCategory` enum
    (`MotorsportF1`/`F2`/`Dtm`/`StockCar`, Football, Auctions, ConcertsShows,
    CulturalFestivals) — the spec's revised taxonomy (backend 1.2, new
    values like `Motorcycling`/`Arts`) isn't implemented yet.
  - **Logout**: clears the local session only; no `POST /api/auth/logout`
    call, since that route doesn't exist yet (backend 1.4).
  - **`SignInPage`**: now genuinely navigates to a real `/dashboard` (no
    longer a placeholder gap — this closes the redirect-target issue noted
    in the Public routes work).
- Verified: `dotnet build` + `npm run build` + `npm run lint` all clean, all
  6 Vitest/RTL tests pass (the 4 existing auth-flow tests plus 2 new
  `AuthGuard` tests — redirect when signed out, render through when signed
  in). Smoke-tested the real, already-implemented backend endpoints via curl
  with a live JWT: `GET/PUT /api/profile`, `GET/POST/DELETE /api/interests`,
  `GET /api/discovery/feed`, `GET /api/destination-guide/history` — all
  responses matched the frontend's TypeScript types exactly.
  **Deliberately not curl-tested**: `POST /api/destination-guide/search` and
  `GET /api/discovery/random-outing`, since both trigger live external API
  calls (currency conversion, etc.) — `.ia/10-backend.md`'s agent operating
  constraint reserves those for the user's manual testing pass, not the
  agent. Their request/response shapes were still typed accurately, taken
  directly from the C# DTOs.

**⚠ Needs manual testing before building further on top of it (pending, not
yet run — no browser-automation tool was available in this session):**
- Visual QA of all eight pages in an actual browser (`npm run dev` in
  `frontend/`, backend running on `:5080`), light and dark mode, mobile
  width. Public routes (Landing/SignIn/SignUp) were flagged for this after
  the previous session too and still haven't been manually opened in a
  browser.
- Full logged-in walkthrough: sign in → Dashboard cards populate/empty-state
  correctly → create a Destination Guide search (this is where the deferred
  external-API calls actually fire, so it's also the first real end-to-end
  check of that path) → view the result page → toggle interests on Profile
  → confirm Discovery reflects them after a matching event exists → collapse
  the sidebar and confirm it persists on refresh → log out and confirm
  `/dashboard` redirects back to `/signin`.
- Confirm the "coming soon" Location/Delete-account placeholders read as
  intentional, not broken.
- Confirm the `picsum.photos` placeholder images load on both auth pages and
  the (currently image-free) protected pages.

**Not started yet:**
- None of the Part 1 backend revisions beyond 1.1 and 1.5 are implemented:
  interest taxonomy (1.2), event pricing/sort (1.3), refresh tokens/logout/
  soft delete (1.4), first-login tracking (1.7), Geography (1.8), Trip
  Planning (1.9) — only one migration exists in the whole project.
- Trip Plans frontend (no backend to build against yet — depends on 1.9).
- Google OAuth on the frontend (explicitly deferred by the spec).

## 4. Next steps

In spec order (`docs/superpowers/specs/2026-07-28-frontend-design.md`, Part
1 — backend revisions to finish before frontend work starts):

1. **1.1** ✅ done — `Dossier` → `Destination Guide` rename throughout
   (controller, service, DTOs, extensions, route `api/dossier` →
   `api/destination-guide`, config keys, README, roadmap).
2. **1.2** Apply the interest taxonomy revision (new enum values,
   Ticketmaster/OpenF1 provider tagging changes) — requires a migration and
   a manual drop/reseed of the dev SQLite DB (human-run, not agent-run).
3. **1.3** Add event pricing fields (`MinPriceAmount`/`MaxPriceAmount`/
   `PriceCurrency`) and the `sort=date|price` query param on discovery
   endpoints — new migration.
4. **1.4** Auth completeness: `RefreshToken` entity + rotation, `POST
   /api/auth/logout`, `DELETE /api/auth/me` (soft delete), registration
   response no longer includes a token — new migration.
5. **1.5** ✅ done — access token lifetime 2h.
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

**Part 2 — frontend build:**
- ✅ **Public routes** (project setup + `LandingPage`, `SignInPage`,
  `SignUpPage`) — done, pushed (`67d175d`), see §3.
- ✅ **Protected routes** (`AuthGuard`, `DashboardShell`, Dashboard,
  Destination Guide search/result, Discovery feed, Profile) — done,
  uncommitted, see §3. Built against the backend as it exists today, with
  the Location select, price sort, delete-account and real logout call
  explicitly deferred pending the matching Part 1 backend items.
- Remaining, and now unblocked mainly by backend work: wiring the
  Location/Delete-account placeholders once 1.8/1.4 land, adding the
  Discovery price sort once 1.3 lands, adding silent-refresh to the Axios
  interceptor once 1.4 ships `/api/auth/refresh`, switching `SignInPage`'s
  redirect to branch on `IsFirstLogin` once 1.7 lands, and a Trip Plans UI
  once 1.9 lands (no backend to build against yet). Google OAuth on the
  frontend stays explicitly deferred (email/password only for now).

**Open assumptions still pending confirmation** (called out in the spec,
not yet decided by the user): refresh token lifetime (14 days proposed);
`LastAccessAtUtc` update scope (session events only, not every API call);
`Region` seed coverage limited to Brazil for this pass.
