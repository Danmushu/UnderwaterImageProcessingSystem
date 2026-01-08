namespace UIPS.API.DTOs;

/// <summary>
/// 重置密码 DTO
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// 新密码
    /// </summary>
    public required string NewPassword { get; set; }
}
