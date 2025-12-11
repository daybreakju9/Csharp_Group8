# 前端项目部署文档 (Frontend)

## 1. 环境要求

- **Node.js**: v16.0 或更高版本
- **包管理器**: npm, yarn, 或 pnpm
- **Web 服务器**: Nginx, Apache, 或其他静态文件服务器

## 2. 安装依赖

在 `frontend` 目录下执行：

```bash
# 使用 npm
npm install

# 或使用 yarn
yarn install
```

## 3. 配置环境变量

在构建之前，需要配置连接后端 API 的地址。

1. 复制 `.env` (如果存在) 或创建 `.env.production` 文件。
2. 设置 API 基础地址：

```properties
# .env.production
# 请替换为实际的后端 API 地址
VITE_API_BASE_URL=http://your-backend-server-ip:5000/api
```

## 4. 开发环境运行

用于本地开发调试：

```bash
npm run dev
```
默认访问地址: `http://localhost:5173`

## 5. 生产环境构建与部署

### 步骤 1: 构建

执行构建命令，生成静态资源文件：

```bash
npm run build
```

构建完成后，会在项目根目录下生成 `dist` 文件夹。该文件夹包含了所有编译后的 HTML, CSS, 和 JavaScript 文件。

### 步骤 2: 部署到 Web 服务器 (以 Nginx 为例)

1. 将 `dist` 文件夹中的所有内容上传到服务器的 Web 根目录（例如 `/var/www/html/image-selection`）。

2. 配置 Nginx (`/etc/nginx/sites-available/default` 或新建配置文件)：

```nginx
server {
    listen 80;
    server_name your-domain.com;

    root /var/www/html/image-selection;
    index index.html;

    # 处理前端路由 (History Mode)
    # 任何找不到的路径都重定向回 index.html，交给 Vue Router 处理
    location / {
        try_files $uri $uri/ /index.html;
    }

    # 可选：反向代理后端 API (解决跨域问题)
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # 可选：代理后端上传的静态资源
    location /uploads/ {
        proxy_pass http://localhost:5000/uploads/;
    }
}
```

3. 重启 Nginx:
   ```bash
   sudo systemctl restart nginx
   ```

## 6. 验证部署

访问配置的域名或 IP，检查：
1. 页面是否正常加载。
2. 尝试登录，检查是否能成功连接到后端 API。
3. 刷新非首页页面，确认没有出现 404 错误（验证 History Mode 配置）。
