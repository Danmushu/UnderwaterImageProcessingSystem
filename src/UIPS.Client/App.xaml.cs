using System.Net.Http; // 必须引用这个
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using UIPS.Client.Services;
using UIPS.Client.ViewModels;

namespace UIPS.Client;

public partial class App : Application
{
    private readonly IHost _host;

    // 允许 View 层访问服务容器
    public IServiceProvider ServiceProvider => _host.Services;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                /******* 基础服务注册 *******/
        
                // 注册 AuthHeaderHandler (Token 拦截器)
                services.AddTransient<AuthHeaderHandler>();

                // 注册 UserSession (用户状态单例)
                services.AddSingleton<UserSession>();

                // Refit 网络客户端配置 关键修复区
                
                // 定义 API 地址
                var apiUrl = "https://localhost:7149";

                // 定义一个忽略 SSL 证书错误的 Handler (开发环境专用)
                // 修复 "An error occurred while sending the request" 的核心代码
                Func<HttpMessageHandler> getDevSslHandler = () => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };

                // 注册 Auth API 不需要 Token，但需要 SSL 忽略
                services.AddRefitClient<IAuthApi>()
                        .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl))
                        .ConfigurePrimaryHttpMessageHandler(getDevSslHandler);

                // 注册 Image API 需要 Token + SSL 忽略
                services.AddRefitClient<IImageApi>() 
                        .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl))
                        .AddHttpMessageHandler<AuthHeaderHandler>() // 先注入 Token
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler // 连通 https 的生命线
                        {
                            ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                        }); // 再处理 SSL

                // 注册 Admin API 需要 Token + SSL 忽略
                services.AddRefitClient<IAdminApi>()
                        .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl))
                        .AddHttpMessageHandler<AuthHeaderHandler>() // 先注入 Token
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                        }); // 再处理 SSL

                // UI 注册 ViewModel & View

                // 登录模块
                services.AddSingleton<LoginViewModel>();
                services.AddTransient<Views.LoginView>();

                // 主界面模块
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<Views.DashboardView>();
                
                // 管理员模块
                services.AddSingleton<AdminViewModel>();
                services.AddSingleton<Views.AdminView>();
                
                services.AddSingleton<MainWindow>();

            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // 启动显示登录窗口
        var loginView = _host.Services.GetRequiredService<Views.LoginView>();
        loginView.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
        base.OnExit(e);
    }
}