using Bunit;
using QuestForge.Core;
using QuestForge.Web.Components;

namespace QuestForge.Web.Tests;

public sealed class ComponentTests
{
    [Fact]
    public void QuestCard_renders_reward_and_invokes_completion()
    {
        using var context = new BunitContext();
        var completed = Guid.Empty;
        var quest = new Quest(Guid.NewGuid(), "user", "Write tests", "", QuestDifficulty.Hard, "Programming", QuestStatus.Active, DateTimeOffset.UtcNow);
        var cut = context.Render<QuestCard>(p => p.Add(x => x.Quest, quest).Add(x => x.OnComplete, id => completed = id));
        Assert.Contains("+50 XP", cut.Markup);
        cut.Find("button.complete").Click();
        Assert.Equal(quest.Id, completed);
    }

    [Fact]
    public void AchievementBadge_exposes_locked_state()
    {
        using var context = new BunitContext();
        var achievement = AchievementCatalog.All[0];
        var cut = context.Render<AchievementBadge>(p => p.Add(x => x.Achievement, achievement).Add(x => x.Unlocked, false));
        Assert.Contains("locked", cut.Find("article").ClassList);
        Assert.Contains(achievement.Name, cut.Markup);
    }
}
