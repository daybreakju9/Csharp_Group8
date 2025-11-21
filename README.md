# 图像选择标注系统 (Image Selection & Annotation System)

一个功能完整的图像选择和标注管理系统，支持多项目、多队列的图像管理和用户协作，提供Web端和桌面客户端两种访问方式。

## 目录

- [功能特性](#功能特性)
- [技术栈](#技术栈)
- [系统架构](#系统架构)
- [环境要求](#环境要求)
- [快速开始](#快速开始)
- [项目结构](#项目结构)
- [API文档](#api文档)
- [用户角色](#用户角色)
- [开发指南](#开发指南)
- [部署说明](#部署说明)

## 功能特性

### 核心功能

- **项目管理**：支持创建和管理多个图像标注项目
- **队列系统**：为每个项目创建多个工作队列，实现任务分配和进度追踪
- **图像选择**：用户可以对图像进行选择标注，记录选择结果
- **数据导出**：支持导出标注数据，便于后续数据分析
- **用户管理**：完整的用户注册、登录和权限管理系统
- **进度追踪**：实时追踪用户的标注进度

### 管理员功能

- 项目创建和管理
- 队列创建和配置
- 批量图像导入（支持最大500MB）
- 用户管理和角色分配
- 数据导出和统计

### 普通用户功能

- 查看分配的项目和队列
- 进行图像选择标注
- 查看个人标注进度
- 提交标注结果

## 技术栈

### 后端 (Backend)

- **框架**：ASP.NET Core 8.0
- **数据库**：MySQL
- **ORM**：Entity Framework Core + Pomelo.EntityFrameworkCore.MySql
- **认证**：JWT (JSON Web Tokens)
- **密码加密**：BCrypt.Net
- **API文档**：Swagger/OpenAPI
- **容器化**：Docker支持


### 桌面客户端 (DesktopClient)

- **框架**：.NET 8.0 Windows Forms
- **目标平台**：Windows
- **功能**：图像标注应用

### 前端 (Frontend)

- **框架**：Vue 3 + TypeScript
- **构建工具**：Vite
- **UI组件库**：Element Plus
- **状态管理**：Pinia
- **路由管理**：Vue Router
- **HTTP客户端**：Axios

## 系统架构

```
┌─────────────────┐         ┌─────────────────┐
│   Web Frontend  │         │ Desktop Client  │
│   (Vue 3 + TS)  │         │  (WinForms)     │
└────────┬────────┘         └────────┬────────┘
         │                           │
         │      HTTP/HTTPS + JWT     │
         │                           │
         └───────────┬───────────────┘
                     │
         ┌───────────▼────────────┐
         │   Backend API Server   │
         │  (ASP.NET Core 8.0)    │
         └───────────┬────────────┘
                     │
         ┌───────────▼────────────┐
         │   MySQL Database       │
         │  (Projects, Queues,    │
         │   Images, Users, etc)  │
         └────────────────────────┘
```

## 环境要求

### 后端

- .NET 8.0 SDK
- MySQL 8.0+
- 操作系统：Windows/Linux/macOS

### 前端

- Node.js 18.0+
- npm 9.0+

### 桌面客户端

- .NET 8.0 Runtime (Windows)
- Windows 10/11

## 快速开始

### 1. 数据库配置

首先确保MySQL服务已启动，然后创建数据库：

```sql
CREATE DATABASE dotnet CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'dotnet'@'%' IDENTIFIED BY 'dotnet';
GRANT ALL PRIVILEGES ON dotnet.* TO 'dotnet'@'%';
FLUSH PRIVILEGES;
```

### 2. 后端启动

```bash
cd Backend

# 配置数据库连接
# 编辑 appsettings.json 中的 ConnectionStrings

# 还原依赖
dotnet restore

# 运行项目
dotnet run
```

后端将在 `http://localhost:5000` 启动，Swagger文档可在 `http://localhost:5000/swagger` 访问。

### 3. 前端启动

```bash
cd frontend

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

前端将在 `http://localhost:5174` 启动。

### 4. 桌面客户端启动

```bash
cd DesktopClient/ImageAnnotationApp

# 运行项目
dotnet run
```

### 5. 默认管理员账号

系统首次启动时，建议通过注册页面创建第一个用户，然后在数据库中手动将该用户的角色设置为"Admin"：

```sql
UPDATE Users SET Role = 'Admin' WHERE Username = 'your_username';
```

## 项目结构

```
final/
├── Backend/                    # 后端API服务
│   ├── Controllers/           # API控制器
│   │   ├── AuthController.cs       # 认证接口
│   │   ├── UsersController.cs      # 用户管理
│   │   ├── ProjectsController.cs   # 项目管理
│   │   ├── QueuesController.cs     # 队列管理
│   │   ├── ImagesController.cs     # 图像管理
│   │   ├── SelectionsController.cs # 选择记录
│   │   └── ExportController.cs     # 数据导出
│   ├── Models/                # 数据模型
│   │   ├── User.cs                 # 用户模型
│   │   ├── Project.cs              # 项目模型
│   │   ├── Queue.cs                # 队列模型
│   │   ├── Image.cs                # 图像模型
│   │   ├── SelectionRecord.cs      # 选择记录
│   │   └── UserProgress.cs         # 用户进度
│   ├── DTOs/                  # 数据传输对象
│   ├── Data/                  # 数据库上下文
│   ├── Services/              # 业务服务层
│   ├── uploads/               # 图像上传目录
│   ├── Program.cs             # 应用入口
│   └── appsettings.json       # 配置文件
│
├── frontend/                   # Web前端应用
│   ├── src/
│   │   ├── api/               # API接口封装
│   │   ├── stores/            # Pinia状态管理
│   │   ├── router/            # 路由配置
│   │   ├── views/             # 页面组件
│   │   │   ├── Login.vue           # 登录页
│   │   │   ├── Register.vue        # 注册页
│   │   │   ├── user/               # 用户功能页面
│   │   │   │   ├── ProjectList.vue
│   │   │   │   ├── QueueList.vue
│   │   │   │   └── ImageSelection.vue
│   │   │   └── admin/              # 管理员页面
│   │   │       ├── ProjectManagement.vue
│   │   │       ├── QueueManagement.vue
│   │   │       ├── ImageImport.vue
│   │   │       ├── DataExport.vue
│   │   │       └── UserManagement.vue
│   │   ├── types/             # TypeScript类型定义
│   │   ├── App.vue            # 根组件
│   │   └── main.ts            # 应用入口
│   ├── package.json
│   └── vite.config.ts
│
└── DesktopClient/              # 桌面客户端
    └── ImageAnnotationApp/     # 图像标注应用
        └── ImageAnnotationApp.csproj
```

## API文档

### 认证接口

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/auth/register` | 用户注册 |
| POST | `/api/auth/login` | 用户登录 |

### 用户管理

| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/users` | 获取用户列表 | Admin |
| GET | `/api/users/{id}` | 获取用户详情 | Admin |
| PUT | `/api/users/{id}` | 更新用户信息 | Admin |
| DELETE | `/api/users/{id}` | 删除用户 | Admin |

### 项目管理

| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/projects` | 获取项目列表 | All |
| GET | `/api/projects/{id}` | 获取项目详情 | All |
| POST | `/api/projects` | 创建项目 | Admin |
| PUT | `/api/projects/{id}` | 更新项目 | Admin |
| DELETE | `/api/projects/{id}` | 删除项目 | Admin |

### 队列管理

| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/queues` | 获取队列列表 | All |
| GET | `/api/queues/{id}` | 获取队列详情 | All |
| POST | `/api/queues` | 创建队列 | Admin |
| PUT | `/api/queues/{id}` | 更新队列 | Admin |
| DELETE | `/api/queues/{id}` | 删除队列 | Admin |

### 图像管理

| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/images/queue/{queueId}` | 获取队列中的图像 | All |
| POST | `/api/images/upload` | 批量上传图像 | Admin |
| DELETE | `/api/images/{id}` | 删除图像 | Admin |

### 选择记录

| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| POST | `/api/selections` | 提交选择记录 | User/Admin |
| GET | `/api/selections/queue/{queueId}` | 获取队列的选择记录 | Admin |

### 数据导出

| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| GET | `/api/export/{projectId}` | 导出项目数据 | Admin |

完整API文档可通过Swagger访问：`http://localhost:5000/swagger`

## 用户角色

系统支持三种用户角色：

### 1. Admin (管理员)
- 完整的系统管理权限
- 可以创建/编辑/删除项目、队列、用户
- 可以导入图像和导出数据
- 可以查看所有用户的标注进度

### 2. User (普通用户)
- 可以查看分配的项目和队列
- 可以进行图像选择标注
- 可以查看自己的标注进度

### 3. Guest (游客)
- 仅可以查看项目列表
- 无法进行标注操作
- 访问权限最低

## 开发指南

### 后端开发

#### 添加新的API端点

1. 在 `Controllers/` 目录下创建或编辑控制器
2. 在 `Models/` 目录下定义数据模型
3. 在 `DTOs/` 目录下定义数据传输对象
4. 在 `Data/AppDbContext.cs` 中注册DbSet

#### 数据库迁移

```bash
# 添加迁移
dotnet ef migrations add MigrationName

# 应用迁移
dotnet ef database update
```

### 前端开发

#### 添加新页面

1. 在 `src/views/` 目录下创建Vue组件
2. 在 `src/router/index.ts` 中注册路由
3. 在 `src/api/` 中添加API调用方法
4. 在 `src/types/` 中定义TypeScript类型

#### 构建生产版本

```bash
npm run build
```

构建产物将生成在 `dist/` 目录。

### 代码规范

- 后端：遵循C# 命名约定和.NET最佳实践
- 前端：使用ESLint和Prettier进行代码格式化
- 提交前确保代码通过所有测试

## 部署说明

### Docker部署

后端项目已包含Dockerfile，可以使用Docker部署：

```bash
cd Backend

# 构建镜像
docker build -t image-selection-backend .

# 运行容器
docker run -d -p 5000:8080 \
  -e ConnectionStrings__DefaultConnection="Server=your_mysql_host;Database=dotnet;User=dotnet;Password=dotnet;" \
  image-selection-backend
```

### 生产环境配置

1. **后端配置**：
   - 修改 `appsettings.json` 中的数据库连接字符串
   - 修改JWT密钥为强随机字符串
   - 配置CORS允许的域名
   - 配置文件上传目录的磁盘空间

2. **前端配置**：
   - 修改API基础URL指向生产环境后端
   - 构建生产版本并部署到Web服务器
   - 配置Nginx或其他反向代理

3. **数据库配置**：
   - 确保MySQL使用utf8mb4字符集
   - 配置数据库备份策略
   - 优化数据库性能参数

### 安全建议

- 使用HTTPS协议
- 定期更新JWT密钥
- 限制文件上传大小和类型
- 定期备份数据库
- 使用强密码策略
- 启用API速率限制

## 许可证

MIT License

## 联系方式

如有问题或建议，请通过以下方式联系：

- 提交Issue
- 发送邮件
- 提交Pull Request

---

生成时间：2025-01-21
