using Refit;
using System.Threading.Tasks;

namespace UIPS.Client.Services
{
    /// <summary>
    /// 图片 API 接口（RESTful 风格）
    /// </summary>
    public interface IImageApi
    {
        /// <summary>
        /// 上传图片文件（创建图片资源）
        /// POST /api/images
        /// </summary>
        [Multipart]
        [Post("/api/images")]
        Task<dynamic> UploadImage([AliasAs("file")] StreamPart file);

        /// <summary>
        /// 批量上传图片（批量创建图片资源）
        /// POST /api/images/batch
        /// </summary>
        [Multipart]
        [Post("/api/images/batch")]
        Task<dynamic> UploadBatch([AliasAs("files")] IEnumerable<StreamPart> files);

        /// <summary>
        /// 获取图片列表
        /// GET /api/images
        /// </summary>
        [Get("/api/images")]
        Task<dynamic> GetImages([Query("PageIndex")] int pageIndex, [Query("PageSize")] int pageSize);

        /// <summary>
        /// 获取唯一文件名列表
        /// GET /api/images/filenames
        /// </summary>
        [Get("/api/images/filenames")]
        Task<dynamic> GetUniqueFileNames();

        /// <summary>
        /// 根据文件名获取图片列表
        /// GET /api/images/by-filename/{fileName}
        /// </summary>
        [Get("/api/images/by-filename/{fileName}")]
        Task<dynamic> GetImagesByFileName(string fileName);

        /// <summary>
        /// 删除图片
        /// DELETE /api/images/{id}
        /// </summary>
        [Delete("/api/images/{id}")]
        Task DeleteImage(long id);

        /// <summary> 
        /// 切换收藏状态
        /// PUT /api/images/{id}/favourite
        /// </summary> 
        [Put("/api/images/{id}/favourite")] 
        Task ToggleSelection(long id); 
    }
}