# Frontend Design — Your Next Travel

Status: revised — supersedes the first approved version to incorporate
project-wide baseline rules added to `.ia/00-project.md`,
`.ia/10-backend.md` and `.ia/20-frontend.md` after the first approval.
Ready for implementation planning.

## Context

The backend is fully scaffolded (auth, traveler profile, interests,
destination dossier, interest-based discovery, external integrations) but
has no frontend yet. This spec covers building the first version of the
React frontend end-to-end, across all four flows, plus the backend changes
the frontend depends on directly.

**Revision note:** after the first version of this spec was approved, the
project's `.ia/` standards gained several baseline rules that apply to
every project, not just this one: a mandatory page set (landing, sign in,
sign up, profile, dashboard) and dashboard layout, a required set of auth
routes whenever users are involved, a fixed access-token lifetime, and two
auth-flow rules (post-signup redirect, first-access onboarding). This
revision folds all of that in. It also now explicitly respects two agent
operating constraints added to `.ia/10-backend.md`: migrations are run
manually by a human, never by the agent, and requests to external
URLs/APIs only happen during the manual testing phase.

## Part 1 — Backend domain revision

Small, contained changes to finish before frontend work starts against the
API, so the frontend is built against final contracts.

### 1.1 Rename "Dossier" → "Destination Guide"

Unchanged from the first version. The word "dossier" reads like a police
file/criminal record in Portuguese and is off-putting for a consumer
travel app.

- `Features/Dossier/*` → `Features/DestinationGuide/*`
  (`DossierController` → `DestinationGuideController`,
  `DossierService`/`IDossierService` → `DestinationGuideService`/`IDestinationGuideService`,
  `DossierDtos` → `DestinationGuideDtos`, `DossierFeatureExtensions` →
  `DestinationGuideFeatureExtensions`, namespace updated)
- Route: `api/dossier` → `api/destination-guide`
- References in `Program.cs`, `appsettings.json`, `EventMatchingService`
  (config keys `DestinationGuide:EventProximityKm` /
  `DestinationGuide:EventDateWindowDays`), `README.md`, `docs/roadmap.md`

### 1.2 Interest category taxonomy revision

Unchanged from the first version.

```csharp
public enum InterestCategory
{
    Motorsport,       // official auto racing broadly: F1, F2, WEC, rally, stock car, etc.
    Motorcycling,      // new — MotoGP, motocross, etc.
    ExtremeSports,     // new — action/extreme sports competitions
    Football,
    Auctions,
    ConcertsShows,     // kept separate from Arts per user decision
    Arts,              // new — cinema, theatre, and performing arts broadly
    CulturalFestivals
}
```

`ConcertsShows` and `Arts` stay separate; revisit post-launch if user
feedback shows they're redundant (tracked in `docs/roadmap.md`).

- `OpenF1EventProvider` tags events as `InterestCategory.Motorsport` (was
  `MotorsportF1`); kept as a supplementary F1-specific source.
- `TicketmasterEventProvider` extended beyond `classificationName=music` to
  also query the `Sports` segment (motorsport/motorcycling/extreme) and
  `Arts & Theatre`/`Film` classifications.
- `FootballDataEventProvider` unchanged.
- Enum's underlying int values shift — dev SQLite DB needs to be dropped
  and reseeded. **Per the new agent operating constraint, this reseed and
  the schema migration are run manually by the user, not by the agent.**
  The implementation plan will produce the migration files and a short
  runbook step ("run `dotnet ef database update`, then restart to reseed")
  for the user to execute — the agent does not run it.

### 1.3 Event pricing + discovery sort

Unchanged from the first version.

- `EventListing` gains nullable `MinPriceAmount`, `MaxPriceAmount`,
  `PriceCurrency`.
- Only `TicketmasterEventProvider` supplies price data when available.
- `GET /api/discovery/feed` and `GET /api/discovery/random-outing` gain an
  optional `sort` query parameter: `date` (default) or `price` (ascending,
  nulls last, never hidden).
