# Views 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.Client/Views/`

**职责**: 视图层，定义用户界面的外观和布局。使用 XAML 声明式语法，通过数据绑定与 ViewModel 交互。

**核心技术**: 
- WPF (Windows Presentation Foundation)
- XAML (Extensible Application Markup Language)
- Material Design In XAML Toolkit

---

## 📄 文件清单

| 文件名 | 类型 | 职责 |
|--------|------|------|
| `LoginView.xaml` | XAML | 登录/注册界面 |
| `LoginView.xaml.cs` | C# | 登录视图代码后置 |
| `DashboardView.xaml` | XAML | 仪表盘主界面 |
| `DashboardView.xaml.cs` | C# | 仪表盘代码后置 |
| `AdminView.xaml` | XAML | 管理员面板界面 |
| `AdminView.xaml.cs` | C# | 管理员面板代码后置 |

---

## 🔐 LoginView - 登录视图

### 界面结构

```
┌─────────────────────────────────────────┐
│              UIPS 登录                   │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ 👤 用户名                        │   │
│  └─────────────────────────────────┘   │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ 🔒 密码                          │   │
│  └─────────────────────────────────┘   │
│                                         │
│  ┌─────────────────────────────────┐   │  (仅注册模式)
│  │ 🔒 确认密码                      │   │
│  └─────────────────────────────────┘   │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │           登 录                  │   │
│  └─────────────────────────────────┘   │
│                                         │
│         没有账号? 立即注册              │
│                                         │
│  ⚠️ 错误信息显示区域                    │
└─────────────────────────────────────────┘
```

### 数据绑定

```xml
<!-- 用户名输入 -->
<TextBox Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}" />

<!-- 密码输入 -->
<PasswordBox x:Name="PasswordBox" />

<!-- 登录按钮 -->
<Button Content="{Binding ActionButtonText}" 
        Command="{Binding ExecuteAuthCommand}"
        IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBooleanConverter}}" />

<!-- 错误信息 -->
<TextBlock Text="{Binding ErrorMessage}" 
           Foreground="{StaticResource ErrorBrush}" />
```

### 代码后置

```csharp
public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // 设置登录成功回调
        viewModel.OnLoginSuccess = () =>
        {
            var mainWindow = App.Current.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
        };
    }
}
```

### 特殊处理

**PasswordBox 绑定问题**:
WPF 的 PasswordBox 出于安全考虑不支持直接绑定，需要在代码后置中处理：

```csharp
private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
{
    if (DataContext is LoginViewModel vm)
    {
        vm.Password = PasswordBox.Password;
    }
}
```

---

## 🖼️ DashboardView - 仪表盘视图

### 界面结构

```
┌─────────────────────────────────────────────────────────────┐
│  上传区域                                                    │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ [选择文件]  文件路径显示  [上传] [批量上传]              ││
│  └─────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│  筛选区域                                                    │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ 文件名下拉选择: [ComboBox ▼]                            ││
│  └─────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│  图片网格                                                    │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐           │
│  │  图片1  │ │  图片2  │ │  图片3  │ │  图片4  │           │
│  │ [选择]  │ │ [选择]  │ │ [取消]  │ │ [选择]  │           │
│  │ [删除]  │ │ [删除]  │ │ [删除]  │ │ [删除]  │           │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘           │
├─────────────────────────────────────────────────────────────┤
│  分页控件                                                    │
│  [上一页] 第 1 / 10 页 (共 100 项) [下一页]  每页: [12 ▼]   │
├─────────────────────────────────────────────────────────────┤
│  状态栏: 已加载 12 张图片                                    │
└─────────────────────────────────────────────────────────────┘
```

### 数据绑定

