using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using UIPS.Client.Core.Services;
using UIPS.Shared.DTOs;

namespace UIPS.Client.Core.ViewModels;

// 继承 ObservableObject 以获得自动属性通知功能
public partial class LoginViewModel : ObservableObject
{
    // 自动生成 UserName 属性和变更通知
    [ObservableProperty]
    private string userName;

    // 自动生成 Password 属性
    [ObservableProperty]
    private string password;

    // 自动生成 IsLoading 属性，用于控制加载动画
    [ObservableProperty]
    private bool isLoading;

    // 自动生成 ErrorMessage 属性，用于显示错误
    [ObservableProperty]
    private string errorMessage;

    public Action? OnLoginSuccess { get; set; }

    // 登录命令：自动处理异步和按钮禁用
    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = "";

            // 创建 API 客户端 (这里暂时硬编码地址，稍后统一配置)
            var authApi = RestService.For<IAuthApi>("https://localhost:7149");
            var request = new LoginRequestDto { UserName = UserName, Password = Password };
            var response = await authApi.LoginAsync(request);

            // 将 Token 存入会话单例
            UserSession.Current.SetSession(response.AccessToken, response.UserName, response.UserId);

            // 触发成功事件
            ErrorMessage = "登录成功，正在跳转...";

            // 延迟一小会儿让用户看到“登录成功”字样 (可选)
            await Task.Delay(500);

            // 通知UI层切换窗口
            OnLoginSuccess?.Invoke();
        }
        catch (ApiException ex)
        {
            // 处理 401/400 等 API 错误
            ErrorMessage = $"登录失败: {ex.StatusCode}";
        }
        catch (Exception ex)
        {
            // 处理网络错误
            ErrorMessage = $"发生错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}