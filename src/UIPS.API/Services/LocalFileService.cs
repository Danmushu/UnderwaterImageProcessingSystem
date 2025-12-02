using Microsoft.Extensions.Options;

namespace UIPS.API.Services;

public class LocalFileService(IConfiguration configuration) : IFileService
{
    // 获取配置中定义的根上传目录 (如: "uploads")
    private readonly string _uploadRootPath = configuration["Storage:UploadPath"]
                                            ?? throw new InvalidOperationException("UploadPath not configured.");

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName)
    {
        // 确保上传目录存在
        if (!Directory.Exists(_uploadRootPath))
        {
            Directory.CreateDirectory(_uploadRootPath);
        }

        // 生成唯一文件名，防止冲突
        // 使用 GUID + 原始扩展名
        var extension = Path.GetExtension(originalFileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        // 构造完整的物理路径
        // 存储路径格式: uploads/2025/11/uniqueId.jpg
        var datePath = Path.Combine(DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("D2"));
        var finalDirectory = Path.Combine(_uploadRootPath, datePath);

        // 再次确保日期目录存在
        if (!Directory.Exists(finalDirectory))
        {
            Directory.CreateDirectory(finalDirectory);
        }

        var fullPath = Path.Combine(finalDirectory, uniqueFileName);

        // 将文件流写入磁盘
        using (var file = new FileStream(fullPath, FileMode.Create))
        {
            fileStream.Seek(0, SeekOrigin.Begin); // 确保从头开始读
            await fileStream.CopyToAsync(file);
        }

        // 返回相对路径给数据库存储
        // Path.Combine 可能会用反斜杠，这里强制用正斜杠以保持 URL 兼容性
        var relativePath = Path.Combine(datePath, uniqueFileName).Replace('\\', '/');
        return relativePath;
    }

    public Task<Stream?> GetFileStreamAsync(string relativePath)
    {
        var fullPath = Path.Combine(_uploadRootPath, relativePath);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        //这里返回 FileStream，调用者有责任 Dispose
        return Task.FromResult<Stream?>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task<bool> DeleteFileAsync(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return Task.FromResult(false); // 路径为空，删除失败

        var fullPath = Path.Combine(_uploadRootPath, relativePath);

        // 如果文件本来就不存在，视作删除成功 (幂等性)
        if (!File.Exists(fullPath))
            return Task.FromResult(true);

        try
        {
            // 本地文件删除是同步的，用 Task.FromResult 包装以适配接口
            File.Delete(fullPath); // 同步删除
            return Task.FromResult(true); // 成功
        }
        catch (Exception ex)
        {
            // 生产环境建议在这里记录日志：Logger.LogError(ex, ...)
            Console.WriteLine($"删除文件异常: {ex.Message}");
            return Task.FromResult(false); // 失败（例如文件被占用）
        }
    }
}