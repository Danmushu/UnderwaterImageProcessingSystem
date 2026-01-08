using Refit;

namespace UIPS.Client.Services;

/// <summary>
/// 认证 API 接口（RESTful 风格）
/// </summary>
public interface IAuthApi
{
    /// <summary>
    /// 用户登录（创建认证令牌）
    /// POST /api/auth/token
    /// </summary>
    [Post("/api/auth/token")]
    Task<dynamic> LoginAsync([Body] object request);

    /// <summary>
    /// 用户注册（创建用户资源）
    /// POST /api/auth/users
    /// </summary>
    [Post("/api/auth/users")]
    Task<dynamic> RegisterAsync([Body] object request);
}