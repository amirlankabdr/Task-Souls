namespace QuestForge.Core;

public enum QuestDifficulty { Easy, Medium, Hard, Epic }
public enum QuestStatus { Active, Completed }

public sealed record Result<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(string error) => new(default, error);
}

public sealed record Quest(
    Guid Id,
    string UserId,
    string Title,
    string Description,
    QuestDifficulty Difficulty,
    string Skill,
    QuestStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt = null);

public sealed record PlayerState(
    int TotalXp,
    int CompletedQuests,
    int HardQuests,
    IReadOnlyDictionary<string, int> SkillXp)
{
    public static PlayerState Empty { get; } = new(0, 0, 0,
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
}

public sealed record Achievement(string Key, string Name, string Description, Func<PlayerState, bool> Rule);
public sealed record XpTransaction(Guid Id, Guid QuestId, string UserId, int Amount, string Skill, DateTimeOffset AwardedAt);

public static class Progression
{
    public static int RewardFor(QuestDifficulty difficulty) => difficulty switch
    {
        QuestDifficulty.Easy => 10,
        QuestDifficulty.Medium => 25,
        QuestDifficulty.Hard => 50,
        QuestDifficulty.Epic => 100,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
    };

    public static int LevelFor(int totalXp) => Math.Max(0, totalXp) / 100 + 1;
    public static int XpIntoLevel(int totalXp) => Math.Max(0, totalXp) % 100;
    public static int XpToNextLevel(int totalXp) => 100 - XpIntoLevel(totalXp);

    public static PlayerState ApplyReward(PlayerState state, Quest quest)
    {
        var reward = RewardFor(quest.Difficulty);
        var skills = new Dictionary<string, int>(state.SkillXp, StringComparer.OrdinalIgnoreCase)
        {
            [quest.Skill] = state.SkillXp.GetValueOrDefault(quest.Skill) + reward
        };

        return state with
        {
            TotalXp = state.TotalXp + reward,
            CompletedQuests = state.CompletedQuests + 1,
            HardQuests = state.HardQuests + (quest.Difficulty is QuestDifficulty.Hard or QuestDifficulty.Epic ? 1 : 0),
            SkillXp = skills
        };
    }
}

public static class Rule
{
    public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right) => value => left(value) && right(value);
    public static Func<T, bool> Or<T>(this Func<T, bool> left, Func<T, bool> right) => value => left(value) || right(value);
    public static Func<T, bool> Not<T>(this Func<T, bool> rule) => value => !rule(value);
}

public static class AchievementCatalog
{
    public static IReadOnlyList<Achievement> All { get; } =
    [
        new("first-quest", "First Quest", "Complete your first quest.", s => s.CompletedQuests >= 1),
        new("getting-started", "Getting Started", "Complete five quests.", s => s.CompletedQuests >= 5),
        new("dedicated", "Dedicated", "Complete twenty quests.", s => s.CompletedQuests >= 20),
        new("hard-worker", "Hard Worker", "Complete five hard or epic quests.", s => s.HardQuests >= 5),
        new("skill-master", "Skill Master", "Earn 500 XP in one skill.", s => s.SkillXp.Values.Any(xp => xp >= 500))
    ];

    public static IReadOnlyList<Achievement> NewlyUnlocked(PlayerState state, ISet<string> unlocked) =>
        All.Where(a => !unlocked.Contains(a.Key) && a.Rule(state)).ToArray();
}

public static class QuestValidation
{
    public static Result<string> Title(string? title) => string.IsNullOrWhiteSpace(title)
        ? Result<string>.Failure("Title is required.")
        : title.Trim().Length > 100
            ? Result<string>.Failure("Title must be 100 characters or fewer.")
            : Result<string>.Success(title.Trim());
}
