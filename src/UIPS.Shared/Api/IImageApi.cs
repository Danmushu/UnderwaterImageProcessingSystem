using Refit;
using System.Runtime.InteropServices;
using UIPS.Shared.DTOs;

namespace UIPS.Shared.Api;

public interface IImageApi
{
    /// <summary>
    /// 上传图片文件 (对应后端 POST /api/images/upload)
    /// </summary>
    [Multipart] // 建议加上这个，表明这是多部分表单
    [Post("/api/images/upload")]
    Task<ImageResponseDto> UploadImage([AliasAs("file")] StreamPart file);

    /// <summary>
    /// 获取图片列表
    /// </summary>
    [Get("/api/images")]
    Task<PaginatedResult<ImageDto>> GetImages([Query] PaginatedRequestDto request);

    /// <summary>
    /// 删除图片
    /// </summary>
    [Delete("/api/images/{id}")]
    Task DeleteImage(long id);

    // 切换选中
    [Post("/api/images/{id}/select")]
    Task ToggleSelection(long id);
}