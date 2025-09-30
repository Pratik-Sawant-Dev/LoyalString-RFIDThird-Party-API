# RFID Jewelry Inventory System - Permission API Documentation

## 🎯 Overview
This document provides comprehensive documentation for the improved permission management system in the RFID Jewelry Inventory Management System. The system now includes proper API naming conventions, complete CRUD operations, and enhanced permission management capabilities.

## 🔧 Fixed Issues

### 1. **API Naming Consistency**
- ✅ Standardized all permission endpoints to follow RESTful conventions
- ✅ Consistent naming pattern: `/api/admin/users/{userId}/permissions`
- ✅ Clear separation between different operations (GET, POST, PUT, DELETE)

### 2. **Complete Permission Operations**
- ✅ **View Permissions**: Get user permissions and permission summaries
- ✅ **Create/Assign Permissions**: Assign new permissions to users
- ✅ **Update Permissions**: Modify existing user permissions
- ✅ **Remove Permissions**: Remove specific or all permissions from users
- ✅ **Bulk Operations**: Manage permissions for multiple users at once

### 3. **Enhanced Permission Management**
- ✅ Added permission removal endpoints
- ✅ Added bulk permission operations
- ✅ Added permission summary and analytics
- ✅ Added available modules endpoint
- ✅ Improved permission validation and error handling

## 📋 API Endpoints

### Permission Management

#### 1. Get User Permissions
```http
GET /api/admin/users/{userId}/permissions
```
**Description**: Retrieve all permissions for a specific user
**Authorization**: Admin users only
**Response**: Array of `UserPermissionDto`

#### 2. Get All User Permissions
```http
GET /api/admin/permissions
```
**Description**: Retrieve all user permissions in the organization
**Authorization**: Admin users only
**Response**: Array of `UserPermissionDto`

#### 3. Create/Assign Permissions
```http
POST /api/admin/users/{userId}/permissions
```
**Description**: Assign new permissions to a user
**Authorization**: Admin users only
**Request Body**: Array of `UserPermissionCreateDto`
**Response**: Success message

#### 4. Update User Permissions
```http
PUT /api/admin/users/{userId}/permissions
```
**Description**: Update existing permissions for a user
**Authorization**: Admin users only
**Request Body**: Array of `UserPermissionCreateDto`
**Response**: Success message

#### 5. Remove All User Permissions
```http
DELETE /api/admin/users/{userId}/permissions
```
**Description**: Remove all permissions from a user
**Authorization**: Admin users only
**Response**: Success message

#### 6. Remove Specific Permission
```http
DELETE /api/admin/users/{userId}/permissions/{module}
```
**Description**: Remove a specific module permission from a user
**Authorization**: Admin users only
**Response**: Success message

### Bulk Operations

#### 7. Bulk Update Permissions
```http
POST /api/admin/permissions/bulk-update
```
**Description**: Update permissions for multiple users
**Authorization**: Admin users only
**Request Body**: `BulkPermissionUpdateDto`
**Response**: Success message

#### 8. Bulk Remove Permissions
```http
POST /api/admin/permissions/bulk-remove
```
**Description**: Remove permissions for multiple users
**Authorization**: Admin users only
**Request Body**: `BulkPermissionRemoveDto`
**Response**: Success message

### Analytics and Utilities

#### 9. Get Available Modules
```http
GET /api/admin/permissions/modules
```
**Description**: Get list of available permission modules
**Authorization**: Admin users only
**Response**: Array of module names

#### 10. Get User Permission Summary
```http
GET /api/admin/users/{userId}/permissions/summary
```
**Description**: Get detailed permission summary for a user
**Authorization**: Admin users only
**Response**: `UserPermissionSummaryDto`

## 📊 Data Transfer Objects (DTOs)

### UserPermissionDto
```csharp
public class UserPermissionDto
{
    public int UserPermissionId { get; set; }
    public int UserId { get; set; }
    public string ClientCode { get; set; }
    public string Module { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }
    public bool CanImport { get; set; }
    public DateTime CreatedOn { get; set; }
    public int CreatedBy { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
}
```

### UserPermissionCreateDto
```csharp
public class UserPermissionCreateDto
{
    [Required]
    public string Module { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }
    public bool CanImport { get; set; }
}
```

### BulkPermissionUpdateDto
```csharp
public class BulkPermissionUpdateDto
{
    public List<int> UserIds { get; set; }
    public List<UserPermissionCreateDto> Permissions { get; set; }
}
```

