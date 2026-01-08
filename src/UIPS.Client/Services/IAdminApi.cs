using Refit;

namespace UIPS.Client.Services;

/// <summary>
/// 管理员 API 接口
/// </summary>
public interface IAdminApi
{
    /// <summary>
    /// 获取所有用户列表
    /// </summary>
    [Get("/api/admin/users")]
    Task<dynamic> GetUsersAsync([Query] int pageIndex, [Query] int pageSize);

    /// <summary>
    /// 获取统计信息
    /// </summary>
    [Get("/api/admin/statistics")]
    Task<dynamic> GetStatisticsAsync();

    /// <summary>
    /// 更新用户角色
    /// </summary>
    [Put("/api/admin/users/{userId}/role")]
    Task UpdateUserRoleAsync(int userId, [Body] object dto);

    /// <summary>
    /// 删除用户
    /// </summary>
    [Delete("/api/admin/users/{userId}")]
    Task DeleteUserAsync(int userId);

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [Post("/api/admin/users/{userId}/reset-password")]
    Task ResetUserPasswordAsync(int userId, [Body] object dto);

    /// <summary>
    /// 获取所有图片（管理员视图）
    /// </summary>
    [Get("/api/admin/images")]
    Task<dynamic> GetAllImagesAsync([Query] int pageIndex, [Query] int pageSize);

    /// <summary>
    /// 批量删除图片
    /// </summary>
    [Post("/api/admin/images/batch-delete")]
    Task BatchDeleteImagesAsync([Body] object dto);
}
