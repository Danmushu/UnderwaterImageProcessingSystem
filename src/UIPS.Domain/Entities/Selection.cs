namespace UIPS.Domain.Entities;

public class Selection
{
    public int Id { get; set; }

    // 关联的用户 ID  User.Id 是 long
    public long UserId { get; set; }

    // 关联的图片 ID Image.Id 是 int
    public int ImageId { get; set; }

    // 选择时间
    public DateTime SelectedAt { get; set; } = DateTime.Now;
}