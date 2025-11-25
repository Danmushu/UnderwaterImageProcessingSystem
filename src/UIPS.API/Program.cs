using Microsoft.EntityFrameworkCore;
using UIPS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. 获取原始连接字符串
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. 【关键】动态计算数据库路径
// 获取当前程序运行的目录 (例如 bin/Debug/net8.0/)
var path = AppDomain.CurrentDomain.BaseDirectory;
// 替换占位符，确保数据库文件生成在程序目录下，而不是 C 盘某个角落
var connectionString = rawConnectionString!.Replace("{DataDirectory}", path);

// 3. 注册数据库服务 (使用高性能的 AddDbContextPool)
builder.Services.AddDbContextPool<UipsDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// 添加控制器服务
builder.Services.AddControllers();
// 添加 Swagger 文档生成
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();