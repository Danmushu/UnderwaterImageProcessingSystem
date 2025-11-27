using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UIPS.Domain.Entities;
using UIPS.Infrastructure.Data;
using UIPS.Shared.DTOs;

namespace UIPS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(UipsDbContext context, IConfiguration configuration) : ControllerBase
{
    // === 注册接口 ===
    [HttpPost("register")]
    public async Task<IActionResult> Register(LoginRequestDto request)
    {
        // 检查是否存在
        if (await context.Users.AnyAsync(u => u.UserName == request.UserName))
        {
            return BadRequest("用户名已存在");
        }

        // 创建用户 (使用 BCrypt 加密密码)
        var user = new User
        {
            UserName = request.UserName,
            // 核心安全点：自动加盐哈希
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        // 存入数据库
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok("注册成功");
    }

    // 登录接口
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        // 查找用户
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        // 验证密码 (使用 BCrypt 验证明文是否匹配哈希值)
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("用户名或密码错误");
        }

        // 生成 JWT 令牌
        var token = GenerateJwtToken(user);

        // 返回标准 DTO
        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = "待实现...", // TODO 实现刷新令牌
            UserId = user.Id,
            UserName = user.UserName,
            ExpiresIn = 120 * 60 // 秒
        });
    }

    // 生成 Token
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

        // 定义令牌里的声明 - 相当于身份证上的信息
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // 用户 ID
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName), // 用户名
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // 唯一标识
        };

        // 签名证书
        var signingKey = new SymmetricSecurityKey(key); // 对称密钥
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256); // 签名算法 

        // 组装令牌
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"], // 签发者
            audience: jwtSettings["Audience"], // 受众
            claims: claims, // 用户声明
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)), // 过期时间
            signingCredentials: creds // 签名凭证
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}