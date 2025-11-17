<template>
  <div class="selection-container">
    <el-container>
      <el-header>
        <div class="header-content">
          <el-button icon="ArrowLeft" @click="goBack">返回</el-button>
          <h2>图片选择</h2>
          <el-button @click="handleLogout">退出登录</el-button>
        </div>
      </el-header>
      
      <el-main v-loading="loading" class="main-content">
        <!-- 进度条 -->
        <div v-if="progress && !completed" class="progress-bar">
          <div class="progress-info">
            <span>进度: {{ progress.completedGroups }} / {{ progress.totalGroups }}</span>
            <span>{{ progress.progressPercentage.toFixed(1) }}%</span>
          </div>
          <el-progress :percentage="progress.progressPercentage" :stroke-width="8" />
        </div>
        
        <!-- 完成提示 -->
        <el-result
          v-if="completed"
          icon="success"
          title="恭喜！"
          sub-title="您已完成所有图片的选择"
        >
          <template #extra>
            <el-button type="primary" @click="goBack">返回队列列表</el-button>
          </template>
        </el-result>
        
        <!-- 图片选择 -->
        <div v-else-if="imageGroup" class="images-container">
          <div class="images-header">
            <h3>图片组: {{ imageGroup.imageGroup }}</h3>
            <span>请选择一张图片</span>
          </div>
          
          <div class="images-grid" :style="gridStyle">
            <div
              v-for="image in imageGroup.images"
              :key="image.id"
              class="image-item"
              :class="{ selected: selectedImageId === image.id }"
              @click="selectImage(image.id)"
            >
              <div class="image-wrapper">
                <img 
                  :src="getImageUrl(image.filePath)" 
                  :alt="image.fileName"
                />
              </div>
              <div class="image-label">
                <span>{{ image.folderName }}</span>
              </div>
              <el-icon v-if="selectedImageId === image.id" class="check-icon">
                <CircleCheck />
              </el-icon>
            </div>
          </div>
          
          <div class="action-buttons">
            <div class="action-left">
              <el-switch
                v-model="autoSubmit"
                active-text="自动提交"
                inactive-text="手动提交"
                @change="saveAutoSubmitPreference"
              />
            </div>
            <div class="action-right">
              <el-button
                type="primary"
                size="large"
                :disabled="!selectedImageId"
                @click="submitSelection"
              >
                提交选择
              </el-button>
            </div>
          </div>
        </div>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref, computed, nextTick } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { imagesApi } from '../../api/images'
import { selectionsApi } from '../../api/selections'
import { ElMessage } from 'element-plus'
import { CircleCheck } from '@element-plus/icons-vue'
import { API_BASE_URL } from '../../api/config'
import type { ImageGroup, UserProgress } from '../../types'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const queueId = Number(route.params.queueId)
const loading = ref(false)
const imageGroup = ref<ImageGroup | null>(null)
const selectedImageId = ref<number | null>(null)
const progress = ref<UserProgress | null>(null)
const completed = ref(false)

// 自动提交选项
const AUTO_SUBMIT_KEY = 'image-selection-auto-submit'
const autoSubmit = ref<boolean>(() => {
  const saved = localStorage.getItem(AUTO_SUBMIT_KEY)
  return saved === 'true'
})
let autoSubmitTimer: number | null = null // 自动提交定时器

// 保存自动提交偏好
const saveAutoSubmitPreference = () => {
  localStorage.setItem(AUTO_SUBMIT_KEY, autoSubmit.value.toString())
}

// 布局相关
const imageDimensions = ref<Map<number, { width: number; height: number }>>(new Map())
const layoutConfig = ref({ rows: 1, cols: 1 })
const viewportSize = ref({ width: 1920, height: 1080 }) // 默认分辨率
const containerSize = ref({ width: 0, height: 0 }) // 图片容器的实际尺寸

// 获取容器可用尺寸
const getContainerSize = (): { width: number; height: number } => {
  const container = document.querySelector('.images-grid') as HTMLElement
  if (container) {
    const rect = container.getBoundingClientRect()
    // 减去padding（左右各10px）
    const padding = 20
    return {
      width: rect.width - padding,
      height: rect.height
    }
  }
  // 如果容器还没渲染，使用视口尺寸估算
  const headerHeight = 60
  const progressHeight = progress.value && !completed.value ? 60 : 0
  const imagesHeaderHeight = 55
  const actionButtonsHeight = 75
  const padding = 20
  
  return {
    width: window.innerWidth - padding * 2,
    height: window.innerHeight - headerHeight - progressHeight - imagesHeaderHeight - actionButtonsHeight - padding * 2
  }
}

