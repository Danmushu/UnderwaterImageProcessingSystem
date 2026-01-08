using System.IO;
using System.Text.Json;

namespace UIPS.Client.Services;

/// <summary>
/// 全局会话管理（支持持久化存储）
/// </summary>
public class UserSession
{
    // Token 持久化文件路径
    private static readonly string TokenFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "UIPS", "session.json");

    public UserSession()
    {
        // 启动时尝试从本地文件恢复会话
        LoadSession();
    }

    // 存储的数据
    public string? AccessToken { get; private set; }
    public string? UserName { get; private set; }
    public long UserId { get; private set; }
    public string Role { get; private set; } = "User";
    public bool IsAdmin => Role == "Admin";
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    /// <summary>
    /// 登录成功后设置并持久化会话
    /// </summary>
    public void SetSession(string token, string userName, long userId, string role)
    {
        AccessToken = token;
        UserName = userName;
        UserId = userId;
        Role = role;

        // 保存到本地文件
        SaveSession();
    }

    /// <summary>
    /// 注销时清除会话（包括本地文件）
    /// </summary>
    public void ClearSession()
    {
        AccessToken = null;
        UserName = null;
        UserId = 0;
        Role = "User";

        // 删除本地文件
        try
        {
            if (File.Exists(TokenFilePath))
                File.Delete(TokenFilePath);
        }
        catch { /* 忽略删除失败 */ }
    }

    /// <summary>
    /// 保存会话到本地文件
    /// </summary>
    private void SaveSession()
    {
        try
        {
            var dir = Path.GetDirectoryName(TokenFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var data = new SessionData
            {
                AccessToken = AccessToken,
                UserName = UserName,
                UserId = UserId,
                Role = Role
            };

            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(TokenFilePath, json);
        }
        catch { /* 保存失败不影响使用 */ }
    }

    /// <summary>
    /// 从本地文件恢复会话
    /// </summary>
    private void LoadSession()
    {
        try
        {
            if (!File.Exists(TokenFilePath)) return;

            var json = File.ReadAllText(TokenFilePath);
            var data = JsonSerializer.Deserialize<SessionData>(json);

            if (data != null && !string.IsNullOrEmpty(data.AccessToken))
            {
                AccessToken = data.AccessToken;
                UserName = data.UserName;
                UserId = data.UserId;
                Role = data.Role ?? "User";
            }
        }
        catch { /* 加载失败则保持未登录状态 */ }
    }

    /// <summary>
    /// 会话数据结构（用于序列化）
    /// </summary>
    private class SessionData
    {
        public string? AccessToken { get; set; }
        public string? UserName { get; set; }
        public long UserId { get; set; }
        public string? Role { get; set; }
    }
}