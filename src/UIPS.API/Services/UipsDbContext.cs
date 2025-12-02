using Microsoft.EntityFrameworkCore;
using UIPS.API.Models;

namespace UIPS.API.Services
{
    public class UipsDbContext(DbContextOptions<UipsDbContext> options) : DbContext(options)
    {
        // 告诉数据库我们需要一张 Users 表
        public DbSet<User> Users { get; set; }

        // 图片表
        public DbSet<Image> Images { get; set; }

        // 选择记录表
        public DbSet<Favourite> Favourites { get; set; }
    }
}
