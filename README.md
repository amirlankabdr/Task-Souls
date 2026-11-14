# QuestForge — Level Up Your Real Life

QuestForge is a Blazor Web App that turns real-life tasks into RPG-style progress. It demonstrates functional programming in C# through pure progression functions, immutable records, higher-order achievement rules, pattern matching, LINQ, and explicit `Result<T>` error handling.

Team: Aslanuly Ulanbek and Amirlan Kabdyrakym.

## Current working slice

- Create validated quests with difficulty and skill.
- Persist quests, player/skill XP, achievements, and XP history in PostgreSQL.
- Complete a quest transactionally and award player + skill XP exactly once.
- Derive level and progress from total XP.
- Unlock five rule-based achievements.
- Responsive dashboard, reusable components, and templated `DataList<TItem>`.
- ASP.NET Core Identity registration/login, `User`/`Admin` roles, and per-user data isolation.
- Unit, bUnit, service-level, Testcontainers, and Playwright test projects.

This updated version adds shared functional validation, quest pagination, guarded UI actions, and serialized per-player database completion. See START_HERE.md for installation and verification limits. Previous test counts do not certify this update; rerun the suite before submission.

## Run locally

```bash
cp .env.example .env
docker compose up --build
```

Open `http://localhost:8080`, register, then create and complete a quest. EF Core applies the PostgreSQL migration during startup.

For local development, start PostgreSQL from Compose and run `dotnet run --project src/QuestForge.Web`.

## Verify

```bash
dotnet restore QuestForge.slnx
dotnet build QuestForge.slnx -m:1 --no-restore
dotnet test QuestForge.slnx -m:1 --no-restore
```

The Playwright project expects the Compose stack at `http://localhost:8080`. Install Chromium once after building it:

```bash
pwsh tests/QuestForge.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
```

See the [user guide](docs/user-guide.md), [developer notes](docs/developer-notes.md), [testing guide](docs/study/05-testing.md), [architecture notes](docs/architecture/overview.md), and [FP map](docs/study/fp-map.md).

## Architecture

- `QuestForge.Core`: pure domain model and application ports.
- `QuestForge.Infrastructure`: stateful service implementations.
- `QuestForge.Web`: Blazor UI and Identity host.
- `tests`: unit and integration-oriented tests.
