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
              <el-table-column label="操作" width="320">
                <template #default="scope">
                  <el-button size="small" @click="editQueue(scope.row)">编辑</el-button>
                  <el-button size="small" type="primary" @click="openManager(scope.row)">图片管理</el-button>
                  <el-button size="small" type="danger" @click="deleteQueue(scope.row.id)">删除</el-button>
                </template>
              </el-table-column>
            </el-table>

            <el-empty v-if="queuesStore.queues.length === 0 && !loading" description="暂无队列数据" />
          </el-card>
        </el-main>
      </el-container>
    </el-container>

    <!-- 图片管理抽屉 -->
    <el-drawer
      v-model="drawerVisible"
      size="80%"
      :title="selectedQueue ? `图片管理 - ${selectedQueue.name}` : '图片管理'"
      :destroy-on-close="true"
      @closed="onDrawerClosed"
    >
      <div v-if="selectedQueue">
        <!-- 队列信息 -->
        <el-card class="info-card" style="margin-bottom: 16px">
          <template #header>
            <div class="card-header">
              <span>队列信息</span>
              <el-tag>{{ selectedQueue.projectName }}</el-tag>
            </div>
          </template>
          <el-descriptions :column="3" border>
            <el-descriptions-item label="队列名称">{{ selectedQueue.name }}</el-descriptions-item>
            <el-descriptions-item label="对比图片数">{{ selectedQueue.imageCount }} 张</el-descriptions-item>
            <el-descriptions-item label="当前总图片数">{{ selectedQueue.totalImages }}</el-descriptions-item>
            <el-descriptions-item label="图片组数">{{ calculateGroups(selectedQueue) }}</el-descriptions-item>
            <el-descriptions-item label="创建时间">{{ formatDate(selectedQueue.createdAt) }}</el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-row :gutter="16">
          <el-col :span="10">
            <!-- 已上传图片概览 -->
            <el-card class="upload-card">
              <template #header>
                <div class="card-header">
                  <div>
                    <el-icon><InfoFilled /></el-icon>
                    <span style="margin-left:8px">已上传文件</span>
                  </div>
                  <el-button size="small" @click="loadExistingImages">刷新</el-button>
                </div>
              </template>
              <div v-if="existingFolders.length === 0">
                <el-empty description="暂无已上传文件" />
              </div>
              <el-collapse v-else>
                <el-collapse-item v-for="folder in existingFolders" :key="folder.name" :name="folder.name">
                  <template #title>
                    <div class="folder-header-title">
                      <el-tag type="success" size="small">{{ folder.name }}</el-tag>
                      <el-tag size="small" style="margin-left:8px">{{ folder.files.length }} 个文件</el-tag>
                    </div>
                  </template>
                  <div class="file-list-container">
                    <div v-for="file in folder.files" :key="file.fileName" class="file-item">
                      <el-icon><Picture /></el-icon>
                      <span class="file-name" :title="file.fileName">{{ file.fileName }}</span>
                    </div>
                  </div>
                </el-collapse-item>
              </el-collapse>
            </el-card>
          </el-col>

          <el-col :span="14">
            <!-- 导入区域 -->
            <el-card class="upload-card">
              <template #header>
                <div class="card-header">
                  <div>
                    <span>添加文件夹并导入（{{ folders.length }} / {{ selectedQueue.imageCount }}）</span>
                  </div>
                  <div>
                    <el-button type="primary" size="small" @click="addFolder" :disabled="folders.length >= selectedQueue.imageCount">
                      <el-icon><Plus /></el-icon>添加文件夹
                    </el-button>
                    <el-button type="danger" size="small" @click="clearAllFolders" :disabled="folders.length === 0">
                      清空
                    </el-button>
                  </div>
                </div>
              </template>

              <div v-if="folders.length === 0">
                <el-empty description="请先添加文件夹" />
              </div>

              <div v-else class="folders-list">
                <el-collapse v-model="expandedFolders" class="folders-collapse">
                  <el-collapse-item
                    v-for="(folder, index) in folders"
                    :key="folder.id"
                    :name="folder.id"
                  >
                    <template #title>
                      <div class="folder-header-title">
                        <el-tag type="info" size="small">文件夹 {{ index + 1 }}</el-tag>
                        <el-input
                          v-model="folder.name"
                          placeholder="输入文件夹名称（如 method_a）"
                          style="width: 240px; margin-left: 10px"
                          size="small"
                          @click.stop
                        />
                        <el-tag :type="folder.files.length > 0 ? 'success' : 'info'" size="small" style="margin-left: 10px">
                          {{ folder.files.length }} 个文件
                        </el-tag>
                        <el-button type="danger" size="small" @click.stop="removeFolder(folder.id)" :icon="Delete" style="margin-left: auto">
                          删除
                        </el-button>
                      </div>
                    </template>

                    <el-card class="folder-card" shadow="never">
                      <el-upload
                        :auto-upload="false"
                        v-model:file-list="folder.files"
                        multiple
                        accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                        drag
                        class="folder-upload"
                      >
                        <el-icon class="el-icon--upload"><UploadFilled /></el-icon>
                        <div class="el-upload__text">
                          将 <strong>{{ folder.name || '此文件夹' }}</strong> 的图片拖到此处或点击选择
                        </div>
                      </el-upload>

                      <div v-if="folder.files.length > 0" class="file-list">
                        <el-divider content-position="left">待上传文件（{{ folder.files.length }} 个）</el-divider>
                        <div class="file-list-container">
                          <div
                            v-for="(file, fIndex) in (folder.files.length > MAX_VISIBLE_FILES && !fileListExpanded.get(folder.id)
                              ? folder.files.slice(0, MAX_VISIBLE_FILES)
                              : folder.files)"
                            :key="fIndex"
                            class="file-item"
                          >
                            <el-icon><Picture /></el-icon>
                            <span class="file-name" :title="file.name">{{ file.name }}</span>
                            <span class="file-size">{{ formatFileSize(file.size || 0) }}</span>
                          </div>
                        </div>
                        <el-button
                          v-if="folder.files.length > MAX_VISIBLE_FILES"
                          link
                          type="primary"
                          size="small"
                          @click="toggleFileList(folder.id)"
                        >
                          {{ fileListExpanded.get(folder.id) ? '收起' : `展开全部（${folder.files.length - MAX_VISIBLE_FILES} 个）` }}
                        </el-button>
                      </div>
                    </el-card>
                  </el-collapse-item>
                </el-collapse>
              </div>

              <div style="margin-top: 16px; text-align: right">
                <el-button type="primary" :loading="uploading" @click="handleImport">
                  {{ uploading ? '上传中...' : '开始导入' }}
                </el-button>
              </div>

              <div v-if="uploading" class="progress-panel">
                <el-progress :percentage="uploadProgress" />
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>
    </el-drawer>
    
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
import { Folder, List, Download, User, Plus, UploadFilled, Picture, InfoFilled, Delete } from '@element-plus/icons-vue'
import type { UploadUserFile } from 'element-plus'
import { imagesApi } from '../../api/images'
import type { Image } from '../../types'

