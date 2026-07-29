# Frontend Design — Your Next Travel

Status: approved by user, ready for implementation planning.

## Context

The backend is fully scaffolded (auth, traveler profile, interests, destination
dossier, interest-based discovery, external integrations) but has no
frontend yet. This spec covers building the first version of the React
frontend end-to-end, across all four flows. Along the way, two backend
domain changes were agreed with the user and are included here because the
frontend depends on them directly.

## Part 1 — Backend domain revision

These are small, contained changes to finish before frontend work starts
against the API, so the frontend is built against final contracts.

### 1.1 Rename "Dossier" → "Destination Guide"

The word "dossier" reads like a police file/criminal record in Portuguese
and is off-putting for a consumer travel app. Renamed throughout:

- `Features/Dossier/*` → `Features/DestinationGuide/*`
  (`DossierController` → `DestinationGuideController`,
  `DossierService`/`IDossierService` → `DestinationGuideService`/`IDestinationGuideService`,
  `DossierDtos` → `DestinationGuideDtos`, `DossierFeatureExtensions` →
  `DestinationGuideFeatureExtensions`, `BudgetSynthesisService`/
  `IBudgetSynthesisService` stay in the same folder, namespace updated)
- Route: `api/dossier` → `api/destination-guide`
- References in `Program.cs`, `appsettings.json` (config section name),
  `EventMatchingService` (config keys `DestinationGuide:EventProximityKm` /
  `DestinationGuide:EventDateWindowDays`), `README.md`, `docs/roadmap.md`
- `DossierResponse` → `DestinationGuideResponse`, `DossierSearchRequest` →
  `DestinationGuideSearchRequest`, etc. (all DTOs in that file renamed
  consistently)

No functional change — pure rename, contained to the 13 files already
identified.

### 1.2 Interest category taxonomy revision

Current `InterestCategory` enum fragments motorsport into specific series
(`MotorsportF1`, `MotorsportF2`, `MotorsportDtm`, `MotorsportStockCar`) and
has no motorcycling, extreme sports, or general-arts categories. Revised to:

```csharp
public enum InterestCategory
{
    Motorsport,       // official auto racing broadly: F1, F2, WEC, rally, stock car, etc.
    Motorcycling,      // new — MotoGP, motocross, etc.
    ExtremeSports,     // new — action/extreme sports competitions
    Football,
    Auctions,
    ConcertsShows,     // kept separate from Arts per user decision
    Arts,              // new — cinema, theatre, and performing arts broadly (not restricted to those)
    CulturalFestivals
}
```

