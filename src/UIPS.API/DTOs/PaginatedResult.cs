namespace UIPS.API.DTOs;

/// <summary>
/// 通用分页结果 DTO
/// 用于返回分页数据及分页信息
/// </summary>
/// <typeparam name="T">数据项类型</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// 当前页的数据项列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总数据量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页数据量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数（计算属性）
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}