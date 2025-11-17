<template>
  <div class="import-container">
    <el-page-header @back="goBack" title="返回">
      <template #content>
        <span class="page-title">批量导入图片</span>
      </template>
    </el-page-header>

    <div class="content">
      <!-- 队列信息 -->
      <el-card v-if="queue" class="info-card">
        <template #header>
          <div class="card-header">
            <span>队列信息</span>
            <el-tag>{{ queue.projectName }}</el-tag>
          </div>
        </template>
        <el-descriptions :column="2" border>
          <el-descriptions-item label="队列名称">{{ queue.name }}</el-descriptions-item>
          <el-descriptions-item label="对比图片数">{{ queue.imageCount }} 张</el-descriptions-item>
          <el-descriptions-item label="当前总图片数">{{ queue.totalImages }}</el-descriptions-item>
          <el-descriptions-item label="图片组数">{{ calculateGroups() }}</el-descriptions-item>
        </el-descriptions>
      </el-card>

      <!-- 导入说明 -->
      <el-card class="tips-card">
        <template #header>
          <div class="card-header">
            <el-icon><InfoFilled /></el-icon>
            <span>导入说明</span>
          </div>
        </template>
        <el-alert type="info" :closable="false" show-icon>
          <template #title>
            <div class="alert-content">
              <p><strong>批量导入规则：</strong></p>
              <ol>
                <li>需要上传 <strong>{{ queue?.imageCount }} 个文件夹</strong>的图片（对应设置的对比图片数）</li>
                <li>每个文件夹给一个名称（如：method_a, method_b 等）</li>
                <li>每个文件夹上传相同的图片文件名（如：image1.jpg, image2.jpg）</li>
                <li>系统会自动按文件名分组，生成对比组</li>
                <li>支持的图片格式：jpg, jpeg, png, gif, webp</li>
              </ol>
            </div>
          </template>
        </el-alert>
      </el-card>

      <!-- 文件夹上传区域 -->
      <el-card class="upload-card">
        <template #header>
          <div class="card-header">
            <span>上传文件夹（{{ folders.length }} / {{ queue?.imageCount }}）</span>
            <div>
              <el-button 
                type="primary" 
                size="small" 
                @click="addFolder"
                :disabled="folders.length >= (queue?.imageCount || 0)"
              >
                <el-icon><Plus /></el-icon>
                添加文件夹
              </el-button>
              <el-button 
                type="danger" 
                size="small" 
                @click="clearAllFolders"
                :disabled="folders.length === 0"
              >
                清空全部
              </el-button>
            </div>
          </div>
        </template>

        <!-- 文件夹列表 -->
        <div v-if="folders.length === 0" class="empty-tip">
          <el-empty description="请先添加文件夹">
            <el-button type="primary" @click="addFolder">添加文件夹</el-button>
          </el-empty>
        </div>

        <div v-else class="folders-list">
          <el-collapse v-model="expandedFolders" class="folders-collapse">
            <el-collapse-item 
              v-for="(folder, index) in folders" 
              :key="folder.id"
              :name="folder.id"
              class="folder-collapse-item"
            >
              <template #title>
                <div class="folder-header-title">
                  <el-tag type="info" size="small">文件夹 {{ index + 1 }}</el-tag>
                  <el-input
                    v-model="folder.name"
                    placeholder="输入文件夹名称（如：method_a）"
                    style="width: 250px; margin-left: 10px"
                    size="small"
                    @click.stop
                  >
                    <template #prepend>
                      <el-icon><Folder /></el-icon>
                    </template>
                  </el-input>
                  <el-tag 
                    :type="folder.files.length > 0 ? 'success' : 'info'" 
                    size="small"
                    style="margin-left: 10px"
                  >
                    {{ folder.files.length }} 个文件
                  </el-tag>
                  <el-button 
                    type="danger" 
                    size="small" 
                    @click.stop="removeFolder(folder.id)"
                    :icon="Delete"
                    style="margin-left: auto"
                  >
                    删除
                  </el-button>
                </div>
              </template>
              
              <el-card 
                class="folder-card"
                :class="{ 'has-files': folder.files.length > 0 }"
                shadow="never"
              >

            <el-upload
              :ref="(el: UploadInstance | null) => setUploadRef(folder.id, el)"
              :auto-upload="false"
              v-model:file-list="folder.files"
              multiple
              accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
              drag
              class="folder-upload"
            >
              <el-icon class="el-icon--upload"><UploadFilled /></el-icon>
              <div class="el-upload__text">
                将 <strong>{{ folder.name || '此文件夹' }}</strong> 的图片拖到此处
              </div>
              <template #tip>
                <div class="el-upload__tip">
                  或点击选择文件
                </div>
              </template>
            </el-upload>

            <!-- 已上传文件列表 -->
            <div v-if="folder.uploadedFiles && folder.uploadedFiles.length > 0" class="uploaded-files-list">
              <el-divider>
                <div class="file-list-header">
                  <span>已上传文件（{{ folder.uploadedFiles.length }} 个）</span>
                </div>
              </el-divider>
              <div class="file-list-container">
                <div 
                  v-for="(uploadedFile, uIndex) in folder.uploadedFiles" 
                  :key="uIndex"
                  class="file-item uploaded-file-item"
                >
                  <el-icon><Picture /></el-icon>
                  <span class="file-name" :title="uploadedFile.fileName">{{ uploadedFile.fileName }}</span>
                  <el-tag type="success" size="small">已上传</el-tag>
                </div>
              </div>
            </div>

            <!-- 待上传文件列表 -->
            <div v-if="folder.files.length > 0" class="file-list">
              <el-divider>
                <div class="file-list-header">
                  <span>待上传文件（{{ folder.files.length }} 个）</span>
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
              </el-divider>
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
            </div>
              </el-card>
            </el-collapse-item>
          </el-collapse>
        </div>
      </el-card>

      <!-- 预览统计 -->
      <el-card v-if="folders.length > 0 && totalFiles > 0" class="preview-card">
        <template #header>
          <div class="card-header">
            <span>导入预览</span>
            <el-tag type="success">{{ groupedFileNames.length }} 个图片组</el-tag>
          </div>
        </template>

        <el-table 
          :data="previewData" 
          border 
          :height="400"
          style="width: 100%"
        >
          <el-table-column label="图片文件名" prop="fileName" width="200" />
          <el-table-column 
            v-for="folder in folders" 
            :key="folder.id"
            :label="folder.name || `文件夹${folders.indexOf(folder) + 1}`"
            align="center"
            width="120"
          >
            <template #default="scope">
              <el-icon v-if="scope.row.folders.includes(folder.id)" color="#67C23A" :size="20">
                <SuccessFilled />
              </el-icon>
              <el-icon v-else color="#F56C6C" :size="20">
                <CircleClose />
              </el-icon>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="100" align="center">
            <template #default="scope">
              <el-tag 
                :type="scope.row.isComplete ? 'success' : 'warning'"
                size="small"
              >
                {{ scope.row.isComplete ? '完整' : '不完整' }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>

        <el-alert 
          v-if="incompleteGroups > 0"
          type="warning"
          :closable="false"
          show-icon
          style="margin-top: 15px"
        >
          <template #title>
            有 {{ incompleteGroups }} 个图片组不完整（未在所有文件夹中找到）
          </template>
        </el-alert>
      </el-card>

      <!-- 上传进度 -->
      <el-card v-if="uploading" class="progress-card">
        <template #header>
          <div class="card-header">
            <span>正在上传...</span>
            <el-tag type="info">{{ uploadProgress }}%</el-tag>
          </div>
        </template>
        <el-progress 
          :percentage="uploadProgress" 
          :status="uploadProgress === 100 ? 'success' : undefined"
          :stroke-width="20"
        >
          <template #default="{ percentage }">
            <span class="progress-text">{{ percentage }}%</span>
          </template>
        </el-progress>
        <div style="margin-top: 15px;">
          <p style="text-align: center; color: #409eff; font-weight: 600;">
            进度：{{ uploadStats.completed }} / {{ uploadStats.total }} 个文件已完成
          </p>
          <p v-if="currentUploadingFile" style="margin-top: 8px; text-align: center; color: #909399; font-size: 13px;">
            当前上传：{{ currentUploadingFile }}
          </p>
          <p style="margin-top: 8px; text-align: center; color: #909399; font-size: 12px;">
            请勿关闭页面，正在并行上传中...
          </p>
        </div>
      </el-card>

      <!-- 操作按钮 -->
      <div class="action-buttons">
        <el-button @click="goBack" :disabled="uploading">取消</el-button>
        <el-button 
          type="primary" 
          @click="handleImport"
          :loading="uploading"
          :disabled="!canImport"
        >
          <el-icon><Upload /></el-icon>
          开始导入 ({{ totalFiles }} 个文件，{{ groupedFileNames.length }} 个组)
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { UploadUserFile, UploadInstance } from 'element-plus'
import { 
  UploadFilled, Upload, InfoFilled, Plus, Delete, Folder, 
  Picture, SuccessFilled, CircleClose 
} from '@element-plus/icons-vue'
import { queuesApi } from '../../api/queues'
import { imagesApi } from '../../api/images'
import type { Queue } from '../../types'

const router = useRouter()
const route = useRoute()

const queueId = ref<number>(parseInt(route.params.queueId as string))
const queue = ref<Queue | null>(null)
const uploading = ref(false)
const uploadProgress = ref(0)
const currentUploadingFile = ref<string>('')
const uploadStats = ref({ completed: 0, total: 0 })
const fileProgressMap = ref<Map<number, number>>(new Map())

interface FolderItem {
  id: string
  name: string
  files: UploadUserFile[]
  uploadedFiles?: Array<{ fileName: string; filePath: string }> // 已上传的文件
}

const folders = ref<FolderItem[]>([])
const uploadRefs = ref<Map<string, UploadInstance>>(new Map())
const expandedFolders = ref<string[]>([]) // 展开的文件夹ID列表（用于el-collapse）
const fileListExpanded = ref<Map<string, boolean>>(new Map()) // 文件列表展开状态
const MAX_VISIBLE_FILES = 10 // 每个文件夹最多显示的文件数

// 设置 upload ref
const setUploadRef = (folderId: string, el: UploadInstance | null) => {
  if (el) {
    uploadRefs.value.set(folderId, el)
  }
}

// 计算总文件数
const totalFiles = computed(() => {
  return folders.value.reduce((sum, folder) => sum + folder.files.length, 0)
})

// 获取所有文件名（去重），包括已上传和待上传的文件
const groupedFileNames = computed(() => {
  const fileNames = new Set<string>()
  folders.value.forEach(folder => {
    // 添加已上传的文件名
    if (folder.uploadedFiles) {
      folder.uploadedFiles.forEach(file => {
        fileNames.add(file.fileName)
      })
    }
    // 添加待上传的文件名
    folder.files.forEach(file => {
      fileNames.add(file.name)
    })
  })
  return Array.from(fileNames).sort()
})

// 预览数据
const previewData = computed(() => {
  return groupedFileNames.value.map(fileName => {
    const foldersWithFile: string[] = []
    folders.value.forEach(folder => {
      // 检查已上传的文件
      const hasUploadedFile = folder.uploadedFiles?.some(f => f.fileName === fileName) || false
      // 检查待上传的文件
      const hasPendingFile = folder.files.some(f => f.name === fileName)
      
      if (hasUploadedFile || hasPendingFile) {
        foldersWithFile.push(folder.id)
      }
    })
    return {
      fileName,
      folders: foldersWithFile,
      isComplete: foldersWithFile.length === folders.value.length
    }
  })
})

// 不完整的组数
const incompleteGroups = computed(() => {
  return previewData.value.filter(item => !item.isComplete).length
})

// 是否可以导入（只要有待上传的文件即可）
const canImport = computed(() => {
  // 至少有一个文件夹
  if (folders.value.length === 0) return false
  
  // 检查是否有待上传的文件
  const hasFilesToUpload = folders.value.some(f => f.files.length > 0)
  if (!hasFilesToUpload) return false
  
  // 每个有待上传文件的文件夹都有名称
  const hasUnnamedFolderWithFiles = folders.value.some(f => 
    f.files.length > 0 && !f.name.trim()
  )
  if (hasUnnamedFolderWithFiles) return false
  
  // 至少有一个完整的图片组（包括已上传的文件）
  return previewData.value.some(item => item.isComplete)
})

onMounted(async () => {
  await loadQueue()
  await loadExistingImages()
  // 如果还没有足够的文件夹，添加额外的文件夹
  if (queue.value) {
    while (folders.value.length < queue.value.imageCount) {
      addFolder()
    }
    // 默认展开第一个文件夹
    if (folders.value.length > 0) {
      expandedFolders.value = [folders.value[0].id]
    }
  }
})

const loadQueue = async () => {
  try {
    queue.value = await queuesApi.getById(queueId.value)
  } catch (error) {
    ElMessage.error('获取队列信息失败')
    router.back()
  }
}

const loadExistingImages = async () => {
  try {
    const existingImages = await imagesApi.getQueueImages(queueId.value)
    
    if (existingImages.length === 0) {
      // 没有已存在的图片，直接返回
      return
    }

    // 按 folderName 分组
    const folderMap = new Map<string, Array<{ fileName: string; filePath: string }>>()
    
    existingImages.forEach(image => {
      const folderName = image.folderName
      if (!folderMap.has(folderName)) {
        folderMap.set(folderName, [])
      }
      // 去重：同一个文件夹中相同文件名的只显示一次
      const files = folderMap.get(folderName)!
      if (!files.some(f => f.fileName === image.fileName)) {
        files.push({
          fileName: image.fileName,
          filePath: image.filePath
        })
      }
    })

    // 为每个已存在的文件夹创建文件夹项
    folderMap.forEach((uploadedFiles, folderName) => {
      const folder: FolderItem = {
        id: `folder_${Date.now()}_${Math.random()}`,
        name: folderName,
        files: [],
        uploadedFiles: uploadedFiles
      }
      folders.value.push(folder)
    })
    
    // 按文件夹名称排序（保持一致性）
    folders.value.sort((a, b) => a.name.localeCompare(b.name))
    
  } catch (error) {
    console.error('加载已存在图片失败:', error)
    // 不阻止页面加载，只是不显示已存在的文件
  }
}

const addFolder = () => {
  const newFolder: FolderItem = {
    id: `folder_${Date.now()}_${Math.random()}`,
    name: `folder_${folders.value.length + 1}`,
    files: []
  }
  folders.value.push(newFolder)
}

const removeFolder = (folderId: string) => {
  const index = folders.value.findIndex(f => f.id === folderId)
  if (index !== -1) {
    folders.value.splice(index, 1)
    uploadRefs.value.delete(folderId)
    const expandedIndex = expandedFolders.value.indexOf(folderId)
    if (expandedIndex > -1) {
      expandedFolders.value.splice(expandedIndex, 1)
    }
    fileListExpanded.value.delete(folderId)
  }
}

const toggleFileList = (folderId: string) => {
  const current = fileListExpanded.value.get(folderId) || false
  fileListExpanded.value.set(folderId, !current)
}

const clearAllFolders = () => {
  folders.value = []
  uploadRefs.value.clear()
  expandedFolders.value = []
  fileListExpanded.value.clear()
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}

const calculateGroups = (): number => {
  if (!queue.value || !queue.value.imageCount) return 0
  return Math.floor(queue.value.totalImages / queue.value.imageCount)
}

const handleImport = async () => {
  // 验证
  if (folders.value.length === 0) {
    ElMessage.warning('请先添加文件夹')
    return
  }

  // 检查是否有待上传文件但文件夹名为空的
  const emptyFolderWithPendingFiles = folders.value.find(f => f.files.length > 0 && !f.name.trim())
  if (emptyFolderWithPendingFiles) {
    ElMessage.warning('请为所有有待上传文件的文件夹命名')
    return
  }

  // 检查是否有待上传的文件
  const hasFilesToUpload = folders.value.some(f => f.files.length > 0)
  if (!hasFilesToUpload) {
    ElMessage.warning('没有待上传的文件')
    return
  }

  // 检查文件夹名称重复（只检查有名称的文件夹）
  const folderNames = folders.value
    .filter(f => f.name.trim())
    .map(f => f.name.trim())
  const uniqueNames = new Set(folderNames)
  if (uniqueNames.size !== folderNames.length) {
    ElMessage.warning('文件夹名称不能重复')
    return
  }

  // 警告不完整的组
  if (incompleteGroups.value > 0) {
    try {
      await ElMessageBox.confirm(
        `有 ${incompleteGroups.value} 个图片组不完整（未在所有文件夹中找到相同文件名）。是否继续导入？`,
        '警告',
        {
          confirmButtonText: '继续导入',
          cancelButtonText: '取消',
          type: 'warning',
        }
      )
    } catch {
      return
    }
  }

  uploading.value = true
  uploadProgress.value = 0
  uploadStats.value = { completed: 0, total: 0 }
  currentUploadingFile.value = ''
  fileProgressMap.value.clear()
  
  try {
    const files: File[] = []
    const folderNames: string[] = []

    // 收集所有文件
    folders.value.forEach(folder => {
      folder.files.forEach(uploadFile => {
        if (uploadFile.raw) {
          files.push(uploadFile.raw)
          folderNames.push(folder.name.trim())
        }
      })
    })

    // 显示文件大小统计
    const totalSize = files.reduce((sum, file) => sum + file.size, 0)
    const totalSizeMB = (totalSize / (1024 * 1024)).toFixed(2)
    console.log(`准备并行上传 ${files.length} 个文件，总大小: ${totalSizeMB} MB`)

    uploadStats.value.total = files.length

    // 使用并行上传，每张图片单独上传
    const result = await imagesApi.importImagesParallel(
      queueId.value, 
      files, 
      folderNames,
      // 单文件进度
      (fileIndex, progress) => {
        fileProgressMap.value.set(fileIndex, progress)
        // 更新当前上传的文件名
        if (progress < 100) {
          currentUploadingFile.value = files[fileIndex].name
        }
      },
      // 总体进度
      (completed, total) => {
        uploadStats.value.completed = completed
        uploadStats.value.total = total
        uploadProgress.value = Math.round((completed / total) * 100)
      },
      5 // 并发数：同时上传5个文件
    )
    
    if (result.failedCount === 0) {
      ElMessage.success(result.message || '导入成功')
    } else if (result.successCount > 0) {
      ElMessage.warning(`${result.message}，${result.errors.length > 0 ? '部分文件上传失败' : ''}`)
      if (result.errors.length > 0) {
        console.error('上传错误详情:', result.errors)
      }
    } else {
      ElMessage.error('所有文件上传失败')
      if (result.errors.length > 0) {
        console.error('上传错误详情:', result.errors)
      }
    }
    
    // 刷新队列信息
    await loadQueue()
    
    // 清空所有文件夹
    clearAllFolders()
    
    // 重新加载已存在的图片（会显示更新的文件夹名称）
    await loadExistingImages()
    
    // 如果还没有足够的文件夹，添加额外的文件夹
    if (queue.value) {
      while (folders.value.length < queue.value.imageCount) {
        addFolder()
      }
      // 默认展开第一个文件夹
      if (folders.value.length > 0 && expandedFolders.value.length === 0) {
        expandedFolders.value = [folders.value[0].id]
      }
    }
  } catch (error: any) {
    console.error('上传失败:', error)
    if (error.code === 'ECONNABORTED') {
      ElMessage.error('上传超时，请检查网络连接')
    } else if (error.code === 'ERR_NETWORK' || error.code === 'ERR_CONNECTION_RESET') {
      ElMessage.error('网络连接中断，请检查后端服务是否正常运行')
    } else {
      ElMessage.error(error.response?.data?.message || error.message || '导入失败')
    }
  } finally {
    uploading.value = false
    uploadProgress.value = 0
    uploadStats.value = { completed: 0, total: 0 }
    currentUploadingFile.value = ''
    fileProgressMap.value.clear()
  }
}

const goBack = () => {
  router.back()
}
</script>

<style scoped>
.import-container {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-title {
  font-size: 18px;
  font-weight: 600;
}

.content {
  margin-top: 20px;
}

.info-card,
.tips-card,
.upload-card,
.preview-card,
.progress-card {
  margin-bottom: 20px;
}

.progress-card {
  border: 2px solid #409eff;
  background: #f0f9ff;
}

.progress-text {
  font-size: 16px;
  font-weight: bold;
  color: #409eff;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
}

.alert-content {
  line-height: 1.8;
}

.alert-content ol {
  margin: 10px 0;
  padding-left: 20px;
}

.alert-content li {
  margin: 5px 0;
}

.empty-tip {
  padding: 40px 0;
  text-align: center;
}

.folders-list {
  display: flex;
  flex-direction: column;
}

.folders-collapse {
  width: 100%;
}

.folders-collapse :deep(.el-collapse-item) {
  margin-bottom: 15px;
  border: 2px solid #e4e7ed;
  border-radius: 4px;
  overflow: hidden;
}

.folders-collapse :deep(.el-collapse-item.is-active) {
  border-color: #409eff;
}

.folders-collapse :deep(.el-collapse-item__header) {
  padding: 15px;
  background-color: #f5f7fa;
}

.folder-header-title {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  flex: 1;
}

.folder-card {
  border: 2px solid #e4e7ed;
  transition: all 0.3s;
}

.folder-card.has-files {
  border-color: #67c23a;
}

.folder-card :deep(.el-card__body) {
  max-height: 500px;
  overflow-y: auto;
}

.folder-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.folder-info {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
}

.folder-upload {
  width: 100%;
}

:deep(.folder-upload .el-upload-dragger) {
  width: 100%;
  padding: 30px;
}

:deep(.folder-upload .el-icon--upload) {
  font-size: 50px;
  color: #409eff;
  margin-bottom: 10px;
}

.file-list {
  margin-top: 20px;
}

.file-list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.file-list-container {
  max-height: 300px;
  overflow-y: auto;
  margin-top: 10px;
}

.file-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.file-item:last-child {
  border-bottom: none;
}

.file-name {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.file-size {
  color: #909399;
  font-size: 12px;
}

.uploaded-files-list {
  margin-top: 20px;
  margin-bottom: 20px;
}

.uploaded-file-item {
  background-color: #f0f9ff;
  border-left: 3px solid #67c23a;
}

.uploaded-file-item .file-name {
  color: #606266;
  font-style: italic;
}

.action-buttons {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin-top: 30px;
  padding: 20px;
}

:deep(.el-table) {
  font-size: 14px;
}

:deep(.el-table th) {
  background-color: #f5f7fa;
}
</style>

