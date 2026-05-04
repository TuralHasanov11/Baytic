<template>
  <div>
    <div class="mb-8 flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-slate-900">Blog Posts</h1>
        <p class="text-slate-600 mt-2">Manage and publish blog posts</p>
      </div>
      <NuxtLink to="/admin/blog/new" class="px-4 py-2 rounded-md bg-emerald-600 text-white font-medium hover:bg-emerald-700 transition">
        + New Post
      </NuxtLink>
    </div>
    <div v-if="loading" class="bg-white rounded-lg shadow-sm border border-slate-200 p-8 text-center">
      <p class="text-slate-600">Loading posts...</p>
    </div>
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">
      Failed to load posts. Please try again.
    </div>
    <div v-else class="bg-white rounded-lg shadow-sm border border-slate-200">
      <table class="w-full">
        <thead class="border-b border-slate-200 bg-slate-50">
          <tr><th class="px-6 py-3 text-left text-sm font-semibold">Title</th><th class="px-6 py-3 text-left text-sm font-semibold">Status</th><th class="px-6 py-3 text-left text-sm font-semibold">Author</th><th class="px-6 py-3 text-left text-sm font-semibold">Published</th></tr>
        </thead>
        <tbody>
          <tr v-for="post in posts" :key="post.id" class="border-b border-slate-100">
            <td class="px-6 py-4 text-sm">{{ post.title }}</td>
            <td class="px-6 py-4 text-sm">{{ post.status }}</td>
            <td class="px-6 py-4 text-sm">{{ post.authorId }}</td>
            <td class="px-6 py-4 text-sm">{{ post.publishedAt ? formatDate(post.publishedAt) : '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'auth' })
useHead({ title: 'Blog Posts' })
const { getAll } = useBlog()
const { data, loading, error, refresh } = getAll()
const posts = computed(() => data.value || [])
const formatDate = (d: string) => d ? new Date(d).toLocaleDateString() : '-'
onMounted(() => refresh())
</script>