- `DiscoverySuggestion` DTO gains the three price fields.
- New EF Core migration — again, applied manually by the user per the
  agent operating constraints.

### 1.4 Auth completeness (new — required by `.ia/10-backend.md` rule (c))

The project baseline now requires register, login, logout, refresh token,
and (soft) delete routes whenever users are involved. Today `AuthController`
only has `register`, `login`, `google`, `me` — no logout, no refresh, no
delete. This revision adds the missing three:

- **Refresh tokens.** New `RefreshToken` entity: `Id`, `UserId`,
  `TokenHash` (never store the raw token), `ExpiresAtUtc`, `CreatedAtUtc`,
  `RevokedAtUtc` (nullable). `POST /api/auth/refresh` accepts a refresh
  token, validates it's unexpired/unrevoked, issues a new access token
  **and rotates the refresh token** (revokes the old one, issues a new
  one) — standard rotation to limit replay if a refresh token leaks.
  Refresh token lifetime: proposing **14 days** as a sensible default for
  this kind of app (not specified by the user) — flagged here as an
  assumption to confirm or override before/while implementing.
- **Logout.** `POST /api/auth/logout` revokes the caller's current refresh
  token (sets `RevokedAtUtc`). The access token itself isn't revocable
  (stateless JWT) but now expires in 2 hours (see 1.5), so logout's
  practical effect is "no more silent refreshes"; the frontend also
  discards the access token from local state immediately.
- **Soft delete.** `User` gains a nullable `DeletedAtUtc`. `DELETE
  /api/auth/me` sets it, revokes all of that user's refresh tokens, and
  from that point `LoginAsync`/`RegisterAsync`(email reuse)/token refresh
  all treat the account as gone (login fails as if the account doesn't
  exist; the email becomes registrable again is explicitly **out of
  scope** for this pass — reusing a soft-deleted email is deferred).
- `RegisterRequest`'s response contract changes: **registration no longer
  returns an access token.** It returns a minimal `RegisterResponse`
  (email, display name) with `201 Created`. This matches the new frontend
  rule that sign-up redirects to sign-in rather than auto-logging in —
  keeping the backend consistent (no point minting a token the frontend is
  told to throw away).
- New EF Core migration (RefreshTokens table + `User.DeletedAtUtc`) —
  applied manually by the user, same as 1.2/1.3.

### 1.5 Access token lifetime (new — required by `.ia/10-backend.md`)

`JwtOptions.ExpiryMinutes` default changes from `1440` (24h) to `120`
(2h), matching the new baseline rule. The existing code comment
justifying the long expiry ("no refresh-token handling for MVP") no longer
applies now that 1.4 adds refresh tokens.

### 1.6 Agent operating constraints — how they shape delivery

Two rules from `.ia/10-backend.md` change how this plan gets executed,
not what gets built:

- **Migrations are manual.** Every step above that needs a schema change
  produces migration files but stops short of applying them. The
  implementation plan calls this out as an explicit handoff point: "stop,
  ask the user to run `dotnet ef database update`."
- **External API calls only in the manual testing phase.** Verifying the
  Ticketmaster classification changes (1.2), the new event pricing fields
  (1.3), or Google OAuth end-to-end cannot be done by the agent running
  the app and hitting those providers live. Automated/unit-level
  verification (does the code compile, do unit tests pass, does the
  mapping logic do the right thing given a canned response) is fine; live
  calls against the real external APIs are left for the user's manual
  testing pass.

### 1.7 User lifecycle timestamps and first-access redirect (new)

Replaces the "Onboarding flag" mechanism from the previous revision of
this spec with a simpler, more direct one requested by the user.

`User` gains three timestamps:
- `CreatedAtUtc` — already exists (set at registration).
- `FirstLoginAtUtc` (nullable) — set the first time the user successfully
  authenticates (`Login` or Google login) *after* being created. Since
  1.4 makes registration no longer return a token, for the password flow
  this is always a separate, later `Login` call; for the Google flow,
  account creation and first authentication happen in the same call, so
  it's set immediately.
