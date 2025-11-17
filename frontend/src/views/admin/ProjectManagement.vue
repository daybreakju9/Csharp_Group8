<template>
  <div class="admin-container">
    <el-container>
      <el-aside width="200px">
        <el-menu default-active="/admin/projects" router>
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
            <h2>项目管理</h2>
            <div>
              <el-button @click="goToUserView">用户视图</el-button>
              <el-button @click="handleLogout">退出登录</el-button>
            </div>
          </div>
        </el-header>
        
        <el-main>
          <el-card>
            <template #header>
              <div class="card-header">
                <span>项目列表</span>
                <el-button type="primary" @click="showCreateDialog">创建项目</el-button>
              </div>
            </template>
            
            <el-table :data="projectsStore.projects" v-loading="loading">
              <el-table-column prop="name" label="项目名称" />
              <el-table-column prop="description" label="描述" />
              <el-table-column prop="queueCount" label="队列数" width="100" />
              <el-table-column prop="createdByUsername" label="创建者" width="120" />
              <el-table-column label="操作" width="180">
                <template #default="scope">
                  <el-button size="small" @click="editProject(scope.row)">编辑</el-button>
                  <el-button size="small" type="danger" @click="deleteProject(scope.row.id)">删除</el-button>
                </template>
              </el-table-column>
            </el-table>
          </el-card>
        </el-main>
      </el-container>
    </el-container>
    
    <!-- 创建/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑项目' : '创建项目'">
      <el-form :model="form" label-width="80px">
        <el-form-item label="项目名称">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitForm">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useProjectsStore } from '../../stores/projects'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Folder, List, Download, User } from '@element-plus/icons-vue'

const router = useRouter()
const authStore = useAuthStore()
const projectsStore = useProjectsStore()

const loading = ref(false)
const dialogVisible = ref(false)
const isEdit = ref(false)
const editId = ref<number | null>(null)

const form = reactive({
  name: '',
  description: '',
})

onMounted(async () => {
  await loadProjects()
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

const showCreateDialog = () => {
  isEdit.value = false
  editId.value = null
  form.name = ''
  form.description = ''
  dialogVisible.value = true
}

const editProject = (project: any) => {
  isEdit.value = true
  editId.value = project.id
  form.name = project.name
  form.description = project.description || ''
  dialogVisible.value = true
}

const submitForm = async () => {
  if (!form.name) {
    ElMessage.warning('请输入项目名称')
    return
  }
  
  try {
    if (isEdit.value && editId.value) {
      await projectsStore.updateProject(editId.value, form)
      ElMessage.success('更新成功')
    } else {
      await projectsStore.createProject(form)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
  } catch (error) {
    ElMessage.error('操作失败')
  }
}

const deleteProject = async (id: number) => {
  try {
    await ElMessageBox.confirm('确定要删除这个项目吗？', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    
    await projectsStore.deleteProject(id)
    ElMessage.success('删除成功')
  } catch (error) {
    // 用户取消
  }
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

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>

