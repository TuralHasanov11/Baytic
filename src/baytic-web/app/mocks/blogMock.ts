import type { BlogPost, BlogPostStatus } from '~/shared/types/BlogPost'

export const MOCK_BLOG_POSTS: BlogPost[] = [
  {
    id: 'post-1',
    authorId: 'author-1',
    title: 'Welcome to Baytic',
    slug: 'welcome-to-baytic',
    body: '<p>This is a mock blog post used for local development.</p>',
    status: BlogPostStatus.Published,
    publishedAt: new Date().toISOString(),
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'post-2',
    authorId: 'author-2',
    title: 'Draft Post Example',
    slug: 'draft-post-example',
    body: '<p>This post is a draft in the mock dataset.</p>',
    status: BlogPostStatus.Draft,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
]

export default MOCK_BLOG_POSTS
