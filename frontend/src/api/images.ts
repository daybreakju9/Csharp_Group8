import apiClient from './config'
import type { Image, ImageGroup } from '../types'

export const imagesApi = {
  // 获取队列的所有图片
  getQueueImages: async (queueId: number): Promise<Image[]> => {
    const response = await apiClient.get<Image[]>(`/images/queue/${queueId}`)
    return response.data
  },

  // 获取下一组待选择的图片
  getNextGroup: async (queueId: number): Promise<ImageGroup | { message: string; completed: boolean }> => {
    const response = await apiClient.get(`/images/next-group/${queueId}`)
    return response.data
  },

  // 导入图片（批量上传 - 已弃用，建议使用并行单文件上传）
  importImages: async (
    queueId: number,
    files: File[],
    folderNames: string[],
    onProgress?: (progress: number) => void
  ): Promise<{ message: string }> => {
    const formData = new FormData()
    formData.append('queueId', queueId.toString())
    
    files.forEach((file) => {
      formData.append('files', file)
    })
    
    folderNames.forEach((name) => {
      formData.append('folderNames', name)
    })

    const response = await apiClient.post('/images/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      timeout: 300000, // 5 分钟超时
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total)
          onProgress(percentCompleted)
        }
      },
    })
    return response.data
  },

  // 单文件上传（并行上传使用）
  importSingleImage: async (
    queueId: number,
    file: File,
    folderName: string,
    onProgress?: (progress: number) => void
  ): Promise<{ message: string; imageId: number; fileName: string }> => {
    const formData = new FormData()
    formData.append('queueId', queueId.toString())
    formData.append('file', file)
    formData.append('folderName', folderName)

    const response = await apiClient.post('/images/import-single', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      timeout: 300000, // 5 分钟超时
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total)
          onProgress(percentCompleted)
        }
      },
    })
    return response.data
  },

  // 批量上传（后端一次性处理，支持跳过已存在）
  uploadBatch: async (
    queueId: number,
    files: File[],
    folderNames: string[],
    onUploadProgress?: (progress: number) => void
  ): Promise<{
    message: string
    successCount: number
    skippedCount: number
    failureCount: number
    errors: string[]
    skippedFiles: string[]
  }> => {
    if (files.length !== folderNames.length) {
      throw new Error('文件和文件夹名称数量不匹配')
    }

    const formData = new FormData()
    formData.append('queueId', queueId.toString())
    files.forEach((file) => formData.append('files', file))
    folderNames.forEach((name) => formData.append('folderNames', name))

    const response = await apiClient.post('/images/upload-batch', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 300000,
      onUploadProgress: (evt) => {
        if (onUploadProgress && evt.total) {
          onUploadProgress(Math.round((evt.loaded * 100) / evt.total))
        }
      },
    })

    const data = response.data as any
    return {
      message: data.message ?? '上传完成',
      successCount: data.successCount ?? 0,
      skippedCount: data.skippedCount ?? 0,
      failureCount: data.failureCount ?? 0,
      errors: data.errors ?? [],
      skippedFiles: data.skippedFiles ?? [],
    }
  },

  // 删除图片
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/images/${id}`)
  },
}

