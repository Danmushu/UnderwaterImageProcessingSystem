# UIPS 水下图像处理系统 - PPT 汇报大纲（19页）

---

## 第一部分：项目梗概（3页）

### 第1页：封面

```
标题：水下图像处理系统 (UIPS)
副标题：Underwater Image Processing System
课程：信息系统开发 (.NET) 大实验
团队成员：[姓名列表]
日期：2026年1月
```

---

### 第2页：项目背景与问题

```
【业务场景】
• 海洋科学研究中的水下图像处理

【核心痛点】
• 水下勘探采集的海量图像因光信号衰减导致降质
• 研究人员需要从大量图片中筛选出清晰、有价值的图片
• 传统人工筛选效率低下，缺乏协作工具

【解决方案】
• 构建前后端分离的图像管理系统
• 支持多用户协作标注和筛选
• 提供管理员统一管理功能
```

---

### 第3页：项目目标与成果

```
【项目目标】
✅ 后端 Web API 服务：管理图像数据，接收选优结果
✅ 前端客户端软件：供研究人员登录、浏览、筛选图像

【实现成果】
✅ 基础目标：100% 完成
   - 用户登录/注册
   - 图像上传/浏览/筛选
   - JWT 安全认证
   - 密码加密存储

✅ 拓展目标：80% 完成
   - ORM 技术 (Entity Framework Core)
   - 版本控制 (Git)
   - 管理员权限系统
   - 分页功能
```

---

## 第二部分：技术栈和引用的库（3页）

### 第4页：后端技术栈

```
【核心框架】
• ASP.NET Core 8.0 - Web API 框架
• Entity Framework Core - ORM 数据访问
• SQLite - 轻量级数据库

【安全认证】
• JWT Bearer Authentication - Token 认证
• BCrypt.Net - 密码哈希加密

【开发工具】
• Swagger/OpenAPI - API 文档自动生成
• Visual Studio 2022 / Rider

【NuGet 包】
├── Microsoft.AspNetCore.Authentication.JwtBearer
├── Microsoft.EntityFrameworkCore.Sqlite
├── BCrypt.Net-Next
└── Swashbuckle.AspNetCore
```

---

### 第5页：前端技术栈

```
【核心框架】
• WPF (.NET 8.0) - Windows 桌面应用
• MVVM 架构模式 - 数据绑定与解耦

【UI 框架】
• Material Design In XAML Toolkit - 现代化 UI 组件

【网络通信】
• Refit - 类型安全的 REST API 客户端
• System.Text.Json - JSON 序列化

【MVVM 工具】
• CommunityToolkit.Mvvm - 简化 MVVM 开发
  - ObservableObject
  - RelayCommand
  - 源生成器

【NuGet 包】
├── MaterialDesignThemes (5.x)
├── Refit (7.x)
├── CommunityToolkit.Mvvm (8.x)
└── Microsoft.Extensions.DependencyInjection
```

---

### 第6页：技术选型理由

```
【为什么选择 ASP.NET Core？】
• 跨平台、高性能
• 内置依赖注入
• 完善的中间件管道
• 与 Entity Framework 无缝集成

【为什么选择 WPF + MVVM？】
• 强大的数据绑定能力
• 清晰的职责分离
• 便于单元测试
• Material Design 提供现代化 UI

【为什么选择 JWT 认证？】
• 无状态，易于扩展
• 跨域支持好
• 移动端友好
• 安全性高（签名验证）

【为什么选择 SQLite？】
• 零配置，开箱即用
• 单文件存储，便于部署
• 适合中小型应用
• 开发调试方便
```

---

## 第三部分：项目架构（3页）

