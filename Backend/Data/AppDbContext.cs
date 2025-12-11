using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Queue> Queues { get; set; }
    public DbSet<ImageGroup> ImageGroups { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<SelectionRecord> SelectionRecords { get; set; }
    public DbSet<UserProgress> UserProgresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置软删除全局查询过滤器
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Queue>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ImageGroup>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Image>().HasQueryFilter(e => !e.IsDeleted);

        // User配置
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Project配置
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Queue配置
        modelBuilder.Entity<Queue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Queues)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ImageGroup配置
        modelBuilder.Entity<ImageGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Queue)
                .WithMany(q => q.ImageGroups)
                .HasForeignKey(e => e.QueueId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.QueueId, e.GroupName });
            entity.HasIndex(e => e.DisplayOrder);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Image配置
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Queue)
                .WithMany(q => q.Images)
                .HasForeignKey(e => e.QueueId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ImageGroup)
                .WithMany(g => g.Images)
                .HasForeignKey(e => e.ImageGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.QueueId);
            entity.HasIndex(e => e.ImageGroupId);
            entity.HasIndex(e => new { e.ImageGroupId, e.DisplayOrder });
            entity.HasIndex(e => e.FileHash);
            entity.HasIndex(e => new { e.QueueId, e.FileHash }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // SelectionRecord配置
        modelBuilder.Entity<SelectionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Queue)
                .WithMany(q => q.SelectionRecords)
                .HasForeignKey(e => e.QueueId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.SelectionRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ImageGroup)
                .WithMany(g => g.SelectionRecords)
                .HasForeignKey(e => e.ImageGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SelectedImage)
                .WithMany(i => i.SelectionRecords)
                .HasForeignKey(e => e.SelectedImageId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.QueueId, e.UserId, e.ImageGroupId }).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // UserProgress配置
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Queue)
                .WithMany(q => q.UserProgresses)
                .HasForeignKey(e => e.QueueId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserProgresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.QueueId, e.UserId }).IsUnique();
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}

