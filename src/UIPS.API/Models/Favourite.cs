namespace UIPS.API.Models;

/// <summary>
/// 收藏（选择）实体模型
/// 记录用户对图片的收藏关系
/// </summary>
public class Favourite
{
    /// <summary>
    /// 收藏记录 ID（主键，自增）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户 ID（外键，关联到 User 表）
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 图片 ID（外键，关联到 Image 表）
    /// </summary>
    public int ImageId { get; set; }

    /// <summary>
    /// 收藏时间
    /// </summary>
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}