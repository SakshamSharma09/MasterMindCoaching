# MasterMind Coaching - Performance Gotchas

> **Performance-related lessons learned and best practices**  
> Last Updated: 2026-04-17

## Database Performance

### N+1 Query Problem
**Symptom**: Slow page loads, many SQL queries in logs  
**Root Cause**: Loading related entities one at a time in a loop  
**Solution**: Use `.Include()` for eager loading

```csharp
// ❌ BAD - N+1 queries
var students = await _context.Students.ToListAsync();
foreach (var student in students) {
    var classes = student.StudentClasses; // Lazy load = extra query
}

// ✅ GOOD - Single query with Include
var students = await _context.Students
    .Include(s => s.StudentClasses)
        .ThenInclude(sc => sc.Class)
    .ToListAsync();
```

### Large Result Sets
**Symptom**: Memory issues, slow responses  
**Root Cause**: Loading entire tables without pagination  
**Solution**: Always use `.Take()` or pagination

```csharp
// ❌ BAD - Loads everything
var allStudents = await _context.Students.ToListAsync();

// ✅ GOOD - Paginated
var students = await _context.Students
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Select Only What You Need
**Symptom**: Slow queries, high memory usage  
**Root Cause**: Loading entire entities when only few fields needed  
**Solution**: Use `.Select()` to project only required fields

```csharp
// ❌ BAD - Loads all columns
var students = await _context.Students.ToListAsync();

// ✅ GOOD - Only loads needed columns
var studentNames = await _context.Students
    .Select(s => new { s.Id, s.FirstName, s.LastName })
    .ToListAsync();
```

---

## API Performance

### Response Caching
**Pattern**: Cache frequently accessed, rarely changing data

```csharp
// In controller
[ResponseCache(Duration = 300)] // 5 minutes
public async Task<ActionResult> GetClasses()
```

### Memory Caching for Expensive Operations
```csharp
// In service
public async Task<DashboardStats> GetDashboardStats()
{
    var cacheKey = "dashboard_stats";
    if (!_cache.TryGetValue(cacheKey, out DashboardStats stats))
    {
        stats = await CalculateStats();
        _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(5));
    }
    return stats;
}
```

---

## Frontend Performance

### Lazy Loading Routes
**Pattern**: Load views only when needed

```typescript
// ✅ GOOD - Lazy loaded
const routes = [
  {
    path: '/admin/students',
    component: () => import('@/views/admin/StudentsView.vue')
  }
]
```

### Debounce Search Inputs
**Pattern**: Don't fire API calls on every keystroke

```typescript
import { useDebounceFn } from '@vueuse/core'

const debouncedSearch = useDebounceFn((query) => {
  searchStudents(query)
}, 300)
```

### Virtual Scrolling for Large Lists
**Pattern**: Only render visible items

```vue
<template>
  <RecycleScroller
    :items="students"
    :item-size="50"
    v-slot="{ item }"
  >
    <StudentRow :student="item" />
  </RecycleScroller>
</template>
```

---

## Azure-Specific Performance

### Connection Pooling
**Pattern**: Reuse database connections

```csharp
// In Program.cs - connection pooling is default, but ensure:
builder.Services.AddDbContext<MasterMindDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(3)));
```

### Serverless Cold Start
**Symptom**: First request after idle period is slow  
**Root Cause**: Azure SQL Serverless auto-pauses  
**Solution**: 
- Use "Always On" for production
- Or accept 1-2 second cold start for cost savings

### Static File Caching
**Pattern**: Cache static assets in Azure Static Web Apps

```json
// staticwebapp.config.json
{
  "globalHeaders": {
    "Cache-Control": "public, max-age=31536000"
  }
}
```

---

## Monitoring & Diagnostics

### Enable Query Logging (Development Only)
```csharp
// In Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
        options.EnableSensitiveDataLogging()
               .LogTo(Console.WriteLine, LogLevel.Information));
}
```

### Key Metrics to Watch
- Response time > 500ms = investigate
- Database queries per request > 10 = N+1 problem
- Memory usage growing = possible memory leak
- Error rate > 1% = investigate failures

---

## Quick Checklist

Before deploying, verify:
- [ ] All list endpoints have pagination or `.Take()` limit
- [ ] Complex queries use `.Include()` for related data
- [ ] Search endpoints are debounced on frontend
- [ ] Static assets have cache headers
- [ ] No `SELECT *` equivalent queries (use `.Select()`)
- [ ] Response caching on read-heavy endpoints

---

*Add new performance lessons here as they are discovered.*
