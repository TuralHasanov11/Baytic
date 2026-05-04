export interface BlogPost {
  id: string
  title: string
  slug: string
  summary?: string
  content: string
  author?: {
    name: string
    avatarUrl?: string
  }
  publishedAt?: string // ISO date
  tags?: string[]
  coverImageUrl?: string
}

export type BlogPostList = BlogPost[]