const router = useRouter()
const authStore = useAuthStore()
const projectsStore = useProjectsStore()
const queuesStore = useQueuesStore()

const loading = ref(false)
const dialogVisible = ref(false)
const isEdit = ref(false)
const editId = ref<number | null>(null)
const selectedProjectId = ref<number | null>(null)
const drawerVisible = ref(false)
const selectedQueue = ref<any | null>(null)
const folders = ref<FolderItem[]>([])
const existingFolders = ref<ExistingFolder[]>([])
const expandedFolders = ref<string[]>([])
const fileListExpanded = ref<Map<string, boolean>>(new Map())
const uploading = ref(false)
const uploadProgress = ref(0)
const MAX_VISIBLE_FILES = 50

const form = reactive({
  projectId: null as number | null,
  name: '',
  imageCount: 3,
})

type UploadedFile = { fileName: string; filePath: string }
type ExistingFolder = { name: string; files: UploadedFile[] }
type FolderItem = { id: string; name: string; files: UploadUserFile[] }

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

const openManager = async (queue: any) => {
  selectedQueue.value = queue
  drawerVisible.value = true
  folders.value = []
  existingFolders.value = []
  expandedFolders.value = []
  fileListExpanded.value = new Map()
  uploadProgress.value = 0
  uploading.value = false
  await loadExistingImages()
  if (selectedQueue.value) {
    while (folders.value.length < selectedQueue.value.imageCount) {
      addFolder()
    }
  }
}

