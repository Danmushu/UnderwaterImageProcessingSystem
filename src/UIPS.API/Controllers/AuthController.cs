using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UIPS.API.Models;
using UIPS.API.Services;
using UIPS.API.DTOs;

namespace UIPS.API.Controllers;

/// <summary>
/// 认证控制器：处理用户注册、登录等身份验证相关操作
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(UipsDbContext context, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// 用户注册接口
    /// </summary>
    /// <param name="request">包含用户名和密码的注册请求</param>
    /// <returns>注册成功或失败的响应</returns>
    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register(LoginRequestDto request)
    {
        // 检查用户名是否已被注册
        if (await context.Users.AnyAsync(u => u.UserName == request.UserName))
        {
            return BadRequest("用户名已存在");
        }

        // 创建新用户实体，使用 BCrypt 对密码进行加密存储
        var user = new User
        {
            UserName = request.UserName,
            // BCrypt 自动加盐并生成哈希值，确保密码安全
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User" // 默认角色为普通用户
        };

        // 将新用户保存到数据库
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok("注册成功");
    }

    /// <summary>
    /// 用户登录接口
    /// </summary>
    /// <param name="request">包含用户名和密码的登录请求</param>
    /// <returns>包含 JWT Token 和用户信息的登录响应</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        // 根据用户名查找用户
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        // 验证用户是否存在以及密码是否正确
        // BCrypt.Verify 会将明文密码与存储的哈希值进行比对
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("用户名或密码错误");
        }

        // 生成 JWT 访问令牌
        var token = GenerateJwtToken(user);

        // 构造并返回登录响应 DTO
        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = "待实现...", // TODO: 实现刷新令牌机制
            UserId = user.Id,
            UserName = user.UserName,
            ExpiresIn = 120 * 60, // Token 有效期（秒）
            Role = user.Role
        });
    }

    /// <summary>
    /// 生成 JWT 访问令牌
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>JWT Token 字符串</returns>
    private string GenerateJwtToken(User user)
    {
        // 从配置文件读取 JWT 相关设置
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        // 定义 JWT 声明（Claims）：包含用户身份信息
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // 用户 ID（Subject）
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName), // 用户名
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT 唯一标识符
            new Claim(ClaimTypes.Role, user.Role) // 用户角色（用于授权）
        };

        // 创建签名凭证：使用对称密钥和 HMAC-SHA256 算法
        var signingKey = new SymmetricSecurityKey(key);
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // 组装 JWT Token
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"], // 令牌签发者
            audience: jwtSettings["Audience"], // 令牌受众
            claims: claims, // 用户声明
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)), // 过期时间
            signingCredentials: signingCredentials // 签名凭证
        );

        // 将 Token 对象序列化为字符串并返回
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}