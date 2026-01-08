# Services 模块说明文档（前端）

## 📁 模块概述

**路径**: `src/UIPS.Client/Services/`

**职责**: 前端服务层，负责与后端 API 通信、用户会话管理和 HTTP 请求拦截。使用 Refit 库实现类型安全的 HTTP 客户端。

**核心技术**: 
- Refit（声明式 HTTP 客户端）
- 依赖注入（Microsoft.Extensions.DependencyInjection）

---

## 📄 文件清单

| 文件名 | 类型 | 职责 |
|--------|------|------|
| `IAuthApi.cs` | 接口 | 认证 API 客户端 |
| `IImageApi.cs` | 接口 | 图片 API 客户端 |
| `IAdminApi.cs` | 接口 | 管理员 API 客户端 |
| `UserSession.cs` | 类 | 用户会话管理 |
| `AuthHeaderHandler.cs` | 类 | JWT Token 拦截器 |

---

## 🔐 IAuthApi - 认证 API 接口

### 接口定义

```csharp
public interface IAuthApi
{
    /// <summary>
    /// 用户登录
    /// </summary>
    [Post("/api/Auth/login")]
    Task<dynamic> LoginAsync([Body] object request);

    /// <summary>
    /// 用户注册
    /// </summary>
    [Post("/api/Auth/register")]
    Task<dynamic> RegisterAsync([Body] object request);
}
```

### 使用示例

```csharp
// 登录
var payload = new { UserName = "admin", Password = "123456" };
dynamic response = await _authApi.LoginAsync(payload);

// 解析响应
var json = (JsonElement)response;
string token = json.GetProperty("accessToken").GetString();
```

### 特点
- 不需要 JWT Token（公开接口）
- 使用 `dynamic` 返回类型，灵活处理响应

---

## 🖼️ IImageApi - 图片 API 接口

### 接口定义

```csharp
public interface IImageApi
{
    /// <summary>
    /// 上传单张图片
    /// </summary>
    [Multipart]
    [Post("/api/images/upload")]
    Task<dynamic> UploadImage([AliasAs("file")] StreamPart file);

    /// <summary>
    /// 批量上传图片
    /// </summary>
    [Multipart]
    [Post("/api/images/upload/batch")]
    Task<dynamic> UploadBatch([AliasAs("files")] IEnumerable<StreamPart> files);

    /// <summary>
    /// 获取图片列表（分页）
    /// </summary>
    [Get("/api/images")]
    Task<dynamic> GetImages([Query] int pageIndex, [Query] int pageSize);

    /// <summary>
    /// 获取唯一文件名列表
    /// </summary>
    [Get("/api/images/filenames")]
    Task<dynamic> GetUniqueFileNames();

    /// <summary>
    /// 根据文件名获取图片
    /// </summary>
    [Get("/api/images/by-filename/{fileName}")]
    Task<dynamic> GetImagesByFileName(string fileName);

    /// <summary>
    /// 删除图片
    /// </summary>
    [Delete("/api/images/{id}")]
    Task DeleteImage(long id);

    /// <summary>
    /// 切换收藏状态
    /// </summary>
    [Post("/api/images/{id}/select")]
    Task ToggleSelection(long id);
}
```

### 文件上传示例

```csharp
// 单张上传
using var stream = File.OpenRead(filePath);
var fileName = Path.GetFileName(filePath);
var filePart = new StreamPart(stream, fileName, "image/jpeg");
var response = await _imageApi.UploadImage(filePart);

// 批量上传
var files = new List<StreamPart>();
foreach (var path in filePaths)
{
    var stream = File.OpenRead(path);
    files.Add(new StreamPart(stream, Path.GetFileName(path), "image/jpeg"));
}
await _imageApi.UploadBatch(files);
```

### 特点
- 需要 JWT Token（通过 AuthHeaderHandler 自动添加）
- 支持 Multipart 文件上传
- 使用 `[AliasAs]` 指定参数名

---

## 👑 IAdminApi - 管理员 API 接口

### 接口定义

```csharp
public interface IAdminApi
{
    /// <summary>
    /// 获取用户列表（分页）
    /// </summary>
    [Get("/api/admin/users")]
    Task<dynamic> GetUsersAsync([Query] int pageIndex, [Query] int pageSize);

    /// <summary>
    /// 获取系统统计信息
    /// </summary>
    [Get("/api/admin/statistics")]
    Task<dynamic> GetStatisticsAsync();

    /// <summary>
    /// 更新用户角色
    /// </summary>
    [Put("/api/admin/users/{userId}/role")]
    Task UpdateUserRoleAsync(int userId, [Body] object dto);

    /// <summary>
    /// 删除用户
    /// </summary>
    [Delete("/api/admin/users/{userId}")]
    Task DeleteUserAsync(int userId);

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [Post("/api/admin/users/{userId}/reset-password")]
    Task ResetUserPasswordAsync(int userId, [Body] object dto);

    /// <summary>
    /// 获取所有图片（管理员视图）
    /// </summary>
    [Get("/api/admin/images")]
    Task<dynamic> GetAllImagesAsync([Query] int pageIndex, [Query] int pageSize);

    /// <summary>
    /// 批量删除图片
    /// </summary>
    [Post("/api/admin/images/batch-delete")]
    Task BatchDeleteImagesAsync([Body] object dto);
}
```

### 使用示例

```csharp
// 获取统计信息
var stats = await _adminApi.GetStatisticsAsync();
var json = (JsonElement)stats;
int totalUsers = json.GetProperty("totalUsers").GetInt32();

// 更新用户角色
await _adminApi.UpdateUserRoleAsync(userId, new { Role = "Admin" });

// 批量删除图片
await _adminApi.BatchDeleteImagesAsync(new { ImageIds = new[] { 1, 2, 3 } });
```

