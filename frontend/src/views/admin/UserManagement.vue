<template>
  <div class="user-management-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <h2>用户管理</h2>
        </div>
      </template>

      <el-tabs v-model="activeTab">
        <!-- 待审核的游客用户 -->
        <el-tab-pane label="待审核游客" name="guests">
          <el-table :data="guestUsers" v-loading="loadingGuests" style="width: 100%">
            <el-table-column prop="id" label="ID" width="80" />
            <el-table-column prop="username" label="用户名" />
            <el-table-column prop="role" label="角色" width="120">
              <template #default="{ row }">
                <el-tag v-if="row.role === 'Guest'" type="warning">游客</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="createdAt" label="注册时间">
              <template #default="{ row }">
                {{ formatDate(row.createdAt) }}
              </template>
            </el-table-column>
            <el-table-column label="操作" width="200">
              <template #default="{ row }">
                <el-button
                  type="success"
                  size="small"
                  @click="handleApprove(row.id)"
                  :loading="approvingId === row.id"
                >
                  批准
                </el-button>
                <el-button
                  type="danger"
                  size="small"
                  @click="handleReject(row.id, row.username)"
                  :loading="deletingId === row.id"
                >
                  拒绝
                </el-button>
              </template>
            </el-table-column>
          </el-table>

          <el-empty v-if="!loadingGuests && guestUsers.length === 0" description="暂无待审核的游客用户" />
        </el-tab-pane>

        <!-- 所有用户 -->
        <el-tab-pane label="所有用户" name="all">
          <el-table :data="allUsers" v-loading="loadingAll" style="width: 100%">
            <el-table-column prop="id" label="ID" width="80" />
            <el-table-column prop="username" label="用户名" />
            <el-table-column prop="role" label="角色" width="120">
              <template #default="{ row }">
                <el-tag v-if="row.role === 'Admin'" type="danger">管理员</el-tag>
                <el-tag v-else-if="row.role === 'User'" type="success">普通用户</el-tag>
                <el-tag v-else-if="row.role === 'Guest'" type="warning">游客</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="createdAt" label="注册时间">
              <template #default="{ row }">
                {{ formatDate(row.createdAt) }}
              </template>
            </el-table-column>
            <el-table-column label="操作" width="120">
              <template #default="{ row }">
                <el-button
                  v-if="row.role !== 'Admin'"
                  type="danger"
                  size="small"
                  @click="handleDelete(row.id, row.username)"
                  :loading="deletingId === row.id"
                >
                  删除
                </el-button>
                <span v-else style="color: #909399;">不可删除</span>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { getGuestUsers, getAllUsers, approveUser, deleteUser } from '../../api/users'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { UserDto } from '../../types'

const activeTab = ref('guests')
const guestUsers = ref<UserDto[]>([])
const allUsers = ref<UserDto[]>([])
const loadingGuests = ref(false)
const loadingAll = ref(false)
const approvingId = ref<number | null>(null)
const deletingId = ref<number | null>(null)

// 加载游客用户列表
const loadGuestUsers = async () => {
  loadingGuests.value = true
  try {
    guestUsers.value = await getGuestUsers()
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '加载游客用户列表失败')
  } finally {
    loadingGuests.value = false
  }
}

// 加载所有用户列表
const loadAllUsers = async () => {
  loadingAll.value = true
  try {
    allUsers.value = await getAllUsers()
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '加载用户列表失败')
  } finally {
    loadingAll.value = false
  }
}

// 批准游客用户
const handleApprove = async (userId: number) => {
  try {
    await ElMessageBox.confirm('确认批准该用户成为普通用户？', '提示', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'info',
    })

    approvingId.value = userId
    await approveUser({ userId })
    ElMessage.success('批准成功')

    // 重新加载两个列表
    await loadGuestUsers()
    await loadAllUsers()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || '批准失败')
    }
  } finally {
    approvingId.value = null
  }
}

// 拒绝游客申请
const handleReject = async (userId: number, username: string) => {
  try {
    await ElMessageBox.confirm(
      `确认拒绝用户"${username}"的申请并删除该账号？此操作不可恢复！`,
      '警告',
      {
        confirmButtonText: '确认删除',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )

    deletingId.value = userId
    await deleteUser(userId)
    ElMessage.success('已拒绝并删除该用户')

    // 重新加载列表
    await loadGuestUsers()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || '删除失败')
    }
  } finally {
    deletingId.value = null
  }
}

// 删除用户
const handleDelete = async (userId: number, username: string) => {
  try {
    await ElMessageBox.confirm(
      `确认删除用户"${username}"？此操作不可恢复！`,
      '警告',
      {
        confirmButtonText: '确认删除',
        cancelButtonText: '取消',
        type: 'warning',
      }
    )

    deletingId.value = userId
    await deleteUser(userId)
    ElMessage.success('删除成功')

    // 重新加载列表
    await loadAllUsers()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || '删除失败')
    }
  } finally {
    deletingId.value = null
  }
}

// 格式化日期
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// 监听标签切换
watch(activeTab, (newTab) => {
  if (newTab === 'guests' && guestUsers.value.length === 0) {
    loadGuestUsers()
  } else if (newTab === 'all' && allUsers.value.length === 0) {
    loadAllUsers()
  }
})

// 组件挂载时加载数据
onMounted(() => {
  loadGuestUsers()
})
</script>

<style scoped>
.user-management-container {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h2 {
  margin: 0;
}
</style>
