using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nory.Infrastructure.Persistence.Models;

namespace Nory.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<UserDbModel, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // entities
    public DbSet<EventDbModel> Events { get; set; }
    public DbSet<EventPhotoDbModel> EventPhotos { get; set; }
    public DbSet<EventCategoryDbModel> EventCategories { get; set; }
    public DbSet<EventAppDbModel> EventApps { get; set; }
    public DbSet<AppTypeDbModel> AppTypes { get; set; }

    // Analytics entities
    public DbSet<ActivityLogDbModel> ActivityLogs { get; set; }
    public DbSet<EventMetricsDbModel> EventMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserDbModel>().ToTable("Users");
        builder.Entity<EventDbModel>().ToTable("Events");
        builder.Entity<EventPhotoDbModel>().ToTable("EventPhotos");
        builder.Entity<EventCategoryDbModel>().ToTable("EventCategories");
        builder.Entity<EventAppDbModel>().ToTable("EventApps");
        builder.Entity<AppTypeDbModel>().ToTable("AppTypes");

        // Analytics tables
        builder.Entity<ActivityLogDbModel>().ToTable("ActivityLogs");
        builder.Entity<EventMetricsDbModel>().ToTable("EventMetrics");

        // indexes
        builder
            .Entity<EventPhotoDbModel>()
            .HasIndex(p => p.EventId)
            .HasDatabaseName("IX_EventPhotos_EventId");

        builder
            .Entity<EventCategoryDbModel>()
            .HasIndex(c => c.EventId)
            .HasDatabaseName("IX_EventCategories_EventId");

        builder
            .Entity<EventAppDbModel>()
            .HasIndex(ea => ea.EventId)
            .HasDatabaseName("IX_EventApps_EventId");

        // Analytics indexes
        builder
            .Entity<ActivityLogDbModel>()
            .HasIndex(a => a.EventId)
            .HasDatabaseName("IX_ActivityLogs_EventId");

        builder
            .Entity<ActivityLogDbModel>()
            .HasIndex(a => new { a.EventId, a.Type })
            .HasDatabaseName("IX_ActivityLogs_EventId_Type");

        builder
            .Entity<ActivityLogDbModel>()
            .HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_ActivityLogs_CreatedAt");

        builder
            .Entity<EventMetricsDbModel>()
            .HasIndex(m => new { m.EventId, m.PeriodType })
            .HasDatabaseName("IX_EventMetrics_EventId_PeriodType");

        // relationships
        builder
            .Entity<EventDbModel>()
            .HasMany(e => e.Photos)
            .WithOne(p => p.Event)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<EventDbModel>()
            .HasMany(e => e.Categories)
            .WithOne(c => c.Event)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<EventDbModel>()
            .HasMany(e => e.EventApps)
            .WithOne(ea => ea.Event)
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Analytics relationships
        builder
            .Entity<ActivityLogDbModel>()
            .HasOne(a => a.Event)
            .WithMany()
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<EventMetricsDbModel>()
            .HasOne(m => m.Event)
            .WithMany()
            .HasForeignKey(m => m.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ActivityLogDbModel>().Property(a => a.Data).HasColumnType("jsonb");

        builder.Entity<EventMetricsDbModel>().Property(m => m.FeatureUsage).HasColumnType("jsonb");
    }
}
