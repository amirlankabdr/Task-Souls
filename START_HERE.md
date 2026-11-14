# QuestForge update

## Install without losing existing progress
1. Back up your current source folder. Keep its .env file.
2. Stop the running application with Ctrl+C in its PowerShell window.
3. Copy the CONTENTS of this ZIP's Task-Souls-main folder into your EXISTING Task-Souls-main folder. Replace matching files. Do not create a nested folder. Do not overwrite .env (it is not included).
4. From that original folder run `docker compose up --build`.
5. Open http://localhost:8080 and log in with your existing account.

Keeping the same folder name and Compose project keeps the existing Docker volumes. Do not run `docker compose down -v`: it deletes stored data.

## Changes
- Result Map/Bind and pure shared draft validation in both services.
- Difficulty, title and skill validation with database length limits.
- Actual use of ApplyReward in the PostgreSQL completion flow.
- PostgreSQL per-user transaction advisory lock for concurrent completions; no schema migration needed.
- Accurate concurrency/save failure feedback and reset of tracked failed writes.
- Previous/Next pagination, stable ordering, filter reset, explicit Search button.
- Busy state, disabled controls, logged errors, edit-state cleanup after delete/complete.
- Dark header, focus styles, narrow quest-card wrapping and defensive empty skill rendering.
- Regression tests for validation, pagination, isolation and parallel database completion.

## Verification status
Source review and ZIP integrity checks performed. This environment has no .NET SDK or Docker; the modified solution has NOT been compiled or executed here. The original app was reported working by the user before this update. Treat this as a reviewable implementation awaiting build and test results, not a certified final submission.

## Run tests
With the .NET 10 SDK installed and Docker Desktop running:
```
dotnet test tests/QuestForge.Core.Tests/QuestForge.Core.Tests.csproj
dotnet test tests/QuestForge.Web.Tests/QuestForge.Web.Tests.csproj
dotnet test tests/QuestForge.IntegrationTests/QuestForge.IntegrationTests.csproj
```
Without a local SDK, run just Core tests via Docker from PowerShell in this folder:
```
docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test tests/QuestForge.Core.Tests/QuestForge.Core.Tests.csproj
```
Browser tests need the running app and Playwright browser installation; see docs/study/05-testing.md.

## Manual acceptance
Create 13 quests; verify page 2 contains the remaining quest and Next disables there. Delete that last quest and verify automatic fallback to page 1. Search and change filters from page 2; verify page resets. Submit spaces as a title. Edit, cancel deletion, confirm deletion, complete quests, refresh and log in again. Check a second account cannot see the first account's data. Complete different quests in two tabs and verify the sum of XP. Check keyboard-only use, a narrow viewport, Chrome and another browser. Record actual results in docs/qa/manual-test-matrix.md.

## Remaining submission work
Run the build/tests, correct any reported failures, record cross-browser/device results, capture final screenshots and prepare the required presentation. Both teammates must understand and genuinely contribute to the code. Roles are seeded but there is no admin dashboard; do not claim full administrative features. In-memory storage is a development/test implementation, not the production database.
