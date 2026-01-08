namespace UIPS.API.DTOs;

/// <summary>
/// 用户信息 DTO（不包含密码）
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// 用户角色
    /// </summary>
    public string Role { get; set; } = "User";
}
