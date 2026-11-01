using Microsoft.EntityFrameworkCore;
using QuestForge.Core;
using QuestForge.Web.Data;
using Testcontainers.PostgreSql;

namespace QuestForge.IntegrationTests;

public sealed class PostgreSqlPersistenceTests
{
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
}
