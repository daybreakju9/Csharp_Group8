import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, LoginDto, RegisterDto } from '../types'
import { authApi } from '../api/auth'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<User | null>(null)

  // 从 localStorage 恢复用户信息
  const storedUser = localStorage.getItem('user')
  if (storedUser) {
    try {
      user.value = JSON.parse(storedUser)
    } catch (e) {
      localStorage.removeItem('user')
    }
  }

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  const login = async (credentials: LoginDto) => {
    const response = await authApi.login(credentials)
    token.value = response.token
    user.value = {
      id: 0, // 后端没有返回 ID，可以从 token 解析或后续获取
      username: response.username,
      role: response.role as 'Admin' | 'User',
    }
    
    localStorage.setItem('token', response.token)
    localStorage.setItem('user', JSON.stringify(user.value))
  }

  const register = async (data: RegisterDto) => {
    const response = await authApi.register(data)
    token.value = response.token
    user.value = {
      id: 0,
      username: response.username,
      role: response.role as 'Admin' | 'User',
    }
    
    localStorage.setItem('token', response.token)
    localStorage.setItem('user', JSON.stringify(user.value))
  }

  const logout = () => {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  return {
    token,
    user,
    isAuthenticated,
    isAdmin,
    login,
    register,
    logout,
  }
})

