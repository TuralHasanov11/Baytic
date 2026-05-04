<template>
  <section class="grid gap-8 lg:grid-cols-[1.3fr_0.7fr] lg:items-start">
    <div class="space-y-6 rounded-[2rem] border border-slate-200/80 bg-white/90 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.16)] backdrop-blur sm:p-10">
      <div class="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-4 py-2 text-sm font-medium text-emerald-800">
        <span class="h-2 w-2 rounded-full bg-emerald-500" />
        Keycloak login is configured for the Baytic web app
      </div>

      <div class="space-y-4">
        <h1 class="max-w-2xl text-4xl font-semibold tracking-tight text-slate-950 sm:text-5xl">
          Secure member access with a session cookie, not ad hoc client state.
        </h1>
        <p class="max-w-2xl text-lg leading-8 text-slate-600">
          This Nuxt app now uses <span class="font-medium text-slate-900">nuxt-auth-utils</span> to handle the Keycloak login callback,
          store a sealed session cookie, and expose the signed-in user through the standard composables.
        </p>
      </div>

      <div class="flex flex-wrap gap-3">
        <NuxtLink
          to="/login"
          class="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
        >
          Continue to login
        </NuxtLink>
        <NuxtLink
          to="/dashboard"
          class="rounded-full border border-slate-300 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:border-slate-400 hover:bg-slate-50"
        >
          Open dashboard
        </NuxtLink>
      </div>

      <div class="grid gap-4 sm:grid-cols-3">
        <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Provider</p>
          <p class="mt-2 text-base font-medium text-slate-900">Keycloak</p>
        </div>
        <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Session</p>
          <p class="mt-2 text-base font-medium text-slate-900">Secure cookie</p>
        </div>
        <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Guarded area</p>
          <p class="mt-2 text-base font-medium text-slate-900">/dashboard</p>
        </div>
      </div>
    </div>

    <div class="space-y-4 rounded-[2rem] border border-slate-800/80 bg-slate-950 p-6 text-slate-100 shadow-[0_20px_80px_rgba(15,23,42,0.28)] sm:p-8">
      <p class="text-sm font-semibold uppercase tracking-[0.28em] text-emerald-300">What is wired</p>
      <ul class="space-y-3 text-sm leading-6 text-slate-300">
        <li class="rounded-2xl border border-white/10 bg-white/5 p-4">Keycloak OAuth route at <span class="font-medium text-white">/auth/keycloak</span>.</li>
        <li class="rounded-2xl border border-white/10 bg-white/5 p-4">A typed session payload built from the Keycloak userinfo response.</li>
        <li class="rounded-2xl border border-white/10 bg-white/5 p-4">A route middleware that protects authenticated pages.</li>
        <li class="rounded-2xl border border-white/10 bg-white/5 p-4">A dashboard page that reads the current session state.</li>
      </ul>
    </div>
  </section>
</template>

<script setup lang="ts">
useHead({
  title: 'Home'
})
</script>