# Roadmap

- [ ] Phase 0 — Repo & solution scaffolding
- [ ] Phase 1 — Domain & persistence foundation
- [ ] Phase 2 — Authentication (JWT + Google OAuth, profiles, interests)
- [ ] Phase 3 — External integrations behind interfaces + background refresh
- [ ] Phase 4 — Fluxo A: destination guide
- [ ] Phase 5 — Fluxo B: interest-based discovery + Random Outing
- [ ] Phase 6 — Budget synthesis
- [ ] Phase 7 — Testing
- [ ] Polish

Deferred to a later phase (out of scope for now): general sightseeing tips
unrelated to matched events, local language/phrasebook info.

Revisit at end of project: adaptive visual theme that shifts tone (e.g. more
vibrant/energetic vs. warm/editorial) based on the user's selected interests
instead of one fixed theme for everyone.

Revisit: `Domain/Geography` (Country/Region/City, for user home location)
and `Domain/Destinations` (Country/City, for travel-destination search)
model overlapping real-world concepts as two separate entity sets. Kept
separate for now (different lifecycles: curated reference data vs.
geocoding-resolved on demand) — reconsider unifying if the duplication
becomes a real maintenance pain.

Revisit: `Region` seed data only covers Brazil for now (seeding the full
world requires an external reference dataset the agent can't fetch under
the no-external-calls constraint) — expand country-by-country as needed.

Revisit after collecting user feedback on the Trip Plan MVP: replace
self-reported lodging/car rental fields with live selection against real
offers — would need `AmadeusLodgingPriceProvider` to expose individual
hotel offers (name/address/price) instead of today's aggregated
min/avg/max, plus a new car rental integration (none exists today).
