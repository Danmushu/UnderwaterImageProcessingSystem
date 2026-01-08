# ViewModels 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.Client/ViewModels/`

**职责**: 视图模型层，实现 MVVM 模式中的 ViewModel。负责处理 UI 逻辑、数据绑定、命令处理和与服务层的交互。

**核心技术**: 
- CommunityToolkit.Mvvm（MVVM 工具包）
- 数据绑定（Data Binding）
- 命令模式（Command Pattern）

---

## 📄 文件清单

| 文件名 | 对应视图 | 职责 |
|--------|----------|------|
| `LoginViewModel.cs` | LoginView | 登录/注册逻辑 |
| `DashboardViewModel.cs` | DashboardView | 图片管理主界面逻辑 |
| `AdminViewModel.cs` | AdminView | 管理员面板逻辑 |

---

## 🔐 LoginViewModel - 登录视图模型

### 类定义

```csharp
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthApi _authApi;
    private readonly UserSession _userSession;
}
```

### 属性

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `UserName` | string | 用户名输入 |
| `Password` | string | 密码输入 |
| `ConfirmPassword` | string | 确认密码（注册模式） |
| `IsLoading` | bool | 加载状态 |
| `ErrorMessage` | string | 错误提示信息 |
| `IsRegisterMode` | bool | 是否为注册模式 |

### 计算属性

| 属性名 | 说明 |
|--------|------|
| `ActionTitle` | 标题（"UIPS 登录" 或 "UIPS 注册"） |
| `ActionButtonText` | 按钮文本（"登 录" 或 "立即注册"） |
| `SwitchModePrompt` | 切换提示（"没有账号?" 或 "已有账号?"） |

### 命令

| 命令名 | 功能 |
|--------|------|
| `ExecuteAuthCommand` | 执行登录或注册 |
| `SwitchModeCommand` | 切换登录/注册模式 |

### 回调

```csharp
public Action? OnLoginSuccess { get; set; }
```

登录成功后触发，用于导航到主界面。

### 核心逻辑

```csharp
// 登录流程
private async Task PerformLoginAsync()
{
    var payload = new { UserName, Password };
    dynamic response = await _authApi.LoginAsync(payload);
    
    // 解析响应
    var json = (JsonElement)response;
    string token = json.GetProperty("accessToken").GetString();
    string userName = json.GetProperty("userName").GetString();
    long userId = json.GetProperty("userId").GetInt64();
    string role = json.GetProperty("role").GetString();
    
    // 设置会话
    _userSession.SetSession(token, userName, userId, role);
    
    // 触发成功回调
    OnLoginSuccess?.Invoke();
}
```

---

## 🖼️ DashboardViewModel - 仪表盘视图模型

### 类定义

```csharp
public partial class DashboardViewModel : ObservableObject
{
    private readonly IImageApi imageApi;
    private readonly UserSession userSession;
}
```

### 属性

#### 图片相关
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `Images` | ObservableCollection<dynamic> | 图片列表 |
| `FileNameGroups` | ObservableCollection<string> | 文件名分组 |
| `SelectedFileName` | string | 选中的文件名 |

#### 上传相关
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `SelectedFilePath` | string | 选中的文件路径 |
| `IsUploading` | bool | 上传状态 |
| `UploadStatus` | string | 上传状态信息 |

#### 分页相关
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `CurrentPage` | int | 当前页码 |
| `PageSize` | int | 每页数量 |
| `TotalCount` | int | 总数量 |
| `TotalPages` | int | 总页数 |

#### 权限相关
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `IsAdmin` | bool | 是否为管理员 |

### 命令

| 命令名 | 功能 | 参数 |
|--------|------|------|
| `UploadFileCommand` | 上传单张图片 | - |
| `UploadMultipleFilesCommand` | 批量上传图片 | - |
| `LoadImagesCommand` | 加载图片列表 | - |
| `LoadFileNameGroupsCommand` | 加载文件名分组 | - |
| `SelectFileNameCommand` | 选择文件名筛选 | string |
| `DeleteImageCommand` | 删除图片 | dynamic |
| `ToggleSelectionCommand` | 切换收藏状态 | dynamic |
| `GoToPreviousPageCommand` | 上一页 | - |
| `GoToNextPageCommand` | 下一页 | - |
| `GoToPageCommand` | 跳转到指定页 | int |

