# DTOs 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.API/DTOs/`

**职责**: 数据传输对象（Data Transfer Objects），用于在前后端之间传输数据。DTO 是纯数据容器，不包含业务逻辑，确保 API 接口的数据结构清晰、安全。

**设计模式**: DTO 模式（Data Transfer Object Pattern）

---

## 📄 文件清单

| 文件名 | 用途 | 使用场景 |
|--------|------|----------|
| `LoginRequestDto.cs` | 登录/注册请求 | 用户认证 |
| `LoginResponseDto.cs` | 登录响应 | 返回 Token 和用户信息 |
| `ImageDto.cs` | 图片信息 | 图片列表展示 |
| `ImageResponseDto.cs` | 图片上传响应 | 上传成功后返回 |
| `PaginatedRequestDto.cs` | 分页请求参数 | 通用分页查询 |
| `PaginatedResult.cs` | 分页结果 | 通用分页响应 |
| `UserDto.cs` | 用户信息 | 管理员查看用户 |
| `UpdateRoleDto.cs` | 更新角色请求 | 管理员修改用户角色 |
| `ResetPasswordDto.cs` | 重置密码请求 | 管理员重置密码 |
| `AdminStatisticsDto.cs` | 系统统计信息 | 管理员面板统计 |
| `AdminImageDto.cs` | 管理员图片视图 | 管理员查看所有图片 |
| `BatchDeleteDto.cs` | 批量删除请求 | 管理员批量删除图片 |

---

## 🔐 认证相关 DTO

### LoginRequestDto - 登录/注册请求

```csharp
public class LoginRequestDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// 密码（明文，仅在传输层使用，服务端会立即进行哈希处理）
    /// </summary>
    public required string Password { get; set; }
}
```

**使用场景**:
- `POST /api/Auth/login` - 用户登录
- `POST /api/Auth/register` - 用户注册

**安全说明**: 密码在传输层为明文，但服务端会立即使用 BCrypt 进行哈希处理，永不存储明文密码。

---

### LoginResponseDto - 登录响应

```csharp
public class LoginResponseDto
{
    public required string AccessToken { get; set; }    // JWT 访问令牌
    public required string RefreshToken { get; set; }   // 刷新令牌（TODO）
    public required int UserId { get; set; }            // 用户 ID
    public required string UserName { get; set; }       // 用户名
    public int ExpiresIn { get; set; }                  // 令牌有效期（秒）
    public string Role { get; set; } = "User";          // 用户角色
}
```

**使用场景**: 登录成功后返回给前端

**字段说明**:
| 字段 | 类型 | 说明 |
|------|------|------|
| `AccessToken` | string | JWT Token，用于后续 API 请求认证 |
| `RefreshToken` | string | 刷新令牌（当前未实现） |
| `UserId` | int | 用户唯一标识 |
| `UserName` | string | 用户名 |
| `ExpiresIn` | int | Token 有效期（秒），默认 7200 |
| `Role` | string | 用户角色（"User" 或 "Admin"） |

---

## 🖼️ 图片相关 DTO

### ImageDto - 图片信息

```csharp
public class ImageDto
{
    public long Id { get; set; }                        // 图片 ID
    public string OriginalFileName { get; set; }        // 原始文件名
    public string PreviewUrl { get; set; }              // 预览 URL
    public DateTime UploadedAt { get; set; }            // 上传时间
    public long FileSize { get; set; }                  // 文件大小（字节）
    public bool IsSelected { get; set; }                // 是否已收藏
}
```

**使用场景**: 
- `GET /api/images` - 获取图片列表
- `GET /api/images/by-filename/{fileName}` - 按文件名查询

**特殊字段**:
- `PreviewUrl`: 格式为 `/api/images/{id}/file`，前端可直接用于 `<img>` 标签
- `IsSelected`: 标记当前用户是否已收藏此图片

---

### ImageResponseDto - 图片上传响应

```csharp
public class ImageResponseDto
{
    public int Id { get; set; }                         // 图片 ID
    public required string OriginalFileName { get; set; } // 原始文件名
    public DateTime UploadedAt { get; set; }            // 上传时间
    public long FileSize { get; set; }                  // 文件大小
    public required string Url { get; set; }            // 访问 URL
}
```

**使用场景**: 
- `POST /api/images/upload` - 单张上传成功后返回
- `POST /api/images/upload/batch` - 批量上传成功后返回列表

---

### AdminImageDto - 管理员图片视图

```csharp
public class AdminImageDto
{
    public int Id { get; set; }                         // 图片 ID
    public required string OriginalFileName { get; set; } // 原始文件名
    public DateTime UploadedAt { get; set; }            // 上传时间
    public long FileSize { get; set; }                  // 文件大小
    public required string OwnerName { get; set; }      // 所有者用户名
    public int OwnerId { get; set; }                    // 所有者 ID
    public required string PreviewUrl { get; set; }     // 预览 URL
}
```

**使用场景**: `GET /api/admin/images` - 管理员查看所有用户的图片

**与 ImageDto 的区别**: 包含 `OwnerName` 和 `OwnerId`，用于显示图片所有者信息。

---

## 📄 分页相关 DTO

### PaginatedRequestDto - 分页请求参数

