# Controllers 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.API/Controllers/`

**职责**: 控制器层，负责处理 HTTP 请求、路由分发、参数验证和响应返回。是 ASP.NET Core Web API 的核心组件，遵循 RESTful API 设计规范。

**设计模式**: MVC 模式中的 Controller 层

---

## 📄 文件清单

| 文件名 | 职责 | 路由前缀 | 认证要求 |
|--------|------|----------|----------|
| `AuthController.cs` | 用户认证（登录/注册） | `/api/Auth` | 无需认证 |
| `ImageController.cs` | 图片管理（上传/查看/删除/收藏） | `/api/images` | JWT 认证 |
| `AdminController.cs` | 管理员功能（用户管理/统计） | `/api/admin` | Admin 角色 |

---

## 🔐 AuthController - 认证控制器

### 功能描述
处理用户身份认证相关的所有操作，包括用户注册和登录。

### API 接口

#### 1. 用户注册
```http
POST /api/Auth/register
Content-Type: application/json

{
    "userName": "string",
    "password": "string"
}
```

**响应**:
- `200 OK`: 注册成功
- `400 Bad Request`: 用户名已存在

**安全措施**:
- 使用 BCrypt 对密码进行哈希加盐存储
- 永不存储明文密码

#### 2. 用户登录
```http
POST /api/Auth/login
Content-Type: application/json

{
    "userName": "string",
    "password": "string"
}
```

**响应**:
- `200 OK`: 返回 JWT Token 和用户信息
- `401 Unauthorized`: 用户名或密码错误

**返回数据结构**:
```json
{
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "待实现...",
    "userId": 1,
    "userName": "admin",
    "expiresIn": 7200,
    "role": "Admin"
}
```

### 核心方法

| 方法名 | 访问修饰符 | 功能 |
|--------|-----------|------|
| `Register()` | public | 处理用户注册请求 |
| `Login()` | public | 处理用户登录请求 |
| `GenerateJwtToken()` | private | 生成 JWT 访问令牌 |

### JWT Token 生成逻辑
```csharp
// Token 包含的 Claims:
// - ClaimTypes.NameIdentifier: 用户 ID
// - ClaimTypes.Name: 用户名
// - ClaimTypes.Role: 用户角色

// Token 有效期: 120 分钟 (可配置)
```

---

## 🖼️ ImageController - 图片控制器

### 功能描述
处理图片的完整生命周期管理，包括上传、查询、预览、删除和收藏功能。

### API 接口

#### 上传相关

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 单张上传 | POST | `/api/images/upload` | 上传单张图片 |
| 批量上传 | POST | `/api/images/upload/batch` | 批量上传多张图片 |

#### 查询相关

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 图片列表 | GET | `/api/images` | 获取当前用户的图片列表（分页） |
| 文件名列表 | GET | `/api/images/filenames` | 获取唯一文件名列表 |
| 按文件名查询 | GET | `/api/images/by-filename/{fileName}` | 根据文件名获取图片 |

#### 文件访问

| 接口 | 方法 | 路径 | 描述 | 认证 |
|------|------|------|------|------|
| 公开预览 | GET | `/api/images/view/{id}` | 查看图片（公开） | 无需 |
| 私有访问 | GET | `/api/images/{id}/file` | 获取图片文件 | 需要 |

#### 操作相关

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 删除图片 | DELETE | `/api/images/{id}` | 删除图片（含物理文件） |
| 切换收藏 | POST | `/api/images/{id}/select` | 收藏/取消收藏图片 |

### 核心方法

| 方法名 | 功能 | 返回类型 |
|--------|------|----------|
| `UploadImage()` | 上传单张图片 | `ImageResponseDto` |
| `UploadBatch()` | 批量上传图片 | `List<ImageResponseDto>` |
| `GetImages()` | 获取图片列表（分页） | `PaginatedResult<ImageDto>` |
| `GetUniqueFileNames()` | 获取唯一文件名 | `List<string>` |
| `GetImagesByFileName()` | 按文件名查询 | `List<ImageDto>` |
| `ViewImage()` | 公开预览图片 | `FileStreamResult` |
| `GetImageFile()` | 私有访问图片 | `FileStreamResult` |
| `DeleteImage()` | 删除图片 | `NoContent` |
| `ToggleSelection()` | 切换收藏状态 | `{ IsSelected: bool }` |

