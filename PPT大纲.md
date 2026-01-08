# UIPS 水下图像处理系统 - PPT 大纲（19页）

---

## 第1页：封面

**标题**：UIPS 水下图像处理系统  
**副标题**：基于 .NET 8 的全栈图片管理平台  
**课程**：.NET 程序设计实验  
**姓名/学号**：XXX / XXXXXXXX  
**日期**：2026年1月

---

## 第2页：目录

1. 项目背景与需求
2. 技术架构设计
3. 核心功能实现
4. 关键技术详解
5. 功能演示
6. 总结与展望

---

## 第3页：项目背景

**实验要求**：
- 开发一个图片上传与管理系统
- 支持用户注册、登录、图片上传/浏览/删除
- 采用前后端分离架构

**项目目标**：
- ✅ 实现完整的用户认证系统
- ✅ 实现图片的增删改查功能
- ✅ 实现管理员后台管理
- ✅ 良好的用户界面体验

---

## 第4页：技术选型

| 类别 | 技术 | 说明 |
|------|------|------|
| 后端框架 | ASP.NET Core 8 | 高性能 Web API |
| 前端框架 | WPF | Windows 桌面应用 |
| UI 组件库 | Material Design | 现代化 UI 风格 |
| 数据库 | SQLite | 轻量级嵌入式数据库 |
| ORM | Entity Framework Core | 对象关系映射 |
| 认证方案 | JWT + BCrypt | 安全的身份认证 |
| HTTP 客户端 | Refit | 声明式 API 调用 |

---

## 第5页：系统架构图

```
┌─────────────────────────────────────────────────────┐
│                    WPF 客户端                        │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐             │
│  │  Views  │←→│ViewModels│←→│Services │             │
│  │ (XAML)  │  │  (C#)   │  │ (Refit) │             │
│  └─────────┘  └─────────┘  └────┬────┘             │
└────────────────────────────────│────────────────────┘
                                 │ HTTP/JSON
                                 ▼
┌─────────────────────────────────────────────────────┐
│                 ASP.NET Core Web API                │
│  ┌───────────┐  ┌──────────┐  ┌──────────┐        │
│  │Controllers│→ │ Services │→ │ EF Core  │        │
│  └───────────┘  └──────────┘  └────┬─────┘        │
└────────────────────────────────────│────────────────┘
                                     ▼
                              ┌──────────┐
                              │  SQLite  │
                              └──────────┘
```

---

## 第6页：数据库设计

**三张核心表**：

```
Users (用户表)
├── Id (主键)
├── UserName (用户名，唯一)
├── PasswordHash (BCrypt加密)
└── Role (角色：User/Admin)

Images (图片表)
├── Id (主键)
├── OriginalFileName (原始文件名)
├── StoredPath (存储路径)
├── FileSize (文件大小)
├── OwnerId (外键 → Users)
└── UploadedAt (上传时间)

Favourites (收藏表)
├── Id (主键)
├── UserId (外键 → Users)
├── ImageId (外键 → Images)
└── SelectedAt (收藏时间)
```

---

## 第7页：项目结构

```
UIPS/
├── src/
│   ├── UIPS.API/              # 后端项目
│   │   ├── Controllers/       # API 控制器
│   │   ├── Models/            # 数据模型
│   │   ├── DTOs/              # 数据传输对象
│   │   ├── Services/          # 业务服务
│   │   └── Program.cs         # 启动配置
│   │
│   └── UIPS.Client/           # 前端项目
│       ├── Views/             # 视图层
│       ├── ViewModels/        # 视图模型
│       ├── Services/          # API 接口
│       └── Resources/         # 样式资源
│
└── tests/                     # 单元测试
```

---

## 第8页：功能模块概览

```
┌────────────────────────────────────────┐
│              UIPS 系统                  │
├────────────┬────────────┬──────────────┤
│  认证模块   │  图片模块   │  管理模块    │
├────────────┼────────────┼──────────────┤
│ • 用户注册  │ • 单文件上传 │ • 数据统计   │
│ • 用户登录  │ • 批量上传  │ • 用户管理   │
│ • JWT认证  │ • 分页浏览  │ • 重置密码   │
│ • 角色区分  │ • 图片预览  │ • 删除用户   │
│            │ • 收藏功能  │              │
│            │ • 删除图片  │              │
└────────────┴────────────┴──────────────┘
```

---

## 第9页：JWT 认证流程

**认证流程图**：
```
1. 用户登录
   └→ POST /api/auth/login {userName, password}

2. 服务端验证
   └→ BCrypt.Verify(password, passwordHash)

3. 生成 Token
   └→ JWT 包含: UserId, UserName, Role, 过期时间

4. 客户端存储
   └→ UserSession 保存 Token

5. 后续请求
   └→ Header: Authorization: Bearer <token>

6. 服务端验证
   └→ [Authorize] 特性自动验证
```

---

## 第10页：JWT 关键代码

**Token 生成**（AuthController.cs）：
```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.UserName),
    new Claim(ClaimTypes.Role, user.Role)
};

var token = new JwtSecurityToken(
    issuer: "UIPS",
    audience: "UIPS-Client",
    claims: claims,
    expires: DateTime.UtcNow.AddHours(24),
    signingCredentials: credentials
);
```

