namespace UIPS.API.DTOs;

/// <summary>
/// 通用分页请求 DTO
/// 用于接收客户端的分页参数
/// </summary>
public class PaginatedRequestDto
{
    /// <summary>
    /// 页码（从 1 开始）
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页数据量
    /// </summary>
    public int PageSize { get; set; } = 10;
}