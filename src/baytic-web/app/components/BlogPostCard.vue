<template>
  <article class="blog-card">
    <div class="content">
      <NuxtLink :to="`/blog/${post.slug}`" class="title-link">
        <h2 class="title">{{ post.title }}</h2>
      </NuxtLink>
      <div class="meta">
        <span class="status" :class="`status-${post.status.toLowerCase()}`">{{ statusBadge }}</span>
        <time :datetime="post.publishedAt || post.createdAt">{{ formattedDate }}</time>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
const props = defineProps<{ post: BlogPost }>()

const formattedDate = computed(() => {
  const dateString = props.post.publishedAt || props.post.createdAt
  if (!dateString) return ''
  try {
    return new Date(dateString).toLocaleDateString()
  } catch {
    return dateString
  }
})

const statusBadge = computed(() => {
  const status = props.post.status as string
  return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase()
})
</script>

<style scoped>
.blog-card { display:flex; gap:1rem; align-items:flex-start; padding:.75rem; border:1px solid var(--border,#e5e7eb); border-radius:8px; transition:all .2s ease }
.blog-card:hover { box-shadow:0 2px 8px rgba(0,0,0,.1); border-color:var(--border-hover,#d1d5db) }
.content { flex:1; min-width:0 }
.title { margin:0 0 .25rem 0; font-size:1.1rem; font-weight:600 }
.status { display:inline-block; padding:.25rem .5rem; border-radius:4px; font-size:.75rem; font-weight:500; margin-right:.5rem }
.status-draft { background:var(--draft-bg,#f3f4f6); color:var(--draft-text,#6b7280) }
.status-scheduled { background:var(--scheduled-bg,#fef3c7); color:var(--scheduled-text,#92400e) }
.status-published { background:var(--published-bg,#d1fae5); color:var(--published-text,#065f46) }
.status-archived { background:var(--archived-bg,#f9fafb); color:var(--archived-text,#4b5563) }
.meta { font-size:.85rem; color:var(--muted, #888); margin-bottom:.5rem }
time { margin-left:.5rem }
</style>
