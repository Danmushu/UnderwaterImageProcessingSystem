using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Text.Json; 
using System.Xml.Linq;
using UIPS.Client.Services;  
using UIPS.Client.Services;

namespace UIPS.Client.ViewModels;

// 构造函数注入 IImageApi，所有上传请求都会自动携带 Token
public partial class DashboardViewModel(IImageApi imageApi, UserSession userSession) : ObservableObject
{
    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private string _uploadStatus = "请选择图片文件上传...";

    [ObservableProperty]
    private bool _isUploading;

    // 修改点 1: 集合类型改为本地模型 ClientImageModel
    [ObservableProperty]
    private ObservableCollection<dynamic> _images = new();

    public bool IsAdmin => userSession.IsAdmin;

    /// <summary>
    /// 上传文件命令
    /// </summary>
    [RelayCommand]
    private async Task UploadFileAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath) || IsUploading)
            return;

        IsUploading = true;
        UploadStatus = "正在上传中...";

        try
        {
            using var stream = File.OpenRead(SelectedFilePath);
            var fileName = Path.GetFileName(SelectedFilePath);
            var extension = Path.GetExtension(SelectedFilePath).ToLower();

            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            var filePart = new StreamPart(stream, fileName, contentType);

            // 调用 API (返回 dynamic)
            var response = await imageApi.UploadImage(filePart);

            // 解析 dynamic (JsonElement)
            // System.Text.Json 默认返回 JsonElement，区分大小写
            var json = (JsonElement)response;

            // 假设后端返回: { "id": 1, "originalFileName": "xxx.jpg" }
            var id = GetJsonLong(json, "id");
            var serverFileName = GetJsonString(json, "originalFileName");

            UploadStatus = $"上传成功! ID: {id}, 文件名: {serverFileName}";
            SelectedFilePath = null;

            await LoadImagesAsync();
        }
        catch (ApiException ex)
        {
            UploadStatus = $"上传失败! {ex.StatusCode}: {ex.Content ?? ex.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            UploadStatus = $"发生未知错误: {ex.Message}";
        }
        finally
        {
            IsUploading = false;
        }
    }

    /// <summary>
    /// 加载图片列表
    /// </summary>
    [RelayCommand]
    public async Task LoadImagesAsync()
    {
        try
        {
            var result = await imageApi.GetImages(1, 50); // Refit 返回的是 JsonElement

            Images.Clear();
            var baseUrl = "https://localhost:7149";
            var token = userSession.AccessToken;

            var jsonRoot = (JsonElement)result;

            if (jsonRoot.TryGetProperty("items", out JsonElement itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemJson in itemsElement.EnumerateArray())
                {
                    // 动态创建对象
                    dynamic img = new ExpandoObject();

                    // 手动把 JSON 里的字段拷出来 注意大小写
                    // 后端 JSON 是小写 (id, originalFileName), 前端绑定习惯大写 (Id, Name)
                    // 你可以在这里自己决定属性名叫什么
                    img.Id = GetJsonLong(itemJson, "id");
                    img.OriginalFileName = GetJsonString(itemJson, "originalFileName");
                    img.IsSelected = itemJson.TryGetProperty("isSelected", out var sel) && sel.GetBoolean();

                    // 处理 URL 拼接逻辑
                    var rawUrl = GetJsonString(itemJson, "previewUrl");
                    img.PreviewUrl = $"{baseUrl}{rawUrl}?access_token={token}";

                    // 加入集合
                    Images.Add(img);
                }
            }
        }
        catch (Exception ex)
        {
            // Error handling
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// 删除图片命令
    /// </summary>
    [RelayCommand]
    private async Task DeleteImageAsync(dynamic? image)
    {
        if (!IsAdmin)
        {
            UploadStatus = "权限不足：只有管理员可以删除图片。";
            return;
        }
        if (image == null) return;

        try
        {
            await imageApi.DeleteImage(image.Id);
            Images.Remove(image);
            UploadStatus = $"图片 '{image.OriginalFileName}' 已成功删除。";
        }
        catch (Exception ex)
        {
            UploadStatus = $"删除失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 切换选中状态
    /// </summary>
    [RelayCommand]
    private async Task ToggleSelectionAsync(dynamic image)
    {
        if (image == null) return;

        try
        {
            await imageApi.ToggleSelection(image.Id);

            // 更新本地模型状态
            image.IsSelected = !image.IsSelected;

            // 触发 UI 刷新 替换集合元素 trick
            var index = Images.IndexOf(image);
            if (index >= 0) Images[index] = image;
        }
        catch (Exception ex)
        {
            UploadStatus = $"操作失败: {ex.Message}";
        }
    }

    // --- Helper Methods ---
    // 为了防止 Key 不存在报错，封装简单的安全读取方法
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
}
