namespace UIPS.API.Models;

/// <summary>
/// 用户实体模型
/// 存储用户的基本信息和认证凭据
/// </summary>
public class User
{
    /// <summary>
    /// 用户 ID（主键，自增）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名（唯一标识，用于登录）
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// 密码哈希值（使用 BCrypt 加密存储，永不存储明文密码）
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// 用户角色（如 "User"、"Admin"）
    /// 用于权限控制和授权
    /// </summary>
    public string Role { get; set; } = "User";
}