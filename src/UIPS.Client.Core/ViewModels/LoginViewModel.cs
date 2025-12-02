using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using UIPS.Client.Core.Services;
using UIPS.Shared.DTOs;
using System.Windows; // 用于 Visibility 转换逻辑

namespace UIPS.Client.Core.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // 依赖服务
    private readonly IAuthApi _authApi;
    private readonly UserSession _userSession;

    // 构造函数
    public LoginViewModel(IAuthApi authApi, UserSession userSession)
    {
        _authApi = authApi;
        _userSession = userSession;
    }

    // 基础属性
    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // 确认密码 仅注册模式使用
    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    // 当前是否为注册模式 默认 False = 登录
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActionTitle))]
    [NotifyPropertyChangedFor(nameof(ActionButtonText))]
    [NotifyPropertyChangedFor(nameof(SwitchModePrompt))]
    private bool _isRegisterMode = false;

    // 动态属性
    public string ActionTitle => IsRegisterMode ? "UIPS 注册" : "UIPS 登录";
    public string ActionButtonText => IsRegisterMode ? "立即注册" : "登 录";
    public string SwitchModePrompt => IsRegisterMode ? "已有账号?" : "没有账号?";

    // 登录成功回调
    public Action? OnLoginSuccess { get; set; }

    // 切换模式命令 
    [RelayCommand]
    private void SwitchMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = "";
        // 切换模式时建议清空密码，避免混淆
        Password = "";
        ConfirmPassword = "";
    }

    // 主执行命令 入口路由
    // View 层只需要绑定这个命令，内部会自动分发到具体的业务函数
    [RelayCommand]
    private async Task ExecuteAuthAsync()
    {
        ErrorMessage = "";

        // 公共基础校验
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "用户名或密码不能为空";
            return;
        }

        IsLoading = true;

        try
        {
            // 根据模式分发到单一职责函数
            if (IsRegisterMode)
            {
                await PerformRegisterAsync();
            }
            else
            {
                await PerformLoginAsync();
            }
        }
        catch (Exception ex)
        {
            // 全局异常兜底
            ErrorMessage = $"系统错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // 注册
    private async Task PerformRegisterAsync()
    {
        // 注册特有的校验
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "两次输入的密码不一致";
            return;
        }

        try
        {
            // 调用 API
            await _authApi.RegisterAsync(new LoginRequestDto
            {
                UserName = UserName,
                Password = Password
            });

            // 注册成功的后续处理
            HandleRegisterSuccess();
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"注册失败: {ex.Content ?? ex.ReasonPhrase}";
        }
    }

    // 登录 
    private async Task PerformLoginAsync()
    {
        try
        {
            // 调用 API
            var request = new LoginRequestDto { UserName = UserName, Password = Password };
            var response = await _authApi.LoginAsync(request);

            // 处理 Session
            _userSession.SetSession(response.AccessToken, response.UserName, response.UserId, response.Role);

            // 登录成功的后续处理
            await HandleLoginSuccessAsync();
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"登录失败: {ex.StatusCode} (请检查用户名密码)";
        }
    }

    // 处理注册成功状态
    private void HandleRegisterSuccess()
    {
        IsRegisterMode = false; // 切回登录模式
        ErrorMessage = "注册成功！请登录。";
        Password = "";
        ConfirmPassword = "";
    }

    // 处理登录成功状态
    private async Task HandleLoginSuccessAsync()
    {
        ErrorMessage = "登录成功，正在跳转...";
        await Task.Delay(500); // 给一点视觉反馈时间
        OnLoginSuccess?.Invoke();
    }
}