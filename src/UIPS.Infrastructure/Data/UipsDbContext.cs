using Microsoft.EntityFrameworkCore;
using UIPS.Domain.Entities; // 引用刚才定义的实体

namespace UIPS.Infrastructure.Data;

// 继承自 DbContext，这是 EF Core 的核心
public class UipsDbContext(DbContextOptions<UipsDbContext> options) : DbContext(options)
{
    // 告诉数据库我们需要一张 Users 表
    public DbSet<User> Users { get; set; }

    // 图片表
    public DbSet<Image> Images { get; set; }

    // 选择记录表
    public DbSet<Selection> Selections { get; set; }
}