<template>
  <div class="px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900">Academic Performance</h1>
      <p class="mt-2 text-sm text-gray-600">
        View your child's performance and teacher remarks.
      </p>
    </div>

    <div class="mb-6">
      <label for="child-select" class="block text-sm font-semibold text-gray-700">Select Child</label>
      <select
        id="child-select"
        v-model.number="selectedChild"
        class="mt-1 block w-full rounded-md border-gray-300 py-2 pl-3 pr-10 text-base focus:border-blue-500 focus:outline-none focus:ring-blue-500 sm:text-sm"
      >
        <option v-for="child in children" :key="child.id" :value="child.id">
          {{ child.firstName }} {{ child.lastName }} - {{ child.className }}
        </option>
      </select>
    </div>

    <div v-if="error" class="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
      {{ error }}
    </div>

    <div class="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <div class="rounded-lg bg-white p-4 shadow">
        <div class="text-sm text-gray-500">Overall Grade</div>
        <div class="mt-1 text-2xl font-bold text-gray-900">{{ performanceStats.averageGrade }}</div>
      </div>
      <div class="rounded-lg bg-white p-4 shadow">
        <div class="text-sm text-gray-500">Average Score</div>
        <div class="mt-1 text-2xl font-bold text-gray-900">{{ performanceStats.overallPercentage }}%</div>
      </div>
      <div class="rounded-lg bg-white p-4 shadow">
        <div class="text-sm text-gray-500">Recent Tests</div>
        <div class="mt-1 text-2xl font-bold text-gray-900">{{ performanceStats.recentTests.length }}</div>
      </div>
      <div class="rounded-lg bg-white p-4 shadow">
        <div class="text-sm text-gray-500">Teacher Remarks</div>
        <div class="mt-1 text-2xl font-bold text-gray-900">{{ performanceStats.recentRemarks.length }}</div>
      </div>
    </div>

    <div class="mb-8 rounded-lg bg-white shadow">
      <div class="border-b px-4 py-3">
        <h3 class="text-lg font-semibold text-gray-900">Subject-wise Performance</h3>
      </div>
      <div class="divide-y">
        <div v-for="subject in performanceStats.subjectPerformance" :key="subject.subject" class="flex items-center justify-between px-4 py-3">
          <div>
            <div class="font-medium text-gray-900">{{ subject.subject }}</div>
            <div class="text-sm text-gray-500">Grade {{ subject.grade }}</div>
          </div>
          <div class="text-sm font-semibold text-gray-900">{{ subject.percentage }}%</div>
        </div>
      </div>
    </div>

    <div class="mb-8 rounded-lg bg-white shadow">
      <div class="border-b px-4 py-3">
        <h3 class="text-lg font-semibold text-gray-900">Recent Tests</h3>
      </div>
      <div class="divide-y">
        <div v-for="test in performanceStats.recentTests" :key="`${test.date}-${test.subject}-${test.topic}`" class="flex items-center justify-between px-4 py-3">
          <div>
            <div class="font-medium text-gray-900">{{ test.subject }} - {{ test.topic }}</div>
            <div class="text-sm text-gray-500">{{ test.date }}</div>
          </div>
          <div class="text-sm font-semibold text-gray-900">{{ test.score }}/{{ test.totalMarks }}</div>
        </div>
      </div>
    </div>

    <div class="rounded-lg bg-white shadow">
      <div class="border-b px-4 py-3">
        <h3 class="text-lg font-semibold text-gray-900">Teacher Remarks</h3>
      </div>
      <div v-if="performanceStats.recentRemarks.length === 0" class="px-4 py-6 text-sm text-gray-500">
        No teacher remarks available for this child.
      </div>
      <div v-else class="divide-y">
        <div v-for="remark in performanceStats.recentRemarks" :key="remark.id" class="px-4 py-3">
          <div class="flex items-center justify-between">
            <div class="text-sm font-semibold text-gray-900">{{ remark.type }}</div>
            <div class="text-xs text-gray-500">{{ remark.date }}</div>
          </div>
          <div class="mt-1 text-sm text-gray-700">{{ remark.content }}</div>
          <div class="mt-1 text-xs text-gray-500">
            <span v-if="remark.subject">Subject: {{ remark.subject }}</span>
            <span v-if="remark.chapterName" class="ml-3">Chapter: {{ remark.chapterName }}</span>
            <span v-if="remark.teacherName" class="ml-3">Teacher: {{ remark.teacherName }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { parentService, type ChildPerformance, type ParentChild } from '@/services/parentService'

const children = ref<ParentChild[]>([])
const selectedChild = ref<number | null>(null)
const performanceData = ref<ChildPerformance>({
  averageGrade: 'N/A',
  overallPercentage: 0,
  subjectPerformance: [],
  recentTests: [],
  recentRemarks: []
})
const error = ref('')

const performanceStats = computed(() => performanceData.value)

const loadPerformance = async () => {
  if (!selectedChild.value) return
  error.value = ''
  try {
    performanceData.value = await parentService.getChildPerformance(selectedChild.value)
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to load performance data'
    performanceData.value = {
      averageGrade: 'N/A',
      overallPercentage: 0,
      subjectPerformance: [],
      recentTests: [],
      recentRemarks: []
    }
  }
}

const loadChildren = async () => {
  try {
    children.value = await parentService.getChildren()
    selectedChild.value = children.value.length > 0 ? children.value[0].id : null
    await loadPerformance()
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to load children'
  }
}

watch(selectedChild, async (value, oldValue) => {
  if (value && value !== oldValue) {
    await loadPerformance()
  }
})

onMounted(async () => {
  await loadChildren()
})
</script>
