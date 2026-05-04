<template>
  <div>
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-slate-900">Admin Dashboard</h1>
      <p class="text-slate-600 mt-2">Welcome to the Baytic administration panel</p>
    </div>

    <!-- Stats Grid -->
    <div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-8">
      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Total Posts</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">24</p>
          </div>
          <div class="text-4xl text-slate-200">📝</div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Total Users</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">156</p>
          </div>
          <div class="text-4xl text-slate-200">👥</div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Published Posts</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">18</p>
          </div>
          <div class="text-4xl text-slate-200">✅</div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Draft Posts</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">6</p>
          </div>
          <div class="text-4xl text-slate-200">📋</div>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="bg-white rounded-lg shadow-sm border border-slate-200 p-6">
      <h2 class="text-lg font-semibold text-slate-900 mb-4">Quick Actions</h2>
      <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <NuxtLink
          to="/admin/blog/new"
          class="flex items-center gap-3 px-4 py-3 rounded-md bg-emerald-50 text-emerald-700 hover:bg-emerald-100 transition border border-emerald-200"
        >
          <span>✍️</span>
          <span class="text-sm font-medium">Create Post</span>
        </NuxtLink>

        <NuxtLink
          to="/admin/blog"
          class="flex items-center gap-3 px-4 py-3 rounded-md bg-blue-50 text-blue-700 hover:bg-blue-100 transition border border-blue-200"
        >
          <span>📚</span>
          <span class="text-sm font-medium">Manage Posts</span>
        </NuxtLink>

        <NuxtLink
          to="/admin/users"
          class="flex items-center gap-3 px-4 py-3 rounded-md bg-purple-50 text-purple-700 hover:bg-purple-100 transition border border-purple-200"
        >
          <span>👤</span>
          <span class="text-sm font-medium">Manage Users</span>
        </NuxtLink>

        <NuxtLink
          to="/admin/settings"
          class="flex items-center gap-3 px-4 py-3 rounded-md bg-slate-100 text-slate-700 hover:bg-slate-200 transition border border-slate-300"
        >
          <span>⚙️</span>
          <span class="text-sm font-medium">Settings</span>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: 'auth' // Protect with auth middleware
})

useHead({
  title: 'Dashboard'
})
</script>
