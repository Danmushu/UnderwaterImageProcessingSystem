using System.Data;
using System.Security.Principal;

namespace UIPS.Client.Core.Services;

/// <summary>
/// 全局会话管理
/// 注意：已移除静态单例属性，完全由 DI 容器 (App.xaml.cs) 管理其生命周期为 Singleton
/// </summary>
public class UserSession
{
    // 必须是 public 构造函数，DI 容器才能创建它
    public UserSession()
    {
    }

    // 移除 static Current 和 _instance
    // 这样强制开发者必须通过构造函数注入来获取实例，确保全应用使用同一个对象

    // 存储的数据
    public string? AccessToken { get; private set; }
    public string? UserName { get; private set; }
    public int UserId { get; private set; }
    public string Role { get; private set; } = "User";
    public bool IsAdmin => Role == "Admin";

    // 登录成功后设置数据
    public void SetSession(string token, string userName, int userId, string role)
    {
        AccessToken = token;
        UserName = userName;
        UserId = userId;
        Role = role; // 角色
    }

    // 注销时清除数据
    public void ClearSession()
    {
        AccessToken = null;
        UserName = null;
        UserId = 0; 
        Role = "User";
    }

    // 判断是否已登录
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);
}