<template>
  <header class="border-b border-white/10 bg-slate-950/70 backdrop-blur">
    <div class="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
      <NuxtLink to="/" class="flex items-center gap-3 text-white no-underline">
        <span class="grid h-10 w-10 place-items-center rounded-2xl bg-emerald-400 text-sm font-black text-slate-950 shadow-lg shadow-emerald-400/20">
          B
        </span>
        <span class="leading-tight">
          <span class="block text-sm font-semibold uppercase tracking-[0.28em] text-emerald-300/90">Baytic</span>
          <span class="block text-xs text-slate-300">{{ $t('header.subtitle') }}</span>
        </span>
      </NuxtLink>

      <nav class="hidden items-center gap-2 sm:flex">
        <NuxtLink
          to="/"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.home') }}
        </NuxtLink>
        <NuxtLink
          to="/about"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.about') }}
        </NuxtLink>
        <NuxtLink
          to="/history"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.history') }}
        </NuxtLink>
        <NuxtLink
          to="/membership"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.membership') }}
        </NuxtLink>
        <NuxtLink
          to="/governance"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.governance') }}
        </NuxtLink>
        <NuxtLink
          to="/blog"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.blog') }}
        </NuxtLink>
        <NuxtLink
          to="/veterinarians"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.veterinarians') }}
        </NuxtLink>
        <NuxtLink
          to="/dashboard"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.dashboard') }}
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
</template>

<script setup lang="ts">
// Header component uses AuthState for auth UI
// Auto-imported from components
</script>