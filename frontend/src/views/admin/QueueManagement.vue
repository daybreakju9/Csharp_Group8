<template>
  <div class="admin-container">
    <el-container>
      <el-aside width="200px">
        <el-menu default-active="/admin/queues" router>
          <el-menu-item index="/admin/projects">
            <el-icon><Folder /></el-icon>
            <span>项目管理</span>
          </el-menu-item>
          <el-menu-item index="/admin/queues">
            <el-icon><List /></el-icon>
            <span>队列管理</span>
          </el-menu-item>
          <el-menu-item index="/admin/users">
            <el-icon><User /></el-icon>
            <span>用户管理</span>
          </el-menu-item>
          <el-menu-item index="/admin/export">
            <el-icon><Download /></el-icon>
            <span>数据导出</span>
          </el-menu-item>
        </el-menu>
      </el-aside>
      
      <el-container>
        <el-header>
          <div class="header-content">
            <h2>队列管理</h2>
            <div>
              <el-button @click="goToUserView">用户视图</el-button>
              <el-button @click="handleLogout">退出登录</el-button>
            </div>
          </div>
        </el-header>
        
        <el-main>
          <!-- 筛选器 -->
          <el-card class="filter-card">
            <el-form :inline="true">
              <el-form-item label="选择项目">
                <el-select v-model="selectedProjectId" placeholder="全部项目" clearable @change="handleProjectChange" style="width: 200px">
                  <el-option label="全部项目" :value="null" />
                  <el-option
                    v-for="project in projectsStore.projects"
                    :key="project.id"
                    :label="project.name"
                    :value="project.id"
                  />
                </el-select>
              </el-form-item>
              <el-form-item>
                <el-button type="primary" @click="showCreateDialog">创建队列</el-button>
              </el-form-item>
            </el-form>
          </el-card>

          <!-- 队列列表 -->
          <el-card>
            <template #header>
              <div class="card-header">
                <span>队列列表</span>
                <el-button type="primary" @click="loadQueues" :loading="loading">刷新</el-button>
              </div>
            </template>
            
            <el-table :data="queuesStore.queues" v-loading="loading">
              <el-table-column prop="name" label="队列名称" width="180" />
              <el-table-column prop="projectName" label="所属项目" width="150" />
              <el-table-column prop="imageCount" label="对比图片数" width="120">
                <template #default="scope">
                  {{ scope.row.imageCount }} 张
                </template>
              </el-table-column>
              <el-table-column prop="totalImages" label="总图片数" width="120" />
              <el-table-column label="图片组数" width="100">
                <template #default="scope">
                  {{ calculateGroups(scope.row) }}
                </template>
              </el-table-column>
              <el-table-column prop="createdAt" label="创建时间" width="180">
                <template #default="scope">
                  {{ formatDate(scope.row.createdAt) }}
                </template>
              </el-table-column>
              <el-table-column label="操作" width="280">
                <template #default="scope">
                  <el-button size="small" @click="editQueue(scope.row)">编辑</el-button>
                  <el-button size="small" type="primary" @click="goToImport(scope.row.id)">导入图片</el-button>
                  <el-button size="small" type="danger" @click="deleteQueue(scope.row.id)">删除</el-button>
                </template>
              </el-table-column>
            </el-table>

            <el-empty v-if="queuesStore.queues.length === 0 && !loading" description="暂无队列数据" />
          </el-card>
        </el-main>
      </el-container>
    </el-container>
    
    <!-- 创建/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑队列' : '创建队列'" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="所属项目" required>
          <el-select v-model="form.projectId" placeholder="请选择项目" :disabled="isEdit">
            <el-option
              v-for="project in projectsStore.projects"
              :key="project.id"
              :label="project.name"
              :value="project.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="队列名称" required>
          <el-input v-model="form.name" placeholder="请输入队列名称" />
        </el-form-item>
        <el-form-item label="对比图片数" required>
          <el-input-number 
            v-model="form.imageCount" 
            :min="2" 
            :max="10" 
            placeholder="2-10张"
          />
          <div class="form-tip">设置每次同时对比的图片数量（2-10张）</div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitForm" :loading="loading">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useProjectsStore } from '../../stores/projects'
import { useQueuesStore } from '../../stores/queues'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Folder, List, Download, User } from '@element-plus/icons-vue'

const router = useRouter()
const authStore = useAuthStore()
const projectsStore = useProjectsStore()
const queuesStore = useQueuesStore()

const loading = ref(false)
const dialogVisible = ref(false)
const isEdit = ref(false)
const editId = ref<number | null>(null)
const selectedProjectId = ref<number | null>(null)

const form = reactive({
  projectId: null as number | null,
  name: '',
  imageCount: 3,
})

onMounted(async () => {
  await loadProjects()
  await loadQueues()
})

const loadProjects = async () => {
  loading.value = true
  try {
    await projectsStore.fetchProjects()
  } catch (error) {
    ElMessage.error('获取项目列表失败')
  } finally {
    loading.value = false
  }
}

const loadQueues = async () => {
  loading.value = true
  try {
    await queuesStore.fetchQueues(selectedProjectId.value || undefined)
  } catch (error) {
    ElMessage.error('获取队列列表失败')
  } finally {
    loading.value = false
  }
}

const handleProjectChange = () => {
  loadQueues()
}

const showCreateDialog = () => {
  if (projectsStore.projects.length === 0) {
    ElMessage.warning('请先创建项目')
    return
  }
  
  isEdit.value = false
  editId.value = null
  form.projectId = selectedProjectId.value || projectsStore.projects[0]?.id || null
  form.name = ''
  form.imageCount = 3
  dialogVisible.value = true
}

const editQueue = (queue: any) => {
  isEdit.value = true
  editId.value = queue.id
  form.projectId = queue.projectId
  form.name = queue.name
  form.imageCount = queue.imageCount
  dialogVisible.value = true
}

const submitForm = async () => {
  if (!form.projectId) {
    ElMessage.warning('请选择项目')
    return
  }
  if (!form.name) {
    ElMessage.warning('请输入队列名称')
    return
  }
  if (!form.imageCount || form.imageCount < 2 || form.imageCount > 10) {
    ElMessage.warning('对比图片数必须在2-10之间')
    return
  }
  
  loading.value = true
  try {
    if (isEdit.value && editId.value) {
      await queuesStore.updateQueue(editId.value, {
        name: form.name,
        imageCount: form.imageCount,
      })
      ElMessage.success('更新成功')
    } else {
      await queuesStore.createQueue({
        projectId: form.projectId,
        name: form.name,
        imageCount: form.imageCount,
      })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '操作失败')
  } finally {
    loading.value = false
  }
}

const deleteQueue = async (id: number) => {
  try {
    await ElMessageBox.confirm('确定要删除这个队列吗？删除后相关的图片和选择记录也会被删除。', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    
    loading.value = true
    await queuesStore.deleteQueue(id)
    ElMessage.success('删除成功')
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
    }
  } finally {
    loading.value = false
  }
}

const goToImport = (queueId: number) => {
  router.push(`/admin/import/${queueId}`)
}

const calculateGroups = (queue: any) => {
  if (!queue.imageCount || queue.imageCount === 0) return 0
  return Math.floor(queue.totalImages / queue.imageCount)
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const goToUserView = () => {
  router.push('/projects')
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.admin-container {
  height: 100vh;
}

.el-aside {
  background-color: #545c64;
  color: #fff;
}

.el-menu {
  border-right: none;
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

.filter-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.form-tip {
  font-size: 12px;
  color: #909399;
  margin-top: 5px;
}
</style>

