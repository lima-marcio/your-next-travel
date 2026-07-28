# Your Next Travel

## Overview

Travel-planning app. Given a destination and trip length (e.g. "1 week in
Rome"), it assembles a **dossier** of real-world information: weather / best
time to visit, local currency exchange rate, a real lodging price estimate,
legal/health requirements (visa, vaccination), and an estimated budget
adjusted by traveler profile (student / tourist / business).

It also has an **interest-based discovery** flow: users register interests
(motorsport, football, auctions, concerts, local cultural festivals) and the
app surfaces destinations/dates where matching events are happening, grouped
into three horizons (within a week, next month, next semester) — including a
"Random Outing" mode that picks a surprise suggestion.

This is an informational dossier, not a booking engine — no cart/payment
flow. Prices and data are pulled from real external APIs so budget estimates
stay grounded in reality.

## Tech Stack

**Backend:** .NET 10, ASP.NET Core Web API, Entity Framework Core, SQLite
(Development), SQL Server (Production), JWT Authentication (+ Google OAuth),
Swagger, Serilog.

**Frontend:** React 19, TypeScript, Vite, Tailwind CSS, Axios, React Router,
TanStack Query, React Hook Form, Zod.

## Getting Started

### Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/YourNextTravel.Api
```

The API listens on the URL printed in the console (also defined in
`YourNextTravel.Api/Properties/launchSettings.json`). Swagger UI is
available at `/swagger` in Development.

External API keys (Amadeus, football-data.org, Ticketmaster, Google OAuth
client id) are configured via `dotnet user-secrets` in Development — see
`.ia/10-backend.md` for the full list of required secrets.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

The app runs at `http://localhost:5173` and expects the backend URL in
`frontend/.env.development` (`VITE_API_BASE_URL`) to match the backend's
actual port.

### Running Both

Start the backend first, then the frontend, then open the frontend URL in a
browser.

## Build

```bash
dotnet publish -c Release
npm run build
```

## Status

See [docs/roadmap.md](./docs/roadmap.md).

## License

MIT — see [LICENSE](./LICENSE).