**密码加密**：
```csharp
// 注册时加密
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

// 登录时验证
BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

---

## 第11页：MVVM 架构实践

**MVVM 模式**：
```
View (视图)
  │  ← 数据绑定 (Binding)
  ▼
ViewModel (视图模型)
  │  ← 命令绑定 (Command)
  ▼
Model / Service (模型/服务)
```

**使用 CommunityToolkit.Mvvm**：
```csharp
public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]  // 自动生成属性和通知
    private string _uploadStatus;

    [RelayCommand]  // 自动生成 ICommand
    private async Task UploadFileAsync() { ... }
}
```

---

## 第12页：Refit 声明式 API

**接口定义**（IImageApi.cs）：
```csharp
public interface IImageApi
{
    [Multipart]
    [Post("/api/images/upload")]
    Task<dynamic> UploadImage([AliasAs("file")] StreamPart file);

    [Get("/api/images")]
    Task<dynamic> GetImages(
        [Query("PageIndex")] int pageIndex, 
        [Query("PageSize")] int pageSize);

    [Delete("/api/images/{id}")]
    Task DeleteImage(long id);
}
```

**优势**：
- 无需手写 HttpClient 代码
- 自动序列化/反序列化
- 类型安全的 API 调用

---

## 第13页：图片上传实现

**后端接收**（ImageController.cs）：
```csharp
[HttpPost("upload")]
public async Task<IActionResult> UploadImage(IFormFile file)
{
    // 1. 验证文件
    if (file == null || file.Length == 0)
        return BadRequest("文件为空");

    // 2. 保存到磁盘
    var storedPath = await fileService.SaveFileAsync(
        file.OpenReadStream(), file.FileName);

    // 3. 保存到数据库
    var image = new Image { ... };
    context.Images.Add(image);
    await context.SaveChangesAsync();

    return Ok(new ImageResponseDto { ... });
}
```

---

## 第14页：分页查询实现

**后端分页**：
```csharp
[HttpGet]
public async Task<ActionResult<PaginatedResult<ImageDto>>> GetImages(
    [FromQuery] PaginatedRequestDto request)
{
    var query = context.Images.Where(i => i.OwnerId == userId);
    var totalCount = await query.CountAsync();

    var images = await query
        .OrderByDescending(i => i.UploadedAt)
        .Skip((request.PageIndex - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync();

    return Ok(new PaginatedResult<ImageDto> {
        Items = images,
        TotalCount = totalCount,
        PageIndex = request.PageIndex
    });
}
```

---

## 第15页：前端界面展示

**登录界面**：
- Material Design 风格
- 登录/注册模式切换
- 表单验证提示

**主界面**：
- 左侧导航栏（图库/管理员）
- 图片卡片网格布局
- 分页控件
- 图片预览弹窗

**管理员界面**：
- 数据统计卡片
- 用户列表 DataGrid
- 操作按钮（重置密码/删除）

*（此页配合截图展示）*

---

## 第16页：开发中遇到的问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 控件访问报 NullReferenceException | 构造函数中控件未初始化 | 使用 Loaded 事件延迟初始化 |
| 分页按钮无法点击 | CanExecute 状态未更新 | 添加 NotifyCanExecuteChangedFor 特性 |
| 图片无法显示 | 需要认证的 URL | URL 携带 access_token 参数 |
| 注册显示"失败：OK" | API 返回格式判断错误 | 修正成功/失败判断逻辑 |

---

## 第17页：功能完成情况

**基础目标（100%完成）**：
- ✅ 用户注册与登录
- ✅ JWT Token 认证
- ✅ 图片上传（单个/批量）
- ✅ 图片浏览与分页
- ✅ 图片删除
- ✅ 收藏功能

**扩展目标（部分完成）**：
- ✅ 管理员角色与权限
- ✅ 用户管理功能
- ✅ 图片预览功能
- ⬚ 图片处理（滤镜等）
- ⬚ 批量下载

---

## 第18页：项目收获

**技术能力提升**：
- 掌握 ASP.NET Core Web API 开发
- 掌握 WPF + MVVM 架构
- 掌握 Entity Framework Core ORM
- 掌握 JWT 认证机制

**工程实践经验**：
- 前后端分离架构设计
- RESTful API 设计规范
- 代码组织与模块划分
- 问题排查与调试能力

---

## 第19页：总结与展望

**项目总结**：
- 完成了一个功能完整的图片管理系统
- 采用现代化的 .NET 技术栈
- 实现了安全的用户认证机制
- 具有良好的代码结构和可扩展性

**未来改进方向**：
- Token 自动刷新机制
- 图片缩略图生成
- 图片处理功能（滤镜、裁剪）
- 移动端适配

**感谢聆听，欢迎提问！**

---

## 附：演示流程

1. 启动后端 → 展示 Swagger
2. 启动前端 → 注册新用户
3. 登录 → 上传图片
4. 浏览图片 → 分页 → 预览
5. 收藏图片
6. 管理员登录 → 查看统计
7. 用户管理操作
