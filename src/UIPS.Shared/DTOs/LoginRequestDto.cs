namespace UIPS.Shared.DTOs;

// 简单的登录请求载荷
public class LoginRequestDto
{
    // 使用 required 关键字确保调用者必须初始化这些属性
    public required string UserName { get; set; }
    public required string Password { get; set; }
}