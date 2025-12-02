namespace UIPS.API.Services;

/// <summary>
/// 文件存储服务接口
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 保存文件到磁盘
    /// </summary>
    /// <param name="fileStream">文件的二进制流</param>
    /// <param name="originalFileName">原始文件名</param>
    /// <returns>返回存储在磁盘上的相对路径</returns>
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName);

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="relativePath">文件在磁盘上的相对路径</param>
    /// <returns>文件流</returns>
    Task<Stream?> GetFileStreamAsync(string relativePath);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="relativePath">文件在磁盘上的相对路径</param>
    Task<bool> DeleteFileAsync(string relativePath);
}