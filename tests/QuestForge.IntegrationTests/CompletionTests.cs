using QuestForge.Core; using QuestForge.Infrastructure;
namespace QuestForge.IntegrationTests;
public sealed class CompletionTests
{
 [Fact] public async Task Completing_same_quest_twice_awards_xp_once(){var service=new InMemoryQuestService(TimeProvider.System);var created=await service.CreateAsync("u",new("Ship feature","",QuestDifficulty.Epic,"Programming"));var first=await service.CompleteAsync("u",created.Value!.Id);var second=await service.CompleteAsync("u",created.Value.Id);var dashboard=await service.DashboardAsync("u");Assert.True(first.IsSuccess);Assert.False(second.IsSuccess);Assert.Equal(100,dashboard.Player.TotalXp);Assert.Contains("first-quest",dashboard.UnlockedAchievements);}
}
