using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UIPS.API.Services;
using UIPS.API.Models;

// 创建 Web 应用程序构建器，用于配置服务和中间件
var builder = WebApplication.CreateBuilder(args);

// 设置日志的最低级别为 Debug，用于输出详细的调试信息
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// 调用自定义方法配置数据库连接
ConfigureDatabase(builder.Services, builder.Configuration);

// 调用自定义方法配置 JWT 认证
ConfigureJwtAuthentication(builder.Services, builder.Configuration);

// 注册 MVC 控制器服务，使应用能够处理 HTTP 请求
builder.Services.AddControllers();
// 注册文件服务为单例模式，整个应用生命周期内只创建一个实例
builder.Services.AddSingleton<IFileService, LocalFileService>();
// 注册 API 端点浏览器服务，用于 Swagger 文档生成
builder.Services.AddEndpointsApiExplorer();
// 注册 Swagger 文档生成服务
builder.Services.AddSwaggerGen();

// 构建 Web 应用程序实例
var app = builder.Build();

// 配置应用监听的 HTTPS 地址和端口
app.Urls.Add("https://localhost:7149");

// 判断当前是否为开发环境
if (app.Environment.IsDevelopment())
{
    // 启用Swagger生成 API 文档的 JSON 端点
    app.UseSwagger();
    // 提供可视化的 API 文档界面
    app.UseSwaggerUI();
}

// 启用HTTPS重定向中间件，将HTTP请求自动重定向到HTTPS
app.UseHttpsRedirection();
// 注意：本项目不使用静态文件服务，图像通过 API 端点提供
// app.UseStaticFiles(); // 已禁用，因为不需要 wwwroot 目录
// 启用身份认证中间件，验证请求中的JWT Token
app.UseAuthentication();
// 启用授权中间件，检查用户是否有权限访问受保护的资源
app.UseAuthorization();
// 映射控制器路由，将 HTTP 请求路由到对应的控制器方法
app.MapControllers();

// 异步调用数据库初始化方法，应用迁移并创建种子数据
await InitializeDatabaseAsync(app.Services);

// 启动应用程序，开始监听HTTP请求
app.Run();

/// <summary>
/// 配置数据库连接
/// </summary>
/// <param name="services">服务集合，用于注册依赖注入服务</param>
/// <param name="configuration">配置对象，用于读取 appsettings.json 中的配置</param>
static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    // 从配置文件中读取名为 "DefaultConnection" 的连接字符串
    // 如果未找到则抛出异常，使用 ?? 运算符进行空值检查
    var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("连接字符串 'DefaultConnection' 未在 appsettings.json 中找到");

    // 获取应用程序的基础目录路径
    var basePath = AppDomain.CurrentDomain.BaseDirectory;
    // 将连接字符串中的占位符 {DataDirectory} 替换为实际的基础路径
    var connectionString = rawConnectionString.Replace("{DataDirectory}", basePath);

    // 注册 DbContext 服务，使用连接池模式提高性能
    services.AddDbContextPool<UipsDbContext>(options =>
        // 配置使用 SQLite 数据库，并传入处理后的连接字符串
        options.UseSqlite(connectionString));
}

/// <summary>
/// 配置 JWT Bearer 认证
/// </summary>
/// <param name="services">服务集合，用于注册认证服务</param>
/// <param name="configuration">配置对象，用于读取 JWT 相关配置</param>
static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration configuration)
{
    // 从配置文件中读取 "Jwt" 配置节
    var jwtSettings = configuration.GetSection("Jwt");
    // 读取 JWT 密钥，如果未配置则抛出异常
    var secretKey = jwtSettings.GetValue<string>("Key")
        ?? throw new InvalidOperationException("JWT Key 未配置");

    // 将密钥字符串转换为字节数组，用于签名验证
    var key = Encoding.UTF8.GetBytes(secretKey);

    // 注册认证服务
    services.AddAuthentication(options =>
    {
        // 设置默认的认证方案为 JWT Bearer
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        // 设置默认的质询方案为 JWT Bearer（当认证失败时返回 401）
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    // 添加 JWT Bearer 认证处理器
    .AddJwtBearer(options =>
    {
        // 开发环境下不要求 HTTPS 元数据（生产环境应设为 true）
        options.RequireHttpsMetadata = false;
        // 将验证通过的 Token 保存到 HttpContext 中，方便后续使用
        options.SaveToken = true;
        // 配置 Token 验证参数
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 启用签名密钥验证
            ValidateIssuerSigningKey = true,
            // 设置用于验证签名的密钥
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // 启用发行者验证
            ValidateIssuer = true,
            // 设置有效的发行者（从配置文件读取）
            ValidIssuer = jwtSettings["Issuer"],
            // 启用受众验证
            ValidateAudience = true,
            // 设置有效的受众（从配置文件读取）
            ValidAudience = jwtSettings["Audience"],
            // 设置时钟偏移为零，不允许 Token 过期时间有任何容差
            ClockSkew = TimeSpan.Zero
        };

        // 配置 JWT Bearer 事件处理器
        options.Events = ConfigureJwtBearerEvents();
    });
}

/// <summary>
/// 配置 JWT Bearer 事件处理
/// </summary>
/// <returns>配置好的 JWT Bearer 事件对象</returns>
static JwtBearerEvents ConfigureJwtBearerEvents()
{
    // 创建并返回 JWT Bearer 事件处理器
    return new JwtBearerEvents
    {
        // 当接收到消息时触发，用于从非标准位置提取 Token
        OnMessageReceived = context =>
        {
            // 尝试从查询字符串中获取 access_token 参数
            var accessToken = context.Request.Query["access_token"];
            // 获取当前请求的路径
            var path = context.HttpContext.Request.Path;

            // 如果 Token 存在且请求路径以 /api/images 开头
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/images"))
            {
                // 将查询字符串中的 Token 设置为当前请求的 Token
                // 这样可以支持通过 URL 参数传递 Token（例如图像下载链接）
                context.Token = accessToken;
            }

            // 返回已完成的任务
            return Task.CompletedTask;
        },

        // 当认证失败时触发，用于记录错误日志
        OnAuthenticationFailed = context =>
        {
            // 输出认证失败的异常信息到控制台
            Console.WriteLine($"\n[鉴权失败] {context.Exception.Message}");

            // 尝试从请求头中获取 Authorization 头
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                // 将 Authorization 头的值转换为字符串
                var headerValue = authHeader.ToString();
                // 如果头值不为空且长度大于 20 个字符
                if (!string.IsNullOrEmpty(headerValue) && headerValue.Length > 20)
                {
                    // 输出 Token 的前 20 个字符到控制台，用于调试
                    Console.WriteLine($"[Token 片段] {headerValue[..20]}...\n");
                }
            }

            // 返回已完成的任务
            return Task.CompletedTask;
        },

        // 当 Token 验证成功时触发，用于记录成功日志（当前已注释）
        OnTokenValidated = context =>
        {
            // 仅在需要调试时启用，避免日志过多
            // Console.WriteLine($"[Token 验证通过] 用户: {context.Principal?.Identity?.Name}");
            // 返回已完成的任务
            return Task.CompletedTask;
        },

        // 当触发 401 质询时触发，用于记录未授权访问的原因
        OnChallenge = context =>
        {
            // 如果错误信息为空
            if (string.IsNullOrEmpty(context.Error))
            {
                // 输出可能的原因：缺少 Authorization 头或认证方案不匹配
                Console.WriteLine("\n[401 质询] 请求缺少 Authorization Header 或 Scheme 不匹配\n");
            }
            else
            {
                // 输出具体的错误信息和描述
                Console.WriteLine($"\n[401 质询] 错误: {context.Error}, 描述: {context.ErrorDescription}\n");
            }

            // 返回已完成的任务
            return Task.CompletedTask;
        }
    };
}

