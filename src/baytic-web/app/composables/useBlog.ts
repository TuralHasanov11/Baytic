const DEFAULT_API_BASE = '/api'

export function useBlog() {
  const config = useRuntimeConfig()
  const base = (config.public?.apiBase as string) || DEFAULT_API_BASE

  /**
   * Fetch all published blog posts (or all drafts if user is admin)
   */
  function getAll() {
    /*
      Mocked `useBlog` composable

      This composable returns mock data from `app/mocks/blogMock.ts` and
      intentionally avoids real network requests. Original network calls
      (useFetch / $fetch) are replaced by in-memory operations. This makes
      the frontend usable during local development without the backend.
    */

    import { ref } from 'vue'
    import type {
      BlogPost,
      BlogPostList,
      CreateBlogPostDto,
      UpdateBlogPostDto,
      PublishBlogPostDto,
    } from '~/shared/types/BlogPost'
    import { MOCK_BLOG_POSTS } from '../mocks/blogMock'

    const cloned = () => MOCK_BLOG_POSTS.map(p => ({ ...p }))

    export function useBlog() {
      // NOTE: Keeping a small in-memory store for mocks so create/update mutate it.
      const store = ref<BlogPostList>(cloned())

      function getAll() {
        const data = ref<BlogPostList | undefined>(store.value)
        const loading = ref(false)
        const error = ref<Error | null>(null)

        async function refresh() {
          loading.value = true
          // simulate a short delay
          await new Promise(r => setTimeout(r, 50))
          data.value = store.value
          loading.value = false
        }

        return { data, loading, error, refresh }
      }

      function getBySlug(slug: string) {
        const found = store.value.find(p => p.slug === slug)
        const data = ref<BlogPost | undefined>(found)
        const loading = ref(false)
        const error = ref<Error | null>(null)

        async function refresh() {
          loading.value = true
          await new Promise(r => setTimeout(r, 50))
          data.value = store.value.find(p => p.slug === slug)
          loading.value = false
        }

        return { data, loading, error, refresh }
      }

      async function create(payload: CreateBlogPostDto) {
        // simulate server behavior
        const now = new Date().toISOString()
        const id = `post-${Date.now()}-${Math.floor(Math.random() * 1000)}`
        const newPost: BlogPost = {
          id,
          authorId: 'mock-author',
          title: payload.title,
          slug: payload.slug,
          body: payload.body,
          status: 'Draft',
          createdAt: now,
          updatedAt: now,
        }
        store.value = [newPost, ...store.value]
        return newPost
      }

      async function update(id: string, payload: UpdateBlogPostDto) {
        const idx = store.value.findIndex(p => p.id === id)
        if (idx === -1) throw new Error('Post not found')
        const updated = { ...store.value[idx], ...payload, updatedAt: new Date().toISOString() }
        store.value.splice(idx, 1, updated)
        return updated
      }

      async function publish(id: string, payload: PublishBlogPostDto) {
        const idx = store.value.findIndex(p => p.id === id)
        if (idx === -1) throw new Error('Post not found')
        const updated = {
          ...store.value[idx],
          status: 'Published',
          publishedAt: payload.publishedAtUtc || new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        }
        store.value.splice(idx, 1, updated)
        return updated
      }

      async function archive(id: string) {
        const idx = store.value.findIndex(p => p.id === id)
        if (idx === -1) throw new Error('Post not found')
        const updated = { ...store.value[idx], status: 'Archived', updatedAt: new Date().toISOString() }
        store.value.splice(idx, 1, updated)
        return updated
      }

      return { getAll, getBySlug, create, update, publish, archive }
    }

    export type UseBlog = ReturnType<typeof useBlog>
