# Database and Transaction Model

PostgreSQL stores Identity accounts plus `Quests`, `PlayerProfiles`, `PlayerSkills`, `XpTransactions`, and `UnlockedAchievements`. Every domain table includes the Identity user ID directly or through the quest so queries can enforce ownership.

Completing a quest starts one EF Core transaction. The service loads only a quest owned by the current user, rejects a completed quest or existing XP record, updates the quest/profile/skill, inserts an append-only XP record, evaluates pure achievement rules, inserts only new achievements, then commits. A unique index on `XpTransactions.QuestId` is the final race-condition guard. PostgreSQL `xmin` is used as the quest concurrency token.

Read paths use `AsNoTracking`, indexes, ordering, `Skip`, and `Take`. The initial migration is in `src/QuestForge.Web/Migrations`, and startup calls `MigrateAsync`.