### 第7页：整体架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      UIPS 系统架构                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────┐         ┌─────────────────────────┐   │
│  │   WPF Client    │  HTTP   │    ASP.NET Core API     │   │
│  │                 │ ◄─────► │                         │   │
│  │  ┌───────────┐  │  REST   │  ┌─────────────────┐   │   │
│  │  │   Views   │  │         │  │   Controllers   │   │   │
│  │  └─────┬─────┘  │         │  └────────┬────────┘   │   │
│  │        │        │         │           │            │   │
│  │  ┌─────▼─────┐  │         │  ┌────────▼────────┐   │   │
│  │  │ ViewModels│  │         │  │    Services     │   │   │
│  │  └─────┬─────┘  │         │  └────────┬────────┘   │   │
│  │        │        │         │           │            │   │
│  │  ┌─────▼─────┐  │         │  ┌────────▼────────┐   │   │
│  │  │  Services │  │         │  │   DbContext     │   │   │
│  │  │  (Refit)  │  │         │  │  (EF Core)      │   │   │
│  │  └───────────┘  │         │  └────────┬────────┘   │   │
│  └─────────────────┘         │           │            │   │
│                              │  ┌────────▼────────┐   │   │
│                              │  │     SQLite      │   │   │
│                              │  │    Database     │   │   │
│                              │  └─────────────────┘   │   │
│                              └─────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

### 第8页：后端项目结构

```
UIPS.API/
├── Controllers/          # API 控制器层
│   ├── AuthController.cs      # 认证：登录/注册
│   ├── ImageController.cs     # 图片：CRUD/收藏
│   └── AdminController.cs     # 管理员：用户管理
│
├── DTOs/                 # 数据传输对象
│   ├── LoginRequestDto.cs     # 登录请求
│   ├── LoginResponseDto.cs    # 登录响应（含 Token）
│   ├── ImageDto.cs            # 图片信息
│   ├── PaginatedResult.cs     # 分页结果
│   └── UserDto.cs             # 用户信息
│
├── Models/               # 数据库实体
│   ├── User.cs               # 用户表
│   ├── Image.cs              # 图片表
│   └── Favourite.cs          # 收藏表
│
├── Services/             # 服务层
│   ├── UipsDbContext.cs      # EF Core 上下文
│   ├── IFileService.cs       # 文件服务接口
│   └── LocalFileService.cs   # 本地文件存储实现
│
└── Program.cs            # 应用入口与配置
```

---

### 第9页：前端项目结构

```
UIPS.Client/
├── Views/                # 视图层 (XAML)
│   ├── LoginView.xaml         # 登录/注册界面
│   ├── DashboardView.xaml     # 主工作台界面
│   └── AdminView.xaml         # 管理员面板
│
├── ViewModels/           # 视图模型层
│   ├── LoginViewModel.cs      # 登录逻辑
│   ├── DashboardViewModel.cs  # 图片管理逻辑
│   └── AdminViewModel.cs      # 管理员逻辑
│
├── Services/             # 服务层
│   ├── IAuthApi.cs           # 认证 API 接口
│   ├── IImageApi.cs          # 图片 API 接口
│   ├── IAdminApi.cs          # 管理员 API 接口
│   ├── UserSession.cs        # 用户会话管理
│   └── AuthHeaderHandler.cs  # JWT Token 注入
│
├── Converters/           # 值转换器
│   └── InverseBooleanConverter.cs
│
├── Resources/            # 资源文件
│   └── Styles.xaml           # 全局样式
│
├── App.xaml              # 应用配置与 DI 容器
└── MainWindow.xaml       # 主窗口（导航容器）
```

---

## 第四部分：各个模块的功能（6页）

### 第10页：用户认证模块

```
【功能概述】
用户身份验证与授权管理

【API 接口】
┌────────────────────────────────────────────────────┐
│ POST /api/auth/register  - 用户注册               │
│ POST /api/auth/login     - 用户登录（返回 JWT）   │
└────────────────────────────────────────────────────┘

【安全特性】
• BCrypt 密码加密（自动加盐）
• JWT Token 认证（2小时有效期）
• 角色授权（User / Admin）

【核心代码示例】
// 密码加密
PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)

// 密码验证
BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)

// JWT 生成
var token = new JwtSecurityToken(
    issuer, audience, claims,
    expires: DateTime.UtcNow.AddMinutes(120),
    signingCredentials: credentials
);
```

---

### 第11页：图片管理模块

