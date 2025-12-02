using System.Net.Http.Headers;

namespace UIPS.Client.Core.Services;

/// <summary>
/// DelegatingHandler 拦截器，用于在 HTTP 请求头中自动添加 JWT Token。
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    // 定义字段来保存注入的 UserSession 实例
    private readonly UserSession _userSession;

    // 构造函数注入
    // 当 Refit 客户端创建时，DI 容器会自动把全局唯一的 UserSession 传进来
    public AuthHeaderHandler(UserSession userSession)
    {
        _userSession = userSession;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 从注入的实例 (_userSession) 读取 Token
        var token = _userSession.AccessToken;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}