- `LastAccessAtUtc` — updated on every successful `Login`, Google login,
  and token refresh. **Scoping decision:** not updated on every single
  API call (that would need a write on every authenticated request, which
  is unnecessary cost for an MVP) — "access" here means "obtained or
  renewed a session," not "made a request." Flagging this interpretation
  explicitly in case a stricter definition is wanted later.

Centralized in `AuthService.BuildAuthResponse`: if `FirstLoginAtUtc` is
`null`, set it to now (this is therefore the *first* login) and remember
that fact; always set `LastAccessAtUtc` to now. `AuthResponse` gains
`IsFirstLogin: bool` reflecting whether this call was the one that just
set `FirstLoginAtUtc`, so the frontend gets the redirect signal directly
from the login response — no extra round trip to `GET /api/profile`.

**Redirect rule (exact, as specified):** after sign in, if the user has no
`FirstLoginAtUtc` yet (i.e. `IsFirstLogin: true` on this response), the
frontend navigates to `/profile`; otherwise it navigates to `/dashboard`.

New EF Core migration for the two columns — manual apply, same as the
others in Part 1.

### 1.8 User location — Country / Region / City (new)

The user asked for the user entity to carry a home location and for the
sub-national terms to be internationalized instead of Brazil-specific.
Reasoning on naming: "estado" doesn't generalize (Japan's equivalent is a
prefecture, the US's is a state, Portugal's is a district) — the neutral
term used across international address forms and ISO 3166-2 for this
administrative level is **Region**. Same issue one level down for
"município" — the neutral term almost universally used in international
address forms for that level, even though not perfectly precise for every
country, is **City**. Entities:

```
Country  { Id, Name, Iso2Code }
Region   { Id, CountryId, Name }     // state / province / prefecture / etc.
City     { Id, RegionId, Name }      // municipality / city / etc.
```

`User` gains `CountryId` (required), `RegionId` (required), `CityId`
(nullable) — matching the user's instruction that only country and region
are mandatory on the profile form.

**Deliberately separate from `Domain/Destinations/Country`/`City`.** The
codebase already has a `Country`/`City` pair under
`Domain/Destinations/`, used for travel-destination search — those are
resolved on demand via geocoding (`OpenMeteoGeocodingResolver`) as users
search arbitrary destinations, an incomplete/ad-hoc set by design. The new
`Domain/Geography/` entities are curated reference data for the closed
question "where does this user live," a different lifecycle and a
different consumer. Merging them was considered and rejected for this
pass — it would couple an unrelated subsystem (destination search) to
this change for a correctness benefit that's more theoretical than real
right now. Flagged in `docs/roadmap.md` as a "revisit" item, same pattern
as the other deferred ideas in this project.

**Seeding scope — constrained by the "no external calls" rule.** Full
`Country` seeding is realistic to hand-author directly as static data
(≈195 countries, well-known, no API call needed). Full `Region` seeding
for every country in the world is not realistic without pulling from an
external reference dataset, and 1.6's constraint means the agent cannot
fetch one during implementation. Proposed scope for this pass:
- `Country`: fully seeded (~195 rows).
- `Region`: seeded for a starter set of countries only — Brazil's 27
  states/DF to start (hand-authored, small, matches the primary market),
  structured so more countries' regions can be added later as plain seed
  data, no code changes needed.
- `City`: **not pre-seeded at all.** Created get-or-create (by name,
  scoped to `RegionId`) when a user types their city on the Profile page.
  This is consistent with city being optional and avoids needing any
  external city dataset.

This scope limitation (Region only fully covering Brazil initially) is
called out explicitly so it can be revisited if broader country coverage
is needed sooner — expanding it later is just adding seed data, not a
structural change.

New `GeographyController` (`api/geography`): `GET /countries`,
`GET /countries/{countryId}/regions` — backs the cascading selects on the
Profile page. Both are static reference reads, `[Authorize]` like the
rest of the API (no reason to expose them anonymously).

