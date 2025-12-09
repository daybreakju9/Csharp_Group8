# 图片标注系统 - Windows 桌面客户端

这是基于 C# WinForms 开发的图片标注系统桌面客户端，对应后端 API 位于 `Backend` 文件夹，前端 Vue.js 版本位于 `frontend` 文件夹。

## 项目结构

```
DesktopClient/ImageAnnotationApp/
├── Models/                      # 数据模型
│   ├── User.cs                 # 用户相关模型和 DTO
│   ├── Project.cs              # 项目相关模型和 DTO
│   ├── Queue.cs                # 队列相关模型和 DTO
│   ├── Image.cs                # 图片相关模型
│   └── Selection.cs            # 选择记录相关模型
├── Services/                    # 服务层
│   ├── HttpClientService.cs    # HTTP 客户端服务（单例）
│   ├── AuthService.cs          # 认证服务
│   ├── ProjectService.cs       # 项目服务
│   ├── QueueService.cs         # 队列服务
│   ├── ImageService.cs         # 图片服务
│   ├── SelectionService.cs     # 选择记录服务
│   ├── UserService.cs          # 用户服务
│   └── ExportService.cs        # 数据导出服务
└── Forms/                       # 窗体
    ├── LoginForm.cs            # 登录窗体
    ├── RegisterForm.cs         # 注册窗体
    ├── MainForm.cs             # 主窗体（导航）
    ├── ProjectListForm.cs      # 项目列表
    ├── QueueListForm.cs        # 队列列表
    ├── ImageSelectionForm.cs   # 图片选择（核心功能）
    ├── ProjectManagementForm.cs # 项目管理（管理员）
    ├── QueueManagementForm.cs  # 队列管理（管理员）
    ├── UserManagementForm.cs   # 用户管理（管理员）
    └── DataExportForm.cs       # 数据导出（管理员）
```

## 技术栈

- **.NET 8.0** - Windows Forms
- **C# 12** - 现代 C# 特性
- **System.Net.Http.Json** - HTTP JSON 通信
- **Newtonsoft.Json** - JSON 序列化

## 功能特性

### 用户功能
1. **用户认证**
   - 登录/注册
   - Token 自动管理
   - 基于角色的访问控制（管理员/普通用户/游客）

2. **项目浏览**
   - 查看所有项目列表
   - 查看项目详情
   - 进入项目队列

3. **队列管理**
   - 查看队列列表
   - 查看队列详情
   - 进入图片选择

4. **图片标注（核心功能）**
   - 动态网格布局（自动计算行列数）
   - 实时进度追踪
   - 可视化图片选择
   - 手动/自动提交模式
   - 异步图片加载

### 管理员功能
1. **项目管理**
   - 创建/编辑/删除项目
   - 查看项目统计

2. **队列管理**
   - 创建/编辑/删除队列
   - 图片批量导入（预留接口）

3. **用户管理**
   - 查看待审核游客
   - 批准/拒绝用户申请
   - 查看所有用户列表

4. **数据导出**
   - 导出选择记录（CSV/JSON）
   - 导出进度数据（CSV/JSON）
   - 支持按队列筛选

## 系统要求

- Windows 10 或更高版本
- .NET 8.0 Runtime 或 SDK
- 后端 API 服务运行在 `http://localhost:5000`

## 安装和运行