/// <summary>
/// 初始化数据库：应用迁移并创建种子数据
/// </summary>
/// <param name="serviceProvider">服务提供者，用于获取依赖注入的服务</param>
static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
{
    // 创建一个服务作用域，用于获取 Scoped 生命周期的服务（如 DbContext）
    using var scope = serviceProvider.CreateScope();
    // 从作用域中获取服务提供者
    var services = scope.ServiceProvider;

    try
    {
        // 从服务提供者中获取数据库上下文实例
        var dbContext = services.GetRequiredService<UipsDbContext>();
        // 从服务提供者中获取日志记录器实例
        var logger = services.GetRequiredService<ILogger<Program>>();

        // 记录日志：开始检查并应用数据库迁移
        logger.LogInformation("正在检查并应用数据库迁移...");
        // 异步应用所有待处理的数据库迁移（如果数据库不存在则创建）
        await dbContext.Database.MigrateAsync();
        // 记录日志：数据库迁移完成
        logger.LogInformation("数据库迁移已完成");

        // 调用方法创建默认管理员账户（种子数据）
        await SeedDefaultAdminAsync(dbContext, logger);

        // 记录日志：系统初始化完成
        logger.LogInformation("系统初始化完成，准备就绪！");
    }
    catch (Exception ex)
    {
        // 捕获异常时重新获取日志记录器
        var logger = services.GetRequiredService<ILogger<Program>>();
        // 记录错误日志，包含异常详细信息
        logger.LogError(ex, "数据库初始化失败，请检查连接字符串或文件权限");
        // 重新抛出异常，阻止应用启动（数据库初始化失败是致命错误）
        throw;
    }
}

/// <summary>
/// 创建默认管理员账户
/// </summary>
/// <param name="dbContext">数据库上下文，用于访问数据库</param>
/// <param name="logger">日志记录器，用于输出日志信息</param>
static async Task SeedDefaultAdminAsync(UipsDbContext dbContext, ILogger logger)
{
    // 记录日志：开始检查种子数据
    logger.LogInformation("正在检查种子数据...");

    // 异步查询数据库，检查是否已存在角色为 "Admin" 的用户
    var adminExists = await dbContext.Users.AnyAsync(u => u.Role == "Admin");

    // 如果不存在管理员账户
    if (!adminExists)
    {
        // 记录日志：未检测到管理员，准备创建
        logger.LogInformation("未检测到管理员账户，正在创建默认管理员...");

        // 创建新的管理员用户对象
        var adminUser = new User
        {
            // 设置用户名为 "admin"
            UserName = "admin",
            // 使用 BCrypt 算法对密码 "admin123" 进行哈希加密
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            // 设置用户角色为 "Admin"
            Role = "Admin"
        };

        // 将管理员用户添加到数据库上下文的 Users 集合中
        dbContext.Users.Add(adminUser);
        // 异步保存更改到数据库
        await dbContext.SaveChangesAsync();

        // 记录警告日志：提示默认管理员已创建及其凭据
        logger.LogWarning(">>> 默认管理员已创建！用户名: admin, 密码: admin123 <<<");
        // 记录警告日志：提醒生产环境需要修改默认密码
        logger.LogWarning(">>> 生产环境请立即修改默认密码！<<<");
    }
    else
    {
        // 如果管理员已存在，记录日志并跳过创建
        logger.LogInformation("管理员账户已存在，跳过种子数据初始化");
    }
}
