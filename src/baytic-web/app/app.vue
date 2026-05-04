<template>
  <UApp>
    <div class="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(34,197,94,0.16),_transparent_34%),linear-gradient(180deg,_#07111f_0%,_#0b1627_50%,_#f6f8fb_50%,_#eef2f7_100%)] text-slate-100">
      <NuxtRouteAnnouncer />
      <header class="border-b border-white/10 bg-slate-950/70 backdrop-blur">
        <div class="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
          <NuxtLink to="/" class="flex items-center gap-3 text-white no-underline">
            <span class="grid h-10 w-10 place-items-center rounded-2xl bg-emerald-400 text-sm font-black text-slate-950 shadow-lg shadow-emerald-400/20">
              B
            </span>
            <span class="leading-tight">
              <span class="block text-sm font-semibold uppercase tracking-[0.28em] text-emerald-300/90">Baytic</span>
              <span class="block text-xs text-slate-300">Keycloak auth for the member portal</span>
            </span>
          </NuxtLink>

          <nav class="hidden items-center gap-2 sm:flex">
            <NuxtLink
              to="/"
              class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
            >
              Home
            </NuxtLink>
            <NuxtLink
              to="/dashboard"
              class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
            >
              Dashboard
            </NuxtLink>
          </nav>

          <AuthState>
            <template #default="{ loggedIn, user, clear }">
              <div class="flex items-center gap-3">
                <span class="hidden rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-medium text-slate-200 sm:inline-flex">
                  {{ user?.name || user?.preferredUsername || user?.email || 'Authenticated user' }}
                </span>
                <NuxtLink
                  v-if="!loggedIn"
                  to="/login"
                  class="rounded-full bg-emerald-400 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-emerald-300"
                >
                  Sign in
                </NuxtLink>
                <button
                  v-else
                  type="button"
                  class="rounded-full bg-white/10 px-4 py-2 text-sm font-semibold text-white transition hover:bg-white/15"
                  @click="clear"
                >
                  Sign out
                </button>
              </div>
            </template>
            <template #placeholder>
              <div class="h-10 w-24 rounded-full bg-white/10" />
            </template>
          </AuthState>
        </div>
      </header>

      <main class="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8 lg:py-12">
        <NuxtPage />
      </main>
    </div>
  </UApp>
</template>

<script setup lang="ts">
useHead({
  titleTemplate: title => (title ? `${title} · Baytic` : 'Baytic')
})
</script>
