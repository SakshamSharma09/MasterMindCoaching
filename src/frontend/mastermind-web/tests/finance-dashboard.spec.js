// 🧪 Finance Dashboard - Automated Test Suite
// Senior Automation Tester - Comprehensive Test Coverage

describe('Finance Dashboard - Full Test Suite', () => {
  beforeEach(() => {
    // Login as admin user
    cy.visit('/login')
    cy.get('[data-testid="email-input"]').type('admin@mastermind.com')
    cy.get('[data-testid="otp-input"]').type('123456')
    cy.get('[data-testid="login-button"]').click()
    
    // Navigate to Finance Dashboard
    cy.visit('/admin/finance')
    cy.url().should('include', '/admin/finance')
  })

  // 🎯 HEADER AND NAVIGATION TESTS
  describe('Header and Navigation', () => {
    it('should display finance management header correctly', () => {
      cy.get('[data-testid="finance-header"]').should('contain', 'Finance Management')
      cy.get('[data-testid="finance-description"]').should('contain', 'Manage fees, payments, expenses, and financial reports')
    })

    it('should display all action buttons', () => {
      cy.get('[data-testid="add-fee-button"]').should('be.visible').and('contain', 'Add Fee')
      cy.get('[data-testid="add-expense-button"]').should('be.visible').and('contain', 'Add Expense')
      cy.get('[data-testid="generate-report-button"]').should('be.visible').and('contain', 'Generate Report')
    })

    it('should display all navigation tabs', () => {
      cy.get('[data-testid="overview-tab"]').should('be.visible').and('contain', 'Overview')
      cy.get('[data-testid="fees-tab"]').should('be.visible').and('contain', 'Fees Management')
      cy.get('[data-testid="fee-collection-tab"]').should('be.visible').and('contain', 'Fee Collection')
      cy.get('[data-testid="expenses-tab"]').should('be.visible').and('contain', 'Expenses')
      cy.get('[data-testid="overdue-tab"]').should('be.visible').and('contain', 'Overdue Fees')
      cy.get('[data-testid="reports-tab"]').should('be.visible').and('contain', 'Reports')
    })
  })

  // 📊 OVERVIEW TAB TESTS
  describe('Overview Tab', () => {
    beforeEach(() => {
      cy.get('[data-testid="overview-tab"]').click()
    })

    it('should display financial summary cards', () => {
      cy.get('[data-testid="total-revenue-card"]').should('be.visible')
      cy.get('[data-testid="pending-payments-card"]').should('be.visible')
      cy.get('[data-testid="expenses-card"]').should('be.visible')
      cy.get('[data-testid="net-profit-card"]').should('be.visible')
    })

    it('should display student statistics', () => {
      cy.get('[data-testid="total-students-card"]').should('be.visible')
      cy.get('[data-testid="paid-students-card"]').should('be.visible')
      cy.get('[data-testid="overdue-students-card"]').should('be.visible')
    })

    it('should display recent transactions table', () => {
      cy.get('[data-testid="recent-transactions-table"]').should('be.visible')
      cy.get('[data-testid="transaction-header"]').should('contain', 'Recent Transactions')
    })

    it('should format currency correctly', () => {
      cy.get('[data-testid="currency-amount"]').each(($el) => {
        const text = $el.text()
        expect(text).to.match(/₹\d{1,3}(,\d{3})*(\.\d{2})?/)
      })
    })
  })

  // 💰 FEES MANAGEMENT TAB TESTS
  describe('Fees Management Tab', () => {
    beforeEach(() => {
      cy.get('[data-testid="fees-tab"]').click()
    })

    it('should display fees table with correct columns', () => {
      cy.get('[data-testid="fees-table"]').should('be.visible')
      cy.get('[data-testid="fees-table-header"]').should('contain', 'Student')
      cy.get('[data-testid="fees-table-header"]').should('contain', 'Class')
      cy.get('[data-testid="fees-table-header"]').should('contain', 'Fee Type')
      cy.get('[data-testid="fees-table-header"]').should('contain', 'Amount')
      cy.get('[data-testid="fees-table-header"]').should('contain', 'Due Date')
      cy.get('[data-testid="fees-table-header"]').should('contain', 'Status')
    })

    it('should open add fee modal when button clicked', () => {
      cy.get('[data-testid="add-fee-button"]').click()
      cy.get('[data-testid="add-fee-modal"]').should('be.visible')
      cy.get('[data-testid="fee-form"]').should('be.visible')
    })

    it('should validate fee form inputs', () => {
      cy.get('[data-testid="add-fee-button"]').click()
      cy.get('[data-testid="submit-fee"]').click()
      cy.get('[data-testid="validation-error"]').should('be.visible')
    })
  })

  // 🎯 FEE COLLECTION TAB TESTS (NEW FEATURE)
  describe('Fee Collection Tab', () => {
    beforeEach(() => {
      cy.get('[data-testid="fee-collection-tab"]').click()
    })

    it('should display fee collection interface', () => {
      cy.get('[data-testid="fee-collection-view"]').should('be.visible')
      cy.get('[data-testid="student-search"]').should('be.visible')
      cy.get('[data-testid="setup-student-fee-button"]').should('be.visible')
    })

    it('should search students by name', () => {
      cy.get('[data-testid="student-search"]').type('John')
      cy.get('[data-testid="student-list"]').should('contain', 'John Doe')
    })

    it('should display student details when selected', () => {
      cy.get('[data-testid="student-search"]').type('John')
      cy.get('[data-testid="student-item"]').first().click()
      cy.get('[data-testid="student-details"]').should('be.visible')
      cy.get('[data-testid="student-name"]').should('contain', 'John Doe')
    })

    it('should display pending fees for selected student', () => {
      cy.get('[data-testid="student-search"]').type('John')
      cy.get('[data-testid="student-item"]').first().click()
      cy.get('[data-testid="pending-fees"]').should('be.visible')
    })

    it('should open fee setup modal', () => {
      cy.get('[data-testid="setup-student-fee-button"]').click()
      cy.get('[data-testid="fee-setup-modal"]').should('be.visible')
      cy.get('[data-testid="fee-type-monthly"]').should('be.visible')
      cy.get('[data-testid="fee-type-fullcourse"]').should('be.visible')
    })

    it('should collect payment and generate receipt', () => {
      cy.get('[data-testid="student-search"]').type('John')
      cy.get('[data-testid="student-item"]').first().click()
      
      // Select pending fees
      cy.get('[data-testid="fee-checkbox"]').first().check()
      
      // Fill payment form
      cy.get('[data-testid="payment-method"]').select('Cash')
      cy.get('[data-testid="collect-payment-button"]').click()
      
      // Verify receipt generation
      cy.get('[data-testid="receipt-modal"]').should('be.visible')
      cy.get('[data-testid="receipt-number"]').should('be.visible')
    })

    it('should send receipt email', () => {
      // Setup payment first
      cy.get('[data-testid="student-search"]').type('John')
      cy.get('[data-testid="student-item"]').first().click()
      cy.get('[data-testid="fee-checkbox"]').first().check()
      cy.get('[data-testid="payment-method"]').select('Cash')
      cy.get('[data-testid="collect-payment-button"]').click()
      
      // Send email
      cy.get('[data-testid="send-email-button"]').click()
      cy.get('[data-testid="email-success-message"]').should('be.visible')
    })
  })

  // 💸 EXPENSES TAB TESTS
  describe('Expenses Tab', () => {
    beforeEach(() => {
      cy.get('[data-testid="expenses-tab"]').click()
    })

    it('should display expenses table', () => {
      cy.get('[data-testid="expenses-table"]').should('be.visible')
      cy.get('[data-testid="expenses-header"]').should('contain', 'Expenses Management')
    })

    it('should open add expense modal', () => {
      cy.get('[data-testid="add-expense-button"]').click()
      cy.get('[data-testid="add-expense-modal"]').should('be.visible')
      cy.get('[data-testid="expense-form"]').should('be.visible')
    })

    it('should filter expenses by category', () => {
      cy.get('[data-testid="category-filter"]').select('Salary')
      cy.get('[data-testid="apply-filter-button"]').click()
      cy.get('[data-testid="expense-row"]').each(($row) => {
        cy.wrap($row).find('[data-testid="expense-category"]').should('contain', 'Salary')
      })
    })
  })

  // ⚠️ OVERDUE FEES TAB TESTS
  describe('Overdue Fees Tab', () => {
    beforeEach(() => {
      cy.get('[data-testid="overdue-tab"]').click()
    })

    it('should display overdue fees table', () => {
      cy.get('[data-testid="overdues-table"]').should('be.visible')
      cy.get('[data-testid="overdues-header"]').should('contain', 'Overdue Fees')
    })

    it('should show overdue indicators', () => {
      cy.get('[data-testid="overdue-badge"]').should('be.visible')
      cy.get('[data-testid="days-overdue"]').should('contain', 'days')
    })

    it('should send reminders for overdue fees', () => {
      cy.get('[data-testid="send-reminders-button"]').click()
      cy.get('[data-testid="reminder-success"]').should('be.visible')
    })
  })

  // 📈 REPORTS TAB TESTS
  describe('Reports Tab', () => {
    beforeEach(() => {
      cy.get('[data-testid="reports-tab"]').click()
    })

    it('should display report generation options', () => {
      cy.get('[data-testid="monthly-report"]').should('be.visible')
      cy.get('[data-testid="custom-range-report"]').should('be.visible')
    })

    it('should generate monthly report', () => {
      cy.get('[data-testid="report-month"]').type('2024-01')
      cy.get('[data-testid="generate-monthly-button"]').click()
      cy.get('[data-testid="report-success"]').should('be.visible')
    })

    it('should generate custom date range report', () => {
      cy.get('[data-testid="start-date"]').type('2024-01-01')
      cy.get('[data-testid="end-date"]').type('2024-01-31')
      cy.get('[data-testid="generate-custom-button"]').click()
      cy.get('[data-testid="report-success"]').should('be.visible')
    })
  })

  // 🔄 INTERACTION TESTS
  describe('Tab Switching', () => {
    it('should switch between tabs correctly', () => {
      const tabs = ['overview', 'fees', 'fee-collection', 'expenses', 'overdue', 'reports']
      
      tabs.forEach((tab) => {
        cy.get(`[data-testid="${tab}-tab"]`).click()
        cy.get(`[data-testid="${tab}-tab"]`).should('have.class', 'border-indigo-500')
      })
    })

    it('should maintain tab state during navigation', () => {
      cy.get('[data-testid="fee-collection-tab"]').click()
      cy.reload()
      cy.get('[data-testid="fee-collection-tab"]').should('have.class', 'border-indigo-500')
    })
  })

  // 📱 RESPONSIVE TESTS
  describe('Responsive Design', () => {
    it('should display correctly on mobile', () => {
      cy.viewport('iphone-x')
      cy.get('[data-testid="finance-header"]').should('be.visible')
      cy.get('[data-testid="action-buttons"]').should('be.visible')
    })

    it('should display correctly on tablet', () => {
      cy.viewport('ipad-2')
      cy.get('[data-testid="finance-dashboard"]').should('be.visible')
    })

    it('should display correctly on desktop', () => {
      cy.viewport(1920, 1080)
      cy.get('[data-testid="finance-dashboard"]').should('be.visible')
    })
  })

  // ⚡ PERFORMANCE TESTS
  describe('Performance', () => {
    it('should load within acceptable time', () => {
      cy.visit('/admin/finance')
      cy.get('[data-testid="finance-dashboard"]').should('be.visible', { timeout: 3000 })
    })

    it('should handle rapid tab switching', () => {
      const tabs = ['overview', 'fees', 'fee-collection', 'expenses', 'overdue', 'reports']
      
      tabs.forEach((tab) => {
        cy.get(`[data-testid="${tab}-tab"]`).click()
        cy.get(`[data-testid="${tab}-content"]`).should('be.visible', { timeout: 1000 })
      })
    })
  })

  // 🔒 SECURITY TESTS
  describe('Security', () => {
    it('should redirect unauthenticated users', () => {
      cy.clearCookies()
      cy.visit('/admin/finance')
      cy.url().should('include', '/login')
    })

    it('should restrict access to non-admin users', () => {
      // Login as teacher
      cy.visit('/login')
      cy.get('[data-testid="email-input"]').type('teacher@mastermind.com')
      cy.get('[data-testid="otp-input"]').type('123456')
      cy.get('[data-testid="login-button"]').click()
      
      cy.visit('/admin/finance')
      cy.url().should('include', '/teacher/dashboard')
    })
  })

  // 🐛 ERROR HANDLING TESTS
  describe('Error Handling', () => {
    it('should handle network errors gracefully', () => {
      cy.intercept('GET', '/api/finance/*', { forceNetworkError: true })
      cy.visit('/admin/finance')
      cy.get('[data-testid="error-message"]').should('be.visible')
    })

    it('should handle API errors gracefully', () => {
      cy.intercept('GET', '/api/finance/*', { statusCode: 500 })
      cy.visit('/admin/finance')
      cy.get('[data-testid="error-message"]').should('be.visible')
    })
  })
})

