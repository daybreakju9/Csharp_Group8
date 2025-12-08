<template>
  <div class="admin-container">
    <el-container>
      <el-aside width="200px">
        <el-menu default-active="/admin/export" router>
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
            <h2>数据导出</h2>
            <div>
              <el-button @click="goToUserView">用户视图</el-button>
              <el-button @click="handleLogout">退出登录</el-button>
            </div>
          </div>
        </el-header>
        
        <el-main>
          <!-- 导出选择记录 -->
          <el-card class="export-card">
            <template #header>
              <div class="card-header">
                <span>导出选择记录</span>
                <el-icon><DocumentCopy /></el-icon>
              </div>
            </template>
            
            <el-form label-width="100px">
              <el-form-item label="选择队列" required>
                <el-select 
                  v-model="selectionsForm.queueId" 
                  placeholder="请选择要导出的队列"
                  filterable
                  style="width: 300px"
                >
                  <el-option
                    v-for="queue in queuesStore.queues"
                    :key="queue.id"
                    :label="`${queue.projectName} - ${queue.name}`"
                    :value="queue.id"
                  />
                </el-select>
              </el-form-item>
              
              <el-form-item label="导出格式">
                <el-radio-group v-model="selectionsForm.format">
                  <el-radio label="csv">CSV (逗号分隔)</el-radio>
                  <el-radio label="json">JSON (结构化数据)</el-radio>
                </el-radio-group>
              </el-form-item>
              
              <el-form-item>
                <el-button 
                  type="primary" 
                  @click="exportSelections"
                  :loading="selectionsLoading"
                  :disabled="!selectionsForm.queueId"
                >
                  <el-icon class="btn-icon"><Download /></el-icon>
                  导出选择记录
                </el-button>
                <div class="form-tip">导出指定队列中所有用户的图片选择记录</div>
              </el-form-item>
            </el-form>
          </el-card>

          <!-- 导出进度数据 -->
          <el-card class="export-card">
            <template #header>
              <div class="card-header">
                <span>导出进度数据</span>
                <el-icon><TrendCharts /></el-icon>
              </div>
            </template>
            
            <el-form label-width="100px">
              <el-form-item label="选择队列">
                <el-select 
                  v-model="progressForm.queueId" 
                  placeholder="不选择则导出所有队列"
                  clearable
                  filterable
                  style="width: 300px"
                >
                  <el-option
                    v-for="queue in queuesStore.queues"
                    :key="queue.id"
                    :label="`${queue.projectName} - ${queue.name}`"
                    :value="queue.id"
                  />
                </el-select>
              </el-form-item>
              
              <el-form-item label="导出格式">
                <el-radio-group v-model="progressForm.format">
                  <el-radio label="csv">CSV (逗号分隔)</el-radio>
                  <el-radio label="json">JSON (结构化数据)</el-radio>
                </el-radio-group>
              </el-form-item>
              
              <el-form-item>
                <el-button 
                  type="success" 
                  @click="exportProgress"
                  :loading="progressLoading"
                >
                  <el-icon class="btn-icon"><Download /></el-icon>
                  导出进度数据
                </el-button>
                <div class="form-tip">导出所有用户在各队列中的完成进度</div>
              </el-form-item>
            </el-form>
          </el-card>

          <!-- 数据统计 -->
          <el-card>
            <template #header>
              <div class="card-header">
                <span>数据统计</span>
                <el-button type="primary" @click="loadQueues" :loading="loading">刷新</el-button>
              </div>
            </template>
            
            <el-table :data="queuesStore.queues" v-loading="loading">
              <el-table-column prop="projectName" label="所属项目" width="150" />
              <el-table-column prop="name" label="队列名称" width="180" />
              <el-table-column prop="totalImageCount" label="总图片数" width="120" />
              <el-table-column label="图片组数" width="120">
                <template #default="scope">
                  {{ calculateGroups(scope.row) }}
                </template>
              </el-table-column>
              <el-table-column label="操作" width="250">
                <template #default="scope">
                  <el-button 
                    size="small" 
                    type="primary"
                    @click="quickExportSelections(scope.row.id)"
                  >
                    导出选择
                  </el-button>
                  <el-button 
                    size="small" 
                    type="success"
                    @click="quickExportProgress(scope.row.id)"
                  >
                    导出进度
                  </el-button>
                </template>
              </el-table-column>
            </el-table>

            <el-empty v-if="queuesStore.queues.length === 0 && !loading" description="暂无队列数据" />
          </el-card>
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useQueuesStore } from '../../stores/queues'
import { exportApi } from '../../api/export'
import { ElMessage } from 'element-plus'
import { Folder, List, Download, DocumentCopy, TrendCharts, User } from '@element-plus/icons-vue'

