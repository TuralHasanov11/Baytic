<template>
  <UPage>
    <section class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div class="overflow-hidden rounded-4xl border border-slate-200/80 bg-white/95 shadow-[0_20px_80px_rgba(15,23,42,0.14)]">
        <div class="grid gap-8 lg:grid-cols-[1.1fr_0.9fr] lg:items-center">
          <div class="space-y-6 p-8 sm:p-10 lg:p-12">
            <UChip color="emerald" variant="soft" size="sm">{{ $t('pages.about.kicker') }}</UChip>
            <div class="space-y-4">
              <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-5xl">{{ $t('pages.about.title') }}</h1>
              <p class="max-w-2xl text-lg leading-8 text-slate-600">{{ $t('pages.about.lead') }}</p>
            </div>

            <div class="flex flex-wrap gap-3">
              <NuxtLink to="/history" class="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:bg-slate-800">
                {{ $t('pages.about.ctaHistory') }}
              </NuxtLink>
              <NuxtLink to="/membership" class="rounded-full border border-slate-300 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:border-slate-400 hover:bg-slate-50">
                {{ $t('pages.about.ctaMembership') }}
              </NuxtLink>
            </div>
          </div>

          <div class="bg-slate-950 p-8 text-slate-100 sm:p-10 lg:p-12">
            <div class="space-y-4">
              <p class="text-sm font-semibold uppercase tracking-[0.28em] text-emerald-300">{{ $t('pages.about.sideTitle') }}</p>
              <p class="text-base leading-7 text-slate-300">{{ $t('pages.about.sideText') }}</p>
            </div>

            <div class="mt-8 grid gap-4 sm:grid-cols-2">
              <UCard variant="subtle" class="bg-white/5 text-white ring-1 ring-white/10">
                <template #header>
                  <p class="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">{{ $t('pages.about.card1Title') }}</p>
                </template>
                <p class="text-sm leading-6 text-slate-200">{{ $t('pages.about.card1Text') }}</p>
              </UCard>
              <UCard variant="subtle" class="bg-white/5 text-white ring-1 ring-white/10">
                <template #header>
                  <p class="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">{{ $t('pages.about.card2Title') }}</p>
                </template>
                <p class="text-sm leading-6 text-slate-200">{{ $t('pages.about.card2Text') }}</p>
              </UCard>
            </div>
          </div>
        </div>
      </div>

      <div class="mt-8 grid gap-4 md:grid-cols-3">
        <UCard v-for="item in highlights" :key="item.title" :title="item.title" :description="item.description" variant="subtle" />
      </div>
    </section>
  </UPage>
</template>

<script setup lang="ts">
const { t } = useI18n()

const highlights = computed(() => [
  { title: t('pages.about.highlight1Title'), description: t('pages.about.highlight1Text') },
  { title: t('pages.about.highlight2Title'), description: t('pages.about.highlight2Text') },
  { title: t('pages.about.highlight3Title'), description: t('pages.about.highlight3Text') },
])

useHead({ title: t('pages.about.title') })
</script>
