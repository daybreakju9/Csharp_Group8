import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/projects',
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { requiresAuth: false },
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/Register.vue'),
    meta: { requiresAuth: false },
  },
  {
    path: '/projects',
    name: 'Projects',
    component: () => import('../views/user/ProjectList.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/projects/:projectId/queues',
    name: 'Queues',
    component: () => import('../views/user/QueueList.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/queues/:queueId/select',
    name: 'ImageSelection',
    component: () => import('../views/user/ImageSelection.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/admin',
    name: 'Admin',
    redirect: '/admin/projects',
    meta: { requiresAuth: true, requiresAdmin: true },
  },
  {
    path: '/admin/projects',
    name: 'AdminProjects',
    component: () => import('../views/admin/ProjectManagement.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
  },
  {
    path: '/admin/queues',
    name: 'AdminQueues',
    component: () => import('../views/admin/QueueManagement.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
  },
  {
    path: '/admin/export',
    name: 'DataExport',
    component: () => import('../views/admin/DataExport.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
  },
  {
    path: '/admin/users',
    name: 'UserManagement',
    component: () => import('../views/admin/UserManagement.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// 路由守卫
router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()

  // 检查是否需要认证
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }

  // 检查是否需要管理员权限
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next('/projects')
    return
  }

  // 检查游客用户权限 - 游客用户只能访问项目列表页面
  if (authStore.isAuthenticated && authStore.user?.role === 'Guest') {
    const allowedPaths = ['/projects', '/login', '/register']
    if (!allowedPaths.includes(to.path)) {
      next('/projects')
      return
    }
  }

  // 如果已登录访问登录页，重定向到首页
  if ((to.path === '/login' || to.path === '/register') && authStore.isAuthenticated) {
    next('/projects')
    return
  }

  next()
})

export default router

