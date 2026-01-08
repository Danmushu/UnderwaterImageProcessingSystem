namespace UIPS.API.DTOs;

/// <summary>
/// 批量删除 DTO
/// </summary>
public class BatchDeleteDto
{
    /// <summary>
    /// 要删除的图片 ID 列表
    /// </summary>
    public required List<int> ImageIds { get; set; }
}
