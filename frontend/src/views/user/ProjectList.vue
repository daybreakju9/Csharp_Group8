<template>
  <div class="project-list-container">
    <el-container>
      <el-header>
        <div class="header-content">
          <h2>项目列表</h2>
          <div class="user-info">
            <span>{{ authStore.user?.username }}</span>
            <el-button @click="handleLogout">退出登录</el-button>
          </div>
        </div>
      </el-header>
      
      <el-main>
        <!-- 游客用户提示 -->
        <el-alert
          v-if="authStore.user?.role === 'Guest'"
          title="等待审核"
          type="warning"
          :closable="false"
          style="margin-bottom: 20px;"
        >
          您的账号正在等待管理员审核。审核通过后，您将能够参与标注工作。在此期间，您只能查看项目列表。
        </el-alert>

        <el-card v-loading="loading">
          <el-row :gutter="20">
            <el-col
              v-for="project in projectsStore.projects"
              :key="project.id"
              :xs="24"
              :sm="12"
              :md="8"
              :lg="6"
            >
              <el-card shadow="hover" class="project-card" @click="goToQueues(project.id)">
                <div class="project-content">
                  <h3>{{ project.name }}</h3>
                  <p class="description">{{ project.description || '无描述' }}</p>
                  <div class="project-info">
                    <el-tag type="info">{{ project.queueCount }} 个队列</el-tag>
                    <span class="created-at">{{ formatDate(project.createdAt) }}</span>
                  </div>
                </div>
              </el-card>
            </el-col>
          </el-row>
          
          <el-empty v-if="!loading && projectsStore.projects.length === 0" description="暂无项目" />
        </el-card>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useProjectsStore } from '../../stores/projects'
import { ElMessage } from 'element-plus'

const router = useRouter()
const authStore = useAuthStore()
const projectsStore = useProjectsStore()
const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    await projectsStore.fetchProjects()
  } catch (error) {
    ElMessage.error('获取项目列表失败')
  } finally {
    loading.value = false
  }
})

const goToQueues = (projectId: number) => {
  // 检查游客权限
  if (authStore.user?.role === 'Guest') {
    ElMessage.warning('游客账号无法参与标注工作，请等待管理员审核')
    return
  }

  router.push(`/projects/${projectId}/queues`)
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN')
}
</script>

<style scoped>
.project-list-container {
  min-height: 100vh;
  background-color: #f5f5f5;
}

.el-header {
  background-color: #409eff;
  color: white;
  display: flex;
  align-items: center;
}

.header-content {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-content h2 {
  margin: 0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.el-main {
  padding: 20px;
}

.project-card {
  cursor: pointer;
  margin-bottom: 20px;
  transition: transform 0.3s;
}

.project-card:hover {
  transform: translateY(-5px);
}

.project-content h3 {
  margin-top: 0;
  color: #303133;
}

.description {
  color: #606266;
  min-height: 40px;
}

.project-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 10px;
}

.created-at {
  font-size: 12px;
  color: #909399;
}
</style>

