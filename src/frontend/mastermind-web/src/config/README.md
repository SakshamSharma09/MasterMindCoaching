# API Configuration Guide

This directory contains the centralized API configuration for THE MASTERMIND COACHING CLASSES frontend application.

## 📁 Files


### `api.ts`
Central configuration file for all API-related settings and endpoints.

### `README.md` (this file)
Documentation for using the API configuration.

## 🚀 Quick Setup

### 1. Configure Base URL

Edit `api.ts` and change the `API_BASE_URL` to match your backend server:

```typescript
// For local development
export const API_BASE_URL = 'http://localhost:5000/api'

// For production deployment (uncomment when deploying)
// export const API_BASE_URL = 'https://your-production-domain.com/api'

// For staging environment (uncomment when needed)
// export const API_BASE_URL = 'https://staging.mastermindcoaching.com/api'
```

### 2. Use in Components

Import and use the centralized API service in your Vue components:

```vue
<script setup lang="ts">
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'

// GET request
const userData = await apiService.get(API_ENDPOINTS.USERS.LIST)

// POST request
const newUser = await apiService.post(API_ENDPOINTS.USERS.CREATE, {
  name: 'John Doe',
  email: 'john@example.com'
})

// PUT request
const updatedUser = await apiService.put(API_ENDPOINTS.USERS.UPDATE(userId), userData)

// DELETE request
await apiService.delete(API_ENDPOINTS.USERS.DELETE(userId))
</script>
```

## 🛠️ Available Methods

### `apiService.get<T>(endpoint, config?)`
Make GET requests to the API.

### `apiService.post<T>(endpoint, data?, config?)`
Make POST requests to the API.

### `apiService.put<T>(endpoint, data?, config?)`
Make PUT requests to the API.

### `apiService.patch<T>(endpoint, data?, config?)`
Make PATCH requests to the API.

### `apiService.delete<T>(endpoint, config?)`
Make DELETE requests to the API.

### `apiService.upload<T>(endpoint, file, onProgress?)`
Upload files to the API with progress tracking.

### `apiService.download(endpoint, filename?)`
Download files from the API.

## 📋 Available Endpoints

All endpoints are organized by feature:

```typescript
// Authentication
API_ENDPOINTS.AUTH.LOGIN
API_ENDPOINTS.AUTH.REGISTER
API_ENDPOINTS.AUTH.VERIFY_OTP

// Dashboard
API_ENDPOINTS.DASHBOARD.PARENT_STATS
API_ENDPOINTS.DASHBOARD.TEACHER_STATS
API_ENDPOINTS.DASHBOARD.ADMIN_STATS

// Students
API_ENDPOINTS.STUDENTS.LIST
API_ENDPOINTS.STUDENTS.CREATE
API_ENDPOINTS.STUDENTS.UPDATE(id)
API_ENDPOINTS.STUDENTS.DELETE(id)

// And many more...
```

## 🔧 Features

### ✅ Automatic Authentication
- Automatically adds Bearer token from localStorage
- Handles 401 responses by clearing tokens and redirecting to login

### ✅ Error Handling
- Centralized error handling for common HTTP status codes
- Automatic retry logic for network errors
- Development-friendly error logging

### ✅ Type Safety
- Full TypeScript support with generic types
- Type definitions for API responses
- Autocomplete for endpoint names

### ✅ File Operations
- Built-in file upload with progress tracking
- File download with proper filename handling
- Multipart form data support

## 🔄 Environment Switching

### Local Development
```typescript
export const API_BASE_URL = 'http://localhost:5000/api'
```

### Production
```typescript
export const API_BASE_URL = 'https://your-production-domain.com/api'
```

### Staging
```typescript
export const API_BASE_URL = 'https://staging.mastermindcoaching.com/api'
```

## 📝 Example Usage

### Fetching User Data
```typescript
import { apiService, API_ENDPOINTS } from '@/services/apiService'

const loadUsers = async () => {
  try {
    const users = await apiService.get<User[]>(API_ENDPOINTS.USERS.LIST)
    console.log('Users:', users)
  } catch (error) {
    console.error('Failed to load users:', error)
  }
}
```

### Creating New Resource
```typescript
const createUser = async (userData: CreateUserRequest) => {
  try {
    const newUser = await apiService.post<User>(
      API_ENDPOINTS.USERS.CREATE,
      userData
    )
    console.log('User created:', newUser)
    return newUser
  } catch (error) {
    console.error('Failed to create user:', error)
    throw error
  }
}
```

### Uploading Files
```typescript
const uploadAvatar = async (file: File) => {
  try {
    const result = await apiService.upload<{ url: string }>(
      API_ENDPOINTS.USERS.AVATAR,
      file,
      (progress) => {
        console.log(`Upload progress: ${progress}%`)
      }
    )
    console.log('Avatar uploaded:', result.url)
    return result.url
  } catch (error) {
    console.error('Failed to upload avatar:', error)
    throw error
  }
}
```

## 🐛 Troubleshooting

### CORS Issues
Make sure your backend allows requests from your frontend URL:
```typescript
// Backend CORS configuration
app.use(cors({
  origin: ['http://localhost:3001', 'https://your-domain.com'],
  credentials: true
}))
```

### Authentication Issues
Ensure tokens are stored in localStorage:
```typescript
// After login
localStorage.setItem('token', response.data.token)
```

### Network Errors
Check the API_BASE_URL matches your backend server address.

## 🎯 Best Practices

1. **Always use the centralized endpoints** - Don't hardcode URLs in components
2. **Handle errors gracefully** - Use try-catch blocks and show user-friendly messages
3. **Use TypeScript types** - Define interfaces for your API responses
4. **Check environment** - Use different URLs for development and production
5. **Monitor API calls** - Use the built-in logging in development mode

## 📚 Additional Resources

- [Axios Documentation](https://axios-http.com/docs/intro)
- [Vue 3 Composition API](https://vuejs.org/guide/extras/composition-api-faq.html)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)

---

**Note**: This configuration is specifically designed for THE MASTERMIND COACHING CLASSES application. Modify the endpoints and settings according to your backend API structure.
