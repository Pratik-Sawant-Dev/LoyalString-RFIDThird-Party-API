# RFID Jewelry Inventory System - Admin User Hierarchy

## 🎯 Overview
This system implements a comprehensive admin-user hierarchy with granular permissions and complete activity tracking for the RFID Jewelry Inventory Management System.

## 🏗️ System Architecture

### User Hierarchy
```
MainAdmin (First registered user)
├── Admin Users (Created by MainAdmin)
│   ├── Regular Users (Created by Admin)
│   └── Regular Users (Created by Admin)
└── Admin Users (Created by MainAdmin)
    ├── Regular Users (Created by Admin)
    └── Regular Users (Created by Admin)
```

### User Types
1. **MainAdmin**: First registered user with full system access
2. **Admin**: Can create and manage users within their organization
3. **User**: Regular users with limited permissions based on assigned roles

## 📊 Database Schema Changes

### New Models Added
1. **UserActivity** - Tracks all user actions
2. **UserPermission** - Granular permission system
3. **User** (Enhanced) - Added admin hierarchy fields

### Enhanced User Model
```csharp
public class User
{
    // ... existing fields ...
    public bool IsAdmin { get; set; }
    public int? AdminUserId { get; set; }
    public string UserType { get; set; } // "MainAdmin", "Admin", "User"
    public DateTime? LastLoginDate { get; set; }
    public virtual User? AdminUser { get; set; }
}
```

### UserActivity Model
```csharp
public class UserActivity
{
    public int ActivityId { get; set; }
    public int UserId { get; set; }
    public string ClientCode { get; set; }
    public string ActivityType { get; set; } // Product, RFID, Invoice, Login, etc.
    public string Action { get; set; } // Create, Update, Delete, View, etc.
    public string? Description { get; set; }
    public string? TableName { get; set; }
    public int? RecordId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedOn { get; set; }
}
```

### UserPermission Model
```csharp
public class UserPermission
{
    public int PermissionId { get; set; }
    public int UserId { get; set; }
    public string Module { get; set; } // Product, RFID, Invoice, Reports, etc.
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }
    public bool CanImport { get; set; }
    public DateTime CreatedOn { get; set; }
    public int CreatedBy { get; set; }
}
```

## 🔐 Permission System

### Module-Based Permissions
- **Product**: Product management
- **RFID**: RFID tag management
- **Invoice**: Sales and billing
- **Reports**: Reporting and analytics
- **StockTransfer**: Stock transfer operations
- **StockVerification**: Inventory verification

### Permission Actions
- **View**: Read access
- **Create**: Add new records
- **Update**: Modify existing records
- **Delete**: Remove records
- **Export**: Export data
- **Import**: Import data

## 🛠️ New Services

### AdminService
- User creation and management
- Permission assignment and updates
- Activity tracking
- Dashboard analytics
- User hierarchy validation

### ActivityLoggingService
- Automatic activity logging
- Specialized logging for different modules
- IP address and user agent tracking
- JSON serialization of old/new values

## 🔒 Security Features

### Permission Middleware
- Automatic permission checking on endpoints
- Skip authentication for public endpoints
- Admin bypass for management operations
- Module-action based validation

### JWT Token Enhancements
```csharp
new Claim("IsAdmin", user.IsAdmin.ToString()),
new Claim("UserType", user.UserType),
new Claim("AdminUserId", user.AdminUserId?.ToString() ?? "")
```

## 📱 API Endpoints

### Admin Management
```http
POST   /api/admin/users                    # Create user
GET    /api/admin/users                    # Get managed users  
GET    /api/admin/users/{id}               # Get user by ID
PUT    /api/admin/users/{id}               # Update user
DELETE /api/admin/users/{id}               # Delete user
POST   /api/admin/users/{id}/activate      # Activate user
POST   /api/admin/users/{id}/deactivate    # Deactivate user
POST   /api/admin/users/{id}/reset-password # Reset password
```

### Permission Management
```http
GET    /api/admin/users/{id}/permissions   # Get user permissions
PUT    /api/admin/users/{id}/permissions   # Update permissions
POST   /api/admin/permissions/bulk-update  # Bulk permission update
```

### Activity Tracking
```http
GET    /api/admin/activities               # Get all activities
GET    /api/admin/users/{id}/activities    # Get user activities
```

