# DTO 说明文档

## 什么是 DTO？

**DTO (Data Transfer Object)** 即"数据传输对象"，是一种设计模式，用于在不同层之间传输数据。

### 核心概念
- **纯数据容器**：DTO 只包含数据字段，不包含业务逻辑
- **跨层传输**：在前端和后端之间、或不同服务之间传输数据
- **数据塑形**：只传输需要的数据，隐藏内部实现细节

## 为什么需要 DTO？

### 1. 安全性
直接暴露数据库实体（Entity）会泄露敏感信息：
```csharp
// ❌ 不好的做法：直接返回 Entity
public class User
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }  // 密码哈希不应该返回给前端！
    public string Role { get; set; }
}

// ✅ 好的做法：使用 DTO
public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    // 不包含 PasswordHash，保护敏感信息
}
```

### 2. 解耦
DTO 将前端和数据库结构解耦，数据库结构变化不影响 API 接口：
```csharp
// 数据库实体可能很复杂
public class Image
{
    public long Id { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }  // 二进制数据
    public long UserId { get; set; }
    public User User { get; set; }  // 导航属性
    public DateTime CreatedAt { get; set; }
    public List<UserImageSelection> Selections { get; set; }  // 关联数据
}

// DTO 只包含前端需要的数据
public class ImageDto
{
    public long Id { get; set; }
    public string FileName { get; set; }
    public string Base64Data { get; set; }  // 转换为 Base64 字符串
    public DateTime CreatedAt { get; set; }
    // 不包含复杂的导航属性和关联数据
}
```

### 3. 性能优化
只传输必要的数据，减少网络传输量：
```csharp
// 列表视图只需要基本信息
public class ImageListDto
{
    public long Id { get; set; }
    public string FileName { get; set; }
    public string ThumbnailUrl { get; set; }  // 缩略图，不是完整图片
}

// 详情视图需要完整信息
public class ImageDetailDto
{
    public long Id { get; set; }
    public string FileName { get; set; }
    public string Base64Data { get; set; }  // 完整图片数据
    public DateTime CreatedAt { get; set; }
    public string OwnerName { get; set; }
}
```

### 4. 数据验证
DTO 可以包含验证规则，确保数据有效性：
```csharp
public class RegisterDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在 3-50 之间")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在 6-100 之间")]
    public string Password { get; set; } = string.Empty;
}
```

## UIPS 项目中的 DTO 示例

### 1. 认证相关 DTO

#### LoginDto - 登录请求
```csharp
/// <summary>
/// 登录请求数据传输对象
/// </summary>
public class LoginDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 密码（明文，仅在传输时使用，后端会立即哈希）
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
```

**使用场景：** 前端发送登录请求时使用
```csharp
// 前端代码
var loginDto = new LoginDto 
{ 
    UserName = "admin", 
    Password = "password123" 
};
var response = await authApi.Login(loginDto);
```

#### LoginResponseDto - 登录响应
```csharp
/// <summary>
/// 登录响应数据传输对象
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户 ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户角色（User 或 Admin）
    /// </summary>
    public string Role { get; set; } = "User";
}
```

**使用场景：** 后端返回登录成功的信息
```csharp
// 后端代码
return Ok(new LoginResponseDto
{
    AccessToken = token,
    UserName = user.UserName,
    UserId = user.Id,
    Role = user.Role
});
```

### 2. 图片相关 DTO

#### ImageDto - 图片信息
```csharp
/// <summary>
/// 图片数据传输对象
/// </summary>
public class ImageDto
{
    /// <summary>
    /// 图片 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Base64 编码的图片数据
    /// </summary>
    public string Base64Data { get; set; } = string.Empty;

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否已被当前用户选择
    /// </summary>
    public bool IsSelected { get; set; }
}
```

**使用场景：** 返回图片列表给前端
```csharp
// 后端代码
var imageDtos = images.Select(img => new ImageDto
{
    Id = img.Id,
    FileName = img.FileName,
    Base64Data = Convert.ToBase64String(img.Data),
    CreatedAt = img.CreatedAt,
    IsSelected = selectedIds.Contains(img.Id)
}).ToList();
```

#### ImageUploadDto - 图片上传请求
```csharp
/// <summary>
/// 图片上传数据传输对象
/// </summary>
public class ImageUploadDto
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Base64 编码的图片数据
    /// </summary>
    public string Base64Data { get; set; } = string.Empty;
}
```

**使用场景：** 前端上传图片时使用
```csharp
// 前端代码
var uploadDto = new ImageUploadDto
{
    FileName = "ocean.jpg",
    Base64Data = Convert.ToBase64String(imageBytes)
};
await imageApi.UploadImage(uploadDto);
```

### 3. 管理员相关 DTO

#### AdminStatisticsDto - 系统统计信息
```csharp
/// <summary>
/// 管理员统计信息数据传输对象
/// </summary>
public class AdminStatisticsDto
{
    /// <summary>
    /// 总用户数
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// 管理员数量
    /// </summary>
    public int AdminCount { get; set; }

    /// <summary>
    /// 图片总数
    /// </summary>
    public int TotalImages { get; set; }

    /// <summary>
    /// 收藏总数
    /// </summary>
    public int TotalSelections { get; set; }
}
```

**使用场景：** 管理员面板显示统计信息
```csharp
// 后端代码
var stats = new AdminStatisticsDto
{
    TotalUsers = await _context.Users.CountAsync(),
    AdminCount = await _context.Users.CountAsync(u => u.Role == "Admin"),
    TotalImages = await _context.Images.CountAsync(),
    TotalSelections = await _context.UserImageSelections.CountAsync()
};
return Ok(stats);
```

