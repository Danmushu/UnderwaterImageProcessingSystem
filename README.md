# Underwater Image Processing System (UIPS)

> **水下图像处理系统**：一个基于 .NET 8 生态的高性能、模块化单体应用。

## 📖 项目简介
[cite_start]本项目旨在构建一个能够高效摄取、存储及处理水下图像数据的集成环境 [cite: 6][cite_start]。采用 **模块化单体 (Modular Monolith)** 架构，通过严格的代码边界实现逻辑解耦，同时保持单体部署的便捷性 [cite: 7]。

## 🏗️ 技术架构
- [cite_start]**核心策略**: 共享契约模式 (Shared Contract Pattern)，前后端通过 DTO 类库实现强类型协作 [cite: 17]。
- [cite_start]**前端 (Frontend)**: WPF (.NET 8) + Material Design + CommunityToolkit.Mvvm [cite: 16, 144]。
- [cite_start]**后端 (Backend)**: ASP.NET Core 8 Web API + EF Core 8 (SQLite) [cite: 17, 18]。
- [cite_start]**通信**: Refit (类型安全的 REST 库) [cite: 36]。

## 📂 项目结构说明
本项目在 Visual Studio 中使用了 **解决方案文件夹 (Solution Folders)** 进行逻辑分组，以匹配架构设计文档。

### 逻辑视图 (Visual Studio) vs 物理路径
[cite_start]虽然物理文件平铺在 `src/` 目录下，但在 IDE 中请参考以下逻辑结构 [cite: 52-60]：

| 逻辑分组 (Solution Folder) | 项目名称 | 物理路径 | 职责说明 |
| :--- | :--- | :--- | :--- |
| **00.Shared** | `UIPS.Shared` | `/src/UIPS.Shared` | **共享契约**。仅包含 DTOs、枚举和常量。前后端通用，无依赖。 |
| **01.Backend** | `UIPS.Domain` | `/src/UIPS.Domain` | **领域核心**。纯净的实体定义 (Entities)，不依赖数据库。 |
| | `UIPS.Infrastructure` | `/src/UIPS.Infrastructure` | **基础设施**。EF Core 数据库上下文 (`UipsDbContext`) 实现。 |
| | `UIPS.API` | `/src/UIPS.API` | **服务入口**。Web API 控制器，处理 HTTP 请求与鉴权。 |
| **02.Frontend** | `UIPS.Client.Core` | `/src/UIPS.Client.Core` | **前端大脑**。ViewModels、Refit API 接口定义。 |
| | `UIPS.Client` | `/src/UIPS.Client` | **前端界面**。WPF 视图 (XAML)、资源字典 (Styles/Colors)。 |

## 🚀 快速开始 (Quick Start)

### 1. 环境准备
- Visual Studio 2022 (v17.8+)
- .NET 8 SDK

### 2. 数据库初始化
本项目使用 SQLite，数据库文件会自动生成。
1. 将 `UIPS.API` 设为启动项目。
2. 打开“程序包管理器控制台”，运行以下命令应用迁移：
    ```powershell
        Update-Database -Project UIPS.Infrastructure -StartupProject UIPS.API
    ```
### 3. 运行项目

  - **启动后端**: 运行 `UIPS.API` (https)，Swagger 文档将自动打开。
  - **启动前端**: 右键 `UIPS.Client` -\> 调试 -\> 启动新实例。

## 📜 关键规范

  - **Git Flow**: `main` 分支禁止直接提交，开发请切出 `feat/xxx` 分支。
  - **样式**: UI 使用 Deep Ocean Palette (深海主题)，资源合并顺序严禁修改 `App.xaml` 。
