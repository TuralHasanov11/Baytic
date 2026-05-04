<template>
  <div>
    <NuxtLink to="/admin/blog" class="text-emerald-600 hover:text-emerald-700 font-medium mb-4 inline-block">← Back</NuxtLink>
    <h1 class="text-3xl font-bold text-slate-900">Edit Post</h1>
    <form class="space-y-6 mt-8" @submit.prevent="submit">
      <div class="bg-white rounded-lg shadow-sm border border-slate-200 p-6">
        <label class="block text-sm font-semibold text-slate-900 mb-2">Title</label>
        <input v-model="form.title" type="text" required class="w-full px-4 py-2 rounded-md border border-slate-300 focus:outline-none focus:ring-2 focus:ring-emerald-500" >
      </div>
      <div class="bg-white rounded-lg shadow-sm border border-slate-200 p-6">
        <label class="block text-sm font-semibold text-slate-900 mb-2">Content</label>
        <textarea v-model="form.body" required rows="12" class="w-full px-4 py-2 rounded-md border border-slate-300 focus:outline-none focus:ring-2 focus:ring-emerald-500 font-mono text-sm" />
      </div>
      <div class="bg-white rounded-lg shadow-sm border border-slate-200 p-6">
        <label class="block text-sm font-semibold text-slate-900 mb-2">Status</label>
        <select v-model="form.status" class="w-full px-4 py-2 rounded-md border border-slate-300 focus:outline-none focus:ring-2 focus:ring-emerald-500">
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Scheduled">Scheduled</option>
          <option value="Archived">Archived</option>
        </select>
      </div>
      <div class="flex gap-3">
        <button type="submit" :disabled="loading" class="px-6 py-2 rounded-md bg-emerald-600 text-white font-medium hover:bg-emerald-700 transition disabled:opacity-50">
          {{ loading ? 'Saving...' : 'Save' }}
        </button>
        <NuxtLink to="/admin/blog" class="px-6 py-2 rounded-md bg-slate-100 text-slate-900 font-medium hover:bg-slate-200 transition">Cancel</NuxtLink>
      </div>
      <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">{{ error }}</div>
    </form>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'auth' })
useHead({ title: 'Edit Post' })
const route = useRoute()
const router = useRouter()
const { update } = useBlog()
const id = route.params.id as string
const form = ref({ title: '', body: '', slug: '', status: 'Draft' })
const loading = ref(false)
const error = ref('')
const submit = async () => {
  error.value = ''
  loading.value = true
  try {
    await update(id, form.value)
    router.push('/admin/blog')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed'
  } finally {
    loading.value = false
  }
}
onMounted(() => {
  console.log('Editing post:', id)
})
</script>
