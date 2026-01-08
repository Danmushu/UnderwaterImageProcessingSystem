using Microsoft.EntityFrameworkCore;
using UIPS.API.Models;

namespace UIPS.API.Services;

/// <summary>
/// 数据库上下文类
/// 管理应用程序的所有数据库实体和数据库连接
/// </summary>
public class UipsDbContext(DbContextOptions<UipsDbContext> options) : DbContext(options)
{
    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 图片表
    /// </summary>
    public DbSet<Image> Images { get; set; }

    /// <summary>
    /// 收藏记录表
    /// </summary>
    public DbSet<Favourite> Favourites { get; set; }

    /// <summary>
    /// 配置实体模型（可选）
    /// 用于配置表关系、索引、约束等
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 为 User 表的 UserName 字段创建唯一索引
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        // 为 Favourite 表创建复合唯一索引，防止重复收藏
        modelBuilder.Entity<Favourite>()
            .HasIndex(f => new { f.UserId, f.ImageId })
            .IsUnique();

        // 为 Image 表的 OwnerId 创建索引，优化查询性能
        modelBuilder.Entity<Image>()
            .HasIndex(i => i.OwnerId);
    }
}