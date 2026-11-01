# Developer Notes

## Local setup

Install .NET 10 SDK and Docker Desktop or Docker Engine. Copy `.env.example` to `.env`, choose a local PostgreSQL password, then run `docker compose up --build`. The application is available at `http://localhost:8080`.

## Data and migrations

The web host uses PostgreSQL through EF Core and applies migrations during startup. For design-time commands, set `ConnectionStrings__DefaultConnection` rather than committing a password. The database schema migration is stored in `src/QuestForge.Web/Migrations`.

## Design boundary

`QuestForge.Core` holds records and pure functions. `QuestForge.Web` adapts those rules to PostgreSQL and Blazor through `EfQuestService`. Entity Framework entities are intentionally mutable only at the persistence boundary; domain records remain immutable.

## Important safety rule

`CompleteAsync` is transactional. It changes the quest status, player profile, skill XP, XP history, and achievements together. The unique index on `XpTransactions.QuestId` makes the reward idempotent even if two completion attempts race.
