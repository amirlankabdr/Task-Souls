namespace QuestForge.Core;

public sealed record QuestDraft(string Title, string Description, QuestDifficulty Difficulty, string Skill);
public sealed record QuestQuery(QuestStatus? Status = null, QuestDifficulty? Difficulty = null, string? Search = null, int Page = 1, int PageSize = 12);
public sealed record DashboardSnapshot(PlayerState Player, IReadOnlyList<Quest> RecentQuests, IReadOnlySet<string> UnlockedAchievements);

public interface IQuestService
{
    Task<Result<Quest>> CreateAsync(string userId, QuestDraft draft, CancellationToken cancellationToken = default);
    Task<Result<Quest>> UpdateAsync(string userId, Guid questId, QuestDraft draft, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(string userId, Guid questId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quest>> ListAsync(string userId, QuestQuery query, CancellationToken cancellationToken = default);
    Task<Result<DashboardSnapshot>> CompleteAsync(string userId, Guid questId, CancellationToken cancellationToken = default);
    Task<DashboardSnapshot> DashboardAsync(string userId, CancellationToken cancellationToken = default);
}
