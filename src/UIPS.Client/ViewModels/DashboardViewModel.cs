using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using UIPS.Client.Services;

namespace UIPS.Client.ViewModels;

public partial class DashboardViewModel(IImageApi imageApi, UserSession userSession) : ObservableObject
{
    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private string _uploadStatus = "请选择图片文件上传...";

    [ObservableProperty]
    private bool _isUploading;

    [ObservableProperty]
    private ObservableCollection<dynamic> _images = new();

    // ===== 新增属性 =====
    [ObservableProperty]
    private ObservableCollection<string> _fileNameGroups = new();

    [ObservableProperty]
    private string? _selectedFileName;

    public bool IsAdmin => userSession.IsAdmin;

    /// <summary>
    /// 单文件上传命令
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
            var response = await imageApi.UploadImage(filePart);

            var json = (JsonElement)response;
            var id = GetJsonLong(json, "id");
            var serverFileName = GetJsonString(json, "originalFileName");

            UploadStatus = $"上传成功! ID: {id}, 文件名: {serverFileName}";
            SelectedFilePath = null;

            await LoadFileNameGroupsAsync();
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
    /// 批量上传命令
    /// </summary>
    [RelayCommand]
    private async Task UploadMultipleFilesAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Multiselect = true,
            Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*",
            Title = "选择要上传的图片文件（可多选）"
        };

        if (dialog.ShowDialog() != true)
            return;

        var files = dialog.FileNames.ToList();

        if (files.Count == 0)
        {
            UploadStatus = "未选择任何文件。";
            return;
        }

        IsUploading = true;
        UploadStatus = $"正在上传 {files.Count} 个文件...";

        try
        {
            var streamParts = new List<StreamPart>();

            foreach (var filePath in files)
            {
                var stream = File.OpenRead(filePath);
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(filePath).ToLower();

                string contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".bmp" => "image/bmp",
                    _ => "application/octet-stream"
                };

                streamParts.Add(new StreamPart(stream, fileName, contentType));
            }

            var response = await imageApi.UploadBatch(streamParts);

            // 关闭所有流
            foreach (var part in streamParts)
            {
                part.Value.Dispose();
            }

            UploadStatus = $"成功上传 {files.Count} 个文件！";

            // 刷新文件名列表
            await LoadFileNameGroupsAsync();
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
    /// 加载图片列表（所有图片）
    /// </summary>
    [RelayCommand]
    public async Task LoadImagesAsync()
    {
        try
        {
            var result = await imageApi.GetImages(1, 50);

            Images.Clear();
            var baseUrl = "https://localhost:7149";
            var token = userSession.AccessToken;

            var jsonRoot = (JsonElement)result;

            if (jsonRoot.TryGetProperty("items", out JsonElement itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemJson in itemsElement.EnumerateArray())
                {
                    dynamic img = new ExpandoObject();

                    img.Id = GetJsonLong(itemJson, "id");
                    img.OriginalFileName = GetJsonString(itemJson, "originalFileName");
                    img.IsSelected = itemJson.TryGetProperty("isSelected", out var sel) && sel.GetBoolean();

                    var rawUrl = GetJsonString(itemJson, "previewUrl");
                    img.PreviewUrl = $"{baseUrl}{rawUrl}?access_token={token}";

                    Images.Add(img);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// 加载文件名分组列表
    /// </summary>
    [RelayCommand]
    public async Task LoadFileNameGroupsAsync()
    {
        try
        {
            var result = await imageApi.GetUniqueFileNames();

            FileNameGroups.Clear();

            var jsonArray = (JsonElement)result;

            if (jsonArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonArray.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var fileName = item.GetString();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            FileNameGroups.Add(fileName);
                        }
                    }
                }
            }

            UploadStatus = $"共找到 {FileNameGroups.Count} 个不同的文件名";
        }
        catch (Exception ex)
        {
            UploadStatus = $"加载文件名列表失败: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// 选择文件名并加载该文件名的所有图片
    /// </summary>
    [RelayCommand]
    private async Task SelectFileNameAsync(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;

        SelectedFileName = fileName;

        try
        {
            var result = await imageApi.GetImagesByFileName(fileName);

            Images.Clear();
            var baseUrl = "https://localhost:7149";
            var token = userSession.AccessToken;

            var jsonArray = (JsonElement)result;

            if (jsonArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemJson in jsonArray.EnumerateArray())
                {
                    dynamic img = new ExpandoObject();

                    img.Id = GetJsonLong(itemJson, "id");
                    img.OriginalFileName = GetJsonString(itemJson, "originalFileName");
                    img.IsSelected = itemJson.TryGetProperty("isSelected", out var sel) && sel.GetBoolean();

                    var rawUrl = GetJsonString(itemJson, "previewUrl");
                    img.PreviewUrl = $"{baseUrl}{rawUrl}?access_token={token}";

                    Images.Add(img);
                }
            }

            UploadStatus = $"已加载文件名 '{fileName}' 的 {Images.Count} 张图片";
        }
        catch (Exception ex)
        {
            UploadStatus = $"加载失败: {ex.Message}";
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

            // 触发 UI 刷新
            var index = Images.IndexOf(image);
            if (index >= 0) Images[index] = image;
        }
        catch (Exception ex)
        {
            UploadStatus = $"操作失败: {ex.Message}";
        }
    }

    // --- Helper Methods ---
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