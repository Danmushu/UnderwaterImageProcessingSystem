using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UIPS.API.Services;
using UIPS.Domain.Entities;
using UIPS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 强制开启 Debug 日志，确保能看到鉴权细节
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

/************* database 配置 *************/
var path = AppDomain.CurrentDomain.BaseDirectory;
// 增加空值检查，防止配置错误难以排查
if (string.IsNullOrEmpty(rawConnectionString))
{
    throw new InvalidOperationException("连接字符串 'DefaultConnection' 未在 appsettings.json 中找到。");
}
var connectionString = rawConnectionString.Replace("{DataDirectory}", path);

builder.Services.AddDbContextPool<UipsDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

/************ JwtBearer 配置 *************/
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyStr = jwtSettings.GetValue<string>("Key");
if (string.IsNullOrEmpty(keyStr))
{
    throw new InvalidOperationException("JWT Key 未配置。");
}
var key = Encoding.UTF8.GetBytes(keyStr);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ClockSkew = TimeSpan.Zero
        };

        // 增加事件监听，直接在控制台输出原因
        options.Events = new JwtBearerEvents
        {
            // 允许从 Query String 获取 Token
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/images"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },

            // 当 Token 格式错误、过期、签名不对时触发
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"\n>>> [鉴权失败] 原因: {context.Exception.Message}");
                // 增加空值检查，防止报错
                if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    var headerStr = authHeader.ToString();
                    if (!string.IsNullOrEmpty(headerStr) && headerStr.Length > 20)
                    {
                        Console.WriteLine($">>> [鉴权失败] Token片段: {headerStr.Substring(0, 20)}...\n");
                    }
                }
                return Task.CompletedTask;
            },
            // 当 Token 验证通过时触发
            OnTokenValidated = context =>
            {
                // 减少日志噪音，仅调试时开启
                // Console.WriteLine($"\n>>> [Token验证通过] 用户: {context.Principal?.Identity?.Name}\n");
                return Task.CompletedTask;
            },
            // 当请求根本没有 Token，或者验证失败导致服务器决定返回 401 时触发
            OnChallenge = context =>
            {
                if (string.IsNullOrEmpty(context.Error))
                {
                    Console.WriteLine($"\n>>> [触发 401 质询] 请求头中可能缺少 Authorization Header，或者 Scheme 不匹配。\n");
                }
                else
                {
                    Console.WriteLine($"\n>>> [触发 401 质询] 错误: {context.Error}, 描述: {context.ErrorDescription}\n");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddSingleton<IFileService, LocalFileService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.Urls.Add("http://localhost:5216");
app.Urls.Add("https://localhost:7149");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 静态文件服务
app.UseStaticFiles();

// 鉴权中间件顺序必须正确
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 数据库自动化 迁移 种子数据

// 创建一个临时的 Scope 来获取 Scoped 服务 (DbContext)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<UipsDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("正在检查并应用数据库迁移...");

        // 应用迁移 (如果数据库不存在则创建，如果存在则更新)
        dbContext.Database.Migrate();

        logger.LogInformation("数据库迁移已完成。正在检查种子数据...");

        // 自动创建默认管理员 (如果不存在)
        // 检查是否有任何 Admin 角色的用户
        if (!dbContext.Users.Any(u => u.Role == "Admin"))
        {
            logger.LogInformation("未检测到管理员账户，正在创建默认管理员...");

            var adminUser = new User
            {
                UserName = "admin",
                // 密码必须哈希处理！这里假设密码是 "admin123"
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            };

            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();

            logger.LogInformation(">>> 默认管理员已创建! 用户名: admin, 密码: admin123 <<<");
        }
        else
        {
            logger.LogInformation("管理员账户已存在，跳过种子数据初始化。");
        }

        logger.LogInformation("系统初始化完成，准备就绪！");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        // 记录严重错误，但不阻止程序启动 (或根据需求决定是否 throw)
        logger.LogError(ex, "数据库初始化失败。请检查连接字符串或文件权限。");
    }
}

app.Run();