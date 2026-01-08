namespace UIPS.API.DTOs;

/// <summary>
/// 更新用户角色 DTO
/// </summary>
public class UpdateRoleDto
{
    /// <summary>
    /// 新角色（"User" 或 "Admin"）
    /// </summary>
    public required string Role { get; set; }
}
