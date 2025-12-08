// 用户相关类型
export interface User {
  id: number
  username: string
  role: 'Admin' | 'User' | 'Guest'
}

export interface LoginDto {
  username: string
  password: string
}

export interface RegisterDto {
  username: string
  password: string
}

export interface AuthResponse {
  token: string
  username: string
  role: string
  expiresAt: string
}

export interface UserDto {
  id: number
  username: string
  role: string
  createdAt: string
}

export interface ApproveUserDto {
  userId: number
}

// 项目相关类型
export interface Project {
  id: number
  name: string
  description?: string
  createdById: number
  createdByUsername: string
  createdAt: string
  queueCount: number
}

export interface CreateProjectDto {
  name: string
  description?: string
}

// 队列相关类型
export interface Queue {
  id: number
  projectId: number
  projectName: string
  name: string
  comparisonCount: number
  totalImageCount: number
  createdAt: string
}

export interface CreateQueueDto {
  projectId: number
  name: string
  comparisonCount: number
}

// 图片相关类型
export interface Image {
  id: number
  queueId: number
  imageGroup: string
  folderName: string
  fileName: string
  filePath: string
  order: number
}

export interface ImageGroup {
  imageGroup: string
  images: Image[]
}

// 选择记录相关类型
export interface Selection {
  id: number
  queueId: number
  userId: number
  username: string
  imageGroup: string
  selectedImageId: number
  selectedImagePath: string
  selectedFolderName: string
  createdAt: string
}

export interface CreateSelectionDto {
  queueId: number
  imageGroup: string
  selectedImageId: number
}

// 进度相关类型
export interface UserProgress {
  queueId: number
  queueName: string
  userId: number
  username: string
  completedGroups: number
  totalGroups: number
  progressPercentage: number
  lastUpdated: string
}

