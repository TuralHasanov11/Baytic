<template>
  <main>
    <div v-if="loading">Loading post…</div>
    <div v-else-if="error">Failed to load post.</div>
    <article v-else-if="post">
      <h1>{{ post.title }}</h1>
      <div class="meta">
        <span v-if="post.author">{{ post.author.name }}</span>
        <time v-if="post.publishedAt" :datetime="post.publishedAt">{{ formattedDate }}</time>
      </div>
      <img v-if="post.coverImageUrl" :src="post.coverImageUrl" :alt="post.title" class="cover" />
      <div class="content" v-html="post.content"></div>
    </article>
  </main>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import { computed, onMounted } from 'vue'
import { useBlog } from '~/composables/useBlog'
import type { BlogPost } from '~/types/BlogPost'

const route = useRoute()
const slug = route.params.slug as string

const { getBySlug } = useBlog()
const { data, loading, error, refresh } = getBySlug(slug)

const post = data as Ref<BlogPost | undefined>

const formattedDate = computed(() => {
  if (!post.value?.publishedAt) return ''
  try { return new Date(post.value.publishedAt).toLocaleDateString() } catch { return post.value.publishedAt }
})

onMounted(() => {
  if (!data.value) refresh()
})
</script>

<style scoped>
.cover { width:100%; max-height:420px; object-fit:cover; border-radius:6px; margin:1rem 0 }
.meta { color:var(--muted,#777); margin-bottom: .5rem }
.content { margin-top:1rem; line-height:1.6 }
</style>
