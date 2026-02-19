<template>
  <div class="space-y-6">
    <h2 class="text-lg font-semibold text-gray-900">Financial Reports</h2>

    <!-- Report Generation Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Monthly Report -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-md font-semibold text-gray-900 mb-4">Monthly Report</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Select Month</label>
            <input v-model="reportFilters.month" type="month" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
          </div>
          <button
            @click="generateMonthlyReport"
            :disabled="loading || !reportFilters.month"
            class="w-full inline-flex justify-center items-center gap-2 px-4 py-2.5 text-sm font-medium text-white bg-indigo-600 rounded-lg shadow-sm hover:bg-indigo-700 disabled:opacity-50 transition-colors"
          >
            <svg v-if="loading" class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Generate Monthly Report
          </button>
        </div>
      </div>

      <!-- Custom Date Range -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-md font-semibold text-gray-900 mb-4">Custom Date Range</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
            <input v-model="reportFilters.startDate" type="date" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
            <input v-model="reportFilters.endDate" type="date" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
          </div>
          <button
            @click="generateCustomReport"
            :disabled="loading || !reportFilters.startDate || !reportFilters.endDate"
            class="w-full inline-flex justify-center items-center gap-2 px-4 py-2.5 text-sm font-medium text-white bg-indigo-600 rounded-lg shadow-sm hover:bg-indigo-700 disabled:opacity-50 transition-colors"
          >
            Generate Custom Report
          </button>
        </div>
      </div>
    </div>

    <!-- Recent Reports -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="px-6 py-4 border-b border-gray-100">
        <h3 class="text-md font-semibold text-gray-900">Recent Reports</h3>
      </div>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="py-3 pl-6 pr-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Report Type</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Period</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Generated On</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 bg-white">
            <tr v-for="report in recentReports" :key="report.id" class="hover:bg-gray-50 transition-colors">
              <td class="whitespace-nowrap py-4 pl-6 pr-3 text-sm font-medium text-gray-900">{{ report.type }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ report.period }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(report.generatedAt) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <button @click="downloadReport(report)" class="text-indigo-600 hover:text-indigo-900 font-medium">Download</button>
              </td>
            </tr>
            <tr v-if="recentReports.length === 0">
              <td colspan="4" class="px-6 py-12 text-center text-sm text-gray-500">
                No reports generated yet.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { financeService } from '@/services/financeService'
import { useToast } from '@/composables/useToast'

const toast = useToast()

interface Report {
  id: number
  type: string
  period: string
  generatedAt: string
  data: Record<string, unknown>
}

const loading = ref(false)
const recentReports = ref<Report[]>([])
const reportFilters = ref({ month: '', startDate: '', endDate: '' })

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const loadRecentReports = async () => {
  try {
    recentReports.value = await financeService.getReports()
  } catch (error) {
    console.error('Error loading reports:', error)
    recentReports.value = []
  }
}

const downloadReportBlob = (data: Record<string, unknown>, filename: string) => {
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  window.URL.revokeObjectURL(url)
}

const generateMonthlyReport = async () => {
  if (!reportFilters.value.month) return
  loading.value = true
  try {
    const [year, month] = reportFilters.value.month.split('-')
    const startDate = `${year}-${month}-01`
    const endDate = new Date(parseInt(year), parseInt(month), 0).toISOString().split('T')[0]
    const report = await financeService.generateReport(startDate, endDate)
    downloadReportBlob(report, `monthly-report-${reportFilters.value.month}.json`)
    await loadRecentReports()
    toast.success('Report downloaded', 'Monthly report generated and downloaded successfully.')
  } catch (error) {
    console.error('Error generating monthly report:', error)
    toast.error('Report generation failed', 'Please try again.')
  } finally {
    loading.value = false
  }
}

const generateCustomReport = async () => {
  if (!reportFilters.value.startDate || !reportFilters.value.endDate) return
  loading.value = true
  try {
    const report = await financeService.generateReport(reportFilters.value.startDate, reportFilters.value.endDate)
    downloadReportBlob(report, `custom-report-${reportFilters.value.startDate}-to-${reportFilters.value.endDate}.json`)
    await loadRecentReports()
    toast.success('Report downloaded', 'Custom report generated and downloaded successfully.')
  } catch (error) {
    console.error('Error generating custom report:', error)
    toast.error('Report generation failed', 'Please try again.')
  } finally {
    loading.value = false
  }
}

const downloadReport = (report: Report) => {
  downloadReportBlob(report.data, `${report.type.toLowerCase().replace(' ', '-')}-${report.period}.json`)
}

onMounted(() => { loadRecentReports() })
</script>
