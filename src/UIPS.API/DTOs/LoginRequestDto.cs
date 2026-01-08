namespace UIPS.API.DTOs;

/// <summary>
/// 登录/注册请求 DTO
/// 用于接收客户端提交的用户名和密码
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// 密码（明文，仅在传输层使用，服务端会立即进行哈希处理）
    /// </summary>
    public required string Password { get; set; }
}