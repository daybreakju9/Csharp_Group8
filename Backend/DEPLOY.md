# 后端服务部署文档 (Backend)

## 1. 环境要求

- **操作系统**: Windows / Linux / macOS
- **运行环境**: .NET 8.0 SDK 或 Runtime
- **数据库**: MySQL 8.0+
- **其他**: Docker (可选，用于容器化部署)

## 2. 配置文件说明

在运行之前，请确保修改配置文件 `appsettings.json` (生产环境) 或 `appsettings.Development.json` (开发环境)。

### 关键配置项

```json
{
  "ConnectionStrings": {
    // 务必修改为实际的 MySQL 连接字符串
    "DefaultConnection": "Server=localhost;Port=3306;Database=image_selection_db;Uid=root;Pwd=your_password;"
  },
  "JwtSettings": {
    // 修改为强随机字符串 (至少32位)
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ImageSelectionSystem",
    "Audience": "ImageSelectionSystemUsers",
    "ExpiryInHours": 24
  },
  "Storage": {
    // 图片上传存储路径，确保程序有读写权限
    "UploadRoot": "uploads"
  }
}
```

## 3. 数据库初始化

由于项目已从 SQLite 迁移至 MySQL，首次部署必须执行数据库迁移。

1. 确保 MySQL 服务已启动，并且连接字符串中的数据库用户具有创建数据库的权限。
2. 在 `Backend` 目录下运行以下命令应用迁移：

```bash
# 如果安装了 .NET SDK
dotnet ef database update

# 或者如果构建生成了 bundle (推荐生产环境)
./efbundle
```

*注意：如果遇到连接错误，请检查防火墙设置和 MySQL 用户权限。*

## 4. 运行方式

### 方式 A: 本地直接运行 (开发/测试)

```bash
cd Backend
dotnet restore
dotnet run
```
服务默认运行在 `http://localhost:5000` 或 `https://localhost:5001`。

### 方式 B: 发布部署 (IIS / Linux Service)

1. **发布项目**:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
2. **运行**:
   将 `publish` 目录上传至服务器，运行：
   ```bash
   dotnet Backend.dll
   ```
3. **IIS / Nginx 反向代理**:
   建议使用 Nginx 或 IIS 作为反向代理服务器，转发请求到 Kestrel 服务器。

### 方式 C: Docker 部署

项目包含 `Dockerfile`。

1. **构建镜像**:
   ```bash
   docker build -t image-selection-backend .
   ```

2. **运行容器**:
   ```bash
   docker run -d \
     -p 8080:8080 \
     -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;..." \
     -v $(pwd)/uploads:/app/uploads \
     --name backend \
     image-selection-backend
   ```

## 5. 注意事项

- **文件权限**: 确保应用程序对 `uploads` 目录拥有读写权限，否则图片上传会失败。
- **端口**: 默认容器内部端口为 8080 (取决于 `Dockerfile` 配置)，请根据实际情况映射。
