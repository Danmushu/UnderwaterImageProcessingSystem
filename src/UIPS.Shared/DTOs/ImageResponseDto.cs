using System;

namespace UIPS.Shared.DTOs;

/// <summary>
/// 上传成功后返回给前端的图片元数据
/// </summary>
public class ImageResponseDto
{
    public int Id { get; set; }
    public required string OriginalFileName { get; set; }
    public DateTime UploadedAt { get; set; }
    public long FileSize { get; set; }

    // 图片的访问 URL，前端可以直接用这个地址显示图片
    public required string Url { get; set; }
}