<template>
  <main class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <section class="rounded-4xl border border-slate-200/80 bg-white/95 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.14)] sm:p-10">
      <div class="max-w-3xl space-y-4">
        <UChip color="emerald" variant="soft" size="sm">{{ $t('pages.governance.kicker') }}</UChip>
        <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-5xl">{{ $t('pages.governance.title') }}</h1>
        <p class="text-lg leading-8 text-slate-600">{{ $t('pages.governance.lead') }}</p>
      </div>

      <div class="mt-10 grid gap-4 lg:grid-cols-3">
        <UCard v-for="item in structure" :key="item.title" :title="item.title" :description="item.description" variant="subtle" />
      </div>

      <div class="mt-10 grid gap-6 lg:grid-cols-[1fr_1fr]">
        <UAccordion :items="accordionItems" type="multiple" />
        <UCard :title="$t('pages.governance.footerTitle')" :description="$t('pages.governance.footerText')" variant="outline" />
      </div>
    </section>
  </main>
</template>

<script setup lang="ts">
import type { AccordionItem } from '@nuxt/ui'

const { t } = useI18n()

const structure = computed(() => [
  { title: t('pages.governance.card1Title'), description: t('pages.governance.card1Text') },
  { title: t('pages.governance.card2Title'), description: t('pages.governance.card2Text') },
  { title: t('pages.governance.card3Title'), description: t('pages.governance.card3Text') },
])

const accordionItems = computed<AccordionItem[]>(() => [
  { label: t('pages.governance.a1Label'), content: t('pages.governance.a1Text') },
  { label: t('pages.governance.a2Label'), content: t('pages.governance.a2Text') },
  { label: t('pages.governance.a3Label'), content: t('pages.governance.a3Text') },
])

useHead({ title: t('pages.governance.title') })
</script>
