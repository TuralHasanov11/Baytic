<script setup lang="ts">
definePageMeta({
  middleware: 'auth'
})

const { user, session } = useUserSession()

useHead({
  title: 'Dashboard'
})
</script>

<template>
  <section class="space-y-6 rounded-[2rem] border border-slate-200/80 bg-white/95 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.14)] sm:p-10">
    <div class="space-y-3">
      <p class="text-sm font-semibold uppercase tracking-[0.28em] text-emerald-700">Member area</p>
      <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">
        Welcome, {{ user?.name || user?.preferredUsername || user?.email || 'member' }}
      </h1>
      <p class="max-w-2xl text-base leading-7 text-slate-600">
        This page is behind the route middleware, so it only renders for authenticated users with a valid Keycloak-backed session.
      </p>
    </div>

    <div class="grid gap-4 md:grid-cols-3">
      <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
        <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">User ID</p>
        <p class="mt-2 break-all text-sm font-medium text-slate-900">{{ user?.id }}</p>
      </div>
      <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
        <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Email</p>
        <p class="mt-2 break-all text-sm font-medium text-slate-900">{{ user?.email || 'Not provided' }}</p>
      </div>
      <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
        <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Logged in at</p>
        <p class="mt-2 text-sm font-medium text-slate-900">
          {{ session?.loggedInAt ? new Date(session.loggedInAt).toLocaleString() : 'Unknown' }}
        </p>
      </div>
    </div>
  </section>
</template>