### 核心逻辑

#### 图片上传
```csharp
[RelayCommand]
private async Task UploadFileAsync()
{
    using var stream = File.OpenRead(SelectedFilePath);
    var fileName = Path.GetFileName(SelectedFilePath);
    var contentType = GetContentType(fileName);
    
    var filePart = new StreamPart(stream, fileName, contentType);
    var response = await imageApi.UploadImage(filePart);
    
    // 刷新列表
    await LoadFileNameGroupsAsync();
}
```

#### 图片加载（分页）
```csharp
[RelayCommand]
public async Task LoadImagesAsync()
{
    var result = await imageApi.GetImages(CurrentPage, PageSize);
    var jsonRoot = (JsonElement)result;
    
    Images.Clear();
    
    // 解析分页信息
    TotalCount = jsonRoot.GetProperty("totalCount").GetInt32();
    TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
    
    // 解析图片列表
    foreach (var item in jsonRoot.GetProperty("items").EnumerateArray())
    {
        dynamic img = new ExpandoObject();
        img.Id = item.GetProperty("id").GetInt64();
        img.OriginalFileName = item.GetProperty("originalFileName").GetString();
        img.PreviewUrl = $"{baseUrl}{rawUrl}?access_token={token}";
        img.IsSelected = item.GetProperty("isSelected").GetBoolean();
        
        Images.Add(img);
    }
}
```

#### 收藏切换
```csharp
[RelayCommand]
private async Task ToggleSelectionAsync(dynamic image)
{
    await imageApi.ToggleSelection(image.Id);
    
    // 更新本地状态
    image.IsSelected = !image.IsSelected;
    
    // 触发 UI 刷新
    var index = Images.IndexOf(image);
    if (index >= 0) Images[index] = image;
}
```

---

## 👑 AdminViewModel - 管理员视图模型

### 类定义

```csharp
public partial class AdminViewModel : ObservableObject
{
    private readonly IAdminApi _adminApi;
    private readonly UserSession _userSession;
}
```

### 属性

#### 数据列表
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `Users` | ObservableCollection<dynamic> | 用户列表 |
| `AllImages` | ObservableCollection<dynamic> | 所有图片列表 |

#### 统计信息
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `TotalUsers` | int | 总用户数 |
| `TotalAdmins` | int | 管理员数量 |
| `TotalImages` | int | 总图片数 |
| `TotalFavourites` | int | 总收藏数 |

#### 状态
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `StatusMessage` | string | 状态信息 |
| `IsLoading` | bool | 加载状态 |

#### 分页
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `CurrentPage` | int | 当前页码 |
| `PageSize` | int | 每页数量 |
| `TotalCount` | int | 总数量 |
| `TotalPages` | int | 总页数 |
| `PageInfo` | string | 分页信息（计算属性） |

### 命令

| 命令名 | 功能 | 参数 |
|--------|------|------|
| `LoadStatisticsCommand` | 加载统计信息 | - |
| `LoadUsersCommand` | 加载用户列表 | - |
| `ToggleUserRoleCommand` | 切换用户角色 | dynamic |
| `DeleteUserCommand` | 删除用户 | dynamic |
| `ResetPasswordCommand` | 重置密码 | dynamic |
| `LoadAllImagesCommand` | 加载所有图片 | - |
| `GoToPreviousPageCommand` | 上一页 | - |
| `GoToNextPageCommand` | 下一页 | - |

### 核心逻辑

#### 加载统计信息
```csharp
[RelayCommand]
public async Task LoadStatisticsAsync()
{
    var result = await _adminApi.GetStatisticsAsync();
    var json = (JsonElement)result;
    
    TotalUsers = json.GetProperty("totalUsers").GetInt32();
    TotalAdmins = json.GetProperty("totalAdmins").GetInt32();
    TotalImages = json.GetProperty("totalImages").GetInt32();
    TotalFavourites = json.GetProperty("totalFavourites").GetInt32();
}
```

