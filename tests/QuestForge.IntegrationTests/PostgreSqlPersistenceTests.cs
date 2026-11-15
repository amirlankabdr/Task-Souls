using Microsoft.EntityFrameworkCore;
using QuestForge.Core;
using QuestForge.Web.Data;
using Testcontainers.PostgreSql;

namespace QuestForge.IntegrationTests;

public sealed class PostgreSqlPersistenceTests
{
    [Fact]
    public async Task Search_treats_percent_and_underscore_as_text()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.MigrateAsync();
        var service = new EfQuestService(db, TimeProvider.System);
        await service.CreateAsync("user", new("Reach 100%", "", QuestDifficulty.Easy, "Programming"));
        await service.CreateAsync("user", new("A_B", "", QuestDifficulty.Easy, "Programming"));
        await service.CreateAsync("user", new("Plain", "", QuestDifficulty.Easy, "Programming"));

        Assert.Equal("Reach 100%", Assert.Single(await service.ListAsync("user", new(Search: "%"))).Title);
        Assert.Equal("A_B", Assert.Single(await service.ListAsync("user", new(Search: "_"))).Title);
    }

    [Fact]
    public async Task Completion_is_persisted_and_idempotent_in_postgresql()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new ApplicationDbContext(options); await db.Database.MigrateAsync();
        var service = new EfQuestService(db, TimeProvider.System);
        var created = await service.CreateAsync("user-1", new("Persist me", "", QuestDifficulty.Hard, "Programming"));
        Assert.True((await service.CompleteAsync("user-1", created.Value!.Id)).IsSuccess);
        Assert.False((await service.CompleteAsync("user-1", created.Value.Id)).IsSuccess);
        Assert.Equal(50, (await service.DashboardAsync("user-1")).Player.TotalXp);
    }

    [Fact]
    public async Task Parallel_completions_preserve_both_rewards()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var setup = new ApplicationDbContext(options);
        await setup.Database.MigrateAsync();
        var seed = new EfQuestService(setup, TimeProvider.System);
        var a = (await seed.CreateAsync("parallel-user", new("A", "", QuestDifficulty.Hard, "Programming"))).Value!;
        var b = (await seed.CreateAsync("parallel-user", new("B", "", QuestDifficulty.Medium, "Programming"))).Value!;
        await using var db1 = new ApplicationDbContext(options);
        await using var db2 = new ApplicationDbContext(options);
        var results = await Task.WhenAll(new EfQuestService(db1, TimeProvider.System).CompleteAsync("parallel-user", a.Id),
            new EfQuestService(db2, TimeProvider.System).CompleteAsync("parallel-user", b.Id));
        Assert.All(results, result => Assert.True(result.IsSuccess, result.Error));
        await using var read = new ApplicationDbContext(options);
        var state = await new EfQuestService(read, TimeProvider.System).DashboardAsync("parallel-user");
        Assert.Equal(75, state.Player.TotalXp); Assert.Equal(2, state.Player.CompletedQuests);
        Assert.Equal(75, state.Player.SkillXp["Programming"]);
    }
}
