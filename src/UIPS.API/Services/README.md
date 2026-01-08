# Services 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.API/Services/`

**职责**: 服务层，提供业务逻辑和基础设施服务。包括数据库上下文、文件存储服务等核心功能。

**设计模式**: 
- 依赖注入（Dependency Injection）
- 接口隔离原则（Interface Segregation Principle）
- 仓储模式（Repository Pattern）

---

## 📄 文件清单

| 文件名 | 类型 | 职责 |
|--------|------|------|
| `UipsDbContext.cs` | 类 | EF Core 数据库上下文 |
| `IFileService.cs` | 接口 | 文件存储服务契约 |
| `LocalFileService.cs` | 类 | 本地文件存储实现 |

---

## 🗄️ UipsDbContext - 数据库上下文

### 类定义

```csharp
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
}
```

### 职责
- 管理数据库连接
- 定义实体集合（DbSet）
- 配置实体映射和关系
- 执行数据库迁移

### 实体配置

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // 1. User 表：用户名唯一索引
    modelBuilder.Entity<User>()
        .HasIndex(u => u.UserName)
        .IsUnique();

    // 2. Favourite 表：复合唯一索引（防止重复收藏）
    modelBuilder.Entity<Favourite>()
        .HasIndex(f => new { f.UserId, f.ImageId })
        .IsUnique();

    // 3. Image 表：OwnerId 索引（优化查询性能）
    modelBuilder.Entity<Image>()
        .HasIndex(i => i.OwnerId);
}
```

### 索引配置说明

| 表 | 索引类型 | 字段 | 目的 |
|---|---------|------|------|
| Users | 唯一索引 | UserName | 确保用户名唯一 |
| Favourites | 复合唯一索引 | UserId + ImageId | 防止重复收藏 |
| Images | 普通索引 | OwnerId | 优化按用户查询图片 |

### 依赖注入配置

```csharp
// Program.cs 中的配置
builder.Services.AddDbContext<UipsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 使用示例

```csharp
// 在控制器中通过构造函数注入
public class ImageController(UipsDbContext context) : ControllerBase
{
    // 查询图片
    var images = await context.Images
        .Where(i => i.OwnerId == userId)
        .ToListAsync();

    // 添加图片
    context.Images.Add(newImage);
    await context.SaveChangesAsync();

    // 删除图片
    context.Images.Remove(image);
    await context.SaveChangesAsync();
}
```

---

## 📁 IFileService - 文件存储服务接口

### 接口定义

```csharp
public interface IFileService
{
    /// <summary>
    /// 保存文件到存储系统
    /// </summary>
    /// <param name="fileStream">文件的二进制流</param>
    /// <param name="originalFileName">原始文件名（包含扩展名）</param>
    /// <returns>文件在存储系统中的相对路径</returns>
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName);

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="relativePath">文件在存储系统中的相对路径</param>
    /// <returns>文件流，如果文件不存在则返回 null</returns>
    Task<Stream?> GetFileStreamAsync(string relativePath);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="relativePath">文件在存储系统中的相对路径</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteFileAsync(string relativePath);
}
```

### 设计目的

1. **抽象存储实现**: 通过接口隔离具体的存储方式
2. **支持多种存储**: 可以轻松切换本地存储、云存储（如 Azure Blob、AWS S3）
3. **便于测试**: 可以使用 Mock 对象进行单元测试

### 方法说明

| 方法 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| `SaveFileAsync` | Stream, string | string | 保存文件，返回相对路径 |
| `GetFileStreamAsync` | string | Stream? | 获取文件流，不存在返回 null |
| `DeleteFileAsync` | string | bool | 删除文件，返回是否成功 |

---

## 💾 LocalFileService - 本地文件存储实现

### 类定义

```csharp
public class LocalFileService(IConfiguration configuration) : IFileService
{
    private readonly string _uploadRootPath = configuration["Storage:UploadPath"]
        ?? throw new InvalidOperationException("配置项 'Storage:UploadPath' 未设置");
}
```

### 配置要求

```json
// appsettings.json
{
    "Storage": {
        "UploadPath": "uploads"
    }
}
```

### 文件存储结构

```
uploads/                          # 根目录（可配置）
├── 2025/                         # 年份目录
│   ├── 01/                       # 月份目录
│   │   ├── a1b2c3d4-...-ef12.jpg # GUID 文件名
│   │   ├── b2c3d4e5-...-f123.png
│   │   └── ...
│   ├── 02/
│   │   └── ...
│   └── ...
└── ...
```

### 方法实现详解

#### 1. SaveFileAsync - 保存文件