```xml
<!-- 图片网格 -->
<ItemsControl ItemsSource="{Binding Images}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border>
                <StackPanel>
                    <!-- 图片预览 -->
                    <Image Source="{Binding PreviewUrl}" />
                    
                    <!-- 文件名 -->
                    <TextBlock Text="{Binding OriginalFileName}" />
                    
                    <!-- 操作按钮 -->
                    <Button Content="{Binding IsSelected, Converter=...}"
                            Command="{Binding DataContext.ToggleSelectionCommand, 
                                      RelativeSource={RelativeSource AncestorType=ItemsControl}}"
                            CommandParameter="{Binding}" />
                </StackPanel>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>

<!-- 分页控件 -->
<StackPanel Orientation="Horizontal">
    <Button Content="上一页" Command="{Binding GoToPreviousPageCommand}" />
    <TextBlock Text="{Binding PageInfo}" />
    <Button Content="下一页" Command="{Binding GoToNextPageCommand}" />
    <ComboBox SelectedItem="{Binding PageSize}">
        <ComboBoxItem Content="6" />
        <ComboBoxItem Content="12" />
        <ComboBoxItem Content="24" />
        <ComboBoxItem Content="48" />
    </ComboBox>
</StackPanel>
```

### 代码后置

```csharp
public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    // 文件选择对话框
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "图像文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp|所有文件|*.*",
                Title = "选择要上传的图片"
            };

            if (dialog.ShowDialog() == true)
            {
                vm.SelectedFilePath = dialog.FileName;
            }
        }
    }

    // 文件名选择变更
    private void FileNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm && !string.IsNullOrEmpty(vm.SelectedFileName))
        {
            vm.SelectFileNameCommand.Execute(vm.SelectedFileName);
        }
    }
}
```

---

## 👑 AdminView - 管理员视图

### 界面结构

```
┌─────────────────────────────────────────────────────────────┐
│  统计卡片区域                                                │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐   │
│  │ 总用户数  │ │ 管理员数  │ │ 总图片数  │ │ 总收藏数  │   │
│  │    100    │ │     5     │ │   1000    │ │    500    │   │
│  └───────────┘ └───────────┘ └───────────┘ └───────────┘   │
├─────────────────────────────────────────────────────────────┤
│  TabControl                                                  │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ [用户管理] [图片管理]                                   ││
│  ├─────────────────────────────────────────────────────────┤│
│  │  用户列表                                               ││
│  │  ┌────┬──────────┬────────┬─────────────────────────┐  ││
│  │  │ ID │ 用户名   │ 角色   │ 操作                    │  ││
│  │  ├────┼──────────┼────────┼─────────────────────────┤  ││
│  │  │ 1  │ admin    │ Admin  │ [切换角色][删除][重置]  │  ││
│  │  │ 2  │ user1    │ User   │ [切换角色][删除][重置]  │  ││
│  │  └────┴──────────┴────────┴─────────────────────────┘  ││
│  │                                                         ││
│  │  [上一页] 第 1 / 5 页 [下一页]                         ││
│  └─────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│  状态栏: 已加载 10 个用户                                    │
└─────────────────────────────────────────────────────────────┘
```

### 数据绑定

