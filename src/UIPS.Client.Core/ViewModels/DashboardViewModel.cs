using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using System.Collections.ObjectModel;
using System.IO;
using UIPS.Client.Core.Services;
using UIPS.Shared.Api;
using UIPS.Shared.DTOs;
using static System.Net.Mime.MediaTypeNames;

namespace UIPS.Client.Core.ViewModels;

// 构造函数注入 IImageApi，所有上传请求都会自动携带 Token
public partial class DashboardViewModel(IImageApi imageApi, UserSession userSession) : ObservableObject
{
    [ObservableProperty]
    private string? _selectedFilePath; // 用户选择的文件路径

    [ObservableProperty]
    private string _uploadStatus = "请选择图片文件上传..."; // 上传状态显示

    [ObservableProperty]
    private bool _isUploading; // 进度条/按钮状态


    // 数据源集合 (绑定到前端 ListBox/ItemsControl)
    [ObservableProperty]
    private ObservableCollection<ImageDto> _images = new();
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
            // 读取文件流
            using var stream = File.OpenRead(SelectedFilePath);

            var fileName = Path.GetFileName(SelectedFilePath);

            // 动态判断 ContentType (防止传错格式)
            var extension = Path.GetExtension(SelectedFilePath).ToLower();
            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            // 参数顺序：(stream, fileName, contentType)
            // 注意：这里不需要传 "file"，因为 Interface 里的 [AliasAs("file")] 已经定义了字段名
            var filePart = new StreamPart(stream, fileName, contentType);

            // 调用 API
            var response = await imageApi.UploadImage(filePart);

            UploadStatus = $"上传成功! ID: {response.Id}, 文件名: {response.OriginalFileName}";
            SelectedFilePath = null;

            // 上传成功后自动刷新列表，让用户立刻看到新图
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
            // 调用接口获取列表
            var result = await imageApi.GetImages(new PaginatedRequestDto
            {
                PageIndex = 1,
                PageSize = 50 // 先加载50张
            });

            Images.Clear();

            // 基础 URL (注意：要与 App.xaml.cs 里的地址一致)
            var baseUrl = "https://localhost:7149";

            // 获取当前 Token
            var token = userSession.AccessToken;

            foreach (var img in result.Items)
            {
                // 拼接完整 URL，并带上 Token
                // 格式: https://localhost:7149/api/images/1/file?access_token=eyJ...
                img.PreviewUrl = $"{baseUrl}{img.PreviewUrl}?access_token={token}";

                Images.Add(img);
            }
        }
        catch (Exception ex)
        {
            UploadStatus = $"列表加载失败: {ex.Message}";
        }
    }


    /// <summary>
    /// 删除图片命令
    /// </summary>
    [RelayCommand]
    private async Task DeleteImageAsync(ImageDto? image)
    {
        if (!IsAdmin)
        {
            UploadStatus = "权限不足：只有管理员可以删除图片。";
            return;
        }
        if (image == null) return;

        // TODO 可选：简单的删除确认 (如果需要 MaterialDesign 的对话框需要更复杂的设置，这里先直接删)
        // var result = MessageBox.Show($"确定要删除图片 {image.OriginalFileName} 吗？", "确认删除", MessageBoxButton.YesNo);
        // if (result != MessageBoxResult.Yes) return;

        try
        {
            // 调用后端接口
            await imageApi.DeleteImage(image.Id);

            // 从 ObservableCollection 中移除，UI 会自动更新
            Images.Remove(image);

            // 更新状态栏
            UploadStatus = $"图片 '{image.OriginalFileName}' 已成功删除。";
        }
        catch (Exception ex)
        {
            UploadStatus = $"删除失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 切换图片选中状态 (点赞/标记)
    /// </summary>
    [RelayCommand]
    private async Task ToggleSelectionAsync(ImageDto? image)
    {
        if (image == null) return;

        try
        {
            // 调用后端接口
            await imageApi.ToggleSelection(image.Id);

            // 切换前端的显示状态 (变红/变灰) TODO
            // 注意：因为 IsSelected 是 ImageDto 的普通属性，不是 ObservableProperty，
            // 此时界面可能不会自动刷新颜色。
            // 解决办法 A: 让 ImageDto 继承 ObservableObject (改动大)
            // 解决办法 B: 简单粗暴地触发一下列表刷新 (性能差)
            // 解决办法 C: (推荐) 手动替换集合中的对象，强迫 UI 更新

            image.IsSelected = !image.IsSelected;

            // 为了通知 UI 更新，我们需要让 ListBox 知道这个 Item 变了。
            // 最简单的 Hack: 找到它的索引，替换成它自己 (或者利用 CommunityToolkit 的 SetProperty 如果 Dto 支持)
            // 如果界面没变色，请尝试:
            var index = Images.IndexOf(image);
            if (index >= 0)
            {
                Images[index] = image; // 触发 CollectionChanged
                // 或者如果 ImageDto 实现了 INotifyPropertyChanged 就不需要这步
            }
        }
        catch (Exception ex)
        {
            UploadStatus = $"操作失败: {ex.Message}";
        }
    }
}
