using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using UIPS.Client.Services;
using System.Text.Json;

namespace UIPS.Client.ViewModels;


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

    // 是否显示密码（明文）
    [ObservableProperty]
    private bool _showPassword = false;

    // 切换密码显示命令
    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

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
            // 用匿名对象传参
            // Refit 会自动将其序列化为 JSON: { "userName": "...", "password": "..." }
            var payload = new
            {
                UserName = this.UserName,
                Password = this.Password
            };

            // 调用 API IAuthApi 需改为接收 object 或 dynamic
            await _authApi.RegisterAsync(payload);

            // 注册成功的后续处理（API 调用成功即表示注册成功）
            HandleRegisterSuccess();
        }
        catch (ApiException ex)
        {
            // 只有 API 抛出异常时才是真正的注册失败
            var errorContent = ex.Content;
            if (string.IsNullOrWhiteSpace(errorContent) || errorContent == "OK")
            {
                // 如果错误内容为空或为 "OK"，说明实际上是成功的
                HandleRegisterSuccess();
            }
            else
            {
                ErrorMessage = $"注册失败: {errorContent}";
            }
        }
    }

    // 登录 
    private async Task PerformLoginAsync()
    {
        try
        {
            // 使用匿名对象传参
            var payload = new
            {
                UserName = this.UserName,
                Password = this.Password
            };

            // 调用 API (IAuthApi 需改为返回 dynamic)
            // 注意：backend 返回的 JSON 属性通常是小写开头 (camelCase)
            dynamic response = await _authApi.LoginAsync(payload);

            // 解析动态对象 
            // 必须严格匹配后端返回的 JSON 字段名
            var json = (JsonElement)response;

            // 检查属性是否存在，避免 KeyNotFound 崩溃
            if (json.TryGetProperty("accessToken", out var tokenProp) &&
                json.TryGetProperty("userName", out var userProp) &&
                json.TryGetProperty("userId", out var idProp) &&
                json.TryGetProperty("role", out var roleProp))
            {
                string token = tokenProp.GetString() ?? "";
                string userName = userProp.GetString() ?? "";
                long userId = idProp.GetInt64(); // 注意类型匹配
                string role = roleProp.GetString() ?? "";

                // 处理 Session
                _userSession.SetSession(token, userName, userId, role);

                // 登录成功的后续处理
                await HandleLoginSuccessAsync();
            }
            else { ErrorMessage = "登录失败：服务器返回的数据格式不正确"; }
        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            // 捕获 dynamic 解析失败的错误 (比如后端改了字段名)
            ErrorMessage = "登录失败: 服务器响应格式不匹配";
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