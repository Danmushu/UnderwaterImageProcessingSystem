# Underwater Image Processing System (UIPS)

> **水下图像处理系统**：一个基于 .NET 8 生态的高性能、模块化单体应用。

## 📖 项目简介
本项目旨在构建一个能够高效摄取、存储及处理水下图像数据的集成环境。采用 **模块化单体 (Modular Monolith)** 架构，通过严格的代码边界实现逻辑解耦，同时保持单体部署的便捷性。

## 🏗️ 技术架构
- **核心策略**: 共享契约模式 (Shared Contract Pattern)，前后端通过 DTO 类库实现强类型协作
- **前端 (Frontend)**: WPF (.NET 8) + Material Design + CommunityToolkit.Mvvm
- **后端 (Backend)**: ASP.NET Core 8 Web API + EF Core 8 (SQLite)
- **通信**: Refit (类型安全的 REST 库)

## 📂 项目结构说明
本项目在 Visual Studio 中使用了 **解决方案文件夹 (Solution Folders)** 进行逻辑分组，以匹配架构设计文档。

### 逻辑视图 (Visual Studio) vs 物理路径
虽然物理文件平铺在 `src/` 目录下，但在 IDE 中请参考以下逻辑结构：

| 逻辑分组 (Solution Group) | 项目名称 | 物理路径 | 职责说明 |
| :--- | :--- | :--- | :--- |
| **01. Backend (Server)** | **`UIPS.API`** | `/src/UIPS.API` | **全栈后端单体**。集成了所有服务端逻辑：<br>1. **入口**: Controllers (API 接口)<br>2. **数据**: EF Core Context (`UipsDbContext`), Migrations, Entities (`Models/`)<br>3. **逻辑**: Services (文件存储), DTOs (数据传输对象)<br>4. **配置**: 数据库连接 (`uips.db`) 与 JWT 鉴权 |
| **02. Frontend (Client)** | **`UIPS.Client`** | `/src/UIPS.Client` | **完整 WPF 客户端**。包含界面与交互逻辑：<br>1. **UI 层**: Views (XAML), Resources (Styles/Colors)<br>2. **逻辑层**: ViewModels (MVVM 核心), Converters<br>3. **网络层**: Services (Refit 接口定义, UserSession, HeaderHandler)<br>4. **入口**: App.xaml (DI 容器配置) |

## 🚀 快速开始 (Quick Start)

### 1. 环境准备
- Visual Studio 2022 (v17.8+)
- .NET 8 SDK

### 2. 数据库初始化
本项目使用 SQLite，数据库文件会自动生成。
1. 将 `UIPS.API`和 `UIPS.Client` 设为启动项目。
2. 启动即可，数据库会自动迁移
    ```
### 3. 运行项目

  - **启动后端**: 运行 `UIPS.API` (https)，Swagger 文档将自动打开。
  - **启动前端**: 右键 `UIPS.Client` -\> 调试 -\> 启动新实例。

## 📜 关键规范

  - **Git Flow**: `main` 分支禁止直接提交，开发请切出 `feat/xxx` 分支。
  - **样式**: UI 使用 Deep Ocean Palette (深海主题)，资源合并顺序严禁修改 `App.xaml` 。

![CI/CD](https://github.com/ekkure/UnderwaterImageProcessingSystem/workflows/CI%2FCD%20Pipeline/badge.svg)

