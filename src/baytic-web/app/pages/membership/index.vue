<template>
  <main class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <section class="rounded-4xl border border-slate-200/80 bg-white/95 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.14)] sm:p-10">
      <div class="max-w-3xl space-y-4">
        <UChip color="emerald" variant="soft" size="sm">{{ $t('pages.membership.kicker') }}</UChip>
        <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-5xl">{{ $t('pages.membership.title') }}</h1>
        <p class="text-lg leading-8 text-slate-600">{{ $t('pages.membership.lead') }}</p>
      </div>

      <div class="mt-10 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <UCard v-for="card in cards" :key="card.title" :title="card.title" :description="card.description" variant="subtle" />
      </div>

      <div class="mt-10">
        <UAccordion :items="accordionItems" type="multiple" />
      </div>
    </section>
  </main>
</template>

<script setup lang="ts">
import type { AccordionItem } from '@nuxt/ui'

const { t } = useI18n()

const cards = computed(() => [
  { title: t('pages.membership.card1Title'), description: t('pages.membership.card1Text') },
  { title: t('pages.membership.card2Title'), description: t('pages.membership.card2Text') },
  { title: t('pages.membership.card3Title'), description: t('pages.membership.card3Text') },
  { title: t('pages.membership.card4Title'), description: t('pages.membership.card4Text') },
])

const accordionItems = computed<AccordionItem[]>(() => [
  { label: t('pages.membership.a1Label'), content: t('pages.membership.a1Text') },
  { label: t('pages.membership.a2Label'), content: t('pages.membership.a2Text') },
  { label: t('pages.membership.a3Label'), content: t('pages.membership.a3Text') },
])

useHead({ title: t('pages.membership.title') })
</script>
