namespace UIPS.Shared.DTOs;

// 单张图片信息
public class ImageDto
{
    public long Id { get; set; } // TODO：后端是 int, DTO 保持 long 也没事，后期再考虑是否修改
    public string OriginalFileName { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty; // 前端用来展示图片的 URL
    public DateTime UploadedAt { get; set; }
    public long FileSize { get; set; }

   
    public bool IsSelected { get; set; }// 当前用户是否已选中
}