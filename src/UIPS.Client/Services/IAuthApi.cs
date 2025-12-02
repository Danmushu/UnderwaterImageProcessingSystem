using Refit;

namespace UIPS.Client.Services;

public interface IAuthApi
{
    // 对应后端的 POST /api/Auth/login 接口 TODO 修改大小写
    [Post("/api/Auth/login")]
    Task<dynamic> LoginAsync([Body] object request);

    // 对应后端的 POST /api/Auth/register 接口 TODO 修改大小写
    [Post("/api/Auth/register")]
    Task<dynamic> RegisterAsync([Body] object request);
}