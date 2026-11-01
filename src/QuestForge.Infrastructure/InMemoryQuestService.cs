using System.Collections.Concurrent;
using QuestForge.Core;

namespace QuestForge.Infrastructure;

public sealed class InMemoryQuestService(TimeProvider timeProvider) : IQuestService
{
    private readonly ConcurrentDictionary<Guid, Quest> _quests = new();
    private readonly ConcurrentDictionary<string, PlayerState> _players = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _achievements = new();
    private readonly ConcurrentDictionary<Guid, XpTransaction> _transactions = new();
    private readonly SemaphoreSlim _completionLock = new(1, 1);

    public Task<Result<Quest>> CreateAsync(string userId, QuestDraft draft, CancellationToken cancellationToken = default)
    {
        var title = QuestValidation.Title(draft.Title);
        if (!title.IsSuccess) return Task.FromResult(Result<Quest>.Failure(title.Error!));
        if (string.IsNullOrWhiteSpace(draft.Skill)) return Task.FromResult(Result<Quest>.Failure("Skill is required."));

        var quest = new Quest(Guid.NewGuid(), userId, title.Value!, draft.Description.Trim(), draft.Difficulty,
            draft.Skill.Trim(), QuestStatus.Active, timeProvider.GetUtcNow());
        _quests[quest.Id] = quest;
        return Task.FromResult(Result<Quest>.Success(quest));
    }

    public Task<IReadOnlyList<Quest>> ListAsync(string userId, QuestQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<Quest> result = _quests.Values.Where(q => q.UserId == userId);
        if (query.Status is not null) result = result.Where(q => q.Status == query.Status);
        if (query.Difficulty is not null) result = result.Where(q => q.Difficulty == query.Difficulty);
        if (!string.IsNullOrWhiteSpace(query.Search)) result = result.Where(q => q.Title.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<IReadOnlyList<Quest>>(result.OrderByDescending(q => q.CreatedAt)
            .Skip((Math.Max(1, query.Page) - 1) * Math.Clamp(query.PageSize, 1, 50)).Take(Math.Clamp(query.PageSize, 1, 50)).ToArray());
    }

    public Task<Result<Quest>> UpdateAsync(string userId, Guid questId, QuestDraft draft, CancellationToken cancellationToken = default)
    {
        if (!_quests.TryGetValue(questId, out var quest) || quest.UserId != userId) return Task.FromResult(Result<Quest>.Failure("Quest was not found."));
        if (quest.Status == QuestStatus.Completed) return Task.FromResult(Result<Quest>.Failure("Completed quests cannot be edited."));
        var title = QuestValidation.Title(draft.Title); if (!title.IsSuccess) return Task.FromResult(Result<Quest>.Failure(title.Error!));
        var updated = quest with { Title = title.Value!, Description = draft.Description.Trim(), Difficulty = draft.Difficulty, Skill = draft.Skill.Trim() };
        _quests[questId] = updated; return Task.FromResult(Result<Quest>.Success(updated));
    }

    public Task<Result<bool>> DeleteAsync(string userId, Guid questId, CancellationToken cancellationToken = default)
    {
        if (!_quests.TryGetValue(questId, out var quest) || quest.UserId != userId) return Task.FromResult(Result<bool>.Failure("Quest was not found."));
        if (quest.Status == QuestStatus.Completed) return Task.FromResult(Result<bool>.Failure("Completed quests are retained in history."));
        return Task.FromResult(_quests.TryRemove(questId, out _) ? Result<bool>.Success(true) : Result<bool>.Failure("Quest could not be deleted."));
    }

    public async Task<Result<DashboardSnapshot>> CompleteAsync(string userId, Guid questId, CancellationToken cancellationToken = default)
    {
        await _completionLock.WaitAsync(cancellationToken);
        try
        {
            if (!_quests.TryGetValue(questId, out var quest) || quest.UserId != userId)
                return Result<DashboardSnapshot>.Failure("Quest was not found.");
            if (quest.Status == QuestStatus.Completed || _transactions.ContainsKey(questId))
                return Result<DashboardSnapshot>.Failure("This quest has already been completed.");

            var completed = quest with { Status = QuestStatus.Completed, CompletedAt = timeProvider.GetUtcNow() };
            var state = Progression.ApplyReward(_players.GetValueOrDefault(userId, PlayerState.Empty), completed);
            _quests[questId] = completed;
            _players[userId] = state;
            _transactions[questId] = new(Guid.NewGuid(), questId, userId, Progression.RewardFor(quest.Difficulty), quest.Skill, timeProvider.GetUtcNow());
            var unlocked = _achievements.GetOrAdd(userId, _ => []);
            foreach (var achievement in AchievementCatalog.NewlyUnlocked(state, unlocked)) unlocked.Add(achievement.Key);
            return Result<DashboardSnapshot>.Success(await DashboardAsync(userId, cancellationToken));
        }
        finally { _completionLock.Release(); }
    }

    public Task<DashboardSnapshot> DashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        var recent = _quests.Values.Where(q => q.UserId == userId).OrderByDescending(q => q.CompletedAt ?? q.CreatedAt).Take(5).ToArray();
        IReadOnlySet<string> unlocked = _achievements.TryGetValue(userId, out var set) ? new HashSet<string>(set) : new HashSet<string>();
        return Task.FromResult(new DashboardSnapshot(_players.GetValueOrDefault(userId, PlayerState.Empty), recent, unlocked));
    }
}
