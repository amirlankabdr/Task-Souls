using Microsoft.EntityFrameworkCore;
using QuestForge.Core;

namespace QuestForge.Web.Data;

public sealed class EfQuestService(ApplicationDbContext db, TimeProvider timeProvider) : IQuestService
{
    public async Task<Result<Quest>> CreateAsync(string userId, QuestDraft draft, CancellationToken ct = default)
    {
        var validated = QuestDraftValidation.Validate(draft);
        if (!validated.IsSuccess) return Result<Quest>.Failure(validated.Error!);
        draft = validated.Value!;
        var entity = new QuestEntity { Id = Guid.NewGuid(), UserId = userId, Title = draft.Title, Description = draft.Description, Difficulty = (int)draft.Difficulty, Skill = draft.Skill, Status = (int)QuestStatus.Active, CreatedAt = timeProvider.GetUtcNow() };
        db.Quests.Add(entity); await db.SaveChangesAsync(ct); return Result<Quest>.Success(ToDomain(entity));
    }

    public async Task<Result<Quest>> UpdateAsync(string userId, Guid questId, QuestDraft draft, CancellationToken ct = default)
    {
        var entity = await db.Quests.SingleOrDefaultAsync(q => q.Id == questId && q.UserId == userId, ct);
        if (entity is null) return Result<Quest>.Failure("Quest was not found.");
        if ((QuestStatus)entity.Status == QuestStatus.Completed) return Result<Quest>.Failure("Completed quests cannot be edited.");
        var validated = QuestDraftValidation.Validate(draft); if (!validated.IsSuccess) return Result<Quest>.Failure(validated.Error!);
        draft = validated.Value!;
        entity.Title = draft.Title; entity.Description = draft.Description; entity.Difficulty = (int)draft.Difficulty; entity.Skill = draft.Skill;
        await db.SaveChangesAsync(ct); return Result<Quest>.Success(ToDomain(entity));
    }

    public async Task<Result<bool>> DeleteAsync(string userId, Guid questId, CancellationToken ct = default)
    {
        var entity = await db.Quests.SingleOrDefaultAsync(q => q.Id == questId && q.UserId == userId, ct);
        if (entity is null) return Result<bool>.Failure("Quest was not found.");
        if ((QuestStatus)entity.Status == QuestStatus.Completed) return Result<bool>.Failure("Completed quests are retained in history.");
        db.Quests.Remove(entity); await db.SaveChangesAsync(ct); return Result<bool>.Success(true);
    }

    public async Task<IReadOnlyList<Quest>> ListAsync(string userId, QuestQuery query, CancellationToken ct = default)
    {
        var source = db.Quests.AsNoTracking().Where(q => q.UserId == userId);
        if (query.Status is not null) source = source.Where(q => q.Status == (int)query.Status);
        if (query.Difficulty is not null) source = source.Where(q => q.Difficulty == (int)query.Difficulty);
        if (!string.IsNullOrWhiteSpace(query.Search)) source = source.Where(q => EF.Functions.ILike(q.Title, QuestSearch.ToLikePattern(query.Search), "\\"));
        return await source.OrderByDescending(q => q.CreatedAt).ThenBy(q => q.Id).Skip((Math.Max(1, query.Page) - 1) * Math.Clamp(query.PageSize, 1, 50)).Take(Math.Clamp(query.PageSize, 1, 50)).Select(q => ToDomain(q)).ToArrayAsync(ct);
    }

    public async Task<Result<DashboardSnapshot>> CompleteAsync(string userId, Guid questId, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({userId}, 0))", ct);
            db.ChangeTracker.Clear();
            var quest = await db.Quests.SingleOrDefaultAsync(q => q.Id == questId && q.UserId == userId, ct);
            if (quest is null) return Result<DashboardSnapshot>.Failure("Quest was not found.");
            if ((QuestStatus)quest.Status == QuestStatus.Completed || await db.XpTransactions.AnyAsync(x => x.QuestId == questId, ct)) return Result<DashboardSnapshot>.Failure("This quest has already been completed.");

