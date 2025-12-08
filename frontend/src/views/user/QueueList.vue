<template>
  <div class="queue-list-container">
    <el-container>
      <el-header>
        <div class="header-content">
          <el-button icon="ArrowLeft" @click="goBack">返回</el-button>
          <h2>队列列表</h2>
          <el-button @click="handleLogout">退出登录</el-button>
        </div>
      </el-header>
      
      <el-main>
        <el-card v-loading="loading">
          <el-table :data="queuesStore.queues" style="width: 100%">
            <el-table-column prop="name" label="队列名称" />
            <el-table-column prop="comparisonCount" label="对比图片数" width="120" />
            <el-table-column prop="totalImageCount" label="总图片组" width="120" />
            <el-table-column label="操作" width="150">
              <template #default="scope">
                <el-button type="primary" size="small" @click="startSelection(scope.row.id)">
                  开始选择
                </el-button>
              </template>
            </el-table-column>
          </el-table>
          
          <el-empty v-if="!loading && queuesStore.queues.length === 0" description="暂无队列" />
        </el-card>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useQueuesStore } from '../../stores/queues'
import { ElMessage } from 'element-plus'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const queuesStore = useQueuesStore()
const loading = ref(false)

const projectId = Number(route.params.projectId)

onMounted(async () => {
  loading.value = true
  try {
    await queuesStore.fetchQueues(projectId)
  } catch (error) {
    ElMessage.error('获取队列列表失败')
  } finally {
    loading.value = false
  }
})

const startSelection = (queueId: number) => {
  router.push(`/queues/${queueId}/select`)
}

const goBack = () => {
  router.push('/projects')
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.queue-list-container {
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
  flex: 1;
  text-align: center;
}

.el-main {
  padding: 20px;
}
</style>

