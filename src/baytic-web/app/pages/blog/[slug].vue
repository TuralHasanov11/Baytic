<template>
  <main>
    <div v-if="loading">Loading post…</div>
    <div v-else-if="error">Failed to load post.</div>
    <article v-else-if="post">
      <h1>{{ post.title }}</h1>
      <div class="meta">
        <span class="status" :class="`status-${post.status.toLowerCase()}`">{{ statusDisplay }}</span>
        <time :datetime="post.publishedAt || post.createdAt">{{ formattedDate }}</time>
        <span class="author-id">(by {{ post.authorId }})</span>
      </div>
      <div class="content" v-html="post.body"></div>
    </article>
  </main>
</template>

<script setup lang="ts">
const route = useRoute()
const slug = route.params.slug as string

const { getBySlug } = useBlog()
const { data, loading, error, refresh } = getBySlug(slug)

const post = data as Ref<BlogPost | undefined>

const formattedDate = computed(() => {
  const dateString = post.value?.publishedAt || post.value?.createdAt
  if (!dateString) return ''
  try { return new Date(dateString).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) } catch { return dateString }
})

const statusDisplay = computed(() => {
  if (!post.value) return ''
  return post.value.status.charAt(0).toUpperCase() + post.value.status.slice(1).toLowerCase()
})

onMounted(() => {
  if (!data.value) refresh()
})
</script>

<style scoped>
main { padding: 2rem 1rem; max-width:800px; margin:0 auto }
h1 { margin-bottom:.5rem; font-size:2rem }
.meta { display:flex; gap:1rem; align-items:center; color:var(--muted,#777); margin-bottom:1.5rem; font-size:.95rem }
.status { display:inline-block; padding:.25rem .5rem; border-radius:4px; font-size:.85rem; font-weight:500 }
.status-draft { background:var(--draft-bg,#f3f4f6); color:var(--draft-text,#6b7280) }
.status-scheduled { background:var(--scheduled-bg,#fef3c7); color:var(--scheduled-text,#92400e) }
.status-published { background:var(--published-bg,#d1fae5); color:var(--published-text,#065f46) }
.status-archived { background:var(--archived-bg,#f9fafb); color:var(--archived-text,#4b5563) }
.author-id { font-size:.85rem }
.content { margin-top:1.5rem; line-height:1.8; word-break:break-word }
.content :deep(h2) { margin:1.5rem 0 .5rem 0; font-size:1.5rem }
.content :deep(p) { margin:.75rem 0 }
.content :deep(blockquote) { margin:1rem 0; padding-left:1rem; border-left:3px solid var(--border,#e5e7eb); color:var(--quote,#666) }
.content :deep(code) { background:var(--code-bg,#f3f4f6); padding:.2rem .4rem; border-radius:3px; font-family:monospace; font-size:.9em }
</style>