`ConcertsShows` and `Arts` are kept as two categories rather than merged,
even though they may overlap. Decision: ship both, watch real user
behavior/feedback, and only collapse them later (hide one category, widen
the other's filter to cover both) if they prove redundant in practice. This
is noted in `docs/roadmap.md` as a post-launch review item.

**Event sourcing impact:**
- `OpenF1EventProvider` now tags its events as `InterestCategory.Motorsport`
  (was `MotorsportF1`). Kept as a supplementary source specifically for the
  F1 calendar (it's free, no API key, reliable for that one series).
- `TicketmasterEventProvider` currently only queries
  `classificationName=music` → `ConcertsShows`. Extended to also query
  Ticketmaster's `Sports` segment (covers ticketed motorsport, motorcycling,
  and extreme/action sports events) and `Arts & Theatre` / `Film`
  classifications, mapped to the new categories. This is how broad
  "official competition" coverage is achieved without adding new external
  integrations — Ticketmaster's own classification breadth does the work.
- `FootballDataEventProvider` unchanged (still feeds `Football`).

**Data migration note:** the enum's underlying int values shift. Since
there are no real users yet, the simplest path is to drop and recreate the
dev SQLite database (`yournexttravel.dev.db`) and reseed, rather than write
a data-preserving migration for pre-launch data.

### 1.3 Event pricing + discovery sort

Per user decision, event value is not tied to traveler profile. Instead,
discovery results get an explicit sort control.

- `EventListing` gains nullable `MinPriceAmount`, `MaxPriceAmount`,
  `PriceCurrency` (decimal/decimal/string, all nullable).
- Only `TicketmasterEventProvider` currently supplies price data
  (`priceRanges`, not always present even there). `OpenF1EventProvider` and
  `FootballDataEventProvider` leave price fields null.
- `GET /api/discovery/feed` and `GET /api/discovery/random-outing` gain an
  optional `sort` query parameter: `date` (default) or `price`.
  - `sort=date`: ascending by `StartUtc` (current behavior).
  - `sort=price`: ascending by `MinPriceAmount`; events with no price data
    sort to the end of their group but are still shown (never hidden).
- `DiscoverySuggestion` DTO gains `MinPriceAmount`, `MaxPriceAmount`,
  `PriceCurrency` so the frontend can render "a partir de R$120" or "preço
  não informado".
- New EF Core migration for the added columns (combined with the reseed
  from 1.2).

## Part 2 — Frontend application

### Architecture

Feature-sliced modules under `features/`, matching the folder structure
already fixed in `.ia/20-frontend.md`
(`api/ components/ features/ hooks/ pages/ routes/ stores/ types/`):

- Each flow (`auth`, `destination-guide`, `discovery`, `preferences`) is a
  module under `features/<name>/` containing its own TanStack Query hooks
  (wrapping calls into `api/`) and small, single-purpose presentational
  components (one component per file, per `.ia/30-conventions.md`).
- `pages/` contains one thin page component per route that composes a
  feature module's components — no business logic in pages.
- Server state (dossier/guide results, discovery feed, interests, profile)
  is entirely TanStack Query — no duplication into a separate store.
- Client state is limited to the auth session (JWT + current user), held in
  a small Zustand store (`stores/authStore.ts`), persisted to
  `localStorage` and rehydrated on load.
- Axios instance (`api/client.ts`) with a request interceptor attaching the
  Bearer token from the auth store, and a response interceptor that maps
  401 → logout + redirect to `/login`.

This was chosen over two alternatives: colocating everything inside
`pages/<Page>/` (rejected — drifts from the folder structure already
defined for this project) and a Redux-style global store for all state
(rejected — redundant with TanStack Query already in the stack; no client
state complex enough to justify it).

### Pages & components

**Public routes**
- `LoginPage` — email/password form (React Hook Form + Zod), calls
  `POST /api/auth/login`. Google sign-in is explicitly deferred — the user
  wants to see the platform working end-to-end before onboarding the first
  real user and adding that dependency.
- `RegisterPage` — email/password/display name, `POST /api/auth/register`.

**Protected routes** (behind an `AuthGuard` that redirects to `/login` when
there's no valid session), inside an `AppShell` with top navigation:

- `DestinationGuideSearchPage` (`/guide`) — search form (destination, start
  date, end date, optional profile override) plus a `GuideHistoryList`
  showing past searches (`GET /api/destination-guide/history`); selecting
  one or submitting a new search navigates to the result page.
- `DestinationGuideResultPage` (`/guide/:searchId`) — composes separate
  components per data block, each independently loading/error-tolerant:
  `WeatherCard`, `CurrencyCard`, `LodgingCard`, `LegalHealthCard`,
  `BudgetCard`, `MatchingEventsList`.
- `DiscoveryFeedPage` (`/discovery`) — a `SortToggle` (date/price) at the
  top; three `DiscoveryGroupSection` blocks (one per `TimeHorizon`), each
  rendering a list of `DiscoverySuggestionCard`; a `RandomOutingButton` that
  triggers `GET /api/discovery/random-outing` and surfaces the result in a
  `RandomOutingCard`.
- `PreferencesPage` (`/preferences`) — merges interests and traveler
  profile into one page, two sections: `InterestsSection` (the 8 categories
  as toggleable chips, add/remove calling `POST`/`DELETE
  /api/interests`) and `TravelerProfileSection` (Student/Tourist/Business
  radio group, `PUT /api/profile`).

**Shared components:** `AppShell` (nav + logout), `AuthGuard`,
`LoadingSpinner`, `ErrorState` (with retry), `EmptyState`.

### Data flow

1. Login/register mutations store the returned JWT + user info in the
   Zustand auth store (and `localStorage`).
2. All authenticated requests go through the shared Axios instance, which
   attaches the token automatically.
3. Every GET endpoint is a TanStack Query hook scoped to its feature
   (`useDestinationGuideHistory`, `useDiscoveryFeed(sort)`, `useInterests`,
   `useProfile`); mutations use `useMutation` and invalidate the relevant
   query keys on success (e.g. adding an interest invalidates
   `['interests']`).
4. `DiscoveryFeedPage`'s sort toggle is local UI state (`useState` or a URL
   query param) that feeds the `sort` param into `useDiscoveryFeed`.

### Error handling

- Axios response interceptor normalizes backend `ProblemDetails` responses
  into a consistent shape the UI can read (`title`, `detail`, field
  errors when present).
- Form-level errors: Zod handles client-side validation; server-side
  validation errors from `ProblemDetails` are mapped onto the matching
  form field when possible, otherwise shown as a form-level banner.
- Page-level query errors render the shared `ErrorState` component with a
  retry action (re-triggers the TanStack Query).
- 401 anywhere → auth store clears session, user is redirected to
  `/login`.
- Network/5xx errors show a generic "algo deu errado, tente novamente"
  message via `ErrorState`; no raw error details are shown to the user.

### Testing

`.ia/20-frontend.md` doesn't yet specify a test stack, so this spec adds
one: **Vitest + React Testing Library** for component-level tests (forms,
cards, guards) and **MSW** to mock the API for page-level integration
tests (e.g. login flow, guide search → result navigation, interest
add/remove). Scope for this first pass: cover the auth flow, the
destination-guide search→result flow, and the preferences add/remove
flow — the three flows with the most user-facing logic. Discovery feed
sorting gets at least one test verifying price-less events sort last
without being hidden.

### Visual direction

Warm/editorial travel style as the starting point (as opposed to a
vibrant/energetic look or a clean minimalist-premium look) — picked to
read like a quality printed travel guide: warm tones, strong imagery,
editorial typography. Actual page visuals are produced during
implementation using the `design-taste-frontend` skill against this page
inventory. A follow-up idea — shifting visual tone based on the user's
selected interests instead of one fixed theme — was deliberately deferred;
tracked in `docs/roadmap.md` under "Revisit at end of project".

### Google OAuth

Deferred. `LoginPage` ships with email/password only; the Google button
can be added later without redesigning the screen, once there's a real
Client ID and the team wants to validate the flow with real users.