### Dashboard & Analytics
```http
GET    /api/admin/dashboard                # Admin dashboard
GET    /api/admin/organization/dashboard   # Organization overview
GET    /api/admin/organization/users       # All organization users
```

## 🔄 User Registration Flow

### 1. First User (Automatic MainAdmin)
```http
POST /api/user/register
{
    "userName": "main_admin",
    "email": "admin@company.com",
    "password": "Admin123!",
    "organisationName": "My Jewelry Store"
}
```
- Automatically becomes MainAdmin
- Creates organization database
- Gets full system access

### 2. Admin Creates Sub-Users
```http
POST /api/admin/users
Authorization: Bearer {mainAdminToken}
{
    "userName": "shop_admin", 
    "email": "shop@company.com",
    "isAdmin": true,
    "permissions": [...]
}
```

### 3. Users Login and Get Tracked
```http
POST /api/user/login
{
    "email": "user@company.com",
    "password": "password"
}
```
- Updates last login date
- Logs login activity
- Returns JWT with user type and permissions

## 📊 Activity Tracking

### Automatic Tracking
- **Login/Logout**: Authentication activities
- **CRUD Operations**: All create, read, update, delete operations
- **Permission Changes**: Admin permission modifications
- **User Management**: User creation, updates, status changes

### Tracked Information
- User ID and client code
- Activity type and action
- Description and affected table
- Old and new values (JSON)
- IP address and user agent
- Timestamp

### Activity Types
- **Authentication**: Login, Logout
- **User**: Create, Update, Delete, Activate, Deactivate
- **Product**: Create, Update, Delete, View
- **RFID**: Create, Update, Delete, Assign, Scan
- **Invoice**: Create, Update, Delete, Print, Email
- **StockTransfer**: Create, Update, Delete, Approve
- **StockVerification**: Create, Update, Complete

## 🎛️ Admin Dashboard Features

### Statistics Overview
- Total users in organization
- Active vs inactive users
- Total admins
- Product, RFID, and invoice counts
- Today's activity count

### Recent Activities
- Last 10 user activities
- Real-time activity feed
- Filterable by user, type, date range

### User Management
- Recently created users
- User status overview
- Quick access to user details

## 🔍 Advanced Features

### Permission Validation
- Middleware automatically checks permissions
- Endpoint-specific permission mapping
- Admin override capabilities
- Granular module-action combinations

### Audit Trail
- Complete activity history
- JSON change tracking
- IP and browser information
- Searchable and filterable logs

### Multi-Tenant Security
- Client code isolation
- Admin hierarchy validation
- Organization-based data segregation
- Cross-tenant access prevention

## 🚀 Usage Examples

### Creating a Sales User
```csharp
var salesUser = new CreateUserByAdminDto
{
    UserName = "sales_rep",
    Email = "sales@company.com",
    IsAdmin = false,
    Permissions = new List<UserPermissionCreateDto>
    {
        new() { Module = "Product", CanView = true },
        new() { Module = "Invoice", CanView = true, CanCreate = true },
        new() { Module = "RFID", CanView = true }
    }
};
```

### Tracking Product Creation
```csharp
await _activityLoggingService.LogProductActivityAsync(
    userId: currentUser.Id,
    clientCode: currentUser.ClientCode,
    action: "Create",
    productId: newProduct.Id,
    newValues: newProduct,
    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
);
```

### Checking Permissions
```csharp
var hasPermission = await _adminService.HasPermissionAsync(
    userId: user.Id,
    module: "Product",
    action: "Delete"
);
```

## 🔧 Configuration

### JWT Claims Enhancement
The JWT token now includes:
- User type (MainAdmin, Admin, User)
- Admin status
- Parent admin ID
- Client code for multi-tenancy

### Database Relationships
- User → AdminUser (self-referencing)
- UserPermission → User
- UserPermission → CreatedByUser
- UserActivity → User

## 📈 Scalability Considerations

### Performance Optimizations
- Indexed activity queries
- Paginated activity feeds
- Efficient permission caching
- Optimized dashboard queries

### Security Measures
- Permission middleware
- Admin hierarchy validation
- IP tracking
- Activity audit trails

This comprehensive admin system provides complete user management, granular permissions, and detailed activity tracking while maintaining the multi-tenant architecture of the RFID Jewelry Inventory System.
