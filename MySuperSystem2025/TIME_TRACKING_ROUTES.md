# ??? Time Tracking Routes Reference

## Time Entry Routes

### Dashboard
```
GET  /Time/Index
GET  /Time/Index?period=daily
GET  /Time/Index?period=weekly
GET  /Time/Index?period=monthly
```

### Create
```
GET   /Time/Create
POST  /Time/Create
```

### Edit
```
GET   /Time/Edit/{id}
POST  /Time/Edit
```

### Delete
```
GET   /Time/Delete/{id}
POST  /Time/Delete
```

### List/Filter
```
GET  /Time/List
GET  /Time/List?period=daily
GET  /Time/List?period=weekly
GET  /Time/List?period=monthly
GET  /Time/List?categoryId={id}
GET  /Time/List?startDate=2025-01-01&endDate=2025-01-31
```

## Category Routes

### Categories List
```
GET  /Time/Categories
```

### Create Category
```
GET   /Time/CreateCategory
POST  /Time/CreateCategory
```

### Edit Category
```
GET   /Time/EditCategory/{id}
POST  /Time/EditCategory
```

### Delete Category
```
GET   /Time/DeleteCategory/{id}
POST  /Time/DeleteCategory
```

## Quick Access

- **Sidebar**: Click "Time Tracker" 
- **Dashboard**: Click "Time Tracker" card
- **Quick Action**: Click "Add Time Entry" button