### 特点
- 需要 Admin 角色的 JWT Token
- 所有接口都需要认证

---

## 👤 UserSession - 用户会话管理

### 类定义

```csharp
public class UserSession
{
    // 存储的数据
    public string? AccessToken { get; private set; }
    public string? UserName { get; private set; }
    public long UserId { get; private set; }
    public string Role { get; private set; } = "User";
    
    // 计算属性
    public bool IsAdmin => Role == "Admin";
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    // 设置会话
    public void SetSession(string token, string userName, long userId, string role);

    // 清除会话
    public void ClearSession();
}
```

### 职责
- 存储当前登录用户的信息
- 提供 JWT Token 给 AuthHeaderHandler
- 判断用户角色和登录状态

### 生命周期
- **Singleton**: 整个应用程序共享一个实例
- 通过依赖注入获取，不使用静态属性

### 使用示例

```csharp
// 登录成功后设置会话
_userSession.SetSession(token, userName, userId, role);

// 检查是否为管理员
if (_userSession.IsAdmin)
{
    // 显示管理员功能
}

// 注销时清除会话
_userSession.ClearSession();
```

---

## 🔑 AuthHeaderHandler - JWT Token 拦截器

### 类定义

```csharp
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly UserSession _userSession;

    public AuthHeaderHandler(UserSession userSession)
    {
        _userSession = userSession;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var token = _userSession.AccessToken;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 职责
- 拦截所有 HTTP 请求
- 自动添加 `Authorization: Bearer {token}` 请求头
- 从 UserSession 获取当前 Token

### 工作流程

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│  ViewModel  │────>│ AuthHeaderHandler │────>│  后端 API   │
│             │     │                  │     │             │
│ 调用 API    │     │ 添加 JWT Token   │     │ 验证 Token  │
└─────────────┘     └──────────────────┘     └─────────────┘
```

---

## ⚙️ 依赖注入配置

### App.xaml.cs 中的配置

```csharp
// 1. 注册 AuthHeaderHandler
services.AddTransient<AuthHeaderHandler>();

// 2. 注册 UserSession（单例）
services.AddSingleton<UserSession>();

// 3. 定义 API 地址
var apiUrl = "https://localhost:7149";

// 4. SSL 证书忽略（开发环境）
Func<HttpMessageHandler> getDevSslHandler = () => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true
};

// 5. 注册 Auth API（不需要 Token）
services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl))
    .ConfigurePrimaryHttpMessageHandler(getDevSslHandler);

// 6. 注册 Image API（需要 Token）
services.AddRefitClient<IImageApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
    });

// 7. 注册 Admin API（需要 Token）
services.AddRefitClient<IAdminApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
    });
```

### 配置说明

| API | Token 要求 | Handler 配置 |
|-----|-----------|-------------|
| IAuthApi | 不需要 | 仅 SSL 忽略 |
| IImageApi | 需要 | AuthHeaderHandler + SSL |
| IAdminApi | 需要 | AuthHeaderHandler + SSL |

---

## 🏗️ 架构设计

### 服务依赖关系

```
┌─────────────────────────────────────────────────────────┐
│                     ViewModels                           │
│  (LoginViewModel, DashboardViewModel, AdminViewModel)    │
└─────────────────────────┬───────────────────────────────┘
                          │
                          │ 依赖注入
                          ▼
┌─────────────────────────────────────────────────────────┐
│                      Services                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │  IAuthApi   │  │  IImageApi  │  │   IAdminApi     │  │
│  └──────┬──────┘  └──────┬──────┘  └────────┬────────┘  │
│         │                │                   │           │
│         │                ▼                   │           │
│         │    ┌──────────────────────┐        │           │
│         │    │  AuthHeaderHandler   │◄───────┘           │
│         │    └──────────┬───────────┘                    │
│         │               │                                │
│         │               ▼                                │
│         │    ┌──────────────────────┐                    │
│         └───>│     UserSession      │◄───────────────────┘
│              └──────────────────────┘                    │
└─────────────────────────────────────────────────────────┘
                          │
                          │ HTTP 请求
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    后端 API                              │
│              https://localhost:7149                      │
└─────────────────────────────────────────────────────────┘
```

---

## 📊 服务统计

| 服务 | 类型 | 生命周期 | 需要 Token |
|------|------|----------|-----------|
| IAuthApi | 接口 | Scoped | ❌ |
| IImageApi | 接口 | Scoped | ✅ |
| IAdminApi | 接口 | Scoped | ✅ |
| UserSession | 类 | Singleton | - |
| AuthHeaderHandler | 类 | Transient | - |

---

## 📝 开发规范

### 1. 接口命名
- API 接口以 `I` 开头，以 `Api` 结尾
- 方法名以 `Async` 结尾

### 2. 返回类型
- 使用 `dynamic` 处理灵活的 JSON 响应
- 使用 `JsonElement` 解析响应数据

### 3. 错误处理
- 捕获 `ApiException` 处理 HTTP 错误
- 捕获 `RuntimeBinderException` 处理 JSON 解析错误

### 4. Token 管理
- 登录成功后立即设置 Token
- 注销时清除 Token
- 通过 Handler 自动添加 Token

---

## 🔗 相关模块

- **ViewModels**: 使用 API 服务进行数据操作
- **App.xaml.cs**: 服务注册和配置
- **Views**: 通过 ViewModel 间接使用服务
