# Presentation and Defence Outline

Team: Aslanuly Ulanbek and Amirlan Kabdyrakym. Topic: **QuestForge — Level Up Your Real Life**.

## Slide plan (15–20 minutes)

1. Team, topic, aim, goals, and agenda — 1 minute.
2. Problem and practical value: turning real tasks into visible, skill-based progress — 1 minute.
3. Existing products: Habitica gamifies tasks, Todoist focuses on productivity, and Notion is flexible but manual. QuestForge combines task management with deterministic XP and explainable functional rules — 1 minute.
4. Key user flow: register, create a quest, complete it, see XP and achievement changes — 2 minutes.
5. Architecture: Core pure rules → service interface → PostgreSQL/Blazor adapter — 2 minutes.
6. Data model: Identity user, quest, player profile, skill, XP transaction, unlocked achievement — 1 minute.
7. Functional programming: immutable records, pure reward/level functions, `Result<T>`, and composable achievement predicates — 2 minutes.
8. Reliability: transaction plus unique `XpTransactions.QuestId` prevents double XP — 1 minute.
9. UI/UX: responsive dashboard, filters, delete confirmation, loading/error/empty states — 1 minute.
10. Tests and QA: 28 passing automated tests, Core coverage 95.58%, Testcontainers, and Playwright — 2 minutes.
11. Team contribution and lessons learned — 1 minute.
12. Limitations and next steps: cross-browser/manual device matrix, admin screens, and optional live updates — 1 minute.

## Live demo order

1. Register a new account.
2. Add a Medium Programming quest.
3. Complete it once and show the XP result.
4. Open Dashboard to show level, skill XP, activity, and achievement state.
5. Explain why a second completion cannot add XP.

## Suggested responsibility split

- **Aslanuly Ulanbek:** product goal, UI/UX, Blazor components, state container, live demo.
- **Amirlan Kabdyrakym:** domain rules, database transaction, Identity, testing, Docker.

Both members should be able to explain the full architecture, especially the completion transaction and functional rules.