### BulkPermissionRemoveDto
```csharp
public class BulkPermissionRemoveDto
{
    public List<int> UserIds { get; set; }
    public List<string> Modules { get; set; }
    public bool RemoveAll { get; set; }
}
```

### UserPermissionSummaryDto
```csharp
public class UserPermissionSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public int TotalPermissions { get; set; }
    public int ActivePermissions { get; set; }
    public List<ModulePermissionSummary> ModuleSummaries { get; set; }
}
```

## 🔐 Permission Modules

The system supports the following permission modules:

- **Product**: Product management operations
- **RFID**: RFID tag management operations
- **Invoice**: Sales and billing operations
- **Reports**: Reporting and analytics operations
- **StockTransfer**: Stock transfer operations
- **StockVerification**: Inventory verification operations
- **ProductImage**: Product image management operations
- **User**: User management operations
- **Admin**: Administrative operations

## 🛡️ Permission Actions

Each module supports the following actions:

- **View**: Read access to module data
- **Create**: Add new records in the module
- **Edit/Update**: Modify existing records in the module
- **Delete**: Remove records from the module
- **Export**: Export module data
- **Import**: Import data into the module

## 🔄 Usage Examples

### 1. Assign Permissions to a User
```http
POST /api/admin/users/123/permissions
Content-Type: application/json

[
  {
    "module": "Product",
    "canView": true,
    "canCreate": true,
    "canEdit": true,
    "canDelete": false,
    "canExport": true,
    "canImport": false
  },
  {
    "module": "RFID",
    "canView": true,
    "canCreate": false,
    "canEdit": false,
    "canDelete": false,
    "canExport": false,
    "canImport": false
  }
]
```

### 2. Update User Permissions
```http
PUT /api/admin/users/123/permissions
Content-Type: application/json

[
  {
    "module": "Product",
    "canView": true,
    "canCreate": true,
    "canEdit": true,
    "canDelete": true,
    "canExport": true,
    "canImport": true
  }
]
```

### 3. Remove Specific Permission
```http
DELETE /api/admin/users/123/permissions/Product
```

### 4. Bulk Update Permissions
```http
POST /api/admin/permissions/bulk-update
Content-Type: application/json

{
  "userIds": [123, 124, 125],
  "permissions": [
    {
      "module": "Product",
      "canView": true,
      "canCreate": true,
      "canEdit": true,
      "canDelete": false,
      "canExport": true,
      "canImport": false
    }
  ]
}
```

### 5. Get Permission Summary
```http
GET /api/admin/users/123/permissions/summary
```

## 🚨 Error Handling

The API returns appropriate HTTP status codes:

- **200 OK**: Successful operation
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: User or permission not found
- **500 Internal Server Error**: Server error

## 🔒 Security Features

1. **Authorization**: All endpoints require admin authentication
2. **Access Control**: Users can only manage permissions for users they have access to
3. **Activity Logging**: All permission changes are logged for audit purposes
4. **Validation**: Input validation ensures data integrity
5. **Hierarchy**: Respects user hierarchy (MainAdmin > Admin > User)

## 📈 Performance Considerations

1. **Bulk Operations**: Use bulk endpoints for multiple user updates
2. **Pagination**: Large permission lists are paginated
3. **Caching**: Permission checks are optimized for performance
4. **Indexing**: Database indexes on frequently queried fields

## 🧪 Testing

The permission system can be tested using:

1. **Unit Tests**: Individual method testing
2. **Integration Tests**: End-to-end API testing
3. **Postman Collection**: Pre-configured API tests
4. **Swagger UI**: Interactive API documentation and testing

## 📝 Migration Notes

When upgrading to this improved permission system:

1. **Backup Data**: Always backup existing permission data
2. **Test Thoroughly**: Test all permission operations in a staging environment
3. **Update Clients**: Update any client applications to use the new API endpoints
4. **Monitor Logs**: Monitor activity logs for any permission-related issues

## 🎯 Best Practices

1. **Principle of Least Privilege**: Grant only necessary permissions
2. **Regular Audits**: Regularly review and audit user permissions
3. **Documentation**: Document permission policies and procedures
4. **Training**: Train administrators on proper permission management
5. **Monitoring**: Monitor permission changes and user activities

---

This improved permission system provides a robust, secure, and user-friendly way to manage user permissions in the RFID Jewelry Inventory Management System.
