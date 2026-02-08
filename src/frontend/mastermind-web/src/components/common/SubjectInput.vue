<template>
  <div class="subject-input-container">
    <label class="block text-sm font-medium text-gray-700 mb-2">
      {{ label }}
    </label>
    
    <!-- Subject Input with Real-time Tagging -->
    <div class="subject-input-wrapper">
      <!-- Display Subject Tags -->
      <div class="subject-tags flex flex-wrap gap-2 mb-2" v-if="subjects.length > 0">
        <span 
          v-for="(subject, index) in subjects" 
          :key="index"
          class="inline-flex items-center px-3 py-1 text-sm font-medium bg-blue-100 text-blue-800 rounded-full border border-blue-200"
        >
          {{ subject }}
          <button 
            @click="removeSubject(index)"
            type="button"
            class="ml-2 text-blue-600 hover:text-blue-800 focus:outline-none"
          >
            <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
        </span>
      </div>
      
      <!-- Input Field -->
      <div class="relative">
        <input
          ref="subjectInput"
          v-model="currentInput"
          @input="handleInput"
          @keydown="handleKeydown"
          @paste="handlePaste"
          type="text"
          :placeholder="placeholder"
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          :class="{ 'border-red-500': error }"
        />
        
        <!-- Input Indicator -->
        <div 
          v-if="showIndicator"
          class="absolute right-2 top-1/2 transform -translate-y-1/2 text-xs text-gray-500"
        >
          Press comma or Enter to add subject
        </div>
      </div>
      
      <!-- Error Message -->
      <div v-if="error" class="mt-1 text-sm text-red-600">
        {{ error }}
      </div>
      
      <!-- Suggestions Dropdown -->
      <div 
        v-if="suggestions.length > 0 && showSuggestions"
        class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-40 overflow-y-auto"
      >
        <div
          v-for="(suggestion, index) in suggestions"
          :key="index"
          @click="selectSuggestion(suggestion)"
          class="px-3 py-2 text-sm cursor-pointer hover:bg-gray-100"
          :class="{ 'bg-blue-50': index === 0 }"
        >
          {{ suggestion }}
        </div>
      </div>
    </div>
    
    <!-- Help Text -->
    <div class="mt-1 text-xs text-gray-500">
      Type subject name and press comma (,) or Enter to add. Use backspace to remove last subject.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue'

interface Props {
  modelValue: string[]
  label?: string
  placeholder?: string
  suggestions?: string[]
  maxSubjects?: number
}

interface Emits {
  (e: 'update:modelValue', value: string[]): void
  (e: 'subject-added', subject: string): void
  (e: 'subject-removed', subject: string): void
}

const props = withDefaults(defineProps<Props>(), {
  label: 'Subjects',
  placeholder: 'e.g., Mathematics, Science, etc.',
  suggestions: () => [],
  maxSubjects: 10
})

const emit = defineEmits<Emits>()

// Reactive data
const subjects = ref<string[]>([])
const currentInput = ref('')
const error = ref('')
const showSuggestions = ref(false)
const showIndicator = ref(false)

const subjectInput = ref<HTMLInputElement>()

// Computed properties
const filteredSuggestions = computed(() => {
  if (!currentInput.value.trim()) return []
  return props.suggestions.filter(suggestion => 
    suggestion.toLowerCase().includes(currentInput.value.toLowerCase()) &&
    !subjects.value.includes(suggestion)
  )
})

// Watch for external changes
watch(() => props.modelValue, (newValue) => {
  subjects.value = [...newValue]
}, { immediate: true })

// Methods
const handleInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  currentInput.value = target.value
  
  // Show indicator when typing
  showIndicator.value = currentInput.value.trim().length > 0
  
  // Show suggestions if available
  showSuggestions.value = filteredSuggestions.value.length > 0
  
  // Clear error
  if (error.value) {
    error.value = ''
  }
}

const handleKeydown = (event: KeyboardEvent) => {
  const target = event.target as HTMLInputElement
  
  switch (event.key) {
    case ',':
    case 'Enter':
      event.preventDefault()
      addCurrentSubject()
      break
      
    case 'Backspace':
      if (currentInput.value === '' && subjects.value.length > 0) {
        event.preventDefault()
        removeSubject(subjects.value.length - 1)
      }
      break
      
    case 'Escape':
      showSuggestions.value = false
      break
      
    case 'ArrowDown':
      if (showSuggestions.value) {
        event.preventDefault()
        // Navigate suggestions (implementation needed)
      }
      break
  }
}

const handlePaste = (event: ClipboardEvent) => {
  event.preventDefault()
  const pastedText = event.clipboardData?.getData('text') || ''
  
  // Split by comma and add as subjects
  const pastedSubjects = pastedText
    .split(',')
    .map(s => s.trim())
    .filter(s => s.length > 0)
  
  pastedSubjects.forEach(subject => {
    if (canAddSubject(subject)) {
      addSubject(subject)
    }
  })
}

const addCurrentSubject = () => {
  const subject = currentInput.value.trim()
  if (subject && canAddSubject(subject)) {
    addSubject(subject)
    currentInput.value = ''
    showIndicator.value = false
    showSuggestions.value = false
  }
}

const addSubject = (subject: string) => {
  if (!canAddSubject(subject)) return
  
  subjects.value.push(subject)
  emit('update:modelValue', subjects.value)
  emit('subject-added', subject)
}

const canAddSubject = (subject: string): boolean => {
  // Check if subject already exists
  if (subjects.value.includes(subject)) {
    error.value = `"${subject}" is already added`
    return false
  }
  
  // Check max subjects limit
  if (subjects.value.length >= props.maxSubjects) {
    error.value = `Maximum ${props.maxSubjects} subjects allowed`
    return false
  }
  
  // Check minimum length
  if (subject.length < 2) {
    error.value = 'Subject name must be at least 2 characters'
    return false
  }
  
  return true
}

const removeSubject = (index: number) => {
  const removedSubject = subjects.value[index]
  subjects.value.splice(index, 1)
  emit('update:modelValue', subjects.value)
  emit('subject-removed', removedSubject)
}

const selectSuggestion = (suggestion: string) => {
  currentInput.value = suggestion
  showSuggestions.value = false
  nextTick(() => {
    subjectInput.value?.focus()
  })
}

// Expose methods for parent component
defineExpose({
  addSubject,
  removeSubject,
  clearAll: () => {
    subjects.value = []
    currentInput.value = ''
    emit('update:modelValue', subjects.value)
  }
})
</script>

<style scoped>
.subject-input-container {
  position: relative;
}

.subject-input-wrapper {
  position: relative;
}

.subject-tags {
  min-height: 32px;
}

/* Custom scrollbar for suggestions */
.overflow-y-auto::-webkit-scrollbar {
  width: 4px;
}

.overflow-y-auto::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 2px;
}

.overflow-y-auto::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 2px;
}

.overflow-y-auto::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>
