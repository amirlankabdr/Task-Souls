using QuestForge.Core;
namespace QuestForge.Core.Tests;
public sealed class ProgressionTests
{
    [Theory] [InlineData(QuestDifficulty.Easy,10)] [InlineData(QuestDifficulty.Medium,25)] [InlineData(QuestDifficulty.Hard,50)] [InlineData(QuestDifficulty.Epic,100)]
    public void Reward_matches_difficulty(QuestDifficulty difficulty,int expected)=>Assert.Equal(expected,Progression.RewardFor(difficulty));
    [Theory] [InlineData(0,1)] [InlineData(99,1)] [InlineData(100,2)] [InlineData(250,3)] public void Level_is_derived_from_xp(int xp,int expected)=>Assert.Equal(expected,Progression.LevelFor(xp));
    [Fact] public void ApplyReward_returns_new_state_without_mutating_original(){var quest=new Quest(Guid.NewGuid(),"u","Test","",QuestDifficulty.Hard,"Programming",QuestStatus.Active,DateTimeOffset.UtcNow);var original=PlayerState.Empty;var next=Progression.ApplyReward(original,quest);Assert.Equal(0,original.TotalXp);Assert.Equal(50,next.TotalXp);Assert.Equal(50,next.SkillXp["Programming"]);}
    [Fact] public void Achievement_rules_unlock_only_eligible_items(){var state=PlayerState.Empty with{CompletedQuests=1};var unlocked=AchievementCatalog.NewlyUnlocked(state,new HashSet<string>());Assert.Single(unlocked);Assert.Equal("first-quest",unlocked[0].Key);}

    [Fact] public void Result_explicitly_represents_success_and_failure()
    {
        var success = Result<int>.Success(42); var failure = Result<int>.Failure("bad input");
        Assert.True(success.IsSuccess); Assert.Equal(42, success.Value); Assert.Null(success.Error);
        Assert.False(failure.IsSuccess); Assert.Equal(0, failure.Value); Assert.Equal("bad input", failure.Error);
    }

    [Theory] [InlineData(-10, 1, 0, 100)] [InlineData(0, 1, 0, 100)] [InlineData(125, 2, 25, 75)]
    public void Progress_is_clamped_and_derived(int xp, int level, int into, int remaining)
    { Assert.Equal(level, Progression.LevelFor(xp)); Assert.Equal(into, Progression.XpIntoLevel(xp)); Assert.Equal(remaining, Progression.XpToNextLevel(xp)); }

    [Fact] public void Unknown_difficulty_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => Progression.RewardFor((QuestDifficulty)999));

    [Fact] public void Easy_reward_does_not_increment_hard_counter()
    {
        var quest = new Quest(Guid.NewGuid(), "u", "Walk", "desc", QuestDifficulty.Easy, "Fitness", QuestStatus.Active, DateTimeOffset.UtcNow);
        var next = Progression.ApplyReward(PlayerState.Empty, quest);
        Assert.Equal(0, next.HardQuests); Assert.Equal(1, next.CompletedQuests); Assert.Equal(10, next.SkillXp["fitness"]);
        Assert.Equal("u", quest.UserId); Assert.Equal("Walk", quest.Title); Assert.Equal("desc", quest.Description); Assert.Null(quest.CompletedAt);
    }

    [Fact] public void Predicate_combinators_preserve_short_readable_rules()
    {
        Func<int, bool> positive = value => value > 0; Func<int, bool> even = value => value % 2 == 0;
        Assert.True(positive.And(even)(2)); Assert.False(positive.And(even)(1));
        Assert.True(positive.Or(even)(-2)); Assert.False(positive.Or(even)(-3));
        Assert.True(positive.Not()(-1)); Assert.False(positive.Not()(1));
    }

    [Fact] public void Catalog_unlocks_all_thresholds_but_not_existing_achievement()
    {
        var state = new PlayerState(1000, 20, 5, new Dictionary<string, int> { ["Programming"] = 500 });
        var unlocked = AchievementCatalog.NewlyUnlocked(state, new HashSet<string> { "first-quest" });
        Assert.Equal(4, unlocked.Count); Assert.DoesNotContain(unlocked, x => x.Key == "first-quest");
        Assert.All(unlocked, achievement => Assert.True(achievement.Rule(state)));
    }

    [Theory] [InlineData(null, false)] [InlineData("   ", false)] [InlineData("Valid quest", true)]
    public void Title_validation_handles_required_and_valid_values(string? value, bool success)
    { var result = QuestValidation.Title(value); Assert.Equal(success, result.IsSuccess); }

    [Fact] public void Title_validation_trims_and_limits_length()
    {
        Assert.Equal("Quest", QuestValidation.Title("  Quest  ").Value);
        Assert.Equal("Title must be 100 characters or fewer.", QuestValidation.Title(new string('x', 101)).Error);
    }

    [Fact] public void Application_records_expose_query_and_snapshot_values()
    {
        var draft = new QuestDraft("A", "B", QuestDifficulty.Medium, "Creativity");
        var query = new QuestQuery(QuestStatus.Completed, QuestDifficulty.Medium, "A", 2, 5);
        var snapshot = new DashboardSnapshot(PlayerState.Empty, [], new HashSet<string>());
        var transaction = new XpTransaction(Guid.NewGuid(), Guid.NewGuid(), "u", 25, "Creativity", DateTimeOffset.UtcNow);
        Assert.Equal("A", draft.Title); Assert.Equal(2, query.Page); Assert.Equal(5, query.PageSize); Assert.Empty(snapshot.RecentQuests); Assert.Equal(25, transaction.Amount);
    }
}