```xml
<!-- 统计卡片 -->
<UniformGrid Columns="4">
    <materialDesign:Card>
        <StackPanel>
            <TextBlock Text="总用户数" />
            <TextBlock Text="{Binding TotalUsers}" FontSize="24" />
        </StackPanel>
    </materialDesign:Card>
    <!-- 其他卡片... -->
</UniformGrid>

<!-- 用户列表 -->
<DataGrid ItemsSource="{Binding Users}" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="ID" Binding="{Binding Id}" />
        <DataGridTextColumn Header="用户名" Binding="{Binding UserName}" />
        <DataGridTextColumn Header="角色" Binding="{Binding Role}" />
        <DataGridTemplateColumn Header="操作">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal">
                        <Button Content="切换角色"
                                Command="{Binding DataContext.ToggleUserRoleCommand, 
                                          RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                CommandParameter="{Binding}" />
                        <Button Content="删除"
                                Command="{Binding DataContext.DeleteUserCommand, 
                                          RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                CommandParameter="{Binding}" />
                        <Button Content="重置密码"
                                Command="{Binding DataContext.ResetPasswordCommand, 
                                          RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                CommandParameter="{Binding}" />
                    </StackPanel>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

### 代码后置

```csharp
public partial class AdminView : UserControl
{
    public AdminView()
    {
        InitializeComponent();
    }
}
```

---

## 🎨 UI 设计规范

### Material Design 组件

| 组件 | 用途 |
|------|------|
| `materialDesign:Card` | 卡片容器 |
| `materialDesign:PackIcon` | 图标 |
| `materialDesign:ColorZone` | 颜色区域 |
| `materialDesign:Chip` | 标签 |
| `materialDesign:Snackbar` | 消息提示 |

### 颜色主题

```xml
<!-- Resources/Colors.xaml -->
<SolidColorBrush x:Key="PrimaryBrush" Color="#0277BD"/>      <!-- 主色：深蓝 -->
<SolidColorBrush x:Key="SecondaryBrush" Color="#00BCD4"/>    <!-- 辅色：青色 -->
<SolidColorBrush x:Key="BackgroundBrush" Color="#263238"/>   <!-- 背景：深灰 -->
<SolidColorBrush x:Key="SurfaceBrush" Color="#37474F"/>      <!-- 表面：灰色 -->
<SolidColorBrush x:Key="ErrorBrush" Color="#F44336"/>        <!-- 错误：红色 -->
<SolidColorBrush x:Key="PrimaryTextBrush" Color="#FFFFFF"/>  <!-- 文字：白色 -->
```

### 深海主题

项目采用深海主题配色，体现水下图像处理系统的特点：
- 深蓝色主色调
- 青色辅助色
- 深灰色背景
- 白色文字

---

## 🏗️ XAML 结构

### 视图基本结构

```xml
<UserControl x:Class="UIPS.Client.Views.DashboardView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    
    <Grid>
        <!-- 布局定义 -->
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- 内容区域 -->
    </Grid>
</UserControl>
```

### 数据绑定模式

```xml
<!-- 简单绑定 -->
<TextBlock Text="{Binding UserName}" />

<!-- 双向绑定 -->
<TextBox Text="{Binding UserName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- 命令绑定 -->
<Button Command="{Binding LoginCommand}" />

<!-- 带参数的命令 -->
<Button Command="{Binding DeleteCommand}" CommandParameter="{Binding}" />

<!-- 相对源绑定（用于 DataTemplate 内部） -->
<Button Command="{Binding DataContext.DeleteCommand, 
                  RelativeSource={RelativeSource AncestorType=ItemsControl}}"
        CommandParameter="{Binding}" />
```

---

## 📊 视图统计

| 视图 | 类型 | 控件数 | 绑定数 |
|------|------|--------|--------|
| LoginView | Window | ~15 | ~10 |
| DashboardView | UserControl | ~30 | ~20 |
| AdminView | UserControl | ~25 | ~15 |
| **总计** | - | **~70** | **~45** |

---

## 📝 开发规范

### 1. 命名规范
- 视图以 `View` 结尾
- XAML 文件与 C# 文件同名

### 2. 布局规范
- 使用 Grid 进行主布局
- 使用 StackPanel 进行线性布局
- 使用 WrapPanel 进行流式布局

### 3. 绑定规范
- 优先使用数据绑定
- 避免在代码后置中操作 UI
- 使用转换器处理数据转换

### 4. 样式规范
- 使用资源字典定义样式
- 使用 Material Design 组件
- 保持视觉一致性

---

## 🔗 相关模块

- **ViewModels**: 视图模型，提供数据和命令
- **Resources**: 资源文件（颜色、样式）
- **Converters**: 值转换器
- **MainWindow.xaml**: 主窗口，承载视图
