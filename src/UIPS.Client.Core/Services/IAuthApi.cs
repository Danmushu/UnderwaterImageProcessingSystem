using Refit;
using UIPS.Shared.DTOs;

namespace UIPS.Client.Core.Services;

public interface IAuthApi
{
    // 对应后端的 POST /api/Auth/login 接口 TODO 修改大小写
    [Post("/api/Auth/login")]
    Task<LoginResponseDto> LoginAsync([Body] LoginRequestDto request);

    // 对应后端的 POST /api/Auth/register 接口 TODO 修改大小写
    [Post("/api/Auth/register")]
    Task<string> RegisterAsync([Body] LoginRequestDto request);
}