### 私有辅助方法

| 方法名 | 功能 |
|--------|------|
| `GetCurrentUserId()` | 从 JWT Claims 获取当前用户 ID |
| `GetUserSelectedImageIdsAsync()` | 获取用户已收藏的图片 ID 集合 |
| `FillIsSelectedStatus()` | 填充图片的收藏状态 |
| `CreateImageResponseDto()` | 创建图片响应 DTO |
| `GetContentType()` | 根据文件扩展名获取 MIME 类型 |

### 权限控制
- 所有接口默认需要 JWT 认证（`[Authorize]`）
- 用户只能操作自己的图片
- `ViewImage` 接口允许匿名访问（`[AllowAnonymous]`）

---

## 👑 AdminController - 管理员控制器

### 功能描述
提供管理员专属功能，包括用户管理、系统统计和图片管理。

### 访问控制
```csharp
[Authorize(Roles = "Admin")] // 仅管理员可访问
```

### API 接口

#### 用户管理

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 用户列表 | GET | `/api/admin/users` | 获取所有用户（分页） |
| 更新角色 | PUT | `/api/admin/users/{userId}/role` | 修改用户角色 |
| 删除用户 | DELETE | `/api/admin/users/{userId}` | 删除用户（级联删除） |
| 重置密码 | POST | `/api/admin/users/{userId}/reset-password` | 重置用户密码 |

#### 统计信息

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 系统统计 | GET | `/api/admin/statistics` | 获取系统统计数据 |

**返回数据**:
```json
{
    "totalUsers": 100,
    "totalAdmins": 5,
    "totalImages": 1000,
    "totalFavourites": 500
}
```

#### 图片管理

| 接口 | 方法 | 路径 | 描述 |
|------|------|------|------|
| 所有图片 | GET | `/api/admin/images` | 获取所有用户的图片 |
| 批量删除 | POST | `/api/admin/images/batch-delete` | 批量删除图片 |

### 安全措施
- 防止管理员修改自己的角色
- 防止管理员删除自己的账号
- 删除用户时级联删除其所有图片和收藏记录

---

## 🏗️ 架构特点

### 1. 依赖注入
所有控制器通过构造函数注入依赖：
```csharp
public class ImageController(UipsDbContext context, IFileService fileService) : ControllerBase
```

### 2. 主构造函数语法
使用 C# 12 的主构造函数简化代码。

### 3. Region 分组
使用 `#region` 组织代码，提高可读性：
- 上传相关接口
- 查询相关接口
- 文件访问接口
- 操作相关接口
- 私有辅助方法

### 4. API 文档化
所有接口都添加了 `[ProducesResponseType]` 特性，支持 Swagger 自动生成文档。

### 5. 统一错误处理
- `400 Bad Request`: 参数验证失败
- `401 Unauthorized`: 未认证
- `403 Forbidden`: 无权限
- `404 Not Found`: 资源不存在

---

## 📊 接口统计

| 控制器 | 接口数量 | 认证要求 |
|--------|----------|----------|
| AuthController | 2 | 无 |
| ImageController | 9 | JWT |
| AdminController | 6 | Admin 角色 |
| **总计** | **17** | - |

---

## 🔗 相关模块

- **DTOs**: 数据传输对象，定义请求和响应的数据结构
- **Models**: 数据库实体模型
- **Services**: 业务逻辑服务（文件存储、数据库上下文）

---

## 📝 开发规范

1. **命名规范**: 控制器以 `Controller` 结尾
2. **路由规范**: 使用 `[Route("api/[controller]")]` 或自定义路由
3. **HTTP 方法**: 遵循 RESTful 规范（GET/POST/PUT/DELETE）
4. **返回类型**: 使用 `ActionResult<T>` 或 `IActionResult`
5. **异步编程**: 所有数据库操作使用 `async/await`
6. **注释规范**: 所有公共方法添加 XML 文档注释
