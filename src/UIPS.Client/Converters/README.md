# Converters 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.Client/Converters/`

**职责**: 值转换器目录，存放 WPF 数据绑定中使用的 IValueConverter 实现。转换器用于在数据源和 UI 之间进行数据格式转换。

**核心技术**: 
- IValueConverter 接口
- IMultiValueConverter 接口
- 数据绑定转换

---

## 📄 文件清单

| 文件名 | 类型 | 职责 |
|--------|------|------|
| `InverseBooleanConverter.cs` | 类 | 布尔值取反转换器 |

---

## 🔄 InverseBooleanConverter - 布尔值取反转换器

### 类定义

```csharp
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
            return !booleanValue;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

### 功能说明

将布尔值取反：
- `true` → `false`
- `false` → `true`

### 使用场景

1. **按钮禁用状态**：当 `IsLoading = true` 时禁用按钮

```xml
<Button Content="登录"
        IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBooleanConverter}}" />
```

2. **元素可见性**：当某条件为真时隐藏元素

```xml
<TextBlock Text="请先登录"
           Visibility="{Binding IsAuthenticated, 
                        Converter={StaticResource InverseBooleanConverter},
                        ConverterParameter=Visibility}" />
```

### 注册方式

```xml
<!-- 在 App.xaml 或视图的 Resources 中注册 -->
<Application.Resources>
    <converters:InverseBooleanConverter x:Key="InverseBooleanConverter"/>
</Application.Resources>
```

---

## 🛠️ 常用转换器扩展

以下是项目中可能需要的其他转换器：

### 1. BooleanToVisibilityConverter

```csharp
/// <summary>
/// 布尔值转可见性转换器
/// true -> Visible, false -> Collapsed
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // 支持反转参数
            if (parameter?.ToString() == "Inverse")
                boolValue = !boolValue;
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
            return visibility == Visibility.Visible;
        return false;
    }
}
```

**使用示例**:
```xml
<!-- 正常：true 显示，false 隐藏 -->
<Border Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVisibility}}" />

<!-- 反转：true 隐藏，false 显示 -->
<Border Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVisibility}, ConverterParameter=Inverse}" />
```

### 2. FileSizeConverter

```csharp
/// <summary>
/// 文件大小格式化转换器
/// 将字节数转换为可读格式（KB, MB, GB）
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

**使用示例**:
```xml
<TextBlock Text="{Binding FileSize, Converter={StaticResource FileSizeConverter}}" />
<!-- 输出：1.5 MB -->
```

### 3. DateTimeConverter

```csharp
/// <summary>
/// 日期时间格式化转换器
/// </summary>
public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            string format = parameter?.ToString() ?? "yyyy-MM-dd HH:mm";
            return dateTime.ToLocalTime().ToString(format);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

**使用示例**:
```xml
<!-- 默认格式 -->
<TextBlock Text="{Binding UploadedAt, Converter={StaticResource DateTimeConverter}}" />
<!-- 输出：2025-01-08 14:30 -->

<!-- 自定义格式 -->
<TextBlock Text="{Binding UploadedAt, Converter={StaticResource DateTimeConverter}, ConverterParameter='yyyy年MM月dd日'}" />
<!-- 输出：2025年01月08日 -->
```

### 4. NullToVisibilityConverter

```csharp
/// <summary>
/// 空值转可见性转换器
/// null/空字符串 -> Collapsed, 有值 -> Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasValue = value != null && !string.IsNullOrEmpty(value.ToString());
        
        if (parameter?.ToString() == "Inverse")
            hasValue = !hasValue;
        
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

### 5. SelectionToBooleanConverter

```csharp
/// <summary>
/// 选中状态转布尔值转换器
/// 用于图片选中/取消选中按钮文本
/// </summary>
public class SelectionToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? "取消选择" : "选择";
        }
        return "选择";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

---

## 🏗️ 转换器架构

### IValueConverter 接口

```csharp
public interface IValueConverter
{
    // 从数据源到 UI 的转换
    object Convert(object value, Type targetType, object parameter, CultureInfo culture);
    
    // 从 UI 到数据源的转换（双向绑定时使用）
    object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
}
```

### 转换流程

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   ViewModel     │────>│   Converter     │────>│      View       │
│                 │     │                 │     │                 │
│ IsLoading=true  │────>│ Convert()       │────>│ IsEnabled=false │
│                 │     │ true -> false   │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

### 多值转换器

```csharp
public interface IMultiValueConverter
{
    object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
    object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
}
```

**使用示例**:
```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource FullNameConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

---

## ⚙️ 注册和使用

### 1. 在 App.xaml 中全局注册

```xml
<Application.Resources>
    <ResourceDictionary>
        <!-- 引入命名空间 -->
        <ResourceDictionary.MergedDictionaries>
            <!-- ... -->
        </ResourceDictionary.MergedDictionaries>
        
        <!-- 注册转换器 -->
        <converters:InverseBooleanConverter x:Key="InverseBooleanConverter"/>
        <converters:BooleanToVisibilityConverter x:Key="BoolToVisibility"/>
        <converters:FileSizeConverter x:Key="FileSizeConverter"/>
        <converters:DateTimeConverter x:Key="DateTimeConverter"/>
    </ResourceDictionary>
</Application.Resources>
```

### 2. 在视图中局部注册

```xml
<UserControl.Resources>
    <converters:InverseBooleanConverter x:Key="InverseBooleanConverter"/>
</UserControl.Resources>
```

### 3. 在 XAML 中使用

```xml
<!-- 基本使用 -->
<Button IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBooleanConverter}}" />

<!-- 带参数 -->
<TextBlock Text="{Binding UploadedAt, Converter={StaticResource DateTimeConverter}, ConverterParameter='yyyy-MM-dd'}" />

<!-- 链式转换（需要自定义） -->
<Border Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVisibility}}" />
```

---

## 📊 转换器统计

| 转换器 | 状态 | 用途 |
|--------|------|------|
| InverseBooleanConverter | ✅ 已实现 | 布尔值取反 |
| BooleanToVisibilityConverter | 📝 建议添加 | 布尔转可见性 |
| FileSizeConverter | 📝 建议添加 | 文件大小格式化 |
| DateTimeConverter | 📝 建议添加 | 日期时间格式化 |
| NullToVisibilityConverter | 📝 建议添加 | 空值转可见性 |
| SelectionToBooleanConverter | 📝 建议添加 | 选中状态转文本 |

---

## 📝 开发规范

### 1. 命名规范
- 转换器类名以 `Converter` 结尾
- 资源键与类名相同或简化

### 2. 实现规范
- 实现 `IValueConverter` 接口
- `Convert` 方法必须实现
- `ConvertBack` 可以抛出 `NotImplementedException`（单向绑定时）

### 3. 参数使用
- 使用 `ConverterParameter` 传递额外参数
- 参数通常为字符串类型

### 4. 错误处理
- 检查输入值类型
- 返回合理的默认值
- 避免抛出异常

---

## 🔗 相关模块

- **Views**: 使用转换器的视图
- **ViewModels**: 提供数据源
- **Resources**: 资源定义
- **App.xaml**: 转换器注册
