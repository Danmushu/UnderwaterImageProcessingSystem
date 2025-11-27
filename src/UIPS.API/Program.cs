using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text;
using UIPS.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens; 

var builder = WebApplication.CreateBuilder(args);

// 获取原始连接字符串
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");




/************* database 配置 *************/
// 动态计算数据库路径
// 获取当前程序运行的目录 (例如 bin/Debug/net8.0/)
var path = AppDomain.CurrentDomain.BaseDirectory;
// 替换占位符，确保数据库文件生成在程序目录下，而不是 C 盘某个角落
var connectionString = rawConnectionString!.Replace("{DataDirectory}", path);

// 注册数据库服务 (使用高性能的 AddDbContextPool)
builder.Services.AddDbContextPool<UipsDbContext>(options =>
{
    options.UseSqlite(connectionString);
});


/************ JwtBearer 配置 *************/
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings.GetValue<string>("Key")!); // 获取密钥


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // 设置默认的认证方案为 JwtBearer
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // 设置默认的挑战方案为 JwtBearer
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // 在开发环境中可以禁用 HTTPS 元数据要求 TODO: 生产环境中应启用,正式记得删除
        options.SaveToken = true; // 保存令牌以供后续使用
        // 配置令牌验证参数
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // 验证签名密钥
            IssuerSigningKey = new SymmetricSecurityKey(key), // 设置签名密钥
            ValidateIssuer = true, // 验证发行者
            ValidIssuer = jwtSettings["Issuer"], // 设置有效的发行者]
            ValidateAudience = true, // 验证受众
            ValidAudience = jwtSettings["Audience"], // 设置有效的受众
            ClockSkew = TimeSpan.Zero // 禁用时间偏差
        };
    });

// 添加控制器服务
builder.Services.AddControllers();
// 添加 Swagger 文档生成
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.Urls.Add("http://localhost:5216");
app.Urls.Add("https://localhost:7149");

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // <--- 新加的：检查你是谁
app.UseAuthorization();  // <--- 原有的：检查你能干什么
app.MapControllers();

app.Run();