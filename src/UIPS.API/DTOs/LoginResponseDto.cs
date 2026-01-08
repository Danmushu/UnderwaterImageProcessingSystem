namespace UIPS.API.DTOs;

/// <summary>
/// 登录响应 DTO
/// 包含 JWT Token 和用户基本信息
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT 访问令牌（用于后续 API 请求的身份验证）
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// 刷新令牌（用于在访问令牌过期后获取新令牌）
    /// TODO: 当前未实现，需要后续开发
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public required int UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// 令牌有效期（秒）
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 用户角色（如 "User"、"Admin"）
    /// </summary>
    public string Role { get; set; } = "User";
}