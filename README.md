# Image Selection System (图像筛选与标注系统)

这是一个高性能的图像筛选与管理系统，旨在帮助团队高效地处理大规模图像数据集。系统包含一个强大的后端 API、一个用于高强度筛选任务的桌面客户端，以及一个用于管理和Web访问的前端应用。

## 📂 项目结构

项目由以下三个主要部分组成：

- **`Backend/`**: 基于 .NET 8 Web API 的后端服务，负责数据持久化、业务逻辑和图像处理。
- **`DesktopClient/`**: 基于 Windows Forms (.NET 8) 的桌面客户端，专为高性能图片浏览和快速筛选设计。
- **`frontend/`**: 基于 Vue 3 + TypeScript + Vite 的现代 Web 前端，提供用户管理、项目概览和 Web 端筛选功能。

## 🛠 技术栈

- **后端**: .NET 8, Entity Framework Core, MySQL 8.0+, JWT Auth
- **桌面端**: .NET 8, WinForms, HttpClient
- **前端**: Vue 3, TypeScript, Vite, Pinia
- **数据库**: MySQL 8

## 🚀 快速开始指南

### 1. 准备工作

确保您的开发环境安装了以下工具：
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v16+)
- [MySQL Server](https://dev.mysql.com/downloads/installer/) (8.0 或更高版本)

### 2. 后端设置 (Backend)

后端是系统的核心，必须首先启动。

1.  **配置数据库**:
    确保 MySQL 服务已运行。打开 `Backend/appsettings.json`，修改 `ConnectionStrings` 中的 `DefaultConnection` 为您的 MySQL 连接信息。

2.  **应用迁移**:
    在 `Backend` 目录下运行：
    ```bash
    dotnet ef database update
    ```

3.  **启动服务**:
    ```bash
    cd Backend
    dotnet run
    ```
    服务默认运行在 `http://localhost:5000`。
    
    > 📄 **详细文档**: 请参阅 [Backend/DEPLOY.md](Backend/DEPLOY.md)

### 3. 桌面客户端 (DesktopClient)

1.  打开 `Backend.sln` 或直接进入 `DesktopClient` 目录。
2.  编译并运行 `DesktopClient` 项目。
3.  在登录界面，输入后端 API 地址（例如 `http://localhost:5000/api`），然后使用管理员账号登录（默认：`admin` / `Admin@123`）。

    > 📄 **使用说明**: 请参阅 [DesktopClient/README.md](DesktopClient/README.md)

### 4. 前端应用 (Frontend)

1.  进入前端目录：
    ```bash
    cd frontend
    ```
2.  安装依赖：
    ```bash
    npm install
    ```
3.  启动开发服务器：
    ```bash
    npm run dev
    ```

    > 📄 **部署文档**: 请参阅 [frontend/DEPLOY.md](frontend/DEPLOY.md)

## ✨ 主要功能

- **多项目管理**: 创建不同的筛选项目，支持批量导入图片。
- **任务队列**: 将图片分发到不同的队列，支持多人协作。
- **高效筛选**: 桌面端支持键盘快捷键、预加载缓存，实现毫秒级图片切换。
- **数据导出**: 支持将筛选结果导出为 Excel 或直接复制文件。
- **权限控制**: 基于角色的访问控制 (RBAC)。

## 📝 注意事项

- **图片存储**: 上传的图片默认存储在 `Backend/uploads` 目录下，请确保该目录有写入权限。
- **API 地址**: 桌面端和前端都需要正确配置后端 API 的地址才能正常工作。