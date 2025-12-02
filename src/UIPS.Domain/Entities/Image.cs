namespace UIPS.Domain.Entities;

public class Image
{
    public int Id { get; set; }

    // 原始文件名 (例如: "sea_turtle.jpg")
    public required string OriginalFileName { get; set; }

    // 存储在磁盘上的相对路径 (例如: "uploads/2025/11/guid.jpg")
    // 规划文档强调：通过相对路径索引，避免数据库膨胀 
    public required string StoredPath { get; set; }

    // 上传时间
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // 文件大小 (字节)，方便前端显示
    public long FileSize { get; set; }

    // 关联上传者 (Foreign Key)
    public int OwnerId { get; set; }
    public User? Owner { get; set; } // 导航属性
}