const router = useRouter()
const authStore = useAuthStore()
const queuesStore = useQueuesStore()

const loading = ref(false)
const selectionsLoading = ref(false)
const progressLoading = ref(false)

const selectionsForm = reactive({
  queueId: null as number | null,
  format: 'csv' as 'csv' | 'json',
})

const progressForm = reactive({
  queueId: null as number | null,
  format: 'csv' as 'csv' | 'json',
})

onMounted(async () => {
  await loadQueues()
})

const loadQueues = async () => {
  loading.value = true
  try {
    await queuesStore.fetchQueues()
  } catch (error) {
    ElMessage.error('获取队列列表失败')
  } finally {
    loading.value = false
  }
}

const exportSelections = async () => {
  if (!selectionsForm.queueId) {
    ElMessage.warning('请选择要导出的队列')
    return
  }

  selectionsLoading.value = true
  try {
    const data = await exportApi.exportSelections(selectionsForm.queueId, selectionsForm.format)
    
    if (selectionsForm.format === 'csv') {
      // 下载 CSV 文件
      downloadBlob(data, `selections_${selectionsForm.queueId}_${Date.now()}.csv`, 'text/csv')
    } else {
      // 下载 JSON 文件
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
      downloadBlob(blob, `selections_${selectionsForm.queueId}_${Date.now()}.json`, 'application/json')
    }
    
    ElMessage.success('导出成功')
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '导出失败')
  } finally {
    selectionsLoading.value = false
  }
}

const exportProgress = async () => {
  progressLoading.value = true
  try {
    const data = await exportApi.exportProgress(progressForm.queueId || undefined, progressForm.format)
    
    if (progressForm.format === 'csv') {
      // 下载 CSV 文件
      downloadBlob(data, `progress_${progressForm.queueId || 'all'}_${Date.now()}.csv`, 'text/csv')
    } else {
      // 下载 JSON 文件
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
      downloadBlob(blob, `progress_${progressForm.queueId || 'all'}_${Date.now()}.json`, 'application/json')
    }
    
    ElMessage.success('导出成功')
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '导出失败')
  } finally {
    progressLoading.value = false
  }
}

const quickExportSelections = async (queueId: number) => {
  selectionsLoading.value = true
  try {
    const data = await exportApi.exportSelections(queueId, 'csv')
    downloadBlob(data, `selections_${queueId}_${Date.now()}.csv`, 'text/csv')
    ElMessage.success('导出成功')
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '导出失败')
  } finally {
    selectionsLoading.value = false
  }
}

const quickExportProgress = async (queueId: number) => {
  progressLoading.value = true
  try {
    const data = await exportApi.exportProgress(queueId, 'csv')
    downloadBlob(data, `progress_${queueId}_${Date.now()}.csv`, 'text/csv')
    ElMessage.success('导出成功')
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '导出失败')
  } finally {
    progressLoading.value = false
  }
}

const downloadBlob = (blob: Blob, filename: string, mimeType: string) => {
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.style.display = 'none'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

const calculateGroups = (queue: any) => {
  if (!queue.comparisonCount || queue.comparisonCount === 0) return 0
  return Math.floor(queue.totalImageCount / queue.comparisonCount)
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

.export-card {
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

.btn-icon {
  margin-right: 5px;
}
</style>

