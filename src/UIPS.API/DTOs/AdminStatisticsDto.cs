namespace UIPS.API.DTOs;

/// <summary>
/// 管理员统计信息 DTO
/// </summary>
public class AdminStatisticsDto
{
    /// <summary>
    /// 总用户数
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// 管理员数量
    /// </summary>
    public int TotalAdmins { get; set; }

    /// <summary>
    /// 总图片数
    /// </summary>
    public int TotalImages { get; set; }

    /// <summary>
    /// 总收藏数
    /// </summary>
    public int TotalFavourites { get; set; }
}
