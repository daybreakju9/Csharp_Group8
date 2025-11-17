import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Queue, CreateQueueDto } from '../types'
import { queuesApi } from '../api/queues'

export const useQueuesStore = defineStore('queues', () => {
  const queues = ref<Queue[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchQueues = async (projectId?: number) => {
    loading.value = true
    error.value = null
    try {
      queues.value = await queuesApi.getAll(projectId)
    } catch (e: any) {
      error.value = e.response?.data?.message || '获取队列列表失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  const createQueue = async (data: CreateQueueDto) => {
    loading.value = true
    error.value = null
    try {
      const queue = await queuesApi.create(data)
      queues.value.unshift(queue)
      return queue
    } catch (e: any) {
      error.value = e.response?.data?.message || '创建队列失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  const updateQueue = async (id: number, data: Partial<CreateQueueDto>) => {
    loading.value = true
    error.value = null
    try {
      const queue = await queuesApi.update(id, data)
      const index = queues.value.findIndex((q) => q.id === id)
      if (index !== -1) {
        queues.value[index] = queue
      }
      return queue
    } catch (e: any) {
      error.value = e.response?.data?.message || '更新队列失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  const deleteQueue = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await queuesApi.delete(id)
      queues.value = queues.value.filter((q) => q.id !== id)
    } catch (e: any) {
      error.value = e.response?.data?.message || '删除队列失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    queues,
    loading,
    error,
    fetchQueues,
    createQueue,
    updateQueue,
    deleteQueue,
  }
})

