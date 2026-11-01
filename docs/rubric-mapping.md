# Rubric Evidence Map

| # | Criterion | Evidence |
|---:|---|---|
| 1 | Code Structure and Design | Core/Infrastructure/Web separation; `docs/architecture/overview.md`. |
| 2 | Component Reusability | `QuestCard`, `ProgressBar`, `SkillCard`, `AchievementBadge`, generic `DataList<TItem>`. |
| 3 | Cross-Platform Compatibility | Docker Compose starts web app and PostgreSQL; responsive CSS. |
| 4 | State Management | Scoped `PlayerStateService`, `OnChange`, `CascadingValue`, disposal. |
| 5 | User Interface (UI) Design | Consistent dark RPG-progress styling and dashboard. |
| 6 | User Experience (UX) | Clear navigation, validation, filters, confirmation, error/empty states. |
| 7 | Responsiveness | Mobile-first CSS grids and media queries; QA matrix. |
| 8 | Core Functionality | Quest CRUD, XP, levels, skills, achievements, history, per-user data. |
| 9 | Advanced Features | ASP.NET Core Identity, `User`/`Admin` roles, row version and idempotent XP. |
| 10 | Error Handling and Resilience | `Result<T>`, validation, global UI error feedback, transaction. |
| 11 | Efficiency | Async calls, `AsNoTracking`, indexes, pagination, bounded lists. |
| 12 | Scalability | Modular layers, database indexes, stateless service contract, Compose deployment. |
| 13 | Resource Utilization | Small Docker build context, server rendering, bounded queries and pagination. |
| 14 | Testing Coverage | 28 passing tests; Core coverage 95.58% lines / 89.47% branches. |
| 15 | Quality Assurance | Defect log, automated end-to-end flow, manual browser/device matrix. |
| 16 | Documentation | README, user guide, developer notes, architecture, FP, database, tests, auth. |
| 17 | Innovation | Composable achievement rules modelled as pure predicates. |
| 18 | Creative Use of Blazor/C#/.NET Features | Templated component, `EventCallback`, cascading state, Identity, EF Core. |
| 19 | Professionalism | Reproducible Docker run, no committed secrets, migration, tests and documentation. |
| 20 | Presentation | `docs/presentation-outline.md` with slide plan, demo sequence, and team split. |
