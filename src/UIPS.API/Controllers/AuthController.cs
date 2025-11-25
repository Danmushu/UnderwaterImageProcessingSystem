using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UIPS.Domain.Entities;
using UIPS.Infrastructure.Data;
using UIPS.Shared.DTOs;

namespace UIPS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(UipsDbContext context) : ControllerBase
{
    // 测试接口：注册一个新用户
    [HttpPost("register-test")]
    public async Task<IActionResult> RegisterTest(LoginRequestDto request)
    {
        // 1. 检查用户是否已存在
        if (await context.Users.AnyAsync(u => u.UserName == request.UserName))
        {
            return BadRequest("用户已存在！");
        }

        // 2. 创建用户实体
        var user = new User
        {
            UserName = request.UserName,
            // TODO 暂时用来测试，实际开发必须使用哈希加密 (BCrypt)
            PasswordHash = request.Password + "_hashed_test",
            Role = "User"
        };

        // 3. 写入数据库
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok(new { Message = "注册成功！数据库写入正常。", UserId = user.Id });
    }

    // 测试接口：查看所有用户
    [HttpGet("users-test")]
    public async Task<IActionResult> GetAllUsersTest()
    {
        var users = await context.Users.ToListAsync();
        return Ok(users);
    }
}