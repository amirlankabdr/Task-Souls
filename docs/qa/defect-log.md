# Defect Log

| ID | Defect | Resolution | Verification |
|---|---|---|---|
| QF-001 | Repeated completion could award duplicate XP in a naive implementation | Status check, unique XP index, concurrency token, and transaction | In-memory and Testcontainers PostgreSQL tests passed |
| QF-002 | Initial prototype used a shared `demo-user` identity | Routes now resolve the authenticated Identity claim and queries filter by user ID | Protected queries use the Identity user ID; automated registration flow passed |
| QF-003 | Generated Identity project used SQLite | Replaced provider with Npgsql and generated clean PostgreSQL migration | Compose startup and migration passed |
| QF-004 | Delete had no safety prompt | Added two-step inline confirmation | Code and bUnit component coverage verified; browser matrix remains outstanding |
| QF-005 | Docker release image returned 404 for `blazor.web.js`, leaving buttons non-interactive | Pinned ASP.NET runtime to 10.0.13 and included the matching framework asset package | HTTP 200 check and Playwright full user flow passed |
| QF-006 | PostgreSQL retry strategy conflicted with the explicit completion transaction | Removed automatic retry from the context configuration; completion remains one explicit transaction | Playwright completion and Testcontainers persistence tests passed |