// 🎯 END-TO-END WORKFLOW TESTS
describe('Finance Dashboard - E2E Workflows', () => {
  beforeEach(() => {
    cy.visit('/login')
    cy.get('[data-testid="email-input"]').type('admin@mastermind.com')
    cy.get('[data-testid="otp-input"]').type('123456')
    cy.get('[data-testid="login-button"]').click()
    cy.visit('/admin/finance')
  })

  it('should complete full fee collection workflow', () => {
    // 1. Navigate to Fee Collection
    cy.get('[data-testid="fee-collection-tab"]').click()
    
    // 2. Search and select student
    cy.get('[data-testid="student-search"]').type('John')
    cy.get('[data-testid="student-item"]').first().click()
    
    // 3. Setup student fee
    cy.get('[data-testid="setup-student-fee-button"]').click()
    cy.get('[data-testid="fee-type-monthly"]').click()
    cy.get('[data-testid="student-select"]').select('John Doe')
    cy.get('[data-testid="fee-structure-select"]').select(1)
    cy.get('[data-testid="number-of-months"]').type(12)
    cy.get('[data-testid="submit-fee-setup"]').click()
    
    // 4. Collect payment
    cy.get('[data-testid="fee-checkbox"]').first().check()
    cy.get('[data-testid="payment-method"]').select('Cash')
    cy.get('[data-testid="collect-payment-button"]').click()
    
    // 5. Verify receipt
    cy.get('[data-testid="receipt-modal"]').should('be.visible')
    cy.get('[data-testid="receipt-number"]').should('match', /RCP-\d+/)
    
    // 6. Send email
    cy.get('[data-testid="send-email-button"]').click()
    cy.get('[data-testid="email-success-message"]').should('contain', 'sent successfully')
    
    // 7. Verify in overview
    cy.get('[data-testid="overview-tab"]').click()
    cy.get('[data-testid="recent-transactions-table"]').should('contain', 'John Doe')
  })

  it('should complete expense management workflow', () => {
    // 1. Navigate to Expenses
    cy.get('[data-testid="expenses-tab"]').click()
    
    // 2. Add expense
    cy.get('[data-testid="add-expense-button"]').click()
    cy.get('[data-testid="expense-category"]').select('Salary')
    cy.get('[data-testid="expense-description"]').type('Teacher Salary')
    cy.get('[data-testid="expense-amount"]').type('50000')
    cy.get('[data-testid="expense-paid-to"]').type('John Teacher')
    cy.get('[data-testid="expense-date"]').type('2024-01-15')
    cy.get('[data-testid="submit-expense"]').click()
    
    // 3. Verify expense added
    cy.get('[data-testid="expenses-table"]').should('contain', 'Teacher Salary')
    
    // 4. Edit expense
    cy.get('[data-testid="edit-expense"]').first().click()
    cy.get('[data-testid="expense-amount"]').clear().type('55000')
    cy.get('[data-testid="update-expense"]').click()
    
    // 5. Verify updated
    cy.get('[data-testid="expenses-table"]').should('contain', '55000')
  })
})

// 🏆 TEST EXECUTION COMPLETE
// All tests designed for comprehensive coverage of Finance Dashboard functionality
