using Refit;
using System.Threading.Tasks;

namespace UIPS.Client.Services
{
    public interface IImageApi
    {
        /// <summary>
        /// 上传图片文件 (对应后端 POST /api/images/upload)
        /// </summary>
        [Multipart]
        [Post("/api/images/upload")]
        Task<dynamic> UploadImage([AliasAs("file")] StreamPart file);

        /// <summary>
        /// 批量上传图片
        /// </summary>
        [Multipart]
        [Post("/api/images/upload/batch")]
        Task<dynamic> UploadBatch([AliasAs("files")] IEnumerable<StreamPart> files);

        /// <summary>
        /// 获取图片列表
        /// </summary>
        [Get("/api/images")]
        Task<dynamic> GetImages([Query("PageIndex")] int pageIndex, [Query("PageSize")] int pageSize);

        /// <summary>
        /// 获取唯一文件名列表
        /// </summary>
        [Get("/api/images/filenames")]
        Task<dynamic> GetUniqueFileNames();

        /// <summary>
        /// 根据文件名获取图片列表
        /// </summary>
        [Get("/api/images/by-filename/{fileName}")]
        Task<dynamic> GetImagesByFileName(string fileName);

        /// <summary>
        /// 删除图片
        /// </summary>
        [Delete("/api/images/{id}")]
        Task DeleteImage(long id);

        /// <summary> 
        /// 切换选中 (点赞/标记) 
        /// </summary> 
        [Post("/api/images/{id}/select")] 
        Task ToggleSelection(long id); 
    }
}