New EF Core migration (Country/Region/City tables + the three FKs on
`User`) — manual apply, same as the others. `CountryId`/`RegionId` land as
`NOT NULL` at the database level (not just app-level validation) since the
dev database is already being dropped and reseeded for 1.2 in the same
pass.

### 1.9 Trip Plans — persisted itinerary per user (new)

A new, fifth concept beyond the original four flows: a user can save a
concrete, editable travel plan (destination, dates, a chosen lodging and
optional car rental, and a set of selected events) rather than only ever
getting a disposable Destination Guide result. New folder
`Domain/TripPlanning/`.

**Design decision — self-reported lodging/car rental, not a live
selection (confirmed).** The app isn't a booking engine — the user still
needs the values to put together their trip plan, and typing a few fields
in is a big usability win for an MVP versus building real inventory
selection. So `TripPlan` stores **self-reported snapshots**: the user
sees the estimate ranges from the Destination Guide as a reference, then
types in the actual lodging/car rental they're considering (name, address,
price, category) themselves. Live selection against real offers (exposing
individual Amadeus hotel offers instead of today's aggregate, and adding a
car rental integration) is explicitly deferred until after collecting
feedback on this MVP — tracked in `docs/roadmap.md`.

```csharp
public sealed class TripPlan
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid CityId { get; }          // reuses Domain.Destinations.City — this
                                          // is "where you're traveling," the same
                                          // concept the Destination Guide already
                                          // uses, unlike the user's home
                                          // Country/Region in 1.8 which is a
                                          // different, curated concept
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    // Lodging — self-reported snapshot, all nullable (filled in once chosen)
    public string? LodgingName { get; }
    public string? LodgingAddress { get; }
    public decimal? LodgingPriceAmount { get; }
    public string? LodgingCurrency { get; }
    public string? LodgingCategory { get; }   // free text: "3 estrelas", "hostel", etc.
                                               // — not an enum, categories vary too
                                               // widely across lodging types to enumerate

    // Car rental — self-reported snapshot
    public bool WillRentCar { get; }
    public string? CarRentalCompanyName { get; }
    public string? CarRentalCompanyAddress { get; }
    public decimal? CarRentalPriceAmount { get; }
    public string? CarRentalCurrency { get; }
    public string? CarRentalCategory { get; }  // free text: "econômico", "SUV", etc.

    public DateTime CreatedAtUtc { get; }
    public DateTime UpdatedAtUtc { get; }
}

public sealed class TripPlanEvent
{
    public Guid Id { get; }
    public Guid TripPlanId { get; }      // FK to TripPlan
    public string Title { get; }         // artist/band/event name
    public string? VenueName { get; }
    public decimal? AveragePriceAmount { get; }
    public string? PriceCurrency { get; }
    public DateTime StartUtc { get; }
    public TimeSpan? Duration { get; }
    public Guid? SourceEventListingId { get; }  // nullable trace-back to the real
                                                 // EventListing this was selected
                                                 // from (via Destination Guide /
                                                 // Discovery matches), if any — kept
                                                 // as a snapshot regardless, so it
                                                 // doesn't shift if the source event
                                                 // is later refreshed or removed
}
```

**API:** new `TripPlanController` (`api/trip-plans`) — `POST` (create,
destination + dates required, lodging/car/events optional and added
later), `GET` (list the caller's plans), `GET /{id}` (one, with its
events), `PUT /{id}` (update destination/dates/lodging/car fields),
`POST /{id}/events` (add a selected event), `DELETE /{id}/events/{eventId}`,
`DELETE /{id}` (remove a plan — a hard delete is fine here, this isn't
user-account data, so the soft-delete rule from `.ia/10-backend.md` rule
(c) doesn't apply to it).

**Price disclaimer (UX requirement, not just a data concern).** Wherever
lodging/car/event prices are shown or entered on a `TripPlan`, the UI must
make clear they're estimates: *"Valor estimado — o valor definitivo será
confirmado diretamente com a empresa selecionada."* This applies to the
Destination Guide's existing estimate displays too, not only the new Trip
Plan screens.

**Frontend scope — deferred.** This message specified the data model, not
the pages. The natural entry point is a "Save as trip plan" action from
`DestinationGuideResultPage` (pre-filling destination/dates from the
search, letting the user then add lodging/car/events), plus a
`TripPlanListPage`/`TripPlanDetailPage` pair reachable from the
`Sidebar`/`DashboardPage`. Not designing these in detail now — flagging so
it's an explicit next step rather than something silently skipped.

New EF Core migration (`TripPlans` + `TripPlanEvents` tables) — manual
apply, same as the rest of Part 1.

## Part 2 — Frontend application

### Architecture

Unchanged: feature-sliced modules under `features/`, per
`.ia/20-frontend.md`'s folder structure
(`api/ components/ features/ hooks/ pages/ routes/ stores/ types/`).
Server state via TanStack Query; client state limited to the auth session
in a small Zustand store (`stores/authStore.ts`), persisted to
`localStorage`. Axios instance with a request interceptor attaching the
Bearer token, and (new, see Data Flow below) silent-refresh handling on
401.

### Pages & components

**Public routes**
- `LandingPage` (`/`) — new, required by the Frontend Baseline. Marketing
  entry point: what the app does (destination guide + interest-based
  discovery), links to Sign In / Sign Up. No data fetching.
- `SignInPage` (`/signin`, renamed from `LoginPage`) — email/password
  form, `POST /api/auth/login`. On success: if the response's
  `IsFirstLogin` is `true`, navigate to `/profile`; otherwise navigate to
  `/dashboard` (see 1.7).
- `SignUpPage` (`/signup`, renamed from `RegisterPage`) — email/password/
  display name, `POST /api/auth/register`. On success (`201`, no token
  returned): navigate to `/signin` with a "account created, sign in"
  confirmation — no auto-login, per the new Auth Flow rule.

**Protected routes** (behind an `AuthGuard`), inside a **`DashboardShell`**
layout (replaces the plain top-nav `AppShell` from the first version):
collapsible left `Sidebar` (links: Dashboard, Destination Guide,
Discovery, Profile) and a top `Navbar` with the user's avatar/icon
right-aligned, opening a dropdown with "Profile" and "Logout" — matching
the Frontend Baseline's dashboard layout rule exactly.

- `DashboardPage` (`/dashboard`) — new, required by the Frontend Baseline.
  Lightweight home base: a card linking into Destination Guide (with the
  most recent search, if any), a card linking into Discovery (with a
  teaser of the next 1–2 matching events, if any), and a "Random Outing"
  quick action. No new backend endpoint — composes existing
  `GET /api/destination-guide/history` and `GET /api/discovery/feed` data
  already fetched elsewhere via TanStack Query's cache.
- `DestinationGuideSearchPage` (`/guide`) — unchanged from the first
  version: search form + `GuideHistoryList`.
- `DestinationGuideResultPage` (`/guide/:searchId`) — unchanged:
  `WeatherCard`, `CurrencyCard`, `LodgingCard`, `LegalHealthCard`,
  `BudgetCard`, `MatchingEventsList`, each independently loading/error-
  tolerant.
- `DiscoveryFeedPage` (`/discovery`) — unchanged: `SortToggle`
  (date/price), three `DiscoveryGroupSection` blocks, `RandomOutingButton`.
- `ProfilePage` (`/profile`, renamed from `PreferencesPage`) — this is now
  the Frontend Baseline's "user profile" page, and (per 1.7) is where the
  user lands on their first sign-in. Three sections: `LocationSection`
  (Country select → Region select, both required; City free-text input,
  optional — cascading via `GET /api/geography/countries` and
  `GET /api/geography/countries/{id}/regions`), `InterestsSection` (the 8
  categories as toggleable chips), and `TravelerProfileSection`
  (Student/Tourist/Business radio group). Country and Region are required
  for the form to submit (client-side validation via Zod, mirroring the
  `NOT NULL` constraint in 1.8). A "Delete account" danger-zone action
  calls `DELETE /api/auth/me` (exists because rule (c) requires the route;
  kept low-key in the UI since account deletion isn't a primary flow).

**Shared components:** `DashboardShell` (`Sidebar` + `Navbar`), `AuthGuard`,
`LoadingSpinner`, `ErrorState` (with retry), `EmptyState`.

### Data flow

1. `SignInPage` login mutation stores the returned JWT + user info in the
   Zustand auth store (`localStorage`); `SignUpPage`'s mutation does not
   (no token in the response — see 1.4).
2. Every authenticated request goes through the shared Axios instance,
   which attaches the access token automatically.
3. **New — silent refresh.** On a `401`, the Axios response interceptor
   attempts one `POST /api/auth/refresh` using the stored refresh token
   before giving up; on success it retries the original request with the
   new access token. If refresh also fails (expired/revoked/absent), the
   auth store is cleared and the user is redirected to `/signin`. This
   matters more now that access tokens expire in 2h instead of 24h.
4. `POST /api/auth/logout` is called on explicit logout (Navbar dropdown),
   then the auth store is cleared client-side regardless of the call's
   outcome.
5. Every GET endpoint is a TanStack Query hook scoped to its feature
   (`useDestinationGuideHistory`, `useDiscoveryFeed(sort)`, `useInterests`,
   `useProfile`, `useCountries`, `useRegions(countryId)`, ...); mutations
   use `useMutation` and invalidate the relevant query keys on success.
   `PUT /api/profile` now also carries `CountryId`, `RegionId`, and
   optional `CityName` (see 1.8), and its response includes the read-only
   `CreatedAtUtc`/`FirstLoginAtUtc`/`LastAccessAtUtc` from 1.7 for display
   on the profile page.
6. `DiscoveryFeedPage`'s sort toggle is local UI state feeding the `sort`
   param into `useDiscoveryFeed`.
7. `Sidebar` collapsed/expanded state is local UI state (persisted to
   `localStorage` so it survives a refresh, but it's a presentation
   preference, not server state — no backend involvement).

### Error handling

Unchanged from the first version, plus the refresh step above:
- Axios response interceptor normalizes `ProblemDetails` responses.
- Zod handles client-side validation; server-side validation errors map
  onto the matching form field when possible, otherwise a form-level
  banner.
- Page-level query errors render the shared `ErrorState` with retry.
- 401 → attempt silent refresh (new) → if that fails, clear session and
  redirect to `/signin`.
- Network/5xx → generic "algo deu errado, tente novamente" via
  `ErrorState`; no raw error details shown to the user.

### Testing

Vitest + React Testing Library for components, MSW to mock the API for
page-level flows. Scope for this pass: the auth flow (sign up → redirect
to sign in → sign in → `IsFirstLogin` redirect to `/profile` vs.
`/dashboard`), the silent-refresh-on-401 interceptor behavior, the
destination-guide search→result flow, the profile flow (country/region
required, city optional, cascading selects), and interests add/remove.
Discovery feed sort keeps its test (price-less events sort last, never
hidden). Per the agent operating constraint in 1.6, none of this hits real
external APIs — MSW mocks everything.

### Visual direction

Unchanged: warm/editorial travel style (quality printed travel guide feel:
warm tones, strong imagery, editorial typography) as the starting point.
Produced during implementation via the `design-taste-frontend` skill
against this page inventory. The adaptive-theme-by-interest idea is still
deferred; tracked in `docs/roadmap.md` under "Revisit at end of project".

### Google OAuth

Still deferred. `SignInPage` ships with email/password only.

## Open assumptions to confirm

- Refresh token lifetime is proposed at **14 days** in 1.4 — not specified
  by the user.
- `LastAccessAtUtc` (1.7) is updated on login/Google login/refresh, not on
  every API call — a scoping choice for MVP cost, not a literal reading of
  "every access."
- `Region` seed data (1.8) covers Brazil only for this pass, due to the
  "no external calls" constraint blocking a full world dataset fetch;
  `Country` is fully seeded, `City` is created on demand.
Called out here so any of them can be overridden before or during
implementation instead of being silently baked in.
