namespace UIPS.API.DTOs;

/// <summary>
/// 图片信息 DTO
/// 用于返回图片列表时的单个图片数据
/// </summary>
public class ImageDto
{
    /// <summary>
    /// 图片 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 原始文件名（用户上传时的文件名）
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// 图片预览 URL（前端可直接使用此 URL 显示图片）
    /// </summary>
    public string PreviewUrl { get; set; } = string.Empty;

    /// <summary>
    /// 上传时间（UTC）
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 当前用户是否已选中（收藏）此图片
    /// </summary>
    public bool IsSelected { get; set; }
}