            var profile = await db.PlayerProfiles.SingleOrDefaultAsync(x => x.UserId == userId, ct) ?? new PlayerProfileEntity { UserId = userId };
            if (db.Entry(profile).State == EntityState.Detached) db.PlayerProfiles.Add(profile);
            var skill = await db.PlayerSkills.SingleOrDefaultAsync(x => x.UserId == userId && x.Skill == quest.Skill, ct) ?? new PlayerSkillEntity { UserId = userId, Skill = quest.Skill };
            if (db.Entry(skill).State == EntityState.Detached) db.PlayerSkills.Add(skill);
            var reward = Progression.RewardFor((QuestDifficulty)quest.Difficulty);
            quest.Status = (int)QuestStatus.Completed; quest.CompletedAt = timeProvider.GetUtcNow(); var currentSkills = await db.PlayerSkills.AsNoTracking().Where(x => x.UserId == userId).ToDictionaryAsync(x => x.Skill, x => x.Xp, ct);
            var next = Progression.ApplyReward(new PlayerState(profile.TotalXp, profile.CompletedQuests, profile.HardQuests, currentSkills), ToDomain(quest));
            profile.TotalXp = next.TotalXp; profile.CompletedQuests = next.CompletedQuests; profile.HardQuests = next.HardQuests; skill.Xp = next.SkillXp[quest.Skill];
            db.XpTransactions.Add(new XpTransactionEntity { Id = Guid.NewGuid(), QuestId = quest.Id, UserId = userId, Amount = reward, Skill = quest.Skill, AwardedAt = timeProvider.GetUtcNow() });

            var skillXp = await db.PlayerSkills.Where(x => x.UserId == userId).ToDictionaryAsync(x => x.Skill, x => x.Xp, ct); skillXp[skill.Skill] = skill.Xp;
            var state = new PlayerState(profile.TotalXp, profile.CompletedQuests, profile.HardQuests, skillXp);
            var existing = await db.UnlockedAchievements.Where(x => x.UserId == userId).Select(x => x.AchievementKey).ToHashSetAsync(ct);
            foreach (var achievement in AchievementCatalog.NewlyUnlocked(state, existing)) db.UnlockedAchievements.Add(new() { UserId = userId, AchievementKey = achievement.Key, UnlockedAt = timeProvider.GetUtcNow() });
            await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return Result<DashboardSnapshot>.Success(await DashboardAsync(userId, ct));
        }
        catch (DbUpdateConcurrencyException) { await transaction.RollbackAsync(ct); db.ChangeTracker.Clear(); return Result<DashboardSnapshot>.Failure("The quest changed in another request. Refresh and try again."); }
        catch (DbUpdateException) { await transaction.RollbackAsync(ct); db.ChangeTracker.Clear(); return Result<DashboardSnapshot>.Failure("Progress could not be saved. Refresh before trying again."); }
    }

    public async Task<DashboardSnapshot> DashboardAsync(string userId, CancellationToken ct = default)
    {
        var profile = await db.PlayerProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, ct);
        var skills = await db.PlayerSkills.AsNoTracking().Where(x => x.UserId == userId).ToDictionaryAsync(x => x.Skill, x => x.Xp, ct);
        var recent = await db.Quests.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.CompletedAt ?? x.CreatedAt).Take(5).Select(x => ToDomain(x)).ToArrayAsync(ct);
        var unlocked = await db.UnlockedAchievements.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.AchievementKey).ToHashSetAsync(ct);
        return new(new(profile?.TotalXp ?? 0, profile?.CompletedQuests ?? 0, profile?.HardQuests ?? 0, skills), recent, unlocked);
    }

    private static Quest ToDomain(QuestEntity q) => new(q.Id, q.UserId, q.Title, q.Description, (QuestDifficulty)q.Difficulty, q.Skill, (QuestStatus)q.Status, q.CreatedAt, q.CompletedAt);
}
