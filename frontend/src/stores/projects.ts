import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Project, CreateProjectDto } from '../types'
import { projectsApi } from '../api/projects'

export const useProjectsStore = defineStore('projects', () => {
  const projects = ref<Project[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchProjects = async () => {
    loading.value = true
    error.value = null
    try {
      projects.value = await projectsApi.getAll()
    } catch (e: any) {
      error.value = e.response?.data?.message || '获取项目列表失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  const createProject = async (data: CreateProjectDto) => {
    loading.value = true
    error.value = null
    try {
      const project = await projectsApi.create(data)
      projects.value.unshift(project)
      return project
    } catch (e: any) {
      error.value = e.response?.data?.message || '创建项目失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  const updateProject = async (id: number, data: CreateProjectDto) => {
    loading.value = true
    error.value = null
    try {
      const project = await projectsApi.update(id, data)
      const index = projects.value.findIndex((p) => p.id === id)
      if (index !== -1) {
        projects.value[index] = project
      }
      return project
    } catch (e: any) {
      error.value = e.response?.data?.message || '更新项目失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  const deleteProject = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await projectsApi.delete(id)
      projects.value = projects.value.filter((p) => p.id !== id)
    } catch (e: any) {
      error.value = e.response?.data?.message || '删除项目失败'
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    projects,
    loading,
    error,
    fetchProjects,
    createProject,
    updateProject,
    deleteProject,
  }
})

