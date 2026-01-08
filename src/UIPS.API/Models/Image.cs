namespace UIPS.API.Models;

/// <summary>
/// 图片实体模型
/// 存储图片的元数据信息
/// </summary>
public class Image
{
    /// <summary>
    /// 图片 ID（主键，自增）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 原始文件名（用户上传时的文件名，如 "sea_turtle.jpg"）
    /// </summary>
    public required string OriginalFileName { get; set; }

    /// <summary>
    /// 存储路径（相对路径，如 "2025/01/guid.jpg"）
    /// 通过相对路径索引，避免数据库存储完整路径导致的膨胀
    /// </summary>
    public required string StoredPath { get; set; }

    /// <summary>
    /// 上传时间（UTC）
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 所有者用户 ID（外键，关联到 User 表）
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 所有者导航属性（EF Core 导航属性，用于关联查询）
    /// </summary>
    public User? Owner { get; set; }
}