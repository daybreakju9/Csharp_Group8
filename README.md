# 图像选择标注系统 · 全新 README

> 基于旧版 `README.md` 重新整理与扩展，覆盖端到端的部署、配置、使用与排障。适用于 Web 前端、后端 API 与 Windows 桌面客户端协同的多角色图像选择/标注场景。

## 目录

- [项目简介](#项目简介)
- [功能概览](#功能概览)
- [技术栈](#技术栈)
- [系统架构](#系统架构)
- [目录结构](#目录结构)
- [环境要求](#环境要求)
- [快速开始](#快速开始)
- [配置详解](#配置详解)
- [数据库迁移](#数据库迁移)
- [API 文档](#api-文档)
- [账号与角色](#账号与角色)
- [开发/构建/测试](#开发构建测试)
- [部署说明](#部署说明)
- [排障指南](#排障指南)
- [许可证](#许可证)
- [联系方式](#联系方式)

## 项目简介

图像选择标注系统支持多项目、多队列的协作式标注工作流，提供 Web（Vue3）与 Windows 桌面客户端（WinForms）两种入口，并通过 ASP.NET Core 后端统一提供认证、队列分发、数据导出和进度统计能力。

## 功能概览

- **核心**：项目/队列管理、图像批量导入、选择标注、进度追踪、数据导出。
- **管理员**：创建/配置项目与队列、用户与角色管理、批量上传、导出统计。
- **普通用户**：查看分配的项目/队列，执行图像选择并提交结果，查看个人进度。
- **桌面客户端增强**：大文件上传 5 分钟超时、图片缓存、导出下载。

## 技术栈

| 层次 | 组件 |
| --- | --- |
| 后端 | ASP.NET Core 8.0 · Entity Framework Core · Pomelo MySQL Provider（生产推荐）· JWT · BCrypt.Net · Swagger/OpenAPI · Docker |
| 前端 | Vue 3 + TypeScript · Vite · Element Plus · Pinia · Vue Router · Axios |
| 桌面 | .NET 8.0 WinForms |
| 数据库 | **默认开发：SQLite `Backend/app.db` 已内置**；生产推荐 MySQL 8.0+ |

## 系统架构

```
Web 前端 (Vue 3)        桌面客户端 (WinForms)
        │                         │
        │  HTTP/HTTPS + JWT       │
        └──────────┬──────────────┘
                   │
          ASP.NET Core 后端 API
                   │
        ┌──────────┴──────────┐
        │  MySQL/SQLite DB    │
        │  (Projects/Queues/  │
        │   Images/Users/...) │
        └──────────┬──────────┘
                   │
            文件存储 uploads/
```

## 目录结构

```
Csharp_Group8/                          # 仓库根
├─ Backend/                             # ASP.NET Core 后端
│  ├─ Controllers/                      # API 控制器
│  ├─ Data/                             # DbContext 与工厂
│  ├─ DTOs/                             # 传输对象
│  ├─ Helpers/                          # 辅助工具类
│  ├─ Middleware/                       # 中间件
│  ├─ Migrations/                       # EF Core 迁移
│  ├─ Models/                           # 领域模型
│  ├─ Repositories/                     # 仓储与 UoW
│  ├─ Services/                         # 业务与存储服务
│  ├─ uploads/                          # 上传目录（可配置）
│  ├─ appsettings*.json                 # 配置（连接串/JWT等）
│  ├─ Dockerfile                        # 后端 Docker 构建
│  └─ Program.cs                        # 入口
├─ frontend/                            # Vue3 前端
│  ├─ src/api/                          # Axios 接口封装
│  ├─ src/router/                       # 路由
│  ├─ src/stores/                       # Pinia
│  ├─ src/types/                        # TS 类型
│  ├─ src/views/                        # 页面（user/admin）
│  ├─ src/App.vue & main.ts             # 前端入口
│  └─ vite.config.ts / package*.json    # 构建与依赖
├─ DesktopClient/                       # WinForms 客户端
│  ├─ Forms/                            # 各窗体
│  ├─ Services/                         # HTTP/业务服务
│  ├─ Models/                           # 客户端模型
│  ├─ Helpers/                          # UI/导航辅助
│  └─ Program.cs                        # 客户端入口
├─ run-all.bat                          # 一键启动后端+桌面
├─ README.md                            # 新版说明（详细）
└─ READMEforUser.md                     # 额外说明
```

## 环境要求

- **后端**：.NET 8 SDK；默认内置 SQLite；生产推荐 MySQL 8.0+；Windows/Linux/macOS。
- **前端**：Node.js 18+，npm 9+。
- **桌面客户端**：Windows 10/11，.NET 8 Desktop Runtime。

## 快速开始

### 方法 A：手动运行（默认使用 SQLite）

1. **启动后端**
   ```bash
   cd Backend
   # 如需 MySQL，修改 appsettings.json 的 DefaultConnection，例如：
   # "Server=localhost;Port=3306;Database=dotnet;User=dotnet;Password=dotnet;"
   dotnet restore
   # 新数据库（MySQL 或新建 SQLite 文件）需初始化表结构：
   dotnet ef database update
   dotnet run
   # 默认监听 http://localhost:5000 ，Swagger: /swagger
   ```

2. **启动前端**
   ```bash
   cd frontend
   # 配置 API 根地址（指向后端根，不含 /api）：
   echo VITE_API_BASE_URL=http://localhost:5000 > .env
   npm install
   npm run dev   # http://localhost:5174
   ```
   前端 axios 基于 `VITE_API_BASE_URL` 自动拼接 `/api`，并在 401 时清除 token 并跳转登录。

3. **启动桌面客户端**

   ```bash
   cd DesktopClient
   # 如需修改 API 地址，编辑 Services/HttpClientService.cs 的 BaseUrl
   # 默认 "http://localhost:5097/api"，建议与后端保持一致如 "http://localhost:5000/api"
   dotnet restore
   dotnet run
   ```

### 方法 B：一键脚本（后端 + 桌面）

在仓库根目录执行：
```bash
run-all.bat
```
脚本会：
1. 还原后端依赖并在新窗口执行 `dotnet run`
2. 最长等待后端 60 秒可访问
3. 还原桌面客户端并在新窗口运行

> 注：前端需单独在 `frontend` 目录 `npm install && npm run dev`。

## 配置详解

### 后端 `Backend/appsettings.json`

- `ConnectionStrings.DefaultConnection`：数据库连接串。示例 MySQL：
  `Server=localhost;Port=3306;Database=dotnet;User=dotnet;Password=dotnet;`
  默认示例为 `Data Source=app.db`（SQLite）。
- `JwtSettings`：`SecretKey`（至少 32 字符）、`Issuer`、`Audience`、`ExpiryInHours`。
- `Storage.UploadRoot`：上传目录（相对应用根），默认 `uploads`；可改为自定义相对/绝对路径，修改后请确保目录已创建且具备写权限。
- `Logging` / `AllowedHosts`：按需调整。

### 前端 `frontend/.env`

- `VITE_API_BASE_URL=http://<backend-host>:<port>`（不要附加 `/api`，代码会自动拼接）。
- Token 存储于 `localStorage`，401 自动清除并跳转 `/login`。

### 桌面客户端

- `Services/HttpClientService.cs` 的 `BaseUrl` 控制 API 根（需要包含 `/api`）。
- Token 通过 `SetToken` 写入 HTTP Authorization 头；401 会清除 Token 并抛出异常。

## 数据库迁移

```bash
cd Backend
# 若未安装 dotnet-ef:
# dotnet tool install --global dotnet-ef

# 添加迁移
dotnet ef migrations add <MigrationName>

# 应用迁移（到配置的数据库）
dotnet ef database update
```

现有迁移示例：`20251203071151_InitialCreate`、`20251207000000_AddImageQueueHashUniqueIndex`。

## API 文档

### 1. 通用说明

| 内容     | 说明                                                         |
| -------- | ------------------------------------------------------------ |
| 认证方式 | JWT（除注册/登录外所有接口需 `Authorization: Bearer <token>`） |
| 角色     | `Admin` / `User` / `Guest`（默认注册为 Guest，需要审批）     |
| 页码参数 | `pageNumber`（默认 1），`pageSize`（默认 50）                |
| 错误格式 | `{ "message": "错误信息" }`                                  |

------

### 2. Auth 认证

| 方法     | 路径                 | 权限 | 请求体                   | 响应                                           |
| -------- | -------------------- | ---- | ------------------------ | ---------------------------------------------- |
| **POST** | `/api/auth/register` | 公共 | `{ username, password }` | `{ userId, token, username, role, expiresAt }` |
| **POST** | `/api/auth/login`    | 公共 | `{ username, password }` | 同上                                           |
| **GET**  | `/api/auth/test`     | 公共 | 无                       | `{ message }`                                  |

------

### 3. 用户（Users - Admin Only）

| 方法       | 路径                   | 权限  | 请求体       | 响应           |
| ---------- | ---------------------- | ----- | ------------ | -------------- |
| **GET**    | `/api/users/guests`    | Admin | 无           | Guest 用户数组 |
| **GET**    | `/api/users`           | Admin | 无           | 全部用户列表   |
| **POST**   | `/api/users/approve`   | Admin | `{ userId }` | `UserDto`      |
| **PUT**    | `/api/users/{id}/role` | Admin | `{ role }`   | `UserDto`      |
| **DELETE** | `/api/users/{id}`      | Admin | 无           | `{ message }`  |

------

### 4. 项目（Projects）

| 方法       | 路径                 | 权限  | 请求体                   | 响应           |
| ---------- | -------------------- | ----- | ------------------------ | -------------- |
| **GET**    | `/api/projects`      | 登录  | 无                       | `ProjectDto[]` |
| **GET**    | `/api/projects/{id}` | 登录  | 无                       | `ProjectDto`   |
| **POST**   | `/api/projects`      | Admin | `{ name, description? }` | `ProjectDto`   |
| **PUT**    | `/api/projects/{id}` | Admin | `{ name, description? }` | `ProjectDto`   |
| **DELETE** | `/api/projects/{id}` | Admin | 无                       | `{ message }`  |

**ProjectDto**

| 字段              | 含义      |
| ----------------- | --------- |
| id                | 项目 ID   |
| name              | 名称      |
| description       | 描述      |
| createdById       | 创建者 ID |
| createdByUsername | 创建者    |
| createdAt         | 时间      |
| queueCount        | 队列数量  |

------

### 5. 队列（Queues）

| 方法       | 路径                     | 权限  | 请求体                                                       | 响应          |
| ---------- | ------------------------ | ----- | ------------------------------------------------------------ | ------------- |
| **GET**    | `/api/queues?projectId=` | 登录  | 无                                                           | `QueueDto[]`  |
| **GET**    | `/api/queues/{id}`       | 登录  | 无                                                           | `QueueDto`    |
| **POST**   | `/api/queues`            | Admin | `{ projectId, name, description?, comparisonCount, isRandomOrder }` | `QueueDto`    |
| **PUT**    | `/api/queues/{id}`       | Admin | `{ name, description?, comparisonCount, status?, isRandomOrder? }` | `QueueDto`    |
| **DELETE** | `/api/queues/{id}`       | Admin | 无                                                           | `{ message }` |

**QueueDto 字段**

| 字段                  | 含义               |
| --------------------- | ------------------ |
| id                    | 队列 ID            |
| projectId             | 所属项目           |
| name                  | 名称               |
| comparisonCount       | 每组对比数（2–10） |
| groupCount            | 分组数量           |
| totalImageCount       | 总图片数           |
| status                | 状态               |
| isRandomOrder         | 是否随机           |
| createdAt / updatedAt | 时间戳             |

------

### 6. 图片与图片组（Images）

#### 图片列表

| 方法    | 路径                               | 权限 | 查询参数                                    | 响应                                  |
| ------- | ---------------------------------- | ---- | ------------------------------------------- | ------------------------------------- |
| **GET** | `/api/images/queue/{queueId}`      | 登录 | `pageNumber, pageSize, searchTerm, groupId` | `PagedResult<ImageDto>`               |
| **GET** | `/api/images/groups/{queueId}`     | 登录 | `pageNumber, pageSize, searchTerm`          | `PagedResult<ImageGroupDto>`          |
| **GET** | `/api/images/next-group/{queueId}` | 登录 | 无                                          | 下一未标注组 or `{ completed: true }` |

#### 上传图片

| 方法     | 路径                       | 权限  | FormData                              | 响应                                                         |
| -------- | -------------------------- | ----- | ------------------------------------- | ------------------------------------------------------------ |
| **POST** | `/api/images/upload`       | Admin | `queueId`, `folderName`, `file`       | `{ message, data: ImageDto, isDuplicate }`                   |
| **POST** | `/api/images/upload-batch` | Admin | `queueId`, `files[]`, `folderNames[]` | `{ successCount, skippedCount, failureCount, totalGroups, errors }` |

#### 删除

| 方法       | 路径                       | 权限  | Body            | 响应               |
| ---------- | -------------------------- | ----- | --------------- | ------------------ |
| **DELETE** | `/api/images/{id}`         | Admin | 无              | `{ message }`      |
| **POST**   | `/api/images/delete-batch` | Admin | `[id1,id2,...]` | `{ deletedCount }` |

**ImageDto 字段**

| 字段         | 含义       |
| ------------ | ---------- |
| id           | 图片 ID    |
| queueId      | 队列       |
| imageGroupId | 图片组     |
| folderName   | 原始文件夹 |
| fileName     | 文件       |
| filePath     | 访问路径   |
| fileSize     | 大小       |
| width/height | 可选维度   |
| displayOrder | 排序       |
| createdAt    | 时间       |

**ImageGroupDto 字段**

| 字段        | 含义     |
| ----------- | -------- |
| id          | 组 ID    |
| groupName   | 名称     |
| imageCount  | 图片数   |
| isCompleted | 是否完成 |
| createdAt   | 时间     |
| images      | 图片数组 |

------

### 7. 选择记录（Selections）

| 方法     | 路径                                      | 权限  | 请求体                                                       | 响应              |
| -------- | ----------------------------------------- | ----- | ------------------------------------------------------------ | ----------------- |
| **POST** | `/api/selections`                         | 登录  | `{ queueId, imageGroupId, selectedImageId, durationSeconds? }` | `SelectionDto`    |
| **GET**  | `/api/selections/{id}`                    | 登录  | 无                                                           | `SelectionDto`    |
| **GET**  | `/api/selections/queue/{queueId}?userId=` | 登录  | 无                                                           | `SelectionDto[]`  |
| **GET**  | `/api/selections/progress/{queueId}`      | 登录  | 无                                                           | `UserProgressDto` |
| **GET**  | `/api/selections/progress/all?queueId=`   | Admin | 无                                                           | 全量用户进度      |

**SelectionDto 字段**

| 字段               | 含义     |
| ------------------ | -------- |
| id                 | 记录 ID  |
| userId / username  | 用户     |
| queueId            | 队列     |
| imageGroupId       | 组       |
| selectedImageId    | 选择     |
| selectedFolderName | 原文件夹 |
| selectedImagePath  | 图片路径 |
| durationSeconds    | 停留时间 |
| createdAt          | 时间     |

**UserProgressDto 字段**

| 字段                | 含义     |
| ------------------- | -------- |
| queueId / queueName | 队列     |
| userId / username   | 用户     |
| completedGroups     | 已完成   |
| totalGroups         | 总组数   |
| progressPercentage  | 百分比   |
| lastUpdated         | 更新时间 |

------

### 8. 导出（Export - Admin）

- `GET /api/export/selections?queueId={id}&format=csv` 或 `format=json`（Admin）
  - 返回选择记录，流式下载，文件名包含队列名与时间戳。
- `GET /api/export/progress?queueId={id?}&format=csv` 或 `format=json`（Admin）
  - 返回进度统计，queueId 可选。



## 账号与角色

- 角色：`Admin`（全权限）、`User`（分配队列标注）、`Guest`（仅浏览项目）。
- 若无管理员，可在注册后手动更新数据库：
  ```sql
  UPDATE Users SET Role = 'Admin' WHERE Username = 'your_username';
  ```

## 开发/构建/测试

- 后端开发：可用 `dotnet watch run` 提升迭代速度。
- 前端构建：`npm run build`（产物位于 `frontend/dist/`）。
- 桌面客户端：`dotnet publish -c Release -r win-x64 --self-contained false` 生成发布包。
- 代码风格：前端使用 ESLint/Prettier；后端遵循 C# 命名规范。

## 部署说明

### 后端 Docker

```bash
cd Backend
docker build -t image-selection-backend .
docker run -d -p 5000:8080 ^
  -e ConnectionStrings__DefaultConnection="Server=your_mysql_host;Database=dotnet;User=dotnet;Password=dotnet;" ^
  image-selection-backend
```

### 生产环境建议

- 将 `JwtSettings:SecretKey` 设置为高强度随机字符串。
- 配置 CORS 允许的前端域名。
- 限制上传文件大小与类型，确保 `uploads` 磁盘空间充足。
- 数据库使用 `utf8mb4`，并配置备份与性能参数。

## 排障指南

- **后端无法连接数据库**：确认连接串、端口、账号权限；MySQL 需确保 `dotnet` 用户可从对应主机访问。
- **前端 401 循环跳转登录**：检查 JWT 是否过期、后端密钥变更后需重新登录；确保 `VITE_API_BASE_URL` 正确。
- **桌面客户端请求 5097 vs 5000 不一致**：调整 `HttpClientService.BaseUrl` 以匹配后端实际地址。
- **上传失败/超时**：后端和前端均设置了 5 分钟超时，大文件建议分批；检查 `Storage.UploadRoot` 权限。
- **端口占用**：修改后端 `ASPNETCORE_URLS` 或 `launchSettings.json`，前端 `npm run dev -- --port 5174`。

## 许可证

MIT License

## 联系方式

- 提交 Issue 或 Pull Request
- 邮件联系（按团队约定）
- 欢迎反馈改进建议

