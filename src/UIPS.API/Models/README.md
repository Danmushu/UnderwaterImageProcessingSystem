# Models 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.API/Models/`

**职责**: 数据库实体模型层，定义与数据库表一一对应的实体类。使用 Entity Framework Core 的 Code First 模式，通过实体类自动生成数据库结构。

**ORM 框架**: Entity Framework Core 8.0

---

## 📄 文件清单

| 文件名 | 对应数据库表 | 职责 |
|--------|-------------|------|
| `User.cs` | Users | 用户实体，存储用户认证信息 |
| `Image.cs` | Images | 图片实体，存储图片元数据 |
| `Favourite.cs` | Favourites | 收藏实体，记录用户与图片的收藏关系 |

---

## 👤 User - 用户实体

### 实体定义

```csharp
public class User
{
    /// <summary>
    /// 用户 ID（主键，自增）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名（唯一标识，用于登录）
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// 密码哈希值（使用 BCrypt 加密存储，永不存储明文密码）
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// 用户角色（如 "User"、"Admin"）
    /// 用于权限控制和授权
    /// </summary>
    public string Role { get; set; } = "User";
}
```

### 数据库表结构

| 列名 | 数据类型 | 约束 | 说明 |
|------|----------|------|------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | 主键 |
| UserName | TEXT | NOT NULL, UNIQUE | 用户名（唯一索引） |
| PasswordHash | TEXT | NOT NULL | BCrypt 哈希密码 |
| Role | TEXT | NOT NULL, DEFAULT 'User' | 用户角色 |

### 索引配置

```csharp
// 在 UipsDbContext.OnModelCreating 中配置
modelBuilder.Entity<User>()
    .HasIndex(u => u.UserName)
    .IsUnique();
```

### 安全说明

⚠️ **密码安全**:
- 使用 BCrypt 算法进行哈希加盐
- 永不存储明文密码
- 每次哈希都会生成不同的盐值

### 角色说明

| 角色 | 权限 |
|------|------|
| `User` | 普通用户，可上传/查看/删除自己的图片 |
| `Admin` | 管理员，可管理所有用户和图片 |

---

## 🖼️ Image - 图片实体

### 实体定义

```csharp
public class Image
{
    /// <summary>
    /// 图片 ID（主键，自增）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 原始文件名（用户上传时的文件名，如 "sea_turtle.jpg"）
    /// </summary>
    public required string OriginalFileName { get; set; }

    /// <summary>
    /// 存储路径（相对路径，如 "2025/01/guid.jpg"）
    /// 通过相对路径索引，避免数据库存储完整路径导致的膨胀
    /// </summary>
    public required string StoredPath { get; set; }

    /// <summary>
    /// 上传时间（UTC）
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 所有者用户 ID（外键，关联到 User 表）
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 所有者导航属性（EF Core 导航属性，用于关联查询）
    /// </summary>
    public User? Owner { get; set; }
}
```

### 数据库表结构

| 列名 | 数据类型 | 约束 | 说明 |
|------|----------|------|------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | 主键 |
| OriginalFileName | TEXT | NOT NULL | 原始文件名 |
| StoredPath | TEXT | NOT NULL | 存储相对路径 |
| UploadedAt | DATETIME | NOT NULL | 上传时间（UTC） |
| FileSize | INTEGER | NOT NULL | 文件大小（字节） |
| OwnerId | INTEGER | NOT NULL, FOREIGN KEY | 所有者 ID |

### 索引配置

```csharp
// 为 OwnerId 创建索引，优化按用户查询图片的性能
modelBuilder.Entity<Image>()
    .HasIndex(i => i.OwnerId);
```

### 关联关系

```
User (1) ──────< Image (N)
  │                │
  │                │
  └── OwnerId ─────┘
```

- **一对多关系**: 一个用户可以拥有多张图片
- **导航属性**: `Image.Owner` 可以直接访问图片所有者

### 存储路径设计

```
存储路径格式: {Year}/{Month}/{GUID}.{Extension}
示例: 2025/01/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg

优点:
1. 避免文件名冲突（使用 GUID）
2. 按日期组织，便于管理
3. 相对路径，便于迁移
```

---

## ⭐ Favourite - 收藏实体

