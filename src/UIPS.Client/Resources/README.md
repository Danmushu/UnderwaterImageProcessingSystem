# Resources 模块说明文档

## 📁 模块概述

**路径**: `src/UIPS.Client/Resources/`

**职责**: 资源文件目录，存放应用程序的共享资源，包括颜色定义、样式模板、图标等。通过资源字典（ResourceDictionary）实现资源的集中管理和复用。

**核心技术**: 
- WPF 资源系统
- ResourceDictionary
- StaticResource / DynamicResource

---

## 📄 文件清单

| 文件名 | 类型 | 职责 |
|--------|------|------|
| `Colors.xaml` | ResourceDictionary | 颜色定义 |
| `Styles.xaml` | ResourceDictionary | 样式模板（预留） |

---

## 🎨 Colors.xaml - 颜色资源

### 文件内容

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- 主色：深蓝色 -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="#0277BD"/>
    
    <!-- 辅色：青色 -->
    <SolidColorBrush x:Key="SecondaryBrush" Color="#00BCD4"/>
    
    <!-- 背景色：深灰色 -->
    <SolidColorBrush x:Key="BackgroundBrush" Color="#263238"/>
    
    <!-- 表面色：灰色 -->
    <SolidColorBrush x:Key="SurfaceBrush" Color="#37474F"/>
    
    <!-- 错误色：红色 -->
    <SolidColorBrush x:Key="ErrorBrush" Color="#F44336"/>
    
    <!-- 主文字色：白色 -->
    <SolidColorBrush x:Key="PrimaryTextBrush" Color="#FFFFFF"/>
    
</ResourceDictionary>
```

### 颜色说明

| 资源键 | 颜色值 | 用途 | 示例 |
|--------|--------|------|------|
| `PrimaryBrush` | #0277BD | 主要按钮、强调元素 | 登录按钮、导航选中 |
| `SecondaryBrush` | #00BCD4 | 次要按钮、图标 | Logo、辅助按钮 |
| `BackgroundBrush` | #263238 | 窗口背景 | 主窗口背景 |
| `SurfaceBrush` | #37474F | 卡片、面板背景 | 侧边栏、卡片 |
| `ErrorBrush` | #F44336 | 错误提示 | 错误信息文字 |
| `PrimaryTextBrush` | #FFFFFF | 主要文字 | 标题、正文 |

### 深海主题设计

项目采用"深海主题"配色方案，体现水下图像处理系统的特点：

```
┌─────────────────────────────────────────────────────────────┐
│  深海主题色板                                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ████████  Primary (#0277BD)    - 深蓝色，如深海            │
│  ████████  Secondary (#00BCD4)  - 青色，如海水              │
│  ████████  Background (#263238) - 深灰色，如海底            │
│  ████████  Surface (#37474F)    - 灰色，如礁石              │
│  ████████  Error (#F44336)      - 红色，如警示灯            │
│  ████████  Text (#FFFFFF)       - 白色，如气泡              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 使用示例

```xml
<!-- 在 XAML 中使用 -->
<Window Background="{StaticResource BackgroundBrush}">
    <Button Background="{StaticResource PrimaryBrush}"
            Foreground="{StaticResource PrimaryTextBrush}"
            Content="登录" />
    
    <TextBlock Text="错误信息"
               Foreground="{StaticResource ErrorBrush}" />
</Window>
```

---

## 📐 Styles.xaml - 样式资源

### 文件内容

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- 预留：自定义样式模板 -->
    
</ResourceDictionary>
```

### 扩展建议

可以在此文件中添加以下样式：

#### 1. 按钮样式

```xml
<Style x:Key="PrimaryButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource PrimaryTextBrush}"/>
    <Setter Property="Padding" Value="16,8"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="4"
                        Padding="{TemplateBinding Padding}">
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

#### 2. 文本框样式

```xml
<Style x:Key="ModernTextBoxStyle" TargetType="TextBox">
    <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
    <Setter Property="Foreground" Value="{StaticResource PrimaryTextBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrush}"/>
    <Setter Property="BorderThickness" Value="0,0,0,2"/>
    <Setter Property="Padding" Value="8,4"/>
    <Setter Property="FontSize" Value="14"/>
</Style>
```

#### 3. 卡片样式

```xml
<Style x:Key="CardStyle" TargetType="Border">
    <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
    <Setter Property="CornerRadius" Value="8"/>
    <Setter Property="Padding" Value="16"/>
    <Setter Property="Effect">
        <Setter.Value>
            <DropShadowEffect BlurRadius="10" 
                              ShadowDepth="2" 
                              Opacity="0.3"/>
        </Setter.Value>
    </Setter>
</Style>
```

---

## ⚙️ 资源引用配置

### App.xaml 中的配置

```xml
<Application x:Class="UIPS.Client.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design 资源 -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.LightBlue.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Cyan.xaml" />
                
                <!-- 自定义资源 -->
                <ResourceDictionary Source="Resources/Colors.xaml"/>
                <ResourceDictionary Source="Resources/Styles.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

---

## 🏗️ 资源系统架构

### 资源查找顺序

```
┌─────────────────────────────────────────────────────────────┐
│                     资源查找顺序                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. 元素本地资源 (Element.Resources)                        │
│         ↓                                                   │
│  2. 父元素资源 (Parent.Resources)                           │
│         ↓                                                   │
│  3. 窗口资源 (Window.Resources)                             │
│         ↓                                                   │
│  4. 应用程序资源 (Application.Resources)                    │
│         ↓                                                   │
│  5. 系统资源 (SystemColors, SystemFonts)                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### StaticResource vs DynamicResource

| 特性 | StaticResource | DynamicResource |
|------|----------------|-----------------|
| 解析时机 | 编译时 | 运行时 |
| 性能 | 更快 | 较慢 |
| 动态更新 | 不支持 | 支持 |
| 使用场景 | 固定资源 | 主题切换 |

```xml
<!-- StaticResource：编译时解析，性能更好 -->
<Button Background="{StaticResource PrimaryBrush}" />

<!-- DynamicResource：运行时解析，支持动态更新 -->
<Button Background="{DynamicResource PrimaryBrush}" />
```

---

## 📊 资源统计

| 资源类型 | 数量 | 文件 |
|----------|------|------|
| 颜色画刷 | 6 | Colors.xaml |
| 样式模板 | 0 | Styles.xaml |
| **总计** | **6** | - |

---

## 📝 开发规范

### 1. 命名规范
- 资源键使用 PascalCase
- 颜色资源以 `Brush` 结尾
- 样式资源以 `Style` 结尾

### 2. 组织规范
- 颜色定义放在 Colors.xaml
- 样式定义放在 Styles.xaml
- 复杂样式可单独创建文件

### 3. 使用规范
- 优先使用 StaticResource
- 需要动态更新时使用 DynamicResource
- 避免硬编码颜色值

### 4. 主题规范
- 保持颜色一致性
- 遵循 Material Design 规范
- 考虑可访问性（对比度）

---

## 🔗 相关模块

- **App.xaml**: 资源引用配置
- **Views**: 使用资源的视图
- **Converters**: 值转换器
