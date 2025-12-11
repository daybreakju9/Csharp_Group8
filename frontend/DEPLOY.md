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

## 6. 使用 Docker 部署

使用 Docker 可以将应用及其依赖打包到一个标准化的容器中，从而简化部署流程并确保环境一致性。

### 步骤 1: 创建 Dockerfile

在 `frontend` 目录下创建一个名为 `Dockerfile` 的文件，并添加以下内容：

```dockerfile
# --- 构建阶段 ---
# 使用官方 Node.js 镜像作为基础镜像
FROM node:18-alpine AS builder

# 设置工作目录
WORKDIR /app

# 复制 package.json 和 package-lock.json (或 yarn.lock)
COPY package*.json ./

# 安装项目依赖
RUN npm install

# 复制项目源代码
COPY . .

# 配置生产环境变量 (如果需要)
# ARG VITE_API_BASE_URL
# ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

# 构建生产版本的应用
RUN npm run build

# --- 运行阶段 ---
# 使用 Nginx 镜像作为基础镜像来托管静态文件
FROM nginx:stable-alpine

# 将构建好的静态文件从构建阶段复制到 Nginx 的 Web 根目录
COPY --from=builder /app/dist /usr/share/nginx/html

# (可选) 复制自定义的 Nginx 配置文件
# COPY nginx.conf /etc/nginx/conf.d/default.conf

# 暴露 80 端口
EXPOSE 80

# 启动 Nginx 服务
CMD ["nginx", "-g", "daemon off;"]
```

### 步骤 2: 构建 Docker 镜像

在 `frontend` 目录下打开终端，执行以下命令：

```bash
# 构建镜像，并命名为 'image-selection-frontend'
docker build -t image-selection-frontend .
```

### 步骤 3: 运行 Docker 容器

构建成功后，使用以下命令运行容器：

```bash
# 以后台模式运行容器，并将容器的 80 端口映射到主机的 8080 端口
docker run -d -p 8080:80 --name image-selection-app image-selection-frontend
```

现在，你可以通过访问 `http://localhost:8080` 来查看你的应用。

---

## 7. CI/CD (持续集成/持续部署)

你可以通过 GitHub Actions 自动化构建和部署流程。

在项目根目录下创建 `.github/workflows/deploy.yml` 文件：

```yaml
name: Deploy Frontend

on:
  push:
    branches:
      - main  # 当 main 分支有推送时触发

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Set up Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Install dependencies
        run: npm install

      - name: Build
        run: npm run build
        env:
          VITE_API_BASE_URL: ${{ secrets.VITE_API_BASE_URL }} # 从 GitHub Secrets 获取 API 地址

      - name: Deploy to Server
        uses: appleboy/scp-action@master
        with:
          host: ${{ secrets.SERVER_HOST }}      # 服务器 IP 或域名
          username: ${{ secrets.SERVER_USER }}  # 服务器用户名
          key: ${{ secrets.SSH_PRIVATE_KEY }} # SSH 私钥
          source: "frontend/dist/*"
          target: "/var/www/html/image-selection" # 部署路径
```
**注意**: 你需要在 GitHub 项目的 `Settings > Secrets and variables > Actions` 中配置 `VITE_API_BASE_URL`, `SERVER_HOST`, `SERVER_USER`, 和 `SSH_PRIVATE_KEY`。

---

## 8. 常见问题排查

1.  **刷新页面后出现 404 错误**
    *   **原因**: Web 服务器没有正确配置以支持前端路由的 History Mode。
    *   **解决方案**: 确保你的 Nginx 或 Apache 配置中包含了重定向规则，将所有未找到的路径都指向 `index.html`。

2.  **API 请求失败 (CORS 跨域错误)**
    *   **原因**: 前端应用和后端 API 不在同一个域，且后端没有配置允许跨域请求。
    *   **解决方案**:
        *   在后端服务器上配置 CORS 策略，允许来自前端域的请求。
        *   或者，在 Nginx 中配置反向代理，将 API 请求转发到后端服务，从而避免跨域问题。

3.  **环境变量未生效**
    *   **原因**: 构建时没有正确地读取 `.env.production` 文件，或者环境变量的命名不符合 Vite 的要求 (必须以 `VITE_` 开头)。
    *   **解决方案**:
        *   确认文件名是 `.env.production`。
        *   确认环境变量名以 `VITE_` 开头，例如 `VITE_API_BASE_URL`。
        *   在代码中通过 `import.meta.env.VITE_API_BASE_URL` 来访问。

---

## 9. 优化建议

*   **启用 Gzip 压缩**: 在 Nginx 配置中启用 Gzip 可以显著减小传输文件的大小，加快加载速度。
*   **浏览器缓存**: 为静态资源 (如 CSS, JS, 图片) 设置合理的缓存策略，减少重复请求。
*   **代码分割 (Code Splitting)**: 如果你的应用比较大，可以配置 Vite 进行代码分割，实现按需加载，加快首屏渲染速度。
*   **使用 HTTPS**: 在生产环境中使用 HTTPS 来保护数据传输安全。你可以使用 Let's Encrypt 等免费证书。

---
## 10. 验证部署

访问配置的域名或 IP，检查：
1. 页面是否正常加载。
2. 尝试登录，检查是否能成功连接到后端 API。
3. 刷新非首页页面，确认没有出现 404 错误（验证 History Mode 配置）。
