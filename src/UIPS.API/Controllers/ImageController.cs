using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UIPS.API.Services;
using UIPS.API.Models;
using UIPS.API.DTOs;

namespace UIPS.API.Controllers;

[Route("api/images")]
[ApiController]
[Authorize] // 保护：只有带有效 JWT Token 的用户才能访问
public class ImageController(UipsDbContext context, IFileService fileService) : ControllerBase
{
    /// <summary>
    /// 接口：上传单张图片
    /// </summary>
    /// <param name="file">通过 IFormFile 接收文件流</param>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ImageResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("未检测到文件或文件为空。");
        }

        // 获取当前用户 ID (从 JWT Token 的 Claims 中解析)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("无法识别用户身份。");
        }

        // 将文件流交给服务层处理 (保存到磁盘)
        var storedPath = await fileService.SaveFileAsync(file.OpenReadStream(), file.FileName);

        // 创建数据库实体
        var imageEntity = new Image
        {
            OriginalFileName = file.FileName,
            StoredPath = storedPath, // 相对路径
            FileSize = file.Length,
            OwnerId = userId,
            UploadedAt = DateTime.UtcNow
        };

        // 写入数据库
        context.Images.Add(imageEntity);
        await context.SaveChangesAsync();

        // 返回上传成功的元数据给前端
        var responseDto = new ImageResponseDto
        {
            Id = imageEntity.Id,
            OriginalFileName = imageEntity.OriginalFileName,
            UploadedAt = imageEntity.UploadedAt,
            FileSize = imageEntity.FileSize,
            Url = $"/api/images/view/{imageEntity.Id}" // 构造图片访问 URL
        };

        return Ok(responseDto);
    }

    /// <summary>
    /// 接口：根据ID查看图片文件 (提供文件流)
    /// </summary>
    [HttpGet("view/{id}")] // 使用小写路径 segment
    [AllowAnonymous] // 图片查看接口通常允许匿名访问
    [ProducesResponseType(200, Type = typeof(FileStreamResult))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ViewImage(int id)
    {
        // 查找元数据
        var imageEntity = await context.Images.FindAsync(id);
        if (imageEntity == null) return NotFound("图片元数据不存在。");
 

        // 获取文件流
        var fileStream = await fileService.GetFileStreamAsync(imageEntity.StoredPath);

        if (fileStream == null) return NotFound("文件在磁盘上丢失。");

        // 返回文件流，并猜测 Content-Type (浏览器需要这个信息才能正确显示图片)
        var contentType = GetContentType(imageEntity.OriginalFileName);
        return File(fileStream, contentType);
    }

    // 用于猜测 Content Type (放在 Controller 类的内部)
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    // 获取图片列表接口
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ImageDto>>> GetImages([FromQuery] PaginatedRequestDto request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

        // 先查出当前用户所有选中的图片 ID (用 HashSet 提高查找性能)
        var mySelectionIds = await context.Favourites
            .Where(s => s.UserId == userId)
            .Select(s => s.ImageId)
            .ToListAsync();
        var selectedSet = mySelectionIds.ToHashSet();

        // 原有的查询逻辑
        var query = context.Images.Where(i => i.OwnerId == userId); // 只能看自己的图
        var totalCount = await query.CountAsync();

        var images = await query
            .OrderByDescending(i => i.UploadedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new ImageDto
            {
                Id = i.Id,
                OriginalFileName = i.OriginalFileName,
                UploadedAt = i.UploadedAt,
                FileSize = i.FileSize,
                PreviewUrl = $"/api/images/{i.Id}/file"
                // 这里没法直接在 SQL 阶段高效地与 HashSet 对比，我们拿到内存后再赋值
            })
            .ToListAsync();

        // 内存中填充 IsSelected 状态
        foreach (var img in images)
        {
            // 因为 ImageDto.Id 是 long, set 里是 int，转一下
            img.IsSelected = selectedSet.Contains((int)img.Id);
        }

        return Ok(new PaginatedResult<ImageDto>
        {
            Items = images,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        });
    }

    // 获取图片文件流接口 (用于前端 <img> 标签显示)
    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetImageFile(long id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

        // 查库：确保图片存在且属于该用户 
        // TODO: 这里强制类型转换了，其实用long更好，看看后期需不需修改
        var image = await context.Images.FindAsync((int)id);
        if (image == null) return NotFound("图片不存在");
        
        // 安全检查：防止看别人的图
        if (image.OwnerId != userId) return Forbid();

        // 调用 LocalFileService 读取物理文件
        // 注意：LocalFileService 之前是用 relativePath 存的，现在取出来用
        var stream = await fileService.GetFileStreamAsync(image.StoredPath);

        if (stream == null) return NotFound("磁盘文件丢失");

        // 动态判断 ContentType
        var extension = Path.GetExtension(image.OriginalFileName).ToLower();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        // 返回文件流
        return File(stream, contentType); // 这里的 File 是 ControllerBase 的方法
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(long id)
    {
        // 获取当前用户 ID
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

        // 查库 TODO:这里有强制转换，后期考虑统一改成Long
        var image = await context.Images.FindAsync((int)id);
        if (image == null) return NotFound("图片不存在");

        // 鉴权：只能删自己的图
        if (image.OwnerId != userId) return Forbid();

        // 删除物理文件 (Await 异步方法)
        var isFileDeleted = await fileService.DeleteFileAsync(image.StoredPath);

        if (!isFileDeleted)
        {
            // 策略：即使物理文件删除失败，通常也要删除数据库记录，以免用户端看到“僵尸图片”
            // TODO: 这里可以考虑记录日志，或者后续做一个定期清理任务
            // 这里仅做控制台警告
            Console.WriteLine($"[警告] 数据库记录即将删除，但物理文件删除失败: {image.StoredPath}");
        }

        // 删除数据库记录
        context.Images.Remove(image);
        await context.SaveChangesAsync();

        return NoContent(); // 返回 204
    }

    // 切换选中状态接口
    [HttpPost("{id}/select")]
    public async Task<IActionResult> ToggleSelection(long id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

        var imageId = (int)id;

        // 检查图片是否存在
        var image = await context.Images.FindAsync(imageId);
        if (image == null) return NotFound("图片不存在");

        // 检查是否已经选过 (查 Selections 表)
        var existingSelection = await context.Favourites
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ImageId == imageId);

        if (existingSelection != null)
        {
            // 如果已选中，则取消选中 (删除记录)
            context.Favourites.Remove(existingSelection);
            await context.SaveChangesAsync();
            return Ok(new { IsSelected = false }); // 告诉前端现在的状态
        }
        else
        {
            // 如果未选中，则添加记录
            context.Favourites.Add(new Favourite
            {
                UserId = userId,
                ImageId = imageId
            });
            await context.SaveChangesAsync();
            return Ok(new { IsSelected = true }); // 告诉前端现在的状态
        }
    }
}