// 计算最佳布局
const calculateOptimalLayout = () => {
  if (!imageGroup.value || imageGroup.value.images.length === 0) {
    layoutConfig.value = { rows: 1, cols: 1 }
    return
  }

  const imageCount = imageGroup.value.images.length
  
  // 如果只有1-2张图片，使用1行布局
  if (imageCount <= 2) {
    layoutConfig.value = { rows: 1, cols: imageCount }
    return
  }

  // 获取容器尺寸和屏幕宽高比
  const container = getContainerSize()
  containerSize.value = container
  const screenAspectRatio = container.width / container.height
  const isWideScreen = screenAspectRatio > 1.5 // 宽屏（16:9及以上）
  const isNarrowScreen = screenAspectRatio < 1.1 // 接近正方形或竖屏

  // 获取所有图片的宽高比
  const aspectRatios: number[] = []
  let hasAllDimensions = true
  
  imageGroup.value.images.forEach(img => {
    const dim = imageDimensions.value.get(img.id)
    if (dim) {
      aspectRatios.push(dim.width / dim.height)
    } else {
      hasAllDimensions = false
    }
  })

  // 如果还没有加载完所有图片尺寸，根据屏幕尺寸使用默认布局
  if (!hasAllDimensions) {
    // 根据屏幕宽高比调整默认布局
    let cols = Math.ceil(Math.sqrt(imageCount))
    let rows = Math.ceil(imageCount / cols)
    
    // 宽屏偏好更多列，窄屏偏好更多行
    if (isWideScreen && cols < rows) {
      // 交换行列，让列多于行
      const temp = cols
      cols = rows
      rows = temp
      // 重新计算以匹配数量
      rows = Math.ceil(imageCount / cols)
    } else if (isNarrowScreen && rows < cols) {
      // 交换行列，让行多于列
      const temp = rows
      rows = cols
      cols = temp
      // 重新计算以匹配数量
      cols = Math.ceil(imageCount / rows)
    }
    
    layoutConfig.value = { rows, cols }
    return
  }

  // 计算平均宽高比
  const avgAspectRatio = aspectRatios.reduce((sum, ratio) => sum + ratio, 0) / aspectRatios.length
  const isPortrait = avgAspectRatio < 1 // 竖图为主
  const isLandscape = avgAspectRatio > 1.5 // 横图为主

  // 尝试不同的行列组合，找到最佳布局
  let bestLayout = { rows: 1, cols: imageCount }
  let bestScore = -Infinity

  // 根据屏幕尺寸调整最大行数
  let maxRows = Math.min(imageCount, Math.ceil(Math.sqrt(imageCount) * 2))
  if (isNarrowScreen) {
    // 窄屏或竖屏，可以允许更多行
    maxRows = Math.min(imageCount, Math.ceil(Math.sqrt(imageCount) * 2.5))
  } else if (isWideScreen) {
    // 宽屏，限制行数，偏好更多列
    maxRows = Math.min(imageCount, Math.ceil(Math.sqrt(imageCount) * 1.5))
  }
  
  for (let rows = 1; rows <= maxRows; rows++) {
    const cols = Math.ceil(imageCount / rows)
    const emptyCells = rows * cols - imageCount
    
    // 计算每个单元格的理想尺寸（基于容器尺寸，考虑gap）
    // gap: 10px，每个方向有 (cols-1) 或 (rows-1) 个gap
    const gapWidth = 10 * (cols - 1)
    const gapHeight = 10 * (rows - 1)
    const cellWidth = (container.width - gapWidth) / cols
    const cellHeight = (container.height - gapHeight) / rows
    
    // 计算评分
    let score = 0
    
    // 1. 偏好接近正方形的网格（行列比接近1）
    const gridAspectRatio = cols / rows
    const gridScore = 1 / (1 + Math.abs(gridAspectRatio - 1))
    
    // 2. 减少空单元格
    const emptyScore = 1 / (1 + emptyCells)
    
    // 3. 考虑图片宽高比：竖图偏好更多列，横图偏好更多行
    let orientationScore = 1
    if (isPortrait && cols > rows) {
      orientationScore = 1.2 // 竖图，列多于行，加分
    } else if (isLandscape && rows > cols) {
      orientationScore = 1.2 // 横图，行多于列，加分
    } else if (!isPortrait && !isLandscape) {
      // 接近正方形，偏好接近正方形的网格
      orientationScore = gridScore
    }
    
    // 4. 考虑屏幕宽高比匹配
    let screenMatchScore = 1
    if (isWideScreen) {
      // 宽屏偏好更多列（网格也应该是宽屏）
      if (gridAspectRatio > 1.2) {
        screenMatchScore = 1.3 // 加分
      } else if (gridAspectRatio < 0.8) {
        screenMatchScore = 0.7 // 减分
      }
    } else if (isNarrowScreen) {
      // 窄屏偏好更多行（网格也应该是窄屏）
      if (gridAspectRatio < 0.9) {
        screenMatchScore = 1.3 // 加分
      } else if (gridAspectRatio > 1.3) {
        screenMatchScore = 0.7 // 减分
      }
    }
    
    // 5. 计算单元格尺寸与图片尺寸的匹配度
    let sizeMatchScore = 1
    if (hasAllDimensions && aspectRatios.length > 0) {
      // 计算平均图片宽高比
      const avgImgRatio = avgAspectRatio
      const cellRatio = cellWidth / cellHeight
      
      // 如果单元格宽高比与图片宽高比接近，得分更高
      const ratioDiff = Math.abs(cellRatio - avgImgRatio)
      sizeMatchScore = 1 / (1 + ratioDiff)
    }
    
    // 6. 避免空单元格过多
    if (emptyCells > cols * 0.5) {
      orientationScore *= 0.5 // 惩罚空单元格过多的布局
    }
    
    // 综合评分（调整权重）
    score = gridScore * 0.2 + 
            emptyScore * 0.25 + 
            orientationScore * 0.25 + 
            screenMatchScore * 0.15 +
            sizeMatchScore * 0.15
    
    if (score > bestScore) {
      bestScore = score
      bestLayout = { rows, cols }
    }
  }
  
  layoutConfig.value = bestLayout
}

