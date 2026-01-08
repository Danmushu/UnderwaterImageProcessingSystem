namespace UIPS.API.Services;

/// <summary>
/// 文件存储服务接口
/// 定义文件操作的标准契约，支持不同的存储实现（本地、云存储等）
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 保存文件到存储系统
    /// </summary>
    /// <param name="fileStream">文件的二进制流</param>
    /// <param name="originalFileName">原始文件名（包含扩展名）</param>
    /// <returns>文件在存储系统中的相对路径</returns>
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName);

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="relativePath">文件在存储系统中的相对路径</param>
    /// <returns>文件流，如果文件不存在则返回 null</returns>
    Task<Stream?> GetFileStreamAsync(string relativePath);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="relativePath">文件在存储系统中的相对路径</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteFileAsync(string relativePath);
}