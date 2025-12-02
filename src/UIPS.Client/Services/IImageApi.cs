using Refit;
using System.Threading.Tasks;

namespace UIPS.Client.Services // 建议命名空间改为 Client 端的
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
        /// 获取图片列表
        /// 这里的 pageIndex 和 pageSize 会自动拼接成URL参数：/api/images?pageIndex=1&pageSize=10
        /// </summary>
        [Get("/api/images")]
        Task<dynamic> GetImages([Query] int pageIndex, [Query] int pageSize);

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