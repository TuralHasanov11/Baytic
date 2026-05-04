# Layout System & Admin Panel Documentation

## Overview

The Baytic application uses Nuxt 3's file-based layout system with two distinct layouts:
- **default**: Public-facing pages (home, blog, etc.)  
- **admin**: Protected admin panel with admin-specific UI

## Directory Structure

\\\
app/
├── components/
│   ├── BaseHeader.vue
│   ├── BaseFooter.vue
│   ├── AdminHeader.vue
│   ├── AdminFooter.vue
│   └── ...
├── layouts/
│   ├── default.vue
│   └── admin.vue
├── pages/
│   ├── blog/
│   │   ├── index.vue
│   │   └── [slug].vue
│   └── admin/
│       ├── index.vue
│       ├── blog/
│       │   ├── index.vue
│       │   ├── new.vue
│       │   └── [id].vue
│       ├── users.vue
│       └── settings.vue
└── app.vue
\\\

## Layout System

### Default Layout
The public-facing layout with:
- BaseHeader (logo, navigation, auth buttons)
- Main content area
- BaseFooter (links, copyright)

### Admin Layout  
The admin-specific layout with:
- AdminHeader (dashboard navigation, user menu, back to site link)
- Main content area
- AdminFooter (minimal footer)

### How It Works

1. Pages use the **default** layout automatically
2. Specify admin layout with \definePageMeta({ layout: 'admin' })\
3. Layouts wrap page content via \<slot />\

## Admin Pages

- **/admin**: Dashboard with statistics and quick actions
- **/admin/blog**: Blog post management (list, create, edit)
- **/admin/users**: User management with roles
- **/admin/settings**: System configuration (General, Security, Email, API)

## Best Practices Implemented

✅ **Component Reusability**: Header/Footer extracted into separate components
✅ **Semantic HTML**: Proper structure with \<header>\, \<main>\, \<footer>\
✅ **Responsive Design**: Mobile-first with Tailwind breakpoints
✅ **Accessibility**: ARIA labels, keyboard navigation, focus states
✅ **Type Safety**: Full TypeScript support
✅ **Route Protection**: Auth middleware on admin pages
✅ **Consistent Styling**: Unified design system with Tailwind
✅ **Auto-imports**: Components and composables auto-imported
✅ **Performance**: Code-splitting by layout/page
✅ **Developer Experience**: Clear structure, easy to extend

## Key Features

- **Responsive tables** with status indicators and actions
- **Tabbed settings** interface  
- **Form validation** and error handling
- **Auth integration** with login/logout
- **Loading states** and success/error messages
- **Auto-slug generation** for blog posts
- **Active route highlighting** in admin navigation

## Extending

### Add New Public Page
Create \pages/new-page.vue\ - automatically uses default layout

### Add New Admin Page  
Create \pages/admin/new-page.vue\ with:
\\\	ypescript
definePageMeta({
  layout: 'admin',
  middleware: 'auth'
})
\\\

## Technologies

- Nuxt 3 (file-based routing)
- Vue 3 Composition API (\<script setup>\)
- TypeScript (strict mode)
- Tailwind CSS (utility-first styling)
- NuxtUI components (optional integration ready)