```csharp
public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName)
{
    // 1. 确保根目录存在
    EnsureDirectoryExists(_uploadRootPath);

    // 2. 生成唯一文件名：GUID + 原始扩展名
    var extension = Path.GetExtension(originalFileName);
    var uniqueFileName = $"{Guid.NewGuid()}{extension}";

    // 3. 构造日期子目录：Year/Month
    var datePath = Path.Combine(
        DateTime.UtcNow.Year.ToString(),
        DateTime.UtcNow.Month.ToString("D2")
    );

    // 4. 创建目标目录
    var targetDirectory = Path.Combine(_uploadRootPath, datePath);
    EnsureDirectoryExists(targetDirectory);

    // 5. 写入文件
    var fullPath = Path.Combine(targetDirectory, uniqueFileName);
    await using var fileStreamWriter = new FileStream(fullPath, FileMode.Create);
    fileStream.Seek(0, SeekOrigin.Begin);
    await fileStream.CopyToAsync(fileStreamWriter);

    // 6. 返回相对路径（使用正斜杠）
    return Path.Combine(datePath, uniqueFileName).Replace('\\', '/');
}
```

**设计亮点**:
- 使用 GUID 避免文件名冲突
- 按日期组织目录，便于管理
- 返回相对路径，便于数据库存储和迁移
- 使用正斜杠，保持跨平台兼容性

#### 2. GetFileStreamAsync - 获取文件流

```csharp
public Task<Stream?> GetFileStreamAsync(string relativePath)
{
    var fullPath = Path.Combine(_uploadRootPath, relativePath);

    if (!File.Exists(fullPath))
    {
        return Task.FromResult<Stream?>(null);
    }

    var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    return Task.FromResult<Stream?>(stream);
}
```

**注意事项**:
- 调用者负责释放 Stream 资源
- 使用 `FileShare.Read` 允许并发读取

#### 3. DeleteFileAsync - 删除文件

```csharp
public Task<bool> DeleteFileAsync(string relativePath)
{
    if (string.IsNullOrWhiteSpace(relativePath))
        return Task.FromResult(false);

    var fullPath = Path.Combine(_uploadRootPath, relativePath);

    // 幂等性：文件不存在视为删除成功
    if (!File.Exists(fullPath))
        return Task.FromResult(true);

    try
    {
        File.Delete(fullPath);
        return Task.FromResult(true);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[错误] 删除文件失败: {fullPath}, 原因: {ex.Message}");
        return Task.FromResult(false);
    }
}
```

**设计亮点**:
- 幂等性设计：文件不存在也返回成功
- 异常处理：捕获删除失败的情况
- 日志记录：记录删除失败的原因

### 依赖注入配置

```csharp
// Program.cs 中的配置
builder.Services.AddScoped<IFileService, LocalFileService>();
```

---

## 🏗️ 架构设计

### 依赖关系图

```
┌─────────────────────────────────────────────────────────┐
│                     Controllers                          │
│  (AuthController, ImageController, AdminController)      │
└─────────────────────────┬───────────────────────────────┘
                          │
                          │ 依赖注入
                          ▼
┌─────────────────────────────────────────────────────────┐
│                      Services                            │
│  ┌─────────────────┐    ┌─────────────────────────────┐ │
│  │  UipsDbContext  │    │       IFileService          │ │
│  │                 │    │            │                │ │
│  │  - Users        │    │            ▼                │ │
│  │  - Images       │    │    LocalFileService         │ │
│  │  - Favourites   │    │    (或其他实现)              │ │
│  └─────────────────┘    └─────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                   Infrastructure                         │
│  ┌─────────────────┐    ┌─────────────────────────────┐ │
│  │    SQLite DB    │    │      File System            │ │
│  │   (uips.db)     │    │    (uploads/)               │ │
│  └─────────────────┘    └─────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 扩展性设计

#### 切换到云存储

```csharp
// 1. 创建新的实现类
public class AzureBlobFileService : IFileService
{
    // 实现 Azure Blob Storage 的文件操作
}

// 2. 修改依赖注入配置
builder.Services.AddScoped<IFileService, AzureBlobFileService>();
```

#### 切换到其他数据库

```csharp
// 修改 DbContext 配置
builder.Services.AddDbContext<UipsDbContext>(options =>
    options.UseSqlServer(connectionString));  // 切换到 SQL Server
```

---

## 📊 服务统计

| 服务 | 类型 | 生命周期 | 职责 |
|------|------|----------|------|
| UipsDbContext | 类 | Scoped | 数据库操作 |
| IFileService | 接口 | - | 文件存储契约 |
| LocalFileService | 类 | Scoped | 本地文件存储 |

---

## 📝 开发规范

### 1. 接口优先
- 所有服务都应定义接口
- 通过接口进行依赖注入

### 2. 异步编程
- 所有 I/O 操作使用 `async/await`
- 方法名以 `Async` 结尾

### 3. 资源管理
- 使用 `await using` 确保资源释放
- 调用者负责释放返回的 Stream

### 4. 配置外部化
- 路径、连接字符串等配置放在 `appsettings.json`
- 使用 `IConfiguration` 读取配置

### 5. 错误处理
- 捕获并记录异常
- 返回有意义的错误信息

---

## 🔗 相关模块

- **Models**: 数据库实体模型
- **Controllers**: 使用服务进行业务操作
- **Program.cs**: 服务注册和配置