### 实体定义

```csharp
public class Favourite
{
    /// <summary>
    /// 收藏记录 ID（主键，自增）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户 ID（外键，关联到 User 表）
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 图片 ID（外键，关联到 Image 表）
    /// </summary>
    public int ImageId { get; set; }

    /// <summary>
    /// 收藏时间
    /// </summary>
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}
```

### 数据库表结构

| 列名 | 数据类型 | 约束 | 说明 |
|------|----------|------|------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | 主键 |
| UserId | INTEGER | NOT NULL, FOREIGN KEY | 用户 ID |
| ImageId | INTEGER | NOT NULL, FOREIGN KEY | 图片 ID |
| SelectedAt | DATETIME | NOT NULL | 收藏时间 |

### 索引配置

```csharp
// 复合唯一索引，防止重复收藏
modelBuilder.Entity<Favourite>()
    .HasIndex(f => new { f.UserId, f.ImageId })
    .IsUnique();
```

### 关联关系

```
User (1) ──────< Favourite (N) >────── Image (1)
  │                  │                    │
  │                  │                    │
  └── UserId ────────┴── ImageId ─────────┘
```

- **多对多关系**: 通过 Favourite 表实现用户与图片的多对多关系
- **中间表**: Favourite 是 User 和 Image 的关联表

### 业务逻辑

| 操作 | 说明 |
|------|------|
| 收藏图片 | 在 Favourite 表中插入一条记录 |
| 取消收藏 | 从 Favourite 表中删除对应记录 |
| 查询收藏 | 根据 UserId 查询所有收藏的 ImageId |

---

## 🗄️ 数据库关系图

```
┌─────────────────┐
│      User       │
├─────────────────┤
│ Id (PK)         │
│ UserName (UQ)   │
│ PasswordHash    │
│ Role            │
└────────┬────────┘
         │
         │ 1:N
         │
         ▼
┌─────────────────┐       ┌─────────────────┐
│     Image       │       │    Favourite    │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │◄──────│ ImageId (FK)    │
│ OriginalFileName│       │ UserId (FK)     │──────┐
│ StoredPath      │       │ Id (PK)         │      │
│ UploadedAt      │       │ SelectedAt      │      │
│ FileSize        │       └─────────────────┘      │
│ OwnerId (FK)    │──────────────────────────────┐ │
└─────────────────┘                              │ │
                                                 │ │
                                                 ▼ ▼
                                          ┌─────────────────┐
                                          │      User       │
                                          └─────────────────┘
```

---

## 🏗️ EF Core 配置

### DbContext 配置

```csharp
public class UipsDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Favourite> Favourites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User 表：用户名唯一索引
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        // Favourite 表：复合唯一索引
        modelBuilder.Entity<Favourite>()
            .HasIndex(f => new { f.UserId, f.ImageId })
            .IsUnique();

        // Image 表：OwnerId 索引
        modelBuilder.Entity<Image>()
            .HasIndex(i => i.OwnerId);
    }
}
```

### 数据库迁移

```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

---

## 📊 实体统计

| 实体 | 字段数 | 索引数 | 外键数 |
|------|--------|--------|--------|
| User | 4 | 1 (UserName) | 0 |
| Image | 6 | 1 (OwnerId) | 1 |
| Favourite | 4 | 1 (复合) | 2 |
| **总计** | **14** | **3** | **3** |

---

## 📝 设计规范

### 1. 命名规范
- 实体类名：单数形式（User, Image）
- 表名：复数形式（Users, Images）
- 外键：`{关联实体}Id`（OwnerId, UserId）

### 2. 主键设计
- 使用自增整数作为主键
- 属性名统一为 `Id`

### 3. 时间字段
- 统一使用 UTC 时间
- 默认值使用 `DateTime.UtcNow`

### 4. 必填字段
- 使用 `required` 关键字标记必填属性
- 确保数据完整性

### 5. 导航属性
- 可空类型（`User?`）
- 用于 EF Core 的关联查询

---

## 🔗 相关模块

- **Services/UipsDbContext.cs**: 数据库上下文，配置实体映射
- **DTOs**: 数据传输对象，用于 API 响应
- **Controllers**: 控制器，使用实体进行数据操作
