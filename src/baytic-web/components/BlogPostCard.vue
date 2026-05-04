<template>
  <article class="blog-card">
    <nuxt-link :to="`/blog/${post.slug}`" class="cover-link">
      <img v-if="post.coverImageUrl" :src="post.coverImageUrl" :alt="post.title" class="cover" />
    </nuxt-link>

    <div class="content">
      <nuxt-link :to="`/blog/${post.slug}`" class="title-link">
        <h2 class="title">{{ post.title }}</h2>
      </nuxt-link>
      <p class="summary" v-if="post.summary">{{ post.summary }}</p>
      <div class="meta">
        <span class="author" v-if="post.author">{{ post.author.name }}</span>
        <time v-if="post.publishedAt" :datetime="post.publishedAt">{{ formattedDate }}</time>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { BlogPost } from '~/types/BlogPost'

const props = defineProps<{ post: BlogPost }>()

const formattedDate = computed(() => {
  if (!props.post.publishedAt) return ''
  try {
    return new Date(props.post.publishedAt).toLocaleDateString()
  } catch {
    return props.post.publishedAt
  }
})
</script>

<style scoped>
.blog-card { display:flex; gap:1rem; align-items:flex-start; }
.cover { width:160px; height:100px; object-fit:cover; border-radius:6px }
.content { flex:1 }
.title { margin:0 0 .25rem 0 }
.summary { margin:0 0 .5rem 0; color:var(--muted, #666) }
.meta { font-size:.85rem; color:var(--muted, #888) }
</style>
