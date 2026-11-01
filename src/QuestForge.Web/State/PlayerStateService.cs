using QuestForge.Core;

namespace QuestForge.Web.State;

public sealed class PlayerStateService(IQuestService quests)
{
    public DashboardSnapshot Current { get; private set; } = new(PlayerState.Empty, [], new HashSet<string>());
    public event Action? OnChange;

    public async Task RefreshAsync(string userId, CancellationToken cancellationToken = default)
    {
        Current = await quests.DashboardAsync(userId, cancellationToken);
        OnChange?.Invoke();
    }

    public async Task<Result<DashboardSnapshot>> CompleteAsync(string userId, Guid questId, CancellationToken cancellationToken = default)
    {
        var result = await quests.CompleteAsync(userId, questId, cancellationToken);
        if (result.IsSuccess) { Current = result.Value!; OnChange?.Invoke(); }
        return result;
    }
}
