namespace UIPS.API.Services;

/// <summary>
/// 本地文件存储服务实现
/// 将文件存储在服务器本地磁盘上
/// </summary>
public class LocalFileService(IConfiguration configuration) : IFileService
{
    /// <summary>
    /// 上传文件的根目录路径（从配置文件读取）
    /// </summary>
    private readonly string _uploadRootPath = configuration["Storage:UploadPath"]
        ?? throw new InvalidOperationException("配置项 'Storage:UploadPath' 未设置");

    /// <summary>
    /// 保存文件到本地磁盘
    /// 文件存储路径格式：{UploadPath}/{Year}/{Month}/{GUID}.{Extension}
    /// 例如：uploads/2025/01/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg
    /// </summary>
    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName)
    {
        // 确保根上传目录存在
        EnsureDirectoryExists(_uploadRootPath);

        // 生成唯一文件名：GUID + 原始扩展名
        var extension = Path.GetExtension(originalFileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        // 构造日期子目录路径：Year/Month（如 2025/01）
        var datePath = Path.Combine(
            DateTime.UtcNow.Year.ToString(),
            DateTime.UtcNow.Month.ToString("D2") // D2 表示两位数字，如 01、02
        );

        // 完整的目标目录路径
        var targetDirectory = Path.Combine(_uploadRootPath, datePath);
        EnsureDirectoryExists(targetDirectory);

        // 完整的文件物理路径
        var fullPath = Path.Combine(targetDirectory, uniqueFileName);

        // 将文件流写入磁盘
        await using (var fileStreamWriter = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            fileStream.Seek(0, SeekOrigin.Begin); // 确保从流的开头读取
            await fileStream.CopyToAsync(fileStreamWriter);
        }

        // 返回相对路径（统一使用正斜杠，保持跨平台兼容性）
        var relativePath = Path.Combine(datePath, uniqueFileName).Replace('\\', '/');
        return relativePath;
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    public Task<Stream?> GetFileStreamAsync(string relativePath)
    {
        // 构造完整的物理路径
        var fullPath = Path.Combine(_uploadRootPath, relativePath);

        // 检查文件是否存在
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        // 返回文件流（调用者负责释放资源）
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public Task<bool> DeleteFileAsync(string relativePath)
    {
        // 验证路径有效性
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.FromResult(false);
        }

        // 构造完整的物理路径
        var fullPath = Path.Combine(_uploadRootPath, relativePath);

        // 如果文件不存在，视为删除成功（幂等性）
        if (!File.Exists(fullPath))
        {
            return Task.FromResult(true);
        }

        try
        {
            // 删除文件
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            // 删除失败（如文件被占用、权限不足等）
            // TODO: 生产环境应使用日志框架记录异常
            Console.WriteLine($"[错误] 删除文件失败: {fullPath}, 原因: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 确保目录存在，如果不存在则创建
    /// </summary>
    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}