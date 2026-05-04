<template>
  <footer class="border-t border-white/10 bg-slate-950/50 backdrop-blur">
    <div class="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
      <div class="grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
        <!-- Brand -->
        <div>
          <div class="flex items-center gap-3 mb-4">
            <span class="grid h-10 w-10 place-items-center rounded-2xl bg-emerald-400 text-sm font-black text-slate-950">
              B
            </span>
            <div>
              <p class="font-semibold text-white">Baytic</p>
              <p class="text-xs text-slate-400">Member Portal</p>
            </div>
          </div>
          <p class="text-xs text-slate-400">Secure authentication and member management platform.</p>
        </div>

        <!-- Links -->
        <div>
          <h3 class="text-sm font-semibold text-white mb-4">Product</h3>
          <ul class="space-y-2 text-xs">
            <li><NuxtLink to="/" class="text-slate-400 hover:text-emerald-300 transition">Home</NuxtLink></li>
            <li><NuxtLink to="/blog" class="text-slate-400 hover:text-emerald-300 transition">Blog</NuxtLink></li>
            <li><NuxtLink to="/dashboard" class="text-slate-400 hover:text-emerald-300 transition">Dashboard</NuxtLink></li>
          </ul>
        </div>

        <!-- Company -->
        <div>
          <h3 class="text-sm font-semibold text-white mb-4">Company</h3>
          <ul class="space-y-2 text-xs">
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">About</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Blog</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Contact</a></li>
          </ul>
        </div>

        <!-- Legal -->
        <div>
          <h3 class="text-sm font-semibold text-white mb-4">Legal</h3>
          <ul class="space-y-2 text-xs">
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Privacy</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Terms</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Cookies</a></li>
          </ul>
        </div>
      </div>

      <!-- Bottom -->
      <div class="mt-8 border-t border-white/10 pt-8 flex items-center justify-between text-xs text-slate-400">
        <p>&copy; 2026 Baytic. All rights reserved.</p>
        <div class="flex gap-4">
          <a href="#" class="hover:text-emerald-300 transition">Twitter</a>
          <a href="#" class="hover:text-emerald-300 transition">GitHub</a>
          <a href="#" class="hover:text-emerald-300 transition">LinkedIn</a>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
// Footer component
// Auto-imported from components
</script>