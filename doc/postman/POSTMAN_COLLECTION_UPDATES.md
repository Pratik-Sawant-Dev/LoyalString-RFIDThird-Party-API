# Postman Collection Updates - Permission Management System

## 🎯 Overview
This document outlines the comprehensive updates made to the Postman collection to reflect the improved permission management system in the RFID Jewelry Inventory Management System.

## 🔄 Changes Made

### 1. **Replaced Old Permission Endpoints**
The following old endpoints have been replaced with new, improved RESTful endpoints:

#### **Old Endpoints (Removed):**
- `POST /api/Admin/assign-permissions`
- `PUT /api/Admin/update-permissions`
- `GET /api/Admin/user-permissions/{userId}`
- `GET /api/Admin/all-user-permissions`

#### **New Endpoints (Added):**
- `GET /api/Admin/users/{userId}/permissions` - Get user permissions
- `GET /api/Admin/permissions` - Get all user permissions
- `POST /api/Admin/users/{userId}/permissions` - Create/assign permissions
- `PUT /api/Admin/users/{userId}/permissions` - Update permissions
- `DELETE /api/Admin/users/{userId}/permissions` - Remove all permissions
- `DELETE /api/Admin/users/{userId}/permissions/{module}` - Remove specific permission

### 2. **Added New Permission Management Section**
Created a dedicated "🔐 Permission Management" section with the following endpoints:

#### **Core Permission Operations:**
1. **Get User Permissions**
   - Method: `GET`
   - URL: `/api/Admin/users/{{subUserId}}/permissions`
   - Description: Retrieve all permissions for a specific user

2. **Get All User Permissions**
   - Method: `GET`
   - URL: `/api/Admin/permissions`
   - Description: Retrieve all user permissions in the organization

3. **Create/Assign Permissions**
   - Method: `POST`
   - URL: `/api/Admin/users/{{subUserId}}/permissions`
   - Description: Assign new permissions to a user
   - Body: Array of `UserPermissionCreateDto`

4. **Update User Permissions**
   - Method: `PUT`
   - URL: `/api/Admin/users/{{subUserId}}/permissions`
   - Description: Update existing permissions for a user
   - Body: Array of `UserPermissionCreateDto`

5. **Remove All User Permissions**
   - Method: `DELETE`
   - URL: `/api/Admin/users/{{subUserId}}/permissions`
   - Description: Remove all permissions from a user

6. **Remove Specific Permission**
   - Method: `DELETE`
   - URL: `/api/Admin/users/{{subUserId}}/permissions/Product`
   - Description: Remove a specific module permission from a user

#### **Bulk Operations:**
7. **Bulk Update Permissions**
   - Method: `POST`
   - URL: `/api/Admin/permissions/bulk-update`
   - Description: Update permissions for multiple users
   - Body: `BulkPermissionUpdateDto`

8. **Bulk Remove Permissions**
   - Method: `POST`
   - URL: `/api/Admin/permissions/bulk-remove`
   - Description: Remove permissions for multiple users
   - Body: `BulkPermissionRemoveDto`

#### **Analytics and Utilities:**
9. **Get Available Modules**
   - Method: `GET`
   - URL: `/api/Admin/permissions/modules`
   - Description: Get list of available permission modules

10. **Get User Permission Summary**
    - Method: `GET`
    - URL: `/api/Admin/users/{{subUserId}}/permissions/summary`
    - Description: Get detailed permission summary for a user

## 📋 Request Examples

### 1. Create/Assign Permissions
```json
POST /api/Admin/users/{{subUserId}}/permissions
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
    "canCreate": true,
    "canEdit": false,
    "canDelete": false,
    "canExport": false,
    "canImport": false
  },
  {
    "module": "Invoice",
    "canView": true,
    "canCreate": true,
    "canEdit": false,
    "canDelete": false,
    "canExport": true,
    "canImport": false
  }
]
```

### 2. Bulk Update Permissions
```json
POST /api/Admin/permissions/bulk-update
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
}
```

### 3. Bulk Remove Permissions
```json
POST /api/Admin/permissions/bulk-remove
Content-Type: application/json

{
  "userIds": [123, 124, 125],
  "modules": ["Product", "RFID"],
  "removeAll": false
}
```

## 🔧 Environment Variables

The collection uses the following environment variables:

