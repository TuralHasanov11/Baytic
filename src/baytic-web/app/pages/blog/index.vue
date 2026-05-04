<template>
  <main>
    <h1>Blog</h1>
    <BlogList :posts="posts || []" :loading="loading" :error="error" />
  </main>
</template>

<script setup lang="ts">
const { getAll } = useBlog()
const { data, loading, error, refresh } = getAll()

const posts = ref<BlogPost[] | undefined>()

onMounted(async () => {
  if (!data.value) {
    await refresh()
  }
  posts.value = data.value
})
</script>

<style scoped>
main { padding: 2rem 1rem; max-width:1200px; margin:0 auto }
h1 { margin-bottom: 1.5rem; font-size:2rem; font-weight:700 }
</style>