#### UserDto - 用户信息（管理员视图）
```csharp
/// <summary>
/// 用户数据传输对象（管理员视图）
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
```

**使用场景：** 管理员查看用户列表
```csharp
// 后端代码
var userDtos = await _context.Users
    .Select(u => new UserDto
    {
        Id = u.Id,
        UserName = u.UserName,
        Role = u.Role,
        CreatedAt = u.CreatedAt
    })
    .ToListAsync();
```

### 4. 分页相关 DTO

#### PagedResultDto - 分页结果
```csharp
/// <summary>
/// 分页结果数据传输对象
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

**使用场景：** 返回分页数据
```csharp
// 后端代码
var result = new PagedResultDto<ImageDto>
{
    Items = imageDtos,
    TotalCount = totalCount,
    PageNumber = pageNumber,
    PageSize = pageSize
};
return Ok(result);
```

## DTO 的工作流程

### 完整的请求-响应流程

```
前端 (Client)                    后端 (API)                      数据库 (Database)
    |                               |                                  |
    | 1. 创建 LoginDto              |                                  |
    |    { UserName, Password }     |                                  |
    |                               |                                  |
    | 2. POST /api/auth/login       |                                  |
    |--------------------------->   |                                  |
    |                               | 3. 验证 DTO 数据                  |
    |                               |                                  |
    |                               | 4. 查询 User Entity               |
    |                               |--------------------------------->|
    |                               |                                  |
    |                               | 5. 返回 User Entity               |
    |                               |<---------------------------------|
    |                               |                                  |
    |                               | 6. 验证密码                       |
    |                               |                                  |
    |                               | 7. 生成 JWT Token                 |
    |                               |                                  |
    |                               | 8. 创建 LoginResponseDto          |
    |                               |    { Token, UserName, UserId }   |
    |                               |                                  |
    | 9. 返回 LoginResponseDto       |                                  |
    |<---------------------------|   |                                  |
    |                               |                                  |
    | 10. 保存 Token 到 UserSession |                                  |
    |                               |                                  |
```

### 代码示例

#### 前端发送请求
```csharp
// 1. 创建 DTO
var loginDto = new LoginDto
{
    UserName = "admin",
    Password = "password123"
};

// 2. 发送请求
var response = await _authApi.Login(loginDto);

// 3. 处理响应 DTO
_userSession.SetSession(
    response.AccessToken,
    response.UserName,
    response.UserId,
    response.Role
);
```

#### 后端处理请求
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    // 1. 接收 DTO
    // 2. 查询数据库 Entity
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

    // 3. 验证密码
    if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
    {
        return Unauthorized("用户名或密码错误");
    }

    // 4. 生成 Token
    var token = GenerateJwtToken(user);

    // 5. 创建响应 DTO
    var responseDto = new LoginResponseDto
    {
        AccessToken = token,
        UserName = user.UserName,
        UserId = user.Id,
        Role = user.Role
    };

    // 6. 返回 DTO
    return Ok(responseDto);
}
```

## DTO vs Entity 对比

| 特性 | Entity (实体) | DTO (数据传输对象) |
|------|--------------|-------------------|
| **用途** | 映射数据库表 | 传输数据 |
| **位置** | 数据访问层 | API 层 |
| **包含内容** | 所有数据库字段、导航属性 | 只包含需要传输的字段 |
| **业务逻辑** | 可能包含一些逻辑 | 纯数据容器 |
| **敏感信息** | 包含（如密码哈希） | 不包含 |
| **关联数据** | 包含导航属性 | 扁平化数据 |
| **变化频率** | 随数据库结构变化 | 随 API 需求变化 |

### 示例对比

```csharp
// Entity - 数据库实体
public class User
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;  // 敏感信息
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; }
    
    // 导航属性
    public List<Image> Images { get; set; } = new();
    public List<UserImageSelection> Selections { get; set; } = new();
}

// DTO - 数据传输对象
public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    // 不包含 PasswordHash（安全）
    // 不包含导航属性（简化）
}
```

## 最佳实践

### 1. 命名规范
- 使用 `Dto` 后缀：`LoginDto`, `UserDto`, `ImageDto`
- 请求 DTO：`CreateUserDto`, `UpdateUserDto`
- 响应 DTO：`UserResponseDto`, `LoginResponseDto`

### 2. 单一职责
每个 DTO 只服务于一个特定的场景：
```csharp
// ✅ 好的做法：不同场景使用不同 DTO
public class CreateUserDto { /* 创建用户需要的字段 */ }
public class UpdateUserDto { /* 更新用户需要的字段 */ }
public class UserListDto { /* 列表显示需要的字段 */ }
public class UserDetailDto { /* 详情显示需要的字段 */ }

// ❌ 不好的做法：一个 DTO 用于所有场景
public class UserDto { /* 包含所有可能的字段 */ }
```

### 3. 数据验证
在 DTO 中添加验证规则：
```csharp
public class RegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
```

### 4. 使用映射工具
对于复杂的 Entity 到 DTO 转换，可以使用 AutoMapper：
```csharp
// 手动映射（简单场景）
var userDto = new UserDto
{
    Id = user.Id,
    UserName = user.UserName,
    Role = user.Role
};

// AutoMapper（复杂场景）
var userDto = _mapper.Map<UserDto>(user);
```

## 总结

DTO 是前后端分离架构中的关键组件：
- **安全性**：隐藏敏感信息和内部实现
- **解耦**：前端和数据库结构独立变化
- **性能**：只传输必要的数据
- **验证**：确保数据有效性
- **清晰**：明确的数据契约

在 UIPS 项目中，所有的 API 请求和响应都使用 DTO，确保了系统的安全性、可维护性和性能。
