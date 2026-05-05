<template>
  <main class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <section class="rounded-4xl border border-slate-200/80 bg-white/95 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.14)] sm:p-10">
      <div class="max-w-3xl space-y-4">
        <UChip color="emerald" variant="soft" size="sm">{{ $t('pages.history.kicker') }}</UChip>
        <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-5xl">{{ $t('pages.history.title') }}</h1>
        <p class="text-lg leading-8 text-slate-600">{{ $t('pages.history.lead') }}</p>
      </div>

      <div class="mt-10 grid gap-4 lg:grid-cols-4">
        <UCard v-for="milestone in milestones" :key="milestone.year" :title="milestone.year" :description="milestone.title" variant="subtle">
          <p class="text-sm leading-6 text-slate-700">{{ milestone.text }}</p>
        </UCard>
      </div>

      <div class="mt-10 grid gap-6 lg:grid-cols-[0.95fr_1.05fr]">
        <UCard :title="$t('pages.history.rightTitle')" variant="outline">
          <ul class="space-y-3 text-sm leading-6 text-slate-700">
            <li v-for="item in values" :key="item">{{ item }}</li>
          </ul>
        </UCard>

        <UCard :title="$t('pages.history.futureTitle')" :description="$t('pages.history.futureText')" variant="subtle" />
      </div>
    </section>
  </main>
</template>

<script setup lang="ts">
const { t } = useI18n()

const milestones = computed(() => [
  { year: t('pages.history.m1Year'), title: t('pages.history.m1Title'), text: t('pages.history.m1Text') },
  { year: t('pages.history.m2Year'), title: t('pages.history.m2Title'), text: t('pages.history.m2Text') },
  { year: t('pages.history.m3Year'), title: t('pages.history.m3Title'), text: t('pages.history.m3Text') },
  { year: t('pages.history.m4Year'), title: t('pages.history.m4Title'), text: t('pages.history.m4Text') },
])

const values = computed(() => [
  t('pages.history.value1'),
  t('pages.history.value2'),
  t('pages.history.value3'),
  t('pages.history.value4'),
])

useHead({ title: t('pages.history.title') })
</script>