```
【功能概述】
图片的上传、存储、查询、删除

【API 接口】
┌────────────────────────────────────────────────────┐
│ POST   /api/images/upload       - 单张上传        │
│ POST   /api/images/upload/batch - 批量上传        │
│ GET    /api/images              - 分页查询        │
│ GET    /api/images/{id}/file    - 获取文件流      │
│ DELETE /api/images/{id}         - 删除图片        │
│ GET    /api/images/filenames    - 获取文件名列表  │
│ GET    /api/images/by-filename/{name} - 按名查询  │
└────────────────────────────────────────────────────┘

【存储策略】
• 文件存储：本地磁盘 (uploads/年/月/GUID.ext)
• 元数据存储：SQLite 数据库
• 访问控制：仅所有者可访问自己的图片

【分页实现】
• 支持 PageIndex / PageSize 参数
• 返回 TotalCount / TotalPages
```

---

### 第12页：图片收藏模块

```
【功能概述】
用户对图片的"选优"标记功能

【API 接口】
┌────────────────────────────────────────────────────┐
│ POST /api/images/{id}/select - 切换收藏状态       │
└────────────────────────────────────────────────────┘

【数据模型】
Favourite 表：
┌─────────┬─────────┬─────────┬─────────────┐
│   Id    │ UserId  │ ImageId │ SelectedAt  │
├─────────┼─────────┼─────────┼─────────────┤
│    1    │    1    │   101   │ 2026-01-08  │
│    2    │    1    │   102   │ 2026-01-08  │
│    3    │    2    │   101   │ 2026-01-08  │
└─────────┴─────────┴─────────┴─────────────┘

【业务逻辑】
• 点击收藏：如果未收藏则添加记录
• 再次点击：如果已收藏则删除记录
• 复合唯一索引：防止重复收藏
```

---

### 第13页：管理员模块

```
【功能概述】
系统管理员的用户和数据管理功能

【API 接口】
┌────────────────────────────────────────────────────┐
│ GET    /api/admin/users              - 用户列表   │
│ PUT    /api/admin/users/{id}/role    - 修改角色   │
│ DELETE /api/admin/users/{id}         - 删除用户   │
│ POST   /api/admin/users/{id}/reset-password       │
│ GET    /api/admin/statistics         - 统计信息   │
│ GET    /api/admin/images             - 所有图片   │
└────────────────────────────────────────────────────┘

【权限控制】
[Authorize(Roles = "Admin")]  // 仅管理员可访问

【管理功能】
• 查看所有用户
• 提升/降级用户角色
• 重置用户密码
• 删除用户（级联删除图片和收藏）
• 查看系统统计信息
```

---

### 第14页：前端登录模块

```
【界面设计】
• Material Design 风格
• 登录/注册模式切换
• 表单验证与错误提示
• 加载状态指示

【MVVM 实现】
┌─────────────────────────────────────────────────┐
│  LoginView.xaml (View)                          │
│       │                                         │
│       │ DataBinding                             │
│       ▼                                         │
│  LoginViewModel.cs (ViewModel)                  │
│       │                                         │
│       │ Refit API Call                          │
│       ▼                                         │
│  IAuthApi.cs (Service)                          │
│       │                                         │
│       │ HTTP POST                               │
│       ▼                                         │
│  AuthController.cs (Backend)                    │
└─────────────────────────────────────────────────┘

【会话管理】
• UserSession 存储 Token 和用户信息
• AuthHeaderHandler 自动注入 Authorization 头
```

---

### 第15页：前端图片管理模块

```
【界面功能】
• 单张/批量图片上传
• 图片网格展示（WrapPanel）
• 按文件名分组筛选
• 收藏/取消收藏（爱心按钮）
• 分页浏览（上一页/下一页）

【核心组件】
┌─────────────────────────────────────────────────┐
│  上传区域                                        │
│  ┌─────────────────┐  ┌─────────────────────┐   │
│  │ 单文件上传      │  │ 按文件名筛选        │   │
│  │ [浏览] [上传]   │  │ [下拉选择框]        │   │
│  │ 批量上传        │  │                     │   │
│  └─────────────────┘  └─────────────────────┘   │
├─────────────────────────────────────────────────┤
│  图片展示区域                                    │
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐       │
│  │ ♡   │ │ ♥   │ │ ♡   │ │ ♡   │ │ ♥   │       │
│  │ IMG │ │ IMG │ │ IMG │ │ IMG │ │ IMG │       │
│  │name │ │name │ │name │ │name │ │name │       │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘       │
├─────────────────────────────────────────────────┤
│  [上一页]  第 1/5 页 (共 48 张)  [下一页]       │
└─────────────────────────────────────────────────┘
```

