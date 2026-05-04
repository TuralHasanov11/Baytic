import type { BlogPost, BlogPostList } from '~/types/BlogPost'

const DEFAULT_API_BASE = '/api'

export function useBlog() {
  const config = useRuntimeConfig()
  const base = (config.public?.apiBase as string) || DEFAULT_API_BASE

  function getAll() {
    const url = `${base}/blog`
    const { data, pending, error, refresh } = useFetch<BlogPostList>(url)
    return { data, loading: pending, error, refresh }
  }

  function getBySlug(slug: string) {
    const url = `${base}/blog/${encodeURIComponent(slug)}`
    const { data, pending, error, refresh } = useFetch<BlogPost>(url)
    return { data, loading: pending, error, refresh }
  }

  return { getAll, getBySlug }
}

export type UseBlog = ReturnType<typeof useBlog>
