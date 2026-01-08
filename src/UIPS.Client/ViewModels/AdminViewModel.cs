using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Text.Json;
using System.Windows;
using UIPS.Client.Services;

namespace UIPS.Client.ViewModels;

/// <summary>
/// 管理员面板 ViewModel
/// </summary>
public partial class AdminViewModel : ObservableObject
{
    private readonly IAdminApi _adminApi;
    private readonly UserSession _userSession;

    public AdminViewModel(IAdminApi adminApi, UserSession userSession)
    {
        _adminApi = adminApi;
        _userSession = userSession;
        
        // 自动加载初始数据
        _ = InitializeAsync();
    }

    /// <summary>
    /// 初始化加载数据
    /// </summary>
    private async Task InitializeAsync()
    {
        await LoadStatisticsAsync();
        await LoadUsersAsync();
    }

    #region 属性

    [ObservableProperty]
    private ObservableCollection<dynamic> _users = new();

    [ObservableProperty]
    private ObservableCollection<dynamic> _allImages = new();

    [ObservableProperty]
    private string _statusMessage = "欢迎使用管理员面板";

    [ObservableProperty]
    private bool _isLoading;

    // 统计信息
    [ObservableProperty]
    private int _totalUsers;

    [ObservableProperty]
    private int _totalAdmins;

    [ObservableProperty]
    private int _totalImages;

    [ObservableProperty]
    private int _totalFavourites;

    // 分页
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _totalPages;

    public string PageInfo => $"第 {CurrentPage} / {TotalPages} 页 (共 {TotalCount} 项)";

    #endregion

    #region 统计信息

    /// <summary>
    /// 加载统计信息
    /// </summary>
    [RelayCommand]
    public async Task LoadStatisticsAsync()
    {
        try
        {
            IsLoading = true;
            var result = await _adminApi.GetStatisticsAsync();
            var json = (JsonElement)result;

            TotalUsers = GetJsonInt(json, "totalUsers");
            TotalAdmins = GetJsonInt(json, "totalAdmins");
            TotalImages = GetJsonInt(json, "totalImages");
            TotalFavourites = GetJsonInt(json, "totalFavourites");

            StatusMessage = "统计信息已更新";
        }
        catch (ApiException ex)
        {
            StatusMessage = $"加载统计信息失败: {ex.StatusCode}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region 用户管理

    /// <summary>
    /// 加载用户列表
    /// </summary>
    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            var result = await _adminApi.GetUsersAsync(CurrentPage, PageSize);
            var jsonRoot = (JsonElement)result;

            Users.Clear();

            // 解析分页信息
            if (jsonRoot.TryGetProperty("totalCount", out var totalCountElement))
            {
                TotalCount = totalCountElement.GetInt32();
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
                OnPropertyChanged(nameof(PageInfo));
            }

            // 解析用户列表
            if (jsonRoot.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemJson in itemsElement.EnumerateArray())
                {
                    dynamic user = new ExpandoObject();
                    user.Id = GetJsonInt(itemJson, "id");
                    user.UserName = GetJsonString(itemJson, "userName");
                    user.Role = GetJsonString(itemJson, "role");
                    user.IsAdmin = user.Role == "Admin";

                    Users.Add(user);
                }
            }

            StatusMessage = $"已加载 {Users.Count} 个用户";
        }
        catch (ApiException ex)
        {
            StatusMessage = $"加载用户失败: {ex.StatusCode}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [RelayCommand]
    private async Task DeleteUserAsync(dynamic user)
    {
        if (user == null) return;

        var result = MessageBox.Show(
            $"确定要删除用户 '{user.UserName}' 吗？\n这将同时删除该用户的所有图片和收藏记录。",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _adminApi.DeleteUserAsync(user.Id);
            Users.Remove(user);
            StatusMessage = $"用户 {user.UserName} 已删除";
            
            // 刷新统计信息
            await LoadStatisticsAsync();
        }
        catch (ApiException ex)
        {
            StatusMessage = $"删除用户失败: {ex.Content ?? ex.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"错误: {ex.Message}";
        }
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [RelayCommand]
    private async Task ResetPasswordAsync(dynamic user)
    {
        if (user == null) return;

        // 简单示例：重置为默认密码 "123456"
        var result = MessageBox.Show(
            $"确定要重置用户 '{user.UserName}' 的密码吗？\n新密码将设置为: 123456",
            "确认重置密码",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var payload = new { NewPassword = "123456" };
            await _adminApi.ResetUserPasswordAsync(user.Id, payload);
            StatusMessage = $"用户 {user.UserName} 的密码已重置为 123456";
        }
        catch (ApiException ex)
        {
            StatusMessage = $"重置密码失败: {ex.Content ?? ex.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"错误: {ex.Message}";
        }
    }

    #endregion

    #region 图片管理

    /// <summary>
    /// 加载所有图片（管理员视图）
    /// </summary>
    [RelayCommand]
    public async Task LoadAllImagesAsync()
    {
        try
        {
            IsLoading = true;
            var result = await _adminApi.GetAllImagesAsync(CurrentPage, PageSize);
            var jsonRoot = (JsonElement)result;

            AllImages.Clear();
            var baseUrl = "https://localhost:7149";
            var token = _userSession.AccessToken;

            // 解析分页信息
            if (jsonRoot.TryGetProperty("totalCount", out var totalCountElement))
            {
                TotalCount = totalCountElement.GetInt32();
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
                OnPropertyChanged(nameof(PageInfo));
            }

            // 解析图片列表
            if (jsonRoot.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemJson in itemsElement.EnumerateArray())
                {
                    dynamic img = new ExpandoObject();
                    img.Id = GetJsonInt(itemJson, "id");
                    img.OriginalFileName = GetJsonString(itemJson, "originalFileName");
                    img.OwnerName = GetJsonString(itemJson, "ownerName");
                    img.OwnerId = GetJsonInt(itemJson, "ownerId");
                    img.FileSize = GetJsonLong(itemJson, "fileSize");

                    var rawUrl = GetJsonString(itemJson, "previewUrl");
                    img.PreviewUrl = $"{baseUrl}{rawUrl}?access_token={token}";

                    AllImages.Add(img);
                }
            }

            StatusMessage = $"已加载 {AllImages.Count} 张图片";
        }
        catch (ApiException ex)
        {
            StatusMessage = $"加载图片失败: {ex.StatusCode}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region 分页

    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadUsersAsync();
        }
    }

    #endregion

    #region 辅助方法

    private int GetJsonInt(JsonElement element, string key)
    {
        if (element.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.Number)
            return val.GetInt32();
        return 0;
    }

    private long GetJsonLong(JsonElement element, string key)
    {
        if (element.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.Number)
            return val.GetInt64();
        return 0;
    }

    private string GetJsonString(JsonElement element, string key)
    {
        if (element.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String)
            return val.GetString() ?? "";
        return "";
    }

    #endregion
}
