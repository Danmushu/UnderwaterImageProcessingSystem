namespace UIPS.Shared.DTOs;

// 通用分页请求
public class PaginatedRequestDto
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}