<template>
  <div>
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-slate-900">User Management</h1>
      <p class="text-slate-600 mt-2">Manage system users and permissions</p>
    </div>

    <!-- Users Table -->
    <div class="bg-white rounded-lg shadow-sm border border-slate-200 overflow-hidden">
      <table class="w-full">
        <thead class="border-b border-slate-200 bg-slate-50">
          <tr>
            <th class="px-6 py-3 text-left text-sm font-semibold text-slate-900">Name</th>
            <th class="px-6 py-3 text-left text-sm font-semibold text-slate-900">Email</th>
            <th class="px-6 py-3 text-left text-sm font-semibold text-slate-900">Role</th>
            <th class="px-6 py-3 text-left text-sm font-semibold text-slate-900">Status</th>
            <th class="px-6 py-3 text-left text-sm font-semibold text-slate-900">Joined</th>
            <th class="px-6 py-3 text-left text-sm font-semibold text-slate-900">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id" class="border-b border-slate-100 hover:bg-slate-50 transition">
            <td class="px-6 py-4 text-sm font-medium text-slate-900">{{ user.name }}</td>
            <td class="px-6 py-4 text-sm text-slate-600">{{ user.email }}</td>
            <td class="px-6 py-4 text-sm">
              <span
                class="inline-block px-2 py-1 rounded text-xs font-medium"
                :class="roleClass(user.role)"
              >
                {{ user.role }}
              </span>
            </td>
            <td class="px-6 py-4 text-sm">
              <span
                class="inline-block px-2 py-1 rounded text-xs font-medium"
                :class="user.active ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-700'"
              >
                {{ user.active ? 'Active' : 'Inactive' }}
              </span>
            </td>
            <td class="px-6 py-4 text-sm text-slate-600">{{ formatDate(user.joinedAt) }}</td>
            <td class="px-6 py-4 text-sm flex gap-2">
              <button class="text-emerald-600 hover:text-emerald-700 font-medium">Edit</button>
              <button class="text-red-600 hover:text-red-700 font-medium">Deactivate</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

definePageMeta({
  layout: 'admin',
  middleware: 'auth'
})

useHead({ title: 'User Management' })

interface User {
  id: string
  name: string
  email: string
  role: 'Admin' | 'Editor' | 'User'
  active: boolean
  joinedAt: string
}

const users = ref<User[]>([
  {
    id: '1',
    name: 'Admin User',
    email: 'admin@baytic.com',
    role: 'Admin',
    active: true,
    joinedAt: '2026-01-15'
  },
  {
    id: '2',
    name: 'Editor One',
    email: 'editor1@baytic.com',
    role: 'Editor',
    active: true,
    joinedAt: '2026-02-20'
  },
  {
    id: '3',
    name: 'Regular User',
    email: 'user@baytic.com',
    role: 'User',
    active: true,
    joinedAt: '2026-03-10'
  }
])

const roleClass = (role: string) => {
  const classes: Record<string, string> = {
    'Admin': 'bg-red-100 text-red-700',
    'Editor': 'bg-blue-100 text-blue-700',
    'User': 'bg-slate-100 text-slate-700'
  }
  return classes[role] || 'bg-slate-100 text-slate-700'
}

const formatDate = (dateString: string) => {
  try {
    return new Date(dateString).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
  } catch {
    return dateString
  }
}
</script>
