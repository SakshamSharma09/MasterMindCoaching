import { apiService } from './apiService'

// Environment variable to control mock API usage
const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true'

// Interfaces
export interface FinancialSummary {
  totalRevenue: number
  pendingPayments: number
  expenses: number
  netProfit: number
  totalStudents: number
  paidStudents: number
  pendingStudents: number
  overdueStudents: number
}

export interface Payment {
  id: number
  studentId: number
  studentName: string
  amount: number
  paymentMethod: string
  date: string
  status: 'Completed' | 'Pending' | 'Failed'
  description?: string
}

export interface Fee {
  id: number
  studentId: number
  studentName: string
  classId: number
  className: string
  feeType: string
  amount: number
  dueDate: string
  status: 'Paid' | 'Pending' | 'Overdue'
  description?: string
}

export interface Expense {
  id: number
  category: string
  description: string
  amount: number
  paidTo: string
  date: string
  receiptNumber?: string
}

export interface CreateExpenseDto {
  category: string
  description: string
  amount: number
  paidTo: string
  date: string
  receiptNumber?: string
}

export const financeService = {
  // ===== FINANCIAL SUMMARY METHODS =====

  // Get financial summary
  async getFinancialSummary(): Promise<FinancialSummary> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        totalRevenue: 245000,
        pendingPayments: 45000,
        expenses: 85000,
        netProfit: 160000,
        totalStudents: 150,
        paidStudents: 120,
        pendingStudents: 25,
        overdueStudents: 5
      }
    }

    const response = await apiService.get('/finance/summary')
    return response.data
  },

  // ===== PAYMENT METHODS =====

  // Get recent payments
  async getRecentPayments(limit: number = 10): Promise<Payment[]> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return [
        {
          id: 1,
          studentId: 1,
          studentName: 'John Doe',
          amount: 5000,
          paymentMethod: 'Cash',
          date: '2024-01-15',
          status: 'Completed',
          description: 'Monthly tuition fee'
        },
        {
          id: 2,
          studentId: 2,
          studentName: 'Jane Smith',
          amount: 4500,
          paymentMethod: 'Online',
          date: '2024-01-14',
          status: 'Completed',
          description: 'Monthly tuition fee'
        }
      ]
    }

    const response = await apiService.get(`/finance/payments?limit=${limit}`)
    return response.data
  },

  // Get pending payments
  async getPendingPayments(): Promise<Payment[]> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return [
        {
          id: 3,
          studentId: 3,
          studentName: 'Bob Johnson',
          amount: 4000,
          paymentMethod: 'Pending',
          date: '2024-01-20',
          status: 'Pending',
          description: 'Monthly tuition fee'
        }
      ]
    }

    const response = await apiService.get('/finance/payments/pending')
    return response.data
  },

  // ===== FEE METHODS =====

  // Create a new fee with enhanced fee type support
  async createFee(data: any): Promise<any> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      return {
        success: true,
        message: 'Fee created successfully',
        data: {
          id: Date.now(),
          studentId: data.studentId,
          feeCategory: data.feeCategory,
          amount: data.amount,
          status: 'Created'
        }
      }
    }

    const response = await apiService.post('/finance/fees', data)
    return response.data
  },

  // Get all fees
  async getFees(): Promise<Fee[]> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const response = await apiService.get('/finance/fees')
    return response.data || []
  },

  // Get overdue fees
  async getOverdueFees(): Promise<Fee[]> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return [
        {
          id: 2,
          studentId: 2,
          studentName: 'Jane Smith',
          classId: 2,
          className: 'Class 9',
          feeType: 'Tuition Fee',
          amount: 4500,
          dueDate: '2024-01-10',
          status: 'Overdue',
          description: 'Monthly tuition fee'
        }
      ]
    }

    const response = await apiService.get('/finance/fees/overdue')
    return response.data || []
  },

  // Send reminders for overdue fees
  async sendReminders(feeIds?: number[]): Promise<boolean> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      return true
    }

    const response = await apiService.post('/finance/fees/reminders', { feeIds })
    return response.data
  },

  // Mark fee as paid
  async markFeeAsPaid(feeId: number): Promise<Fee> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: feeId,
        studentId: 2,
        studentName: 'Jane Smith',
        classId: 2,
        className: 'Class 9',
        feeType: 'Tuition Fee',
        amount: 4500,
        dueDate: '2024-01-10',
        status: 'Paid',
        description: 'Monthly tuition fee'
      }
    }

    const response = await apiService.post(`/finance/fees/${feeId}/mark-paid`)
    return response.data
  },

  // ===== EXPENSE METHODS =====

  // Get expenses
  async getExpenses(filters?: {
    category?: string
    startDate?: string
    endDate?: string
  }): Promise<Expense[]> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return [
        {
          id: 1,
          category: 'Salary',
          description: 'Teacher salaries',
          amount: 50000,
          paidTo: 'Various Teachers',
          date: '2024-01-01',
          receiptNumber: 'REC-001'
        },
        {
          id: 2,
          category: 'Rent',
          description: 'Monthly rent',
          amount: 20000,
          paidTo: 'Landlord',
          date: '2024-01-05',
          receiptNumber: 'REC-002'
        }
      ]
    }

    const params = new URLSearchParams()
    if (filters?.category) params.append('category', filters.category)
    if (filters?.startDate) params.append('startDate', filters.startDate)
    if (filters?.endDate) params.append('endDate', filters.endDate)

    const response = await apiService.get(`/finance/expenses?${params.toString()}`)
    return response.data || []
  },

  // Create expense
  async createExpense(data: CreateExpenseDto): Promise<Expense> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        category: data.category,
        description: data.description,
        amount: data.amount,
        paidTo: data.paidTo,
        date: data.date,
        receiptNumber: data.receiptNumber
      }
    }

    const response = await apiService.post('/finance/expenses', data)
    return response.data
  },

  // ===== REPORT METHODS =====

  // Generate financial report
  async generateReport(startDate: string, endDate: string): Promise<any> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      return {
        reportId: Date.now(),
        generatedAt: new Date().toISOString(),
        period: { startDate, endDate },
        data: {
          totalRevenue: 245000,
          totalExpenses: 85000,
          netProfit: 160000,
          payments: [],
          expenses: [],
          fees: [],
          summary: {
            totalStudents: 150,
            paidStudents: 120,
            pendingStudents: 25,
            overdueStudents: 5
          }
        }
      }
    }

    const response = await apiService.post('/finance/reports/generate', {
      startDate,
      endDate
    })
    return response.data
  },

  // Get reports
  async getReports(): Promise<any[]> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return [
        {
          id: 1,
          type: 'Monthly Report',
          period: 'January 2024',
          generatedAt: new Date().toISOString(),
          data: {}
        }
      ]
    }

    const response = await apiService.get('/finance/reports')
    return response.data || []
  },

  // ===== FEE COLLECTION METHODS =====

  // Setup student fee (Monthly or Full Course)
  async setupStudentFee(data: any): Promise<any> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      return {
        studentId: data.studentId,
        studentName: 'John Doe',
        feeType: data.feeType,
        totalAmount: data.totalAmount,
        monthlyAmount: data.monthlyAmount,
        numberOfInstallments: data.numberOfInstallments,
        startDate: data.startDate,
        endDate: data.endDate,
        status: 'Active'
      }
    }

    const response = await apiService.post('/finance/fees/setup', data)
    return response.data
  },

  // Collect payment
  async collectPayment(data: any): Promise<any> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        receiptNumber: `RCP${Date.now()}`,
        studentName: 'John Doe',
        studentClass: 'Class 10',
        totalAmount: data.amount,
        paidAmount: data.amount,
        balanceAmount: 0,
        paymentMethod: data.paymentMethod,
        receiptDate: new Date().toISOString(),
        paymentStatus: 'Completed',
        collectedBy: 'Admin',
        remarks: data.remarks || '',
        receiptItems: [
          {
            feeType: data.feeType,
            amount: data.amount,
            dueDate: data.dueDate,
            status: 'Paid'
          }
        ]
      }
    }

    const response = await apiService.post('/finance/payments/collect', data)
    return response.data
  },

  // Get receipt by ID
  async getReceipt(receiptId: number): Promise<any> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: receiptId,
        receiptNumber: `RCP${receiptId}`,
        studentName: 'John Doe',
        studentClass: 'Class 10',
        totalAmount: 5000,
        paidAmount: 5000,
        balanceAmount: 0,
        paymentMethod: 'Cash',
        receiptDate: new Date().toISOString(),
        paymentStatus: 'Completed',
        collectedBy: 'Admin',
        remarks: 'Payment completed successfully',
        receiptItems: []
      }
    }

    const response = await apiService.get(`/finance/payments/receipt/${receiptId}`)
    return response.data
  },

  // Send receipt via email
  async sendReceiptEmail(receiptId: number, email: string): Promise<boolean> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      return true
    }

    const response = await apiService.post(`/finance/payments/receipt/${receiptId}/send-email`, { email })
    return response.data
  },

  // Get student fee details
  async getStudentFeeDetails(studentId: number): Promise<any> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        studentId: studentId,
        studentName: 'John Doe',
        studentClass: 'Class 10',
        parentName: 'Parent Name',
        parentEmail: 'parent@example.com',
        parentMobile: '+91 9876543210',
        pendingFees: [
          {
            studentFeeId: 1,
            feeType: 'Tuition Fee',
            amount: 5000,
            dueDate: '2024-02-01',
            status: 'Pending',
            isOverdue: false
          }
        ]
      }
    }

    const response = await apiService.get(`/finance/students/${studentId}/fees`)
    return response.data
  },

  // Get fee structures
  async getFeeStructures(): Promise<any[]> {
    if (USE_MOCK_API) {
      await new Promise(resolve => setTimeout(resolve, 500))
      return [
        {
          id: 1,
          name: 'Class 10 - Monthly Tuition',
          type: 'Monthly',
          category: 'Tuition',
          amount: 5000,
          frequency: 'Monthly',
          className: 'Class 10',
          description: 'Monthly tuition fee for Class 10',
          academicYear: '2024-25'
        }
      ]
    }

    const response = await apiService.get('/fees/structures')
    return response.data
  }
}