const onDrawerClosed = () => {
  selectedQueue.value = null
  folders.value = []
  existingFolders.value = []
  expandedFolders.value = []
  fileListExpanded.value = new Map()
  uploadProgress.value = 0
  uploading.value = false
}

const loadExistingImages = async () => {
  if (!selectedQueue.value) return
  try {
    const images = await imagesApi.getQueueImages(selectedQueue.value.id)
    const grouped: Record<string, UploadedFile[]> = {}
    images.forEach((img: Image) => {
      if (!grouped[img.folderName]) grouped[img.folderName] = []
      if (!grouped[img.folderName].some(f => f.fileName === img.fileName)) {
        grouped[img.folderName].push({ fileName: img.fileName, filePath: img.filePath })
      }
    })
    existingFolders.value = Object.keys(grouped).map(key => ({
      name: key,
      files: grouped[key],
    }))
  } catch (error) {
    console.error('加载已存在图片失败', error)
  }
}

const addFolder = () => {
  const newFolder: FolderItem = {
    id: `folder_${Date.now()}_${Math.random()}`,
    name: `folder_${folders.value.length + 1}`,
    files: [],
  }
  folders.value.push(newFolder)
}

const removeFolder = (folderId: string) => {
  const index = folders.value.findIndex(f => f.id === folderId)
  if (index !== -1) {
    folders.value.splice(index, 1)
    fileListExpanded.value.delete(folderId)
    const expandedIndex = expandedFolders.value.indexOf(folderId)
    if (expandedIndex > -1) {
      expandedFolders.value.splice(expandedIndex, 1)
    }
  }
}

const clearAllFolders = () => {
  folders.value = []
  expandedFolders.value = []
  fileListExpanded.value = new Map()
}

const toggleFileList = (folderId: string) => {
  const current = fileListExpanded.value.get(folderId) || false
  fileListExpanded.value.set(folderId, !current)
}

const handleImport = async () => {
  if (!selectedQueue.value) {
    ElMessage.warning('请先选择队列')
    return
  }
  if (folders.value.length === 0) {
    ElMessage.warning('请先添加文件夹')
    return
  }

  // 校验名称
  const folderNames = folders.value.map(f => f.name.trim())
  if (folderNames.some(n => !n)) {
    ElMessage.warning('请为所有文件夹命名')
    return
  }
  const nameSet = new Set(folderNames)
  if (nameSet.size !== folderNames.length) {
    ElMessage.warning('文件夹名称不能重复')
    return
  }

  const files: File[] = []
  const names: string[] = []
  folders.value.forEach(folder => {
    folder.files.forEach(file => {
      if (file.raw) {
        files.push(file.raw)
        names.push(folder.name.trim())
      }
    })
  })

  if (files.length === 0) {
    ElMessage.warning('没有可上传的文件')
    return
  }

  uploading.value = true
  uploadProgress.value = 0
  try {
    const result = await imagesApi.uploadBatch(
      selectedQueue.value.id,
      files,
      names,
      (p) => uploadProgress.value = p
    )

    const summary = `成功 ${result.successCount}，已存在跳过 ${result.skippedCount}，失败 ${result.failureCount}`
    if (result.failureCount === 0) {
      ElMessage.success(result.message || `导入完成，${summary}`)
    } else {
      ElMessage.warning(result.message || `导入结束，${summary}`)
    }

    if (result.errors.length > 0) {
      console.warn('上传错误详情:', result.errors.slice(0, 20))
    }

    await loadExistingImages()
    await loadQueues()
    clearAllFolders()
    if (selectedQueue.value) {
      while (folders.value.length < selectedQueue.value.imageCount) {
        addFolder()
      }
    }
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || error.message || '导入失败')
  } finally {
    uploading.value = false
  }
}

const formatFileSize = (bytes: number): string => {
  if (!bytes) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
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

