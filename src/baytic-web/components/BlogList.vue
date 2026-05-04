<template>
  <section class="blog-list">
    <div v-if="loading">Loading posts…</div>
    <div v-else-if="error">Failed to load posts.</div>
    <div v-else>
      <BlogPostCard v-for="p in posts" :key="p.id" :post="p" />
    </div>
  </section>
</template>

<script setup lang="ts">
import BlogPostCard from '~/components/BlogPostCard.vue'
import type { BlogPostList } from '~/types/BlogPost'

const props = defineProps<{
  posts: BlogPostList
  loading?: boolean
  error?: unknown
}>()

const posts = props.posts || []
</script>

<style scoped>
.blog-list { display:flex; flex-direction:column; gap:1rem }
</style>
