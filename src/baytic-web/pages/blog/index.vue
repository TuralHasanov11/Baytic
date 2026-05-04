<template>
  <main>
    <h1>Blog</h1>
    <BlogList :posts="posts" :loading="loading" :error="error" />
  </main>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useBlog } from '~/composables/useBlog'
import BlogList from '~/components/BlogList.vue'
import type { BlogPost } from '~/types/BlogPost'

const { getAll } = useBlog()
const { data, loading, error, refresh } = getAll()

const posts = data as Ref<BlogPost[] | undefined>

onMounted(() => {
  // ensure fetch runs on mount
  if (!data.value) refresh()
})
</script>

<style scoped>
main { padding: 1rem }
h1 { margin-bottom: 1rem }
</style>