// 监听图片组变化，重新计算布局
const updateLayout = async () => {
  if (!imageGroup.value) return
  
  // 清空之前的尺寸数据
  imageDimensions.value.clear()
  
  // 更新视口尺寸
  viewportSize.value = {
    width: window.innerWidth,
    height: window.innerHeight
  }
  
  // 等待DOM更新
  await nextTick()
  
  // 先使用默认布局（基于屏幕尺寸）
  calculateOptimalLayout()
  
  // 获取图片尺寸（延迟执行以确保图片开始加载）
  setTimeout(() => {
    if (!imageGroup.value) return
    
    const images = document.querySelectorAll('.image-item img')
    let loadedCount = 0
    const totalImages = imageGroup.value.images.length
    
    images.forEach((img, index) => {
      const imageElement = img as HTMLImageElement
      const image = imageGroup.value!.images[index]
      
      if (!image) return
      
      const handleLoad = () => {
        if (imageElement.naturalWidth > 0 && imageElement.naturalHeight > 0) {
          imageDimensions.value.set(image.id, {
            width: imageElement.naturalWidth,
            height: imageElement.naturalHeight
          })
          loadedCount++
          
          // 当所有图片加载完成或加载了足够多的图片时，重新计算布局
          if (loadedCount === totalImages || loadedCount >= Math.min(totalImages, 3)) {
            calculateOptimalLayout()
          }
        }
      }
      
      if (imageElement.complete && imageElement.naturalWidth > 0) {
        handleLoad()
      } else {
        imageElement.addEventListener('load', handleLoad, { once: true })
      }
    })
  }, 100)
}

// 计算网格样式
const gridStyle = computed(() => {
  return {
    gridTemplateRows: `repeat(${layoutConfig.value.rows}, 1fr)`,
    gridTemplateColumns: `repeat(${layoutConfig.value.cols}, 1fr)`
  }
})

// 防抖处理resize事件
let resizeTimer: number | null = null
const handleResize = () => {
  viewportSize.value = {
    width: window.innerWidth,
    height: window.innerHeight
  }
  
  // 防抖：延迟重新计算布局
  if (resizeTimer) {
    clearTimeout(resizeTimer)
  }
  resizeTimer = window.setTimeout(() => {
    calculateOptimalLayout()
    resizeTimer = null
  }, 150)
}

onMounted(async () => {
  // 初始化视口尺寸
  viewportSize.value = {
    width: window.innerWidth,
    height: window.innerHeight
  }
  
  // 监听窗口大小变化
  window.addEventListener('resize', handleResize)
  
  await loadProgress()
  await loadNextGroup()
})

onUnmounted(() => {
  // 清理事件监听和定时器
  window.removeEventListener('resize', handleResize)
  if (resizeTimer) {
    clearTimeout(resizeTimer)
    resizeTimer = null
  }
  if (autoSubmitTimer) {
    clearTimeout(autoSubmitTimer)
    autoSubmitTimer = null
  }
})

const loadProgress = async () => {
  try {
    progress.value = await selectionsApi.getProgress(queueId)
  } catch (error) {
    console.error('获取进度失败:', error)
  }
}

