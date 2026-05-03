<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <h1 class="text-2xl font-bold text-surface-900">Notes Tracker</h1>
      <button class="btn-premium px-4 py-2" @click="openNew">New Note</button>
    </div>

    <div class="grid gap-4">
      <div v-for="n in notes" :key="n.id" class="card-premium">
        <div class="flex items-start justify-between gap-4">
          <div>
            <h3 class="text-lg font-semibold text-surface-900">{{ n.title }}</h3>
            <p class="text-xs text-surface-500 mt-1">{{ formatDate(n.noteDate) }}</p>
            <p class="text-surface-700 mt-3 whitespace-pre-wrap">{{ n.content }}</p>
          </div>
          <div class="flex items-center gap-2">
            <button class="btn-ghost p-2" @click="edit(n)">Edit</button>
            <button class="btn-ghost p-2 text-error-600" @click="remove(n.id)">Delete</button>
          </div>
        </div>
      </div>
      <div v-if="notes.length === 0" class="card-premium text-surface-500">No notes yet.</div>
    </div>

    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" role="dialog" aria-modal="true" aria-label="Notes editor">
      <form class="bg-white rounded-xl shadow-xl w-full max-w-2xl p-6" @submit.prevent="save">
        <h3 class="text-lg font-semibold mb-4">{{ editingId ? 'Edit Note' : 'New Note' }}</h3>
        <div class="space-y-3">
          <input v-model="form.title" placeholder="Title" class="w-full rounded-lg border-gray-300 text-sm" />
          <input v-model="form.noteDate" type="date" class="w-full rounded-lg border-gray-300 text-sm" />
          <textarea v-model="form.content" rows="6" placeholder="Write your note..." class="w-full rounded-lg border-gray-300 text-sm"></textarea>
          <p v-if="errorMessage" class="text-sm text-red-600">{{ errorMessage }}</p>
        </div>
        <div class="mt-4 flex justify-end gap-2">
          <button type="button" class="px-3 py-2 text-sm bg-gray-100 rounded-lg" @click="closeModal">Cancel</button>
          <button type="submit" :disabled="isSaving" class="px-3 py-2 text-sm text-white bg-indigo-600 rounded-lg disabled:opacity-60 disabled:cursor-not-allowed">
            {{ isSaving ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { adminNotesService, type AdminNote } from '@/services/adminNotesService'

const notes = ref<AdminNote[]>([])
const showModal = ref(false)
const editingId = ref<number | null>(null)
const form = ref({ title: '', content: '', noteDate: new Date().toISOString().slice(0, 10) })
const isSaving = ref(false)
const errorMessage = ref('')

const refresh = async () => {
  notes.value = await adminNotesService.getAll()
}

const openNew = () => {
  editingId.value = null
  form.value = { title: '', content: '', noteDate: new Date().toISOString().slice(0, 10) }
  errorMessage.value = ''
  showModal.value = true
}

const edit = (n: AdminNote) => {
  editingId.value = n.id
  form.value = { title: n.title, content: n.content, noteDate: n.noteDate?.slice(0, 10) || new Date().toISOString().slice(0, 10) }
  errorMessage.value = ''
  showModal.value = true
}

const save = async () => {
  errorMessage.value = ''
  if (!form.value.title.trim() || !form.value.content.trim()) {
    errorMessage.value = 'Title and content are required.'
    return
  }

  isSaving.value = true
  try {
    if (editingId.value) {
      await adminNotesService.update(editingId.value, form.value)
    } else {
      await adminNotesService.create(form.value)
    }
    showModal.value = false
    await refresh()
  } catch (error: any) {
    errorMessage.value = error?.response?.data?.message || 'Unable to save note. Please try again.'
  } finally {
    isSaving.value = false
  }
}

const remove = async (id: number) => {
  await adminNotesService.remove(id)
  await refresh()
}

const closeModal = () => {
  showModal.value = false
  errorMessage.value = ''
}
const formatDate = (d: string) => new Date(d).toLocaleDateString()

onMounted(refresh)
</script>