- `{{baseUrl}}` - Base URL of the API
- `{{authToken}}` - Bearer token for authentication
- `{{subUserId}}` - ID of the sub-user for testing
- `{{adminUserId}}` - ID of the admin user for testing

## 🧪 Testing Workflow

### 1. **Setup Phase**
1. Set up environment variables
2. Login to get authentication token
3. Create admin user and sub-user for testing

### 2. **Permission Management Testing**
1. **Get Available Modules** - Verify available permission modules
2. **Create/Assign Permissions** - Assign initial permissions to user
3. **Get User Permissions** - Verify permissions were assigned
4. **Update User Permissions** - Modify existing permissions
5. **Get User Permission Summary** - Check permission analytics
6. **Remove Specific Permission** - Test removing individual permissions
7. **Bulk Update Permissions** - Test bulk operations
8. **Bulk Remove Permissions** - Test bulk removal
9. **Remove All User Permissions** - Test complete permission removal

### 3. **Validation Phase**
1. Verify all endpoints return expected status codes
2. Check response formats match DTOs
3. Validate permission changes are persisted
4. Test error handling for invalid requests

## 📊 Response Examples

### 1. User Permissions Response
```json
[
  {
    "userPermissionId": 1,
    "userId": 123,
    "clientCode": "CLIENT001",
    "module": "Product",
    "canView": true,
    "canCreate": true,
    "canEdit": true,
    "canDelete": false,
    "canExport": true,
    "canImport": false,
    "createdOn": "2024-01-15T10:30:00Z",
    "createdBy": 1,
    "userName": "John User",
    "userEmail": "john@example.com"
  }
]
```

### 2. Permission Summary Response
```json
{
  "userId": 123,
  "userName": "John User",
  "userEmail": "john@example.com",
  "totalPermissions": 18,
  "activePermissions": 12,
  "moduleSummaries": [
    {
      "module": "Product",
      "canView": true,
      "canCreate": true,
      "canEdit": true,
      "canDelete": false,
      "canExport": true,
      "canImport": false,
      "permissionCount": 5
    }
  ]
}
```

### 3. Available Modules Response
```json
[
  "Product",
  "RFID",
  "Invoice",
  "Reports",
  "StockTransfer",
  "StockVerification",
  "ProductImage",
  "User",
  "Admin"
]
```

## 🚨 Error Handling

The collection includes proper error handling for:

- **401 Unauthorized** - Invalid or missing authentication token
- **403 Forbidden** - Insufficient permissions
- **404 Not Found** - User or permission not found
- **400 Bad Request** - Invalid request data
- **500 Internal Server Error** - Server errors

## 🔒 Security Features

1. **Authentication Required** - All endpoints require Bearer token
2. **Authorization Checks** - Proper permission validation
3. **Input Validation** - Request body validation
4. **Error Responses** - Detailed error messages

## 📈 Performance Considerations

1. **Bulk Operations** - Use bulk endpoints for multiple user updates
2. **Pagination** - Large permission lists are paginated
3. **Caching** - Permission checks are optimized
4. **Indexing** - Database indexes on frequently queried fields

## 🎯 Best Practices

1. **Use Environment Variables** - Store dynamic values in environment
2. **Test in Order** - Follow the testing workflow sequence
3. **Validate Responses** - Check response formats and status codes
4. **Handle Errors** - Test error scenarios
5. **Clean Up** - Remove test data after testing

## 📝 Migration Notes

When upgrading to this improved Postman collection:

1. **Backup Existing Collection** - Save current collection before updating
2. **Update Environment Variables** - Ensure all variables are set correctly
3. **Test Thoroughly** - Run all tests in a staging environment
4. **Update Documentation** - Update any related documentation
5. **Train Team** - Ensure team members understand new endpoints

## 🎉 Benefits of Updated Collection

1. **✅ RESTful Design** - Follows REST API best practices
2. **✅ Complete Coverage** - All permission operations included
3. **✅ Better Organization** - Logical grouping of endpoints
4. **✅ Enhanced Testing** - Comprehensive test scenarios
5. **✅ Improved Documentation** - Clear descriptions and examples
6. **✅ Error Handling** - Proper error response testing
7. **✅ Bulk Operations** - Efficient multi-user management
8. **✅ Analytics Support** - Permission summary and insights

---

This updated Postman collection provides a comprehensive testing suite for the improved permission management system, ensuring all functionality works correctly and follows best practices.
