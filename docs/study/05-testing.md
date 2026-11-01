# Testing

## Test layers

- Core xUnit: reward table, levels, immutability, and achievement rules.
- Service test: create and duplicate-completion behavior using the fast in-memory adapter.
- bUnit: component rendering and `EventCallback` behavior.
- PostgreSQL Testcontainers: real migration, persistence, and duplicate XP test. Requires Docker.
- Playwright: register → create quest → complete → observe XP feedback. Requires the running Compose app and installed browser.

## Verified result

On 26 October 2026, `dotnet test QuestForge.slnx -m:1 --no-restore` passed 28 tests: 23 Core unit tests, 2 bUnit component tests, 2 integration tests (including Testcontainers PostgreSQL), and 1 Playwright E2E test. Core coverage measured with Coverlet was 95.58% line coverage and 89.47% branch coverage.

## Commands

```bash
dotnet restore QuestForge.slnx
dotnet build QuestForge.slnx -m:1
dotnet test QuestForge.slnx -m:1 --collect:"XPlat Code Coverage"
dotnet tool restore
```

Install Playwright browser after building:

```bash
pwsh tests/QuestForge.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
```

The Docker and browser tests are active, not skipped. A failed external dependency should be fixed or reported; it must not be hidden by skipping the test.
