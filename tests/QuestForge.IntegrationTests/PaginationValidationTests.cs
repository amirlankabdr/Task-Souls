using QuestForge.Core;
using QuestForge.Infrastructure;
namespace QuestForge.IntegrationTests;
public sealed class PaginationValidationTests
{
    [Fact] public async Task Pages_do_not_overlap_and_users_are_isolated() {
        var service = new InMemoryQuestService(TimeProvider.System);
        for (var i = 0; i < 13; i++) await service.CreateAsync("a", new($"Quest {i}", "", QuestDifficulty.Easy, "Programming"));
        await service.CreateAsync("b", new("Other", "", QuestDifficulty.Easy, "Programming"));
        var first = await service.ListAsync("a", new(Page: 1));
        var second = await service.ListAsync("a", new(Page: 2));
        Assert.Equal(12, first.Count); Assert.Single(second);
        Assert.DoesNotContain(second[0].Id, first.Select(q => q.Id));
        Assert.All(first, q => Assert.Equal("a", q.UserId));
    }
    [Fact] public async Task Invalid_update_keeps_existing_quest() {
        var service = new InMemoryQuestService(TimeProvider.System);
        var quest = (await service.CreateAsync("a", new("Original", "", QuestDifficulty.Easy, "Programming"))).Value!;
        Assert.False((await service.UpdateAsync("a", quest.Id, new("Changed", "", QuestDifficulty.Easy, " "))).IsSuccess);
        Assert.Equal("Original", (await service.ListAsync("a", new()))[0].Title);
    }
}