```csharp
public class PaginatedRequestDto
{
    /// <summary>
    /// 页码（从 1 开始）
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页数据量
    /// </summary>
    public int PageSize { get; set; } = 10;
}
```

**使用场景**: 所有需要分页的 GET 请求

**默认值**:
- `PageIndex`: 1（第一页）
- `PageSize`: 10（每页 10 条）

---

### PaginatedResult<T> - 分页结果（泛型）

```csharp
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();         // 当前页数据
    public int TotalCount { get; set; }                 // 总数据量
    public int PageIndex { get; set; }                  // 当前页码
    public int PageSize { get; set; }                   // 每页数量
    public int TotalPages => /* 计算属性 */;            // 总页数
}
```

**使用场景**: 所有分页查询的响应

**计算属性**:
```csharp
public int TotalPages => PageSize > 0 
    ? (int)Math.Ceiling((double)TotalCount / PageSize) 
    : 0;
```

**泛型实例化示例**:
- `PaginatedResult<ImageDto>` - 图片分页
- `PaginatedResult<UserDto>` - 用户分页
- `PaginatedResult<AdminImageDto>` - 管理员图片分页

---

## 👑 管理员相关 DTO

### UserDto - 用户信息

```csharp
public class UserDto
{
    public int Id { get; set; }                         // 用户 ID
    public required string UserName { get; set; }       // 用户名
    public string Role { get; set; } = "User";          // 用户角色
}
```

**使用场景**: `GET /api/admin/users` - 管理员查看用户列表

**安全说明**: 不包含 `PasswordHash`，保护敏感信息。

---

### UpdateRoleDto - 更新角色请求

```csharp
public class UpdateRoleDto
{
    /// <summary>
    /// 新角色（"User" 或 "Admin"）
    /// </summary>
    public required string Role { get; set; }
}
```

**使用场景**: `PUT /api/admin/users/{userId}/role`

**验证规则**: 角色值只能是 "User" 或 "Admin"。

---

### ResetPasswordDto - 重置密码请求

```csharp
public class ResetPasswordDto
{
    /// <summary>
    /// 新密码
    /// </summary>
    public required string NewPassword { get; set; }
}
```

**使用场景**: `POST /api/admin/users/{userId}/reset-password`

---

### AdminStatisticsDto - 系统统计信息

```csharp
public class AdminStatisticsDto
{
    public int TotalUsers { get; set; }                 // 总用户数
    public int TotalAdmins { get; set; }                // 管理员数量
    public int TotalImages { get; set; }                // 总图片数
    public int TotalFavourites { get; set; }            // 总收藏数
}
```

**使用场景**: `GET /api/admin/statistics`

---

### BatchDeleteDto - 批量删除请求

```csharp
public class BatchDeleteDto
{
    /// <summary>
    /// 要删除的图片 ID 列表
    /// </summary>
    public required List<int> ImageIds { get; set; }
}
```

**使用场景**: `POST /api/admin/images/batch-delete`

---

## 🏗️ 设计原则

### 1. 单一职责
每个 DTO 只服务于一个特定的场景，避免"万能 DTO"。

### 2. 数据安全
- 不暴露敏感信息（如密码哈希）
- 只传输必要的数据

### 3. 类型安全
- 使用 `required` 关键字标记必填字段
- 使用强类型而非 `dynamic`

### 4. 文档完善
- 所有属性都有 XML 文档注释
- 说明字段用途和格式

---

## 📊 DTO 分类统计

| 分类 | 数量 | DTO 列表 |
|------|------|----------|
| 认证相关 | 2 | LoginRequestDto, LoginResponseDto |
| 图片相关 | 3 | ImageDto, ImageResponseDto, AdminImageDto |
| 分页相关 | 2 | PaginatedRequestDto, PaginatedResult<T> |
| 管理员相关 | 5 | UserDto, UpdateRoleDto, ResetPasswordDto, AdminStatisticsDto, BatchDeleteDto |
| **总计** | **12** | - |

---

## 🔗 DTO 与 API 对应关系

| DTO | 请求/响应 | 对应 API |
|-----|----------|----------|
| LoginRequestDto | 请求 | POST /api/Auth/login, register |
| LoginResponseDto | 响应 | POST /api/Auth/login |
| ImageDto | 响应 | GET /api/images |
| ImageResponseDto | 响应 | POST /api/images/upload |
| PaginatedRequestDto | 请求 | 所有分页 GET 接口 |
| PaginatedResult<T> | 响应 | 所有分页 GET 接口 |
| UserDto | 响应 | GET /api/admin/users |
| UpdateRoleDto | 请求 | PUT /api/admin/users/{id}/role |
| ResetPasswordDto | 请求 | POST /api/admin/users/{id}/reset-password |
| AdminStatisticsDto | 响应 | GET /api/admin/statistics |
| AdminImageDto | 响应 | GET /api/admin/images |
| BatchDeleteDto | 请求 | POST /api/admin/images/batch-delete |

---

## 📝 命名规范

1. **请求 DTO**: 以 `Dto` 结尾，如 `LoginRequestDto`
2. **响应 DTO**: 以 `Dto` 或 `ResponseDto` 结尾
3. **泛型 DTO**: 使用 `<T>` 参数化，如 `PaginatedResult<T>`
4. **属性命名**: PascalCase，与 JSON 序列化时的 camelCase 自动转换
