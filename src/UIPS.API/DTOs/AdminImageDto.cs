namespace UIPS.API.DTOs;

/// <summary>
/// 管理员视图的图片信息 DTO
/// 包含所有者信息
/// </summary>
public class AdminImageDto
{
    /// <summary>
    /// 图片 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 原始文件名
    /// </summary>
    public required string OriginalFileName { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 所有者用户名
    /// </summary>
    public required string OwnerName { get; set; }

    /// <summary>
    /// 所有者 ID
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 预览 URL
    /// </summary>
    public required string PreviewUrl { get; set; }
}