const loadNextGroup = async () => {
  loading.value = true
  try {
    const result = await imagesApi.getNextGroup(queueId)
    
    if ('completed' in result && result.completed) {
      completed.value = true
    } else {
      imageGroup.value = result as ImageGroup
      selectedImageId.value = null
      imageDimensions.value.clear()
      // 重新计算布局
      await updateLayout()
    }
  } catch (error: any) {
    ElMessage.error('获取图片失败')
  } finally {
    loading.value = false
  }
}

const selectImage = (imageId: number) => {
  selectedImageId.value = imageId
  
  // 清除之前的自动提交定时器
  if (autoSubmitTimer) {
    clearTimeout(autoSubmitTimer)
    autoSubmitTimer = null
  }
  
  
  // 如果启用了自动提交，延迟提交（避免过于频繁）
  if (autoSubmit.value && !loading.value) {
    // 添加短暂延迟，让用户看到选中效果
    autoSubmitTimer = window.setTimeout(() => {
      if (selectedImageId.value === imageId && autoSubmit.value && !loading.value) {
        submitSelection()
      }
      autoSubmitTimer = null
    }, 100)
  }
}

const submitSelection = async () => {
  if (!selectedImageId.value || !imageGroup.value || loading.value) return
  
  // 清除自动提交定时器，避免重复提交
  if (autoSubmitTimer) {
    clearTimeout(autoSubmitTimer)
    autoSubmitTimer = null
  }
  
  loading.value = true
  try {
    await selectionsApi.create({
      queueId,
      imageGroup: imageGroup.value.imageGroup,
      selectedImageId: selectedImageId.value,
    })
    
    ElMessage.success('选择已提交')
    await loadProgress()
    await loadNextGroup()
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '提交失败')
  } finally {
    loading.value = false
  }
}

const getImageUrl = (filePath: string) => {
  return `${API_BASE_URL}${filePath}`
}

const goBack = () => {
  router.back()
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.selection-container {
  height: 100vh;
  background-color: #f5f5f5;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.selection-container :deep(.el-container) {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.el-header {
  background-color: #409eff;
  color: white;
  display: flex;
  align-items: center;
  flex-shrink: 0;
  height: 60px;
  padding: 0 20px;
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
  font-size: 18px;
}

.main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 0;
  overflow: hidden;
  position: relative;
}

.progress-bar {
  padding: 15px 20px;
  background-color: white;
  border-bottom: 1px solid #e4e7ed;
  flex-shrink: 0;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-weight: 600;
  font-size: 14px;
  color: #606266;
}

.images-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.images-header {
  padding: 15px 20px;
  background-color: white;
  border-bottom: 1px solid #e4e7ed;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.images-header h3 {
  margin: 0;
  font-size: 16px;
  color: #303133;
}

.images-header span {
  color: #909399;
  font-size: 14px;
}

.images-grid {
  flex: 1;
  display: grid;
  gap: 10px;
  padding: 10px;
  overflow: hidden;
  min-height: 0;
}

.image-item {
  display: flex;
  flex-direction: column;
  position: relative;
  border: 3px solid #dcdfe6;
  border-radius: 8px;
  background-color: white;
  cursor: pointer;
  transition: all 0.3s;
  overflow: hidden;
  min-width: 0;
  min-height: 0;
}

.image-item:hover {
  border-color: #409eff;
  box-shadow: 0 4px 12px rgba(64, 158, 255, 0.3);
  transform: translateY(-2px);
}

.image-item.selected {
  border-color: #67c23a;
  background-color: #f0f9ff;
  box-shadow: 0 4px 12px rgba(103, 194, 58, 0.3);
}

.image-wrapper {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  min-height: 0;
  padding: 5px;
}

.image-item img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
}

.image-label {
  padding: 8px 10px;
  background-color: #f5f7fa;
  border-top: 1px solid #e4e7ed;
  text-align: center;
  flex-shrink: 0;
}

.image-label span {
  font-size: 13px;
  color: #606266;
  font-weight: 500;
}

.check-icon {
  position: absolute;
  top: 15px;
  right: 15px;
  color: #67c23a;
  background: white;
  border-radius: 50%;
  font-size: 28px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  z-index: 10;
}

.action-buttons {
  padding: 15px 20px;
  background-color: white;
  border-top: 1px solid #e4e7ed;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.action-left {
  display: flex;
  align-items: center;
}

.action-right {
  display: flex;
  align-items: center;
}

.action-buttons .el-button {
  min-width: 150px;
  height: 45px;
  font-size: 16px;
}
</style>

