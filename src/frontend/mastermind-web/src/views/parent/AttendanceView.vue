<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Attendance Records</h1>
        <p class="mt-2 text-sm text-gray-700">
          View your child's attendance history and statistics.
        </p>
      </div>
    </div>

    <div class="mb-6">
      <label for="child-select" class="block text-sm font-medium text-gray-700">Select Child</label>
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

    <div class="mb-8 grid grid-cols-1 gap-5 sm:grid-cols-3">
      <div class="overflow-hidden rounded-lg bg-white shadow">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="truncate text-sm font-medium text-gray-500">Present Days</dt>
                <dd class="text-lg font-medium text-gray-900">{{ attendanceStats.present }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>
      <div class="overflow-hidden rounded-lg bg-white shadow">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="truncate text-sm font-medium text-gray-500">Absent Days</dt>
                <dd class="text-lg font-medium text-gray-900">{{ attendanceStats.absent }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>
      <div class="overflow-hidden rounded-lg bg-white shadow">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="truncate text-sm font-medium text-gray-500">Late Days</dt>
                <dd class="text-lg font-medium text-gray-900">{{ attendanceStats.late }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="overflow-hidden rounded-md bg-white shadow">
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="record in attendanceRecords" :key="`${record.date}-${record.subject}-${record.status}`">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div
                    :class="[
                      record.status.toLowerCase() === 'present' ? 'bg-green-100' :
                      record.status.toLowerCase() === 'absent' ? 'bg-red-100' :
                      'bg-yellow-100'
                    ]"
                    class="flex h-8 w-8 items-center justify-center rounded-full"
                  >
                    <svg
                      :class="[
                        record.status.toLowerCase() === 'present' ? 'text-green-600' :
                        record.status.toLowerCase() === 'absent' ? 'text-red-600' :
                        'text-yellow-600'
                      ]"
                      class="h-4 w-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path v-if="record.status.toLowerCase() === 'present'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                      <path v-else-if="record.status.toLowerCase() === 'absent'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                      <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                  </div>
                </div>
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">{{ record.date }}</div>
                  <div class="text-sm text-gray-500">{{ record.subject }}</div>
                </div>
              </div>
              <span
                :class="[
                  record.status.toLowerCase() === 'present' ? 'bg-green-100 text-green-800' :
                  record.status.toLowerCase() === 'absent' ? 'bg-red-100 text-red-800' :
                  'bg-yellow-100 text-yellow-800'
                ]"
                class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
              >
                {{ record.status }}
              </span>
            </div>
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { parentService, type ChildAttendance, type ParentChild } from '@/services/parentService'

const children = ref<ParentChild[]>([])
const selectedChild = ref<number | null>(null)
const attendanceData = ref<ChildAttendance>({
  totalClasses: 0,
  present: 0,
  absent: 0,
  late: 0,
  percentage: 0,
  records: []
})
const error = ref('')

const attendanceStats = computed(() => ({
  present: attendanceData.value.present,
  absent: attendanceData.value.absent,
  late: attendanceData.value.late
}))

const attendanceRecords = computed(() => attendanceData.value.records || [])

const loadAttendance = async () => {
  if (!selectedChild.value) return
  error.value = ''
  try {
    attendanceData.value = await parentService.getChildAttendance(selectedChild.value)
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to load attendance data'
    attendanceData.value = { totalClasses: 0, present: 0, absent: 0, late: 0, percentage: 0, records: [] }
  }
}

const loadChildren = async () => {
  try {
    children.value = await parentService.getChildren()
    selectedChild.value = children.value.length > 0 ? children.value[0].id : null
    await loadAttendance()
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to load children'
  }
}

watch(selectedChild, async (value, oldValue) => {
  if (value && value !== oldValue) {
    await loadAttendance()
  }
})

onMounted(async () => {
  await loadChildren()
})
</script>
