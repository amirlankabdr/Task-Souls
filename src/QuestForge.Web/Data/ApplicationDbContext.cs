using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuestForge.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<QuestEntity> Quests => Set<QuestEntity>();
    public DbSet<PlayerProfileEntity> PlayerProfiles => Set<PlayerProfileEntity>();
    public DbSet<PlayerSkillEntity> PlayerSkills => Set<PlayerSkillEntity>();
    public DbSet<XpTransactionEntity> XpTransactions => Set<XpTransactionEntity>();
    public DbSet<UnlockedAchievementEntity> UnlockedAchievements => Set<UnlockedAchievementEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<QuestEntity>(e => { e.HasKey(x => x.Id); e.Property(x => x.Title).HasMaxLength(100); e.Property(x => x.Skill).HasMaxLength(40); e.HasIndex(x => new { x.UserId, x.Status, x.CreatedAt }); e.Property(x => x.Version).IsRowVersion(); });
        builder.Entity<PlayerProfileEntity>().HasKey(x => x.UserId);
        builder.Entity<PlayerSkillEntity>().HasKey(x => new { x.UserId, x.Skill });
        builder.Entity<XpTransactionEntity>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.QuestId).IsUnique(); e.HasIndex(x => new { x.UserId, x.AwardedAt }); });
        builder.Entity<UnlockedAchievementEntity>().HasKey(x => new { x.UserId, x.AchievementKey });
    }
}

public sealed class QuestEntity { public Guid Id { get; set; } public string UserId { get; set; } = ""; public string Title { get; set; } = ""; public string Description { get; set; } = ""; public int Difficulty { get; set; } public string Skill { get; set; } = ""; public int Status { get; set; } public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset? CompletedAt { get; set; } public uint Version { get; set; } }
public sealed class PlayerProfileEntity { public string UserId { get; set; } = ""; public int TotalXp { get; set; } public int CompletedQuests { get; set; } public int HardQuests { get; set; } }
public sealed class PlayerSkillEntity { public string UserId { get; set; } = ""; public string Skill { get; set; } = ""; public int Xp { get; set; } }
public sealed class XpTransactionEntity { public Guid Id { get; set; } public Guid QuestId { get; set; } public string UserId { get; set; } = ""; public int Amount { get; set; } public string Skill { get; set; } = ""; public DateTimeOffset AwardedAt { get; set; } }
public sealed class UnlockedAchievementEntity { public string UserId { get; set; } = ""; public string AchievementKey { get; set; } = ""; public DateTimeOffset UnlockedAt { get; set; } }
