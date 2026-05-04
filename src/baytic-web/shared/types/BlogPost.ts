/**
 * BlogPostStatus enum matching domain model
 * - Draft: Not yet published
 * - Scheduled: Scheduled for future publishing
 * - Published: Currently published
 * - Archived: No longer active
 */
export enum BlogPostStatus {
  Draft = 'Draft',
  Scheduled = 'Scheduled',
  Published = 'Published',
  Archived = 'Archived',
}

/**
 * BlogPost interface matching the domain aggregate
 * Reflects the actual C# BlogPost entity structure
 */
export interface BlogPost {
  id: string
  authorId: string
  title: string
  slug: string
  body: string // Main content (HTML or Markdown)
  status: BlogPostStatus
  publishedAt?: string // ISO date (nullable in domain)
  createdAt: string // ISO date
  updatedAt: string // ISO date
}

/**
 * BlogPost creation payload (for API requests)
 * Matches CreateDraft domain method signature
 */
export interface CreateBlogPostDto {
  title: string
  slug: string
  body: string
}

/**
 * BlogPost update payload (for API requests)
 * Matches UpdateDraft domain method signature
 */
export interface UpdateBlogPostDto {
  title: string
  slug: string
  body: string
}

/**
 * BlogPost publish payload (for API requests)
 * Matches Publish domain method signature
 */
export interface PublishBlogPostDto {
  publishedAtUtc: string // ISO date
}

export type BlogPostList = BlogPost[]
