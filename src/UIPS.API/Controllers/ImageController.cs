using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UIPS.API.Services;
using UIPS.API.Models;
using UIPS.API.DTOs;

namespace UIPS.API.Controllers;

/// <summary>
/// 图片管理控制器：处理图片的上传、查看、删除、选择等操作
/// </summary>
[Route("api/images")]
[ApiController]
[Authorize] // 所有接口默认需要 JWT 认证
public class ImageController(UipsDbContext context, IFileService fileService) : ControllerBase
{
    #region 上传相关接口

    /// <summary>
    /// 上传单张图片
    /// </summary>
    /// <param name="file">上传的图片文件</param>
    /// <returns>上传成功后的图片元数据</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ImageResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        // 验证文件是否有效
        if (file == null || file.Length == 0)
        {
            return BadRequest("未检测到文件或文件为空");
        }

        // 获取当前登录用户的 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("无法识别用户身份");
        }

        // 保存文件到磁盘，获取存储路径
        var storedPath = await fileService.SaveFileAsync(file.OpenReadStream(), file.FileName);

        // 创建图片数据库实体
        var imageEntity = new Image
        {
            OriginalFileName = file.FileName,
            StoredPath = storedPath, // 相对路径，如 "2025/01/guid.jpg"
            FileSize = file.Length,
            OwnerId = userId.Value,
            UploadedAt = DateTime.UtcNow
        };

        // 保存到数据库
        context.Images.Add(imageEntity);
        await context.SaveChangesAsync();

        // 构造响应 DTO 并返回
        var responseDto = CreateImageResponseDto(imageEntity);
        return Ok(responseDto);
    }

    /// <summary>
    /// 批量上传图片
    /// </summary>
    /// <param name="files">上传的图片文件列表</param>
    /// <returns>所有上传成功的图片元数据列表</returns>
    [HttpPost("upload/batch")]
    [ProducesResponseType(typeof(List<ImageResponseDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UploadBatch([FromForm] List<IFormFile> files)
    {
        // 验证文件列表是否有效
        if (files == null || files.Count == 0)
        {
            return BadRequest("未检测到文件");
        }

        // 获取当前登录用户的 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("无法识别用户身份");
        }

        var responses = new List<ImageResponseDto>();

        // 遍历处理每个文件
        foreach (var file in files)
        {
            // 跳过空文件
            if (file.Length == 0) continue;

            // 保存文件到磁盘
            var storedPath = await fileService.SaveFileAsync(file.OpenReadStream(), file.FileName);

            // 创建数据库实体
            var imageEntity = new Image
            {
                OriginalFileName = file.FileName,
                StoredPath = storedPath,
                FileSize = file.Length,
                OwnerId = userId.Value,
                UploadedAt = DateTime.UtcNow
            };

            context.Images.Add(imageEntity);
            await context.SaveChangesAsync();

            // 添加到响应列表
            responses.Add(CreateImageResponseDto(imageEntity));
        }

        return Ok(responses);
    }

    #endregion

    #region 查询相关接口

    /// <summary>
    /// 获取当前用户的图片列表（分页）
    /// </summary>
    /// <param name="request">分页请求参数</param>
    /// <returns>分页后的图片列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ImageDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<PaginatedResult<ImageDto>>> GetImages([FromQuery] PaginatedRequestDto request)
    {
        // 获取当前用户 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // 查询当前用户已选中的图片 ID 集合（用于标记 IsSelected 状态）
        var selectedImageIds = await GetUserSelectedImageIdsAsync(userId.Value);

        // 构建查询：只查询当前用户的图片
        var query = context.Images.Where(i => i.OwnerId == userId.Value);
        var totalCount = await query.CountAsync();

        // 分页查询并投影到 DTO
        var images = await query
            .OrderByDescending(i => i.UploadedAt) // 按上传时间倒序
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new ImageDto
            {
                Id = i.Id,
                OriginalFileName = i.OriginalFileName,
                UploadedAt = i.UploadedAt,
                FileSize = i.FileSize,
                PreviewUrl = $"/api/images/{i.Id}/file",
                IsSelected = false // 稍后在内存中填充
            })
            .ToListAsync();

        // 在内存中填充 IsSelected 状态
        FillIsSelectedStatus(images, selectedImageIds);

        // 返回分页结果
        return Ok(new PaginatedResult<ImageDto>
        {
            Items = images,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        });
    }

    /// <summary>
    /// 获取当前用户的所有唯一文件名列表
    /// </summary>
    /// <returns>去重后的文件名列表</returns>
    [HttpGet("filenames")]
    [ProducesResponseType(typeof(List<string>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<string>>> GetUniqueFileNames()
    {
        // 获取当前用户 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // 查询当前用户的所有唯一文件名并排序
        var fileNames = await context.Images
            .Where(i => i.OwnerId == userId.Value)
            .Select(i => i.OriginalFileName)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync();

        return Ok(fileNames);
    }

    /// <summary>
    /// 根据文件名获取所有同名图片
    /// </summary>
    /// <param name="fileName">原始文件名</param>
    /// <returns>该文件名对应的所有图片列表</returns>
    [HttpGet("by-filename/{fileName}")]
    [ProducesResponseType(typeof(List<ImageDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<ImageDto>>> GetImagesByFileName(string fileName)
    {
        // 获取当前用户 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // 查询当前用户已选中的图片 ID 集合
        var selectedImageIds = await GetUserSelectedImageIdsAsync(userId.Value);

        // 查询该文件名的所有图片
        var images = await context.Images
            .Where(i => i.OwnerId == userId.Value && i.OriginalFileName == fileName)
            .OrderByDescending(i => i.UploadedAt)
            .Select(i => new ImageDto
            {
                Id = i.Id,
                OriginalFileName = i.OriginalFileName,
                UploadedAt = i.UploadedAt,
                FileSize = i.FileSize,
                PreviewUrl = $"/api/images/{i.Id}/file",
                IsSelected = false // 稍后填充
            })
            .ToListAsync();

        // 填充选中状态
        FillIsSelectedStatus(images, selectedImageIds);

        return Ok(images);
    }

    #endregion

    #region 文件访问接口

    /// <summary>
    /// 查看图片文件（返回文件流）
    /// 此接口允许匿名访问，用于公开展示图片
    /// </summary>
    /// <param name="id">图片 ID</param>
    /// <returns>图片文件流</returns>
    [HttpGet("view/{id}")]
    [AllowAnonymous] // 允许匿名访问
    [ProducesResponseType(200, Type = typeof(FileStreamResult))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ViewImage(int id)
    {
        // 查找图片元数据
        var imageEntity = await context.Images.FindAsync(id);
        if (imageEntity == null)
        {
            return NotFound("图片元数据不存在");
        }

        // 获取文件流
        var fileStream = await fileService.GetFileStreamAsync(imageEntity.StoredPath);
        if (fileStream == null)
        {
            return NotFound("文件在磁盘上丢失");
        }

        // 根据文件扩展名确定 Content-Type
        var contentType = GetContentType(imageEntity.OriginalFileName);
        return File(fileStream, contentType);
    }

    /// <summary>
    /// 获取图片文件流（需要认证和授权）
    /// 用于前端 img 标签显示，确保只能访问自己的图片
    /// </summary>
    /// <param name="id">图片 ID</param>
    /// <returns>图片文件流</returns>
    [HttpGet("{id}/file")]
    [ProducesResponseType(200, Type = typeof(FileStreamResult))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetImageFile(int id)
    {
        // 获取当前用户 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // 查找图片
        var image = await context.Images.FindAsync(id);
        if (image == null)
        {
            return NotFound("图片不存在");
        }

        // 权限检查：只能访问自己的图片
        if (image.OwnerId != userId.Value)
        {
            return Forbid();
        }

        // 获取文件流
        var stream = await fileService.GetFileStreamAsync(image.StoredPath);
        if (stream == null)
        {
            return NotFound("磁盘文件丢失");
        }

        // 确定 Content-Type 并返回文件流
        var contentType = GetContentType(image.OriginalFileName);
        return File(stream, contentType);
    }

    #endregion

    #region 删除和选择操作

    /// <summary>
    /// 删除图片（同时删除数据库记录和物理文件）
    /// </summary>
    /// <param name="id">图片 ID</param>
    /// <returns>204 No Content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteImage(int id)
    {
        // 获取当前用户 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // 查找图片
        var image = await context.Images.FindAsync(id);
        if (image == null)
        {
            return NotFound("图片不存在");
        }

        // 权限检查：只能删除自己的图片
        if (image.OwnerId != userId.Value)
        {
            return Forbid();
        }

        // 尝试删除物理文件
        var isFileDeleted = await fileService.DeleteFileAsync(image.StoredPath);
        if (!isFileDeleted)
        {
            // 即使物理文件删除失败，也继续删除数据库记录，避免"僵尸"数据
            // TODO: 可以考虑记录日志或实现定期清理任务
            Console.WriteLine($"[警告] 物理文件删除失败，但将继续删除数据库记录: {image.StoredPath}");
        }

        // 删除数据库记录
        context.Images.Remove(image);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 切换图片的选中状态（收藏/取消收藏）
    /// </summary>
    /// <param name="id">图片 ID</param>
    /// <returns>切换后的选中状态</returns>
    [HttpPost("{id}/select")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleSelection(int id)
    {
        // 获取当前用户 ID
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // 检查图片是否存在
        var image = await context.Images.FindAsync(id);
        if (image == null)
        {
            return NotFound("图片不存在");
        }

        // 查找是否已存在选择记录
        var existingSelection = await context.Favourites
            .FirstOrDefaultAsync(s => s.UserId == userId.Value && s.ImageId == id);

        if (existingSelection != null)
        {
            // 已选中 -> 取消选中（删除记录）
            context.Favourites.Remove(existingSelection);
            await context.SaveChangesAsync();
            return Ok(new { IsSelected = false });
        }
        else
        {
            // 未选中 -> 添加选中（创建记录）
            context.Favourites.Add(new Favourite
            {
                UserId = userId.Value,
                ImageId = id,
                SelectedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return Ok(new { IsSelected = true });
        }
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 从 JWT Claims 中获取当前用户 ID
    /// </summary>
    /// <returns>用户 ID，如果获取失败则返回 null</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// 获取指定用户已选中的所有图片 ID 集合
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <returns>图片 ID 的 HashSet（用于快速查找）</returns>
    private async Task<HashSet<int>> GetUserSelectedImageIdsAsync(int userId)
    {
        var selectedIds = await context.Favourites
            .Where(f => f.UserId == userId)
            .Select(f => f.ImageId)
            .ToListAsync();

        return selectedIds.ToHashSet();
    }

    /// <summary>
    /// 为图片 DTO 列表填充 IsSelected 状态
    /// </summary>
    /// <param name="images">图片 DTO 列表</param>
    /// <param name="selectedImageIds">已选中的图片 ID 集合</param>
    private static void FillIsSelectedStatus(List<ImageDto> images, HashSet<int> selectedImageIds)
    {
        foreach (var image in images)
        {
            image.IsSelected = selectedImageIds.Contains((int)image.Id);
        }
    }

    /// <summary>
    /// 根据 Image 实体创建 ImageResponseDto
    /// </summary>
    /// <param name="image">图片实体</param>
    /// <returns>图片响应 DTO</returns>
    private static ImageResponseDto CreateImageResponseDto(Image image)
    {
        return new ImageResponseDto
        {
            Id = image.Id,
            OriginalFileName = image.OriginalFileName,
            UploadedAt = image.UploadedAt,
            FileSize = image.FileSize,
            Url = $"/api/images/view/{image.Id}"
        };
    }

    /// <summary>
    /// 根据文件名推断 MIME Content-Type
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>MIME 类型字符串</returns>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    #endregion
}
