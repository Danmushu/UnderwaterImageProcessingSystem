namespace UIPS.Shared.DTOs;

public class LoginResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; } // 双令牌机制 
    public required int UserId { get; set; }
    public required string UserName { get; set; }
    public int ExpiresIn { get; set; }
    public string Role { get; set; } = "User"; // default: User
}