---

## 第五部分：功能展示（4页）

### 第16页：登录与注册演示

```
【演示流程】

1. 启动应用 → 显示登录界面
   [截图：登录界面]

2. 点击"没有账号?" → 切换到注册模式
   [截图：注册界面]

3. 输入用户名/密码 → 点击注册
   [截图：注册成功提示]

4. 自动切换回登录模式 → 输入凭据登录
   [截图：登录成功，跳转到主界面]

【关键点】
• 密码不一致时的错误提示
• 用户名已存在时的错误提示
• 登录失败时的错误提示
```

---

### 第17页：图片上传与浏览演示

```
【演示流程】

1. 单张上传
   [截图：选择文件 → 上传成功]

2. 批量上传
   [截图：选择多个文件 → 批量上传成功]

3. 图片浏览
   [截图：图片网格展示]

4. 按文件名筛选
   [截图：下拉选择 → 显示同名图片]

5. 分页浏览
   [截图：点击下一页 → 切换页面]

【关键点】
• 上传进度指示
• 图片预览效果
• 分页信息显示
```

---

### 第18页：图片收藏与管理演示

```
【演示流程】

1. 收藏图片
   [截图：点击爱心 → 变为红色实心]

2. 取消收藏
   [截图：再次点击 → 变为空心]

3. 删除图片（管理员）
   [截图：点击删除按钮 → 确认删除]

【管理员功能演示】

4. 查看用户列表
   [截图：管理员面板 - 用户列表]

5. 修改用户角色
   [截图：点击切换角色按钮]

6. 查看统计信息
   [截图：统计卡片显示]

【关键点】
• 收藏状态实时更新
• 权限控制（普通用户看不到删除按钮）
• 管理员专属功能
```

---

### 第19页：总结与展望

```
【项目亮点】
✅ 完整的前后端分离架构
✅ 安全的 JWT 认证机制
✅ 规范的 RESTful API 设计
✅ 现代化的 Material Design UI
✅ 清晰的 MVVM 代码结构
✅ 完善的权限管理系统

【技术收获】
• 掌握 ASP.NET Core Web API 开发
• 理解 JWT 认证原理与实现
• 熟悉 Entity Framework Core ORM
• 学会 WPF + MVVM 模式开发
• 了解前后端分离协作模式

【未来改进方向】
• 添加图像增强算法集成
• 实现实时协作标注
• 支持云存储（Azure Blob）
• 添加数据导出功能
• 移动端 App 开发

【致谢】
感谢老师的指导！
欢迎提问！
```

---

## 附录：演示准备清单

```
【启动顺序】
1. 启动后端 API（确保数据库已迁移）
2. 启动前端客户端
3. 使用默认管理员账号登录：admin / admin123

【测试账号】
• 管理员：admin / admin123
• 普通用户：可现场注册

【演示数据】
• 提前上传 20-30 张测试图片
• 确保有多个同名文件用于演示分组功能
• 创建 2-3 个测试用户用于演示管理功能

【备用方案】
• 准备截图/录屏以防现场演示失败
• 准备 Swagger UI 用于 API 演示
```

---

## PPT 设计建议

```
【配色方案】
• 主色：Material Design Blue (#2196F3)
• 辅色：Material Design Teal (#009688)
• 背景：浅灰色 (#FAFAFA)
• 文字：深灰色 (#212121)

【字体建议】
• 标题：微软雅黑 Bold / 28-36pt
• 正文：微软雅黑 Regular / 18-24pt
• 代码：Consolas / 14-16pt

【每页时间分配】（约15分钟总时长）
• 项目梗概：2分钟（3页）
• 技术栈：2分钟（3页）
• 项目架构：3分钟（3页）
• 模块功能：5分钟（6页）
• 功能展示：3分钟（4页）
```
