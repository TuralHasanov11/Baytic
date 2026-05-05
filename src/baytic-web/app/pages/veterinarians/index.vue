<template>
  <main class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <section class="rounded-4xl border border-slate-200/80 bg-white/95 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.14)] sm:p-10">
      <div class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div class="space-y-3">
          <p class="text-sm font-semibold uppercase tracking-[0.28em] text-emerald-700">{{ $t('vets.kicker') }}</p>
          <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">{{ $t('vets.title') }}</h1>
          <p class="max-w-2xl text-base leading-7 text-slate-600">{{ $t('vets.subtitle') }}</p>
        </div>

        <div class="grid gap-3 sm:min-w-[20rem]">
          <label class="text-sm font-medium text-slate-700" for="vet-search">{{ $t('vets.searchLabel') }}</label>
          <input
            id="vet-search"
            v-model="query"
            type="search"
            :placeholder="$t('vets.searchPlaceholder')"
            class="rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-200"
          />
        </div>
      </div>

      <div class="mt-8 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <article
          v-for="vet in filteredVets"
          :key="vet.id"
          class="flex h-full flex-col rounded-[1.75rem] border border-slate-200 bg-slate-50 p-6 shadow-sm transition hover:-translate-y-0.5 hover:shadow-lg"
        >
          <div class="flex items-start justify-between gap-3">
            <div>
              <h2 class="text-xl font-semibold text-slate-950">{{ vet.displayName }}</h2>
              <p class="mt-1 text-sm text-slate-600">{{ vet.location }} · {{ vet.yearsOfExperience }} {{ $t('vets.years') }}</p>
            </div>
            <span
              class="rounded-full px-3 py-1 text-xs font-semibold"
              :class="vet.verificationStatus === 'Verified' ? 'bg-emerald-100 text-emerald-700' : vet.verificationStatus === 'Pending' ? 'bg-amber-100 text-amber-700' : 'bg-rose-100 text-rose-700'"
            >
              {{ vet.verificationStatus }}
            </span>
          </div>

          <p class="mt-4 text-sm leading-6 text-slate-700">{{ vet.biography }}</p>

          <div class="mt-4 flex flex-wrap gap-2">
            <span
              v-for="tag in vet.expertiseTags"
              :key="tag"
              class="rounded-full bg-white px-3 py-1 text-xs font-medium text-slate-600 ring-1 ring-slate-200"
            >
              {{ tag }}
            </span>
          </div>

          <dl class="mt-5 grid gap-3 text-sm text-slate-700">
            <div>
              <dt class="font-medium text-slate-500">{{ $t('vets.credentials') }}</dt>
              <dd class="mt-1 space-y-1">
                <div v-for="credential in vet.credentials" :key="credential.credentialId" class="rounded-2xl bg-white p-3 ring-1 ring-slate-200">
                  <p class="font-medium text-slate-900">{{ credential.credentialName }}</p>
                  <p class="text-xs text-slate-500">{{ credential.issuer }} · {{ credential.issuedOn }}</p>
                </div>
              </dd>
            </div>
            <div>
              <dt class="font-medium text-slate-500">{{ $t('vets.links') }}</dt>
              <dd class="mt-1 space-y-1">
                <a
                  v-for="link in vet.profileLinks"
                  :key="link.linkId"
                  :href="link.url"
                  target="_blank"
                  rel="noreferrer"
                  class="block rounded-2xl bg-white px-3 py-2 text-sm font-medium text-emerald-700 ring-1 ring-slate-200 transition hover:bg-emerald-50"
                >
                  {{ link.label }}
                </a>
              </dd>
            </div>
          </dl>

          <div class="mt-6 flex items-center justify-between border-t border-slate-200 pt-4 text-xs text-slate-500">
            <span>{{ vet.isFeatured ? $t('vets.featured') : $t('vets.profile') }}</span>
            <span>{{ vet.userId }}</span>
          </div>
        </article>
      </div>

      <div v-if="!filteredVets.length" class="mt-8 rounded-2xl border border-dashed border-slate-300 bg-slate-50 p-6 text-center text-sm text-slate-600">
        {{ $t('vets.empty') }}
      </div>
    </section>
  </main>
</template>

<script setup lang="ts">
import type { VeterinarianProfile } from '~/shared/types/Veterinarian'
import { MOCK_VETERINARIAN_PROFILES } from '~/mocks/veterinarians'

const query = ref('')
const vets = ref<VeterinarianProfile[]>(MOCK_VETERINARIAN_PROFILES)

const filteredVets = computed(() => {
  const normalizedQuery = query.value.trim().toLowerCase()
  if (!normalizedQuery) {
    return vets.value
  }

  return vets.value.filter(vet => {
    const searchText = [
      vet.displayName,
      vet.location,
      vet.biography,
      vet.expertiseTags.join(' '),
      vet.credentials.map(credential => credential.credentialName).join(' '),
    ].join(' ').toLowerCase()

    return searchText.includes(normalizedQuery)
  })
})

useHead({ title: 'Veterinarians' })
</script>
