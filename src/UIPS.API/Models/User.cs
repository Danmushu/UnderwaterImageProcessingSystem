namespace UIPS.API.Models
{
    public class User
    {
        public int Id { get; set; }

        // required 强制初始化
        public required string UserName { get; set; }

        // 存储加密后的哈希值，而不是明文密码
        public required string PasswordHash { get; set; }

        // 简单的角色管理
        public string Role { get; set; } = "User";
    }
}
