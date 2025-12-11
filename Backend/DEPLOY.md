# 后端服务部署文档 (Backend)

## 1. 环境要求

- **操作系统**: Windows / Linux / macOS
- **运行环境**: .NET 8.0 SDK 或 Runtime
- **数据库**: MySQL 8.0+
- **其他**: 
  - Docker (可选，用于容器化部署)
  - [.NET EF Core 工具](https://learn.microsoft.com/zh-cn/ef/core/cli/dotnet) (用于数据库迁移)

## 2. 首次设置

### 2.1. 克隆仓库

```bash
git clone <repository-url>
cd image-selection-system/Backend
```

### 2.2. 安装 .NET EF Core 工具

如果尚未安装，请运行以下命令：

```bash
dotnet tool install --global dotnet-ef
```

## 3. 配置文件说明

在运行之前，请确保修改根目录下的 `appsettings.json` (生产环境) 或 `appsettings.Development.json` (开发环境) 配置文件。

### 关键配置项

```json
{
  // ... (日志等其他配置)
  "ConnectionStrings": {
    // 务必修改为实际的 MySQL 连接字符串
    "DefaultConnection": "Server=localhost;Port=3306;Database=image_selection_db;Uid=root;Pwd=your_password;"
  },
  "JwtSettings": {
    // 【重要】修改为强随机字符串 (至少32位)
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ImageSelectionSystem",
    "Audience": "ImageSelectionSystemUsers",
    "ExpiryInHours": 24
  },
  "Storage": {
    // 图片上传存储的根路径 (相对或绝对路径)
    // 确保应用程序对此目录有读写权限
    "UploadRoot": "uploads"
  }
}
```

- **`ConnectionStrings:DefaultConnection`**: 数据库连接信息。
- **`JwtSettings:SecretKey`**: 用于签发 JWT 的密钥，**必须修改**以保证安全。
- **`Storage:UploadRoot`**: 用于存储上传图片的目录。如果不存在，程序启动时会自动创建。

## 4. 数据库初始化

项目启动时会自动应用数据库迁移并创建初始数据。

1. **确保数据库服务已启动**，并且连接字符串中的用户具有创建数据库和表的权限。
2. **首次运行项目时**，EF Core 迁移会自动执行。您也可以手动执行：

   ```bash
   # 在 Backend 目录下运行
   dotnet ef database update
   ```

3. **初始管理员账号**:
   - **用户名**: `admin`
   - **初始密码**: `Admin@123`
   
   系统启动后，如果 `admin` 用户不存在，会自动创建此账号。**强烈建议首次登录后立即修改密码！**

## 5. 运行方式

### 方式 A: 本地开发环境运行

此方式适用于开发和测试。

```bash
# 进入后端项目目录
cd Backend

# 还原依赖
dotnet restore

# 运行项目 (使用 http profile)
dotnet run --launch-profile http
```
根据 `Properties/launchSettings.json` 配置，服务默认会运行在 `http://localhost:5097`。

### 方式 B: 发布并部署

此方式适用于生产环境。

1. **发布项目**:
   在 `Backend` 目录下，运行以下命令将项目发布到 `publish` 文件夹。

   ```bash
   dotnet publish -c Release -o ./publish
   ```
   
2. **部署和运行**:
   - 将 `publish` 目录的全部内容上传至服务器。
   - 确保服务器上已安装 .NET 8.0 Runtime。
   - 运行应用程序：
     ```bash
     dotnet Backend.dll
     ```

3. **使用反向代理 (推荐)**:
   在生产环境中，强烈建议使用 Nginx 或 IIS 作为反向代理，将外部请求转发到 Kestrel 服务器。这可以提供负载均衡、SSL 终止和更高的安全性。

### 方式 C: Docker 部署

项目已提供 `Dockerfile`，可以方便地进行容器化部署。

1. **构建 Docker 镜像**:
   在 `Backend` 目录下，运行：
   ```bash
   docker build -t image-selection-backend .
   ```

2. **运行 Docker 容器**:
   ```bash
   docker run -d \
     -p 8080:8080 \
     -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal;Port=3306;Database=db;Uid=user;Pwd=pass;" \
     -v /path/on/host/uploads:/app/uploads \
     --name backend-container \
     image-selection-backend
   ```
   
   **参数说明**:
   - `-p 8080:8080`: 将主机的 8080 端口映射到容器的 8080 端口。
   - `-e "..."`: 通过环境变量覆盖 `appsettings.json` 中的配置。**请务必修改为您的数据库连接字符串**。
     - `host.docker.internal` 是一个特殊的 DNS 名称，用于从 Docker 容器内部访问宿主机。
   - `-v /path/on/host/uploads:/app/uploads`: 将主机上的目录挂载到容器内，用于持久化存储上传的图片。**请务必修改 `/path/on/host/uploads`**。

## 6. API 和接口文档

服务启动后，可以通过访问 `/swagger` 路径来查看和测试 API 接口 (仅限开发环境)。

- **Swagger UI**: `http://localhost:5097/swagger`

## 7. 高级配置与技术细节

### 7.1. 文件上传限制

为了处理大文件上传，服务进行了如下配置 (`Program.cs`)：
- **最大请求体大小**: `2 GB`
- **请求头超时**: 5 分钟

这意味着理论上可以上传接近 2GB 的单个文件，但请注意网络稳定性和服务器磁盘空间。

### 7.2. 静态文件缓存

在生产环境中，所有通过 `/uploads` 路径访问的图片资源都会被设置 `Cache-Control` 响应头，缓存有效期为 **1 小时** (`max-age=3600`)。这有助于减轻服务器压力并提升客户端加载速度。开发环境中缓存是禁用的。

### 7.3. 通过环境变量覆盖配置

在 Docker 或其他生产环境中，最佳实践是通过环境变量覆盖 `appsettings.json` 中的配置项。环境变量的格式为 `Section__NestedKey=Value`。

例如，除了数据库连接字符串，你还可以覆盖 JWT 密钥：

```bash
docker run -d \
  -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=..." \
  -e "JwtSettings__SecretKey=A_VERY_SECURE_AND_LONG_SECRET_FROM_ENV_VARS" \
  -v /path/on/host/uploads:/app/uploads \
  --name backend-container \
  image-selection-backend
```

## 8. 安全建议

为确保生产环境的安全，请务必遵守以下准则：

- **最小权限原则**: 运行应用程序和连接数据库的用户应仅授予必要的最小权限。
- **修改默认凭据**:
  - **立即**修改初始管理员账号 `admin` 的密码。
  - **务必**使用强密码作为数据库连接字符串中的密码。
- **保护 Secret Key**:
  - **务必**将 `JwtSettings:SecretKey` 替换为一个长而复杂的随机字符串。
  - 在生产环境中，强烈建议通过环境变量 (`JwtSettings__SecretKey`) 或更安全的密钥管理服务来提供此密钥，而不是硬编码在 `appsettings.json` 文件中。
- **禁用开发功能**:
  - **不要**在生产环境中使用 `appsettings.Development.json`。
  - 确保 Swagger UI (`/swagger`) 在生产环境中被禁用，以防暴露过多 API 细节。

## 9. 故障排查 (Troubleshooting)

### 9.1. 查看日志

应用程序的日志默认输出到**控制台**。这是排查问题的首要步骤。
- **直接运行 (`dotnet Backend.dll`)**: 日志会直接显示在当前终端。
- **Docker 部署**: 使用 `docker logs` 命令查看容器的输出。
  ```bash
  docker logs backend-container
  # 持续跟踪日志
  docker logs -f backend-container
  ```

### 9.2. 常见问题

#### 问题 1: 程序启动失败或数据库连接错误

- **现象**: 启动时日志中出现 `MySqlException` 或与数据库连接相关的错误。
- **排查步骤**:
  1. **检查连接字符串**: 确认 `appsettings.json` 或环境变量中的 `ConnectionStrings:DefaultConnection` 是否完全正确（服务器地址、端口、数据库名、用户名、密码）。
  2. **网络与防火墙**: 确保应用服务器可以访问到 MySQL 服务器的端口（默认为 `3306`）。检查中间是否有防火墙或安全组规则阻挡了连接。
  3. **数据库用户权限**: 确认连接字符串中使用的用户具有对目标数据库的 `CREATE`, `READ`, `WRITE`, `UPDATE`, `DELETE` 等权限。首次部署时，最好有 `CREATE DATABASE` 权限。
  4. **MySQL 服务状态**: 确认 MySQL 服务正在运行。

#### 问题 2: 图片上传失败 (通常返回 HTTP 400, 413 或 500 错误)

- **现象**: 调用上传接口时失败。
- **排查步骤**:
  1. **检查文件权限**: 这是最常见的原因。确认运行 .NET 应用程序的用户（或 Docker 容器）对 `Storage:UploadRoot` 配置的目录（默认为 `uploads`）具有**读写权限**。
     - 对于 Linux 系统，可以使用 `ls -ld uploads` 查看权限，并使用 `chown` / `chmod` 修改。
  2. **检查磁盘空间**: 确保服务器的磁盘空间充足。
  3. **检查请求大小**: 确认上传的文件大小没有超过 Nginx 等反向代理服务器的 `client_max_body_size` 限制（如果使用了反向代理）。服务本身配置的限制是 2GB，通常足够。

#### 问题 3: API 返回 401 Unauthorized (未授权)

- **现象**: 访问需要登录的接口时，即使提供了 Token，依然被拒绝。
- **排查步骤**:
  1. **检查 Token 是否正确传递**: 确认请求头中包含了 `Authorization: Bearer <your_token>`。
  2. **检查 Secret Key**: 确认生产环境中使用的 `JwtSettings:SecretKey` 与签发 Token 时使用的密钥完全一致。如果不一致（例如，一个来自 `appsettings.json`，一个来自环境变量），验证将失败。
  3. **检查 Token 是否过期**: JWT 具有时效性（本项目默认为 24 小时）。
  4. **检查 Issuer 和 Audience**: 确认 `JwtSettings` 中的 `Issuer` 和 `Audience` 配置与预期相符。