### 1. 安装 .NET 8.0 SDK
从 [Microsoft 官网](https://dotnet.microsoft.com/download/dotnet/8.0) 下载并安装

### 2. 编译项目
```bash
cd DesktopClient/ImageAnnotationApp
dotnet build
```

### 3. 启动后端（在 Backend 目录）
```bash
cd Backend
dotnet run
```

### 4. 运行桌面端
```bash
dotnet run
```

或者直接运行编译后的可执行文件：
```bash
cd bin/Debug/net8.0-windows
ImageAnnotationApp.exe
```

### 5. 配置 API 地址（可选）
默认 API 地址为 `http://localhost:5000/api`

如需修改，请编辑 `Services/HttpClientService.cs` 中的 `BaseUrl` 属性：
```csharp
public string BaseUrl { get; set; } = "http://your-api-server:port/api";
```

## 使用说明

### 首次使用

1. **启动后端服务**
   ```bash
   cd Backend
   dotnet run
   ```

2. **启动桌面客户端**
   - 双击运行 `ImageAnnotationApp.exe`
   - 或使用 `dotnet run` 命令

3. **注册账号**
   - 点击"注册"按钮
   - 输入用户名和密码
   - 新注册用户为游客身份，需要管理员审核

4. **管理员审核**
   - 使用管理员账号登录
   - 进入"管理员功能" -> "用户管理"
   - 在"待审核游客"标签页批准新用户

### 普通用户工作流程

1. **登录系统**
   - 输入用户名和密码
   - 点击"登录"

2. **选择项目**
   - 在"用户功能" -> "项目列表"
   - 双击项目进入队列列表

3. **选择队列**
   - 双击队列开始图片标注

4. **标注图片**
   - 点击选择最佳图片
   - 点击"提交选择"或启用"自动提交"
   - 系统自动加载下一组图片
   - 完成所有图片后显示完成提示

### 管理员工作流程

1. **项目管理**
   - "管理员功能" -> "项目管理"
   - 新建/编辑/删除项目

2. **队列管理**
   - "管理员功能" -> "队列管理"
   - 新建队列（设置对比图片数）
   - 导入图片到队列

3. **用户管理**
   - "管理员功能" -> "用户管理"
   - 审核待批准的游客用户
   - 查看所有用户列表

4. **数据导出**
   - "管理员功能" -> "数据导出"
   - 选择队列和格式
   - 导出选择记录或进度数据

## API 接口说明

所有 API 调用通过 `HttpClientService` 单例进行，自动处理：
- Token 注入（Bearer 认证）
- 401 错误自动登出
- 统一异常处理
- 超时控制（5 分钟）

### 主要服务方法

#### AuthService
- `LoginAsync(LoginDto)` - 用户登录
- `RegisterAsync(RegisterDto)` - 用户注册
- `Logout()` - 退出登录

#### ProjectService
- `GetAllAsync()` - 获取所有项目
- `CreateAsync(CreateProjectDto)` - 创建项目
- `UpdateAsync(id, UpdateProjectDto)` - 更新项目
- `DeleteAsync(id)` - 删除项目

#### QueueService
- `GetAllAsync(projectId?)` - 获取队列列表
- `CreateAsync(CreateQueueDto)` - 创建队列
- `DeleteAsync(id)` - 删除队列

#### ImageService
- `GetNextGroupAsync(queueId)` - 获取下一组图片
- `GetImageDataAsync(imagePath)` - 获取图片数据
- `UploadImageAsync(...)` - 上传单张图片
- `UploadImagesParallelAsync(...)` - 并行批量上传

#### SelectionService
- `CreateAsync(CreateSelectionDto)` - 创建选择记录
- `GetProgressAsync(queueId)` - 获取用户进度

#### UserService
- `GetGuestUsersAsync()` - 获取待审核游客
- `ApproveUserAsync(userId)` - 批准用户
- `DeleteUserAsync(userId)` - 删除用户

#### ExportService
- `ExportSelectionsAsync(queueId?, format)` - 导出选择记录
- `ExportProgressAsync(queueId?, format)` - 导出进度数据

## 核心特性实现

### 1. 动态图片网格布局
```csharp
// 自动计算最佳网格布局
int cols = (int)Math.Ceiling(Math.Sqrt(imageCount));
int rows = (int)Math.Ceiling((double)imageCount / cols);
```

### 2. 异步图片加载
```csharp
private async void LoadImageAsync(PictureBox pictureBox, string imagePath)
{
    var imageData = await _imageService.GetImageDataAsync(imagePath);
    using var ms = new MemoryStream(imageData);
    pictureBox.Image = System.Drawing.Image.FromStream(ms);
}
```

### 3. 自动/手动提交模式
```csharp
if (chkAutoSubmit.Checked)
{
    await Task.Delay(100); // 防止误操作
    await SubmitSelectionAsync();
}
```

### 4. 实时进度追踪
```csharp
progressBar.Maximum = progress.TotalGroups;
progressBar.Value = progress.CompletedGroups;
lblProgress.Text = $"进度: {progress.CompletedGroups}/{progress.TotalGroups} ({progress.ProgressPercentage:F1}%)";
```

## 与 Vue.js 前端的对应关系

| Vue.js 组件 | WinForms 窗体 | 功能 |
|------------|--------------|------|
| Login.vue | LoginForm.cs | 登录 |
| Register.vue | RegisterForm.cs | 注册 |
| ProjectList.vue | ProjectListForm.cs | 项目列表 |
| QueueList.vue | QueueListForm.cs | 队列列表 |
| ImageSelection.vue | ImageSelectionForm.cs | 图片选择 |
| ProjectManagement.vue | ProjectManagementForm.cs | 项目管理 |
| QueueManagement.vue | QueueManagementForm.cs | 队列管理 |
| UserManagement.vue | UserManagementForm.cs | 用户管理 |
| DataExport.vue | DataExportForm.cs | 数据导出 |

## 已知限制

1. **图片批量导入 UI 尚在完善**：后台接口已就绪，桌面端批量导入仍在迭代
2. **图片缓存缺失**：当前每次都从服务器拉取，尚未加入本地缓存/预加载
3. **离线模式缺失**：暂不支持离线标注、断网续传
4. **多语言缺失**：目前仅提供中文界面

## 未来改进方向

- [ ] 完善图片批量导入 UI（含文件夹选择、进度反馈、错误提示）
- [ ] 图片本地缓存与预加载，减少重复拉取、提升列表/标注加载速度
- [ ] 键盘快捷键支持，提升标注效率
- [ ] 标注统计与可视化（进度、准确率等）
- [ ] 多语言界面支持
- [ ] 离线标注/断网续传支持
- [ ] 大量图片加载性能优化（分页/懒加载/并发控制）

## 故障排除

### 1. 无法连接到服务器
- 检查后端服务是否运行
- 确认 API 地址配置正确
- 检查防火墙设置

### 2. 登录失败
- 确认用户名密码正确
- 检查账号是否被管理员批准（非游客身份）

### 3. 图片无法加载
- 确认图片路径正确
- 检查后端图片存储服务

### 4. 编译错误
- 确保安装了 .NET 8.0 SDK
- 运行 `dotnet restore` 恢复依赖
- 清理并重新编译：`dotnet clean && dotnet build`

## 许可证

与主项目保持一致

## 联系方式

如有问题或建议，请联系开发团队
