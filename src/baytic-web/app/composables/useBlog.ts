const DEFAULT_API_BASE = '/api'

export function useBlog() {
  const config = useRuntimeConfig()
  const base = (config.public?.apiBase as string) || DEFAULT_API_BASE

  /**
   * Fetch all published blog posts (or all drafts if user is admin)
   */
  function getAll() {
    const url = `${base}/blog`
    const { data, pending, error, refresh } = useFetch<BlogPostList>(url)
    return { data, loading: pending, error, refresh }
  }

  /**
   * Fetch a single blog post by slug
   */
  function getBySlug(slug: string) {
    const url = `${base}/blog/${encodeURIComponent(slug)}`
    const { data, pending, error, refresh } = useFetch<BlogPost>(url)
    return { data, loading: pending, error, refresh }
  }

  /**
   * Create a new blog post draft
   */
  async function create(payload: CreateBlogPostDto) {
    const url = `${base}/blog`
    const response = await $fetch<BlogPost>(url, {
      method: 'POST',
      body: payload,
    })
    return response
  }

  /**
   * Update an existing blog post draft
   */
  async function update(id: string, payload: UpdateBlogPostDto) {
    const url = `${base}/blog/${id}`
    const response = await $fetch<BlogPost>(url, {
      method: 'PUT',
      body: payload,
    })
    return response
  }

  /**
   * Publish a blog post
   */
  async function publish(id: string, payload: PublishBlogPostDto) {
    const url = `${base}/blog/${id}/publish`
    const response = await $fetch<BlogPost>(url, {
      method: 'POST',
      body: payload,
    })
    return response
  }

  /**
   * Archive a blog post
   */
  async function archive(id: string) {
    const url = `${base}/blog/${id}/archive`
    const response = await $fetch<BlogPost>(url, {
      method: 'POST',
    })
    return response
  }

  return { getAll, getBySlug, create, update, publish, archive }
}

export type UseBlog = ReturnType<typeof useBlog>
