using Refit;

namespace UIPS.Client.Services;

/// <summary>
/// 管理员 API 接口（RESTful 风格）
/// </summary>
public interface IAdminApi
{
    /// <summary>
    /// 获取所有用户列表
    /// GET /api/admin/users
    /// </summary>
    [Get("/api/admin/users")]
    Task<dynamic> GetUsersAsync([Query] int pageIndex, [Query] int pageSize);

    /// <summary>
    /// 获取统计信息
    /// GET /api/admin/statistics
    /// </summary>
    [Get("/api/admin/statistics")]
    Task<dynamic> GetStatisticsAsync();

    /// <summary>
    /// 更新用户角色
    /// PUT /api/admin/users/{userId}/role
    /// </summary>
    [Put("/api/admin/users/{userId}/role")]
    Task UpdateUserRoleAsync(int userId, [Body] object dto);

    /// <summary>
    /// 删除用户
    /// DELETE /api/admin/users/{userId}
    /// </summary>
    [Delete("/api/admin/users/{userId}")]
    Task DeleteUserAsync(int userId);

    /// <summary>
    /// 重置用户密码
    /// PUT /api/admin/users/{userId}/password
    /// </summary>
    [Put("/api/admin/users/{userId}/password")]
    Task ResetUserPasswordAsync(int userId, [Body] object dto);

    /// <summary>
    /// 获取所有图片（管理员视图）
    /// GET /api/admin/images
    /// </summary>
    [Get("/api/admin/images")]
    Task<dynamic> GetAllImagesAsync([Query] int pageIndex, [Query] int pageSize);

    /// <summary>
    /// 批量删除图片
    /// DELETE /api/admin/images/batch
    /// </summary>
    [Delete("/api/admin/images/batch")]
    Task BatchDeleteImagesAsync([Body] object dto);
}