#### 切换用户角色
```csharp
[RelayCommand]
private async Task ToggleUserRoleAsync(dynamic user)
{
    var newRole = user.Role == "Admin" ? "User" : "Admin";
    await _adminApi.UpdateUserRoleAsync(user.Id, new { Role = newRole });
    
    // 更新本地状态
    user.Role = newRole;
    user.IsAdmin = newRole == "Admin";
    
    StatusMessage = $"用户 {user.UserName} 的角色已更新为 {newRole}";
}
```

#### 删除用户（带确认）
```csharp
[RelayCommand]
private async Task DeleteUserAsync(dynamic user)
{
    var result = MessageBox.Show(
        $"确定要删除用户 '{user.UserName}' 吗？",
        "确认删除",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);
    
    if (result != MessageBoxResult.Yes) return;
    
    await _adminApi.DeleteUserAsync(user.Id);
    Users.Remove(user);
    StatusMessage = $"用户 {user.UserName} 已删除";
}
```

---

## 🏗️ MVVM 架构

### 数据绑定流程

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│      View       │◄───>│   ViewModel     │◄───>│    Services     │
│    (XAML)       │     │                 │     │                 │
│                 │     │ - Properties    │     │ - IAuthApi      │
│ - TextBox       │────>│ - Commands      │────>│ - IImageApi     │
│ - Button        │     │ - Methods       │     │ - IAdminApi     │
│ - ListView      │◄────│                 │◄────│ - UserSession   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
     DataBinding              Logic                  Data
```

### 属性变更通知

```csharp
// 使用 [ObservableProperty] 特性自动生成属性变更通知
[ObservableProperty]
private string _userName = string.Empty;

// 等价于：
private string _userName = string.Empty;
public string UserName
{
    get => _userName;
    set => SetProperty(ref _userName, value);
}
```

### 命令生成

```csharp
// 使用 [RelayCommand] 特性自动生成命令
[RelayCommand]
private async Task LoadImagesAsync()
{
    // 命令逻辑
}

// 自动生成：
// public IAsyncRelayCommand LoadImagesCommand { get; }
```

### 命令可执行性

```csharp
// 带条件的命令
[RelayCommand(CanExecute = nameof(CanGoPrevious))]
private async Task GoToPreviousPageAsync()
{
    // ...
}

private bool CanGoPrevious => CurrentPage > 1;
```

---

## 📊 ViewModel 统计

| ViewModel | 属性数 | 命令数 | 依赖服务 |
|-----------|--------|--------|----------|
| LoginViewModel | 7 | 2 | IAuthApi, UserSession |
| DashboardViewModel | 12 | 10 | IImageApi, UserSession |
| AdminViewModel | 12 | 8 | IAdminApi, UserSession |
| **总计** | **31** | **20** | - |

---

## 📝 开发规范

### 1. 命名规范
- ViewModel 以 `ViewModel` 结尾
- 私有字段以 `_` 开头
- 命令方法以 `Async` 结尾

### 2. 属性声明
- 使用 `[ObservableProperty]` 特性
- 私有字段使用小写开头

### 3. 命令声明
- 使用 `[RelayCommand]` 特性
- 异步命令返回 `Task`

### 4. 依赖注入
- 通过构造函数注入依赖
- 使用私有只读字段存储

### 5. 错误处理
- 捕获 `ApiException` 处理 API 错误
- 使用 `StatusMessage` 或 `ErrorMessage` 显示错误

### 6. UI 更新
- 使用 `ObservableCollection` 实现列表自动更新
- 手动触发 `OnPropertyChanged` 更新计算属性

---

## 🔗 相关模块

- **Views**: 视图层，绑定 ViewModel
- **Services**: 服务层，提供数据操作
- **App.xaml.cs**: ViewModel 注册
