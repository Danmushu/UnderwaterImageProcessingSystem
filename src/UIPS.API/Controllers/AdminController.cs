using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UIPS.API.Models;
using UIPS.API.Services;
using UIPS.API.DTOs;

namespace UIPS.API.Controllers;

/// <summary>
/// 管理员控制器：处理用户管理等管理员专属功能
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] // 仅管理员可访问
public class AdminController(UipsDbContext context) : ControllerBase
{
    #region 用户管理

    /// <summary>
    /// 获取所有用户列表（分页）
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PaginatedResult<UserDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<PaginatedResult<UserDto>>> GetUsers([FromQuery] PaginatedRequestDto request)
    {
        var query = context.Users.AsQueryable();
        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Id)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Role = u.Role
            })
            .ToListAsync();

        return Ok(new PaginatedResult<UserDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        });
    }

    /// <summary>
    /// 获取用户统计信息
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(AdminStatisticsDto), 200)]
    public async Task<ActionResult<AdminStatisticsDto>> GetStatistics()
    {
        var totalUsers = await context.Users.CountAsync();
        var totalAdmins = await context.Users.CountAsync(u => u.Role == "Admin");
        var totalImages = await context.Images.CountAsync();
        var totalFavourites = await context.Favourites.CountAsync();

        return Ok(new AdminStatisticsDto
        {
            TotalUsers = totalUsers,
            TotalAdmins = totalAdmins,
            TotalImages = totalImages,
            TotalFavourites = totalFavourites
        });
    }

    /// <summary>
    /// 更新用户角色
    /// </summary>
    [HttpPut("users/{userId}/role")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateRoleDto dto)
    {
        // 验证角色值
        if (dto.Role != "User" && dto.Role != "Admin")
        {
            return BadRequest("角色只能是 'User' 或 'Admin'");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("用户不存在");
        }

        // 防止管理员修改自己的角色
        var currentUserId = GetCurrentUserId();
        if (userId == currentUserId)
        {
            return BadRequest("不能修改自己的角色");
        }

        user.Role = dto.Role;
        await context.SaveChangesAsync();

        return Ok(new { Message = $"用户 {user.UserName} 的角色已更新为 {dto.Role}" });
    }

    /// <summary>
    /// 删除用户（同时删除其所有图片和收藏）
    /// </summary>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("用户不存在");
        }

        // 防止管理员删除自己
        var currentUserId = GetCurrentUserId();
        if (userId == currentUserId)
        {
            return BadRequest("不能删除自己的账号");
        }

        // 删除用户的所有图片
        var userImages = await context.Images.Where(i => i.OwnerId == userId).ToListAsync();
        context.Images.RemoveRange(userImages);

        // 删除用户的所有收藏
        var userFavourites = await context.Favourites.Where(f => f.UserId == userId).ToListAsync();
        context.Favourites.RemoveRange(userFavourites);

        // 删除用户
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 重置用户密码（管理员功能）
    /// </summary>
    [HttpPut("users/{userId}/password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResetUserPassword(int userId, [FromBody] ResetPasswordDto dto)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("用户不存在");
        }

        // 使用 BCrypt 加密新密码
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await context.SaveChangesAsync();

        return Ok(new { Message = $"用户 {user.UserName} 的密码已重置" });
    }

    #endregion

    #region 图片管理

    /// <summary>
    /// 获取所有图片（管理员视图，可以看到所有用户的图片）
    /// </summary>
    [HttpGet("images")]
    [ProducesResponseType(typeof(PaginatedResult<AdminImageDto>), 200)]
    public async Task<ActionResult<PaginatedResult<AdminImageDto>>> GetAllImages([FromQuery] PaginatedRequestDto request)
    {
        var query = context.Images.Include(i => i.Owner).AsQueryable();
        var totalCount = await query.CountAsync();

        var images = await query
            .OrderByDescending(i => i.UploadedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new AdminImageDto
            {
                Id = i.Id,
                OriginalFileName = i.OriginalFileName,
                UploadedAt = i.UploadedAt,
                FileSize = i.FileSize,
                OwnerName = i.Owner!.UserName,
                OwnerId = i.OwnerId,
                PreviewUrl = $"/api/images/{i.Id}/file"
            })
            .ToListAsync();

        return Ok(new PaginatedResult<AdminImageDto>
        {
            Items = images,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        });
    }

    /// <summary>
    /// 批量删除图片
    /// </summary>
    [HttpDelete("images/batch")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> BatchDeleteImages([FromBody] BatchDeleteDto dto)
    {
        var images = await context.Images.Where(i => dto.ImageIds.Contains(i.Id)).ToListAsync();
        
        context.Images.RemoveRange(images);
        await context.SaveChangesAsync();

        return Ok(new { Message = $"成功删除 {images.Count} 张图片" });
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 从 JWT Claims 中获取当前用户 ID
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return 0;
    }

    #endregion
}
