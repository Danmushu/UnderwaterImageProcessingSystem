namespace UIPS.Client.Core.Services;

/// <summary>
/// 全局会话管理 (单例模式)
/// 用于在内存中存储当前登录用户的 Token 和信息
/// </summary>
public class UserSession
{
    // 1. 单例实例
    private static UserSession _instance;
    public static UserSession Current => _instance ??= new UserSession();

    private UserSession() { } // 私有构造函数，防止外部 new

    // 2. 存储的数据
    public string AccessToken { get; private set; }
    public string UserName { get; private set; }
    public int UserId { get; private set; }

    // 3. 登录成功后设置数据
    public void SetSession(string token, string userName, int userId)
    {
        AccessToken = token;
        UserName = userName;
        UserId = userId;
    }

    // 4. 注销时清除数据
    public void ClearSession()
    {
        AccessToken = null;
        UserName = null;
        UserId = 0;
    }

    // 判断是否已登录
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);
}