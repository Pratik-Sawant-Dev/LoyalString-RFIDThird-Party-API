# 🏗️ RFID Jewelry Inventory System - Entity Relationship Diagram

## 📊 Complete Database Schema Overview

This document contains the complete Entity Relationship (ER) diagram for the RFID Jewelry Inventory Management System. The system is designed with a multi-tenant architecture where each client has their own isolated database.

---

## 🔗 Mermaid ER Diagram

```mermaid
erDiagram
    %% ========================================
    %% USER MANAGEMENT & AUTHENTICATION
    %% ========================================
    User {
        int UserId PK
        string UserName UK
        string Email UK
        string PasswordHash
        string FullName
        string MobileNumber
        string FaxNumber
        string City
        string Address
        string OrganisationName
        string ShowroomType
        string ClientCode FK
        string DatabaseName
        string ConnectionString
        bool IsActive
        datetime CreatedOn
    }

    Role {
        int RoleId PK
        string RoleName UK
        string Description
    }

    UserRole {
        int UserRoleId PK
        int UserId FK
        int RoleId FK
    }

    Permission {
        int PermissionId PK
        int RoleId FK
        string PageName
        bool CanView
        bool CanEdit
        bool CanDelete
    }

    %% ========================================
    %% MASTER DATA TABLES
    %% ========================================
    CategoryMaster {
        int CategoryId PK
        string CategoryName UK
    }

    ProductMaster {
        int ProductId PK
        string ProductName UK
    }

    DesignMaster {
        int DesignId PK
        string DesignName UK
    }

    PurityMaster {
        int PurityId PK
        string PurityName UK
    }

    BranchMaster {
        int BranchId PK
        string BranchName UK
        string ClientCode FK
    }

    CounterMaster {
        int CounterId PK
        string CounterName UK
        int BranchId FK
        string ClientCode FK
    }

    %% ========================================
    %% CORE PRODUCT & RFID TABLES
    %% ========================================
    ProductDetails {
        int Id PK
        string ClientCode FK
        int BranchId FK
        int CounterId FK
        string ItemCode UK
        int CategoryId FK
        int ProductId FK
        int DesignId FK
        int PurityId FK
        float GrossWeight
        float StoneWeight
        float DiamondHeight
        float NetWeight
        string BoxDetails
        int Size
        decimal StoneAmount
        decimal DiamondAmount
        decimal HallmarkAmount
        decimal MakingPerGram
        decimal MakingPercentage
        decimal MakingFixedAmount
        decimal Mrp
        string ImageUrl
        string Status
        datetime CreatedOn
    }

    Rfid {
        string RFIDCode PK
        string EPCValue UK
        string ClientCode FK
        bool IsActive
        datetime CreatedOn
    }

    ProductRfidAssignment {
        int Id PK
        int ProductId FK
        string RFIDCode FK
        datetime AssignedOn
        datetime UnassignedOn
        bool IsActive
    }

    ProductImage {
        int Id PK
        string ClientCode FK
        int ProductId FK
        string FileName
        string FilePath
        string ContentType
        long FileSize
        string OriginalFileName
        string ImageType
        int DisplayOrder
        bool IsActive
        datetime CreatedOn
        datetime UpdatedOn
    }

    %% ========================================
    %% STOCK VERIFICATION SYSTEM
    %% ========================================
    StockVerification {
        int Id PK
        string ClientCode FK
        string VerificationSessionName
        string Description
        date VerificationDate
        time VerificationTime
        int BranchId FK
        int CounterId FK
        int CategoryId FK
        int TotalItemsScanned
        int MatchedItemsCount
        int UnmatchedItemsCount
        int MissingItemsCount
        decimal TotalMatchedValue
        decimal TotalUnmatchedValue
        decimal TotalMissingValue
        string VerifiedBy
        string Status
        string Remarks
        datetime CreatedOn
        datetime UpdatedOn
        datetime CompletedOn
        bool IsActive
    }

    StockVerificationDetail {
        int Id PK
        int StockVerificationId FK
        string ClientCode FK
        string ItemCode
        string RfidCode
        string VerificationStatus
        datetime ScannedAt
        time ScannedTime
        string ScannedBy
        string Remarks
        datetime CreatedOn
        datetime UpdatedOn
        bool IsActive
    }

    %% ========================================
    %% INVOICE & SALES MANAGEMENT
    %% ========================================
    Invoice {
        int Id PK
        string ClientCode FK
        string InvoiceNumber UK
        int ProductId FK
        string RfidCode
        decimal SellingPrice
        decimal DiscountAmount
        decimal FinalAmount
        string InvoiceType
        string CustomerName
        string CustomerPhone
        string CustomerAddress
        string PaymentMethod
        string PaymentReference
        datetime SoldOn
        datetime CreatedOn
        datetime UpdatedOn
        string Remarks
        bool IsActive
    }

    %% ========================================
    %% STOCK TRACKING & MOVEMENT
    %% ========================================
    StockMovement {
        int Id PK
        string ClientCode FK
        int ProductId FK
        string RfidCode
        string MovementType
        int Quantity
        decimal UnitPrice
        decimal TotalAmount
        int BranchId FK
        int CounterId FK
        int CategoryId FK
        string ReferenceNumber
        string ReferenceType
        string Remarks
        date MovementDate
        datetime CreatedOn
        datetime UpdatedOn
        bool IsActive
    }

    DailyStockBalance {
        int Id PK
        string ClientCode FK
        int ProductId FK
        string RfidCode
        int BranchId FK
        int CounterId FK
        int CategoryId FK
        date BalanceDate
        int OpeningQuantity
        int ClosingQuantity
        int AddedQuantity
        int SoldQuantity
        int ReturnedQuantity
        int TransferredInQuantity
        int TransferredOutQuantity
        decimal OpeningValue
        decimal ClosingValue
        decimal AddedValue
        decimal SoldValue
        decimal ReturnedValue
        datetime CreatedOn
        datetime UpdatedOn
        bool IsActive
    }

    %% ========================================
    %% RELATIONSHIPS
    %% ========================================
    
    %% User Management Relationships
    User ||--o{ UserRole : "has"
    Role ||--o{ UserRole : "assigned_to"
    Role ||--o{ Permission : "has"
    
    %% Master Data Relationships
    BranchMaster ||--o{ CounterMaster : "contains"
    BranchMaster ||--o{ ProductDetails : "located_in"
    CounterMaster ||--o{ ProductDetails : "displayed_at"
    CategoryMaster ||--o{ ProductDetails : "categorized_as"
    ProductMaster ||--o{ ProductDetails : "type_of"
    DesignMaster ||--o{ ProductDetails : "designed_as"
    PurityMaster ||--o{ ProductDetails : "purity_level"
    
    %% Product & RFID Relationships
    ProductDetails ||--o{ ProductRfidAssignment : "assigned_to"
    Rfid ||--o{ ProductRfidAssignment : "assigned_from"
    ProductDetails ||--o{ ProductImage : "has"
    ProductDetails ||--o{ Invoice : "sold_in"
    ProductDetails ||--o{ StockMovement : "tracked_in"
    ProductDetails ||--o{ DailyStockBalance : "balanced_in"
    
    %% Stock Verification Relationships
    StockVerification ||--o{ StockVerificationDetail : "contains"
    BranchMaster ||--o{ StockVerification : "verified_at"
    CounterMaster ||--o{ StockVerification : "verified_at"
    CategoryMaster ||--o{ StockVerification : "verified_for"
    
    %% Multi-tenancy (ClientCode relationships)
    User ||--o{ BranchMaster : "owns"
    User ||--o{ CounterMaster : "owns"
    User ||--o{ ProductDetails : "owns"
    User ||--o{ Rfid : "owns"
    User ||--o{ ProductImage : "owns"
    User ||--o{ Invoice : "owns"
    User ||--o{ StockMovement : "owns"
    User ||--o{ DailyStockBalance : "owns"
    User ||--o{ StockVerification : "owns"
    User ||--o{ StockVerificationDetail : "owns"
```

---

## 📋 Table Descriptions

### 🔐 **User Management & Authentication**
- **`User`**: Main user table with multi-tenant support
- **`Role`**: User roles for access control
- **`UserRole`**: Many-to-many relationship between users and roles
- **`Permission`**: Granular permissions for each role

### 🏢 **Master Data Tables**
- **`CategoryMaster`**: Product categories (e.g., Gold, Silver, Diamond)
- **`ProductMaster`**: Product types (e.g., Ring, Necklace, Earring)
- **`DesignMaster`**: Design patterns and styles
- **`PurityMaster`**: Purity levels (e.g., 24K, 22K, 18K)
- **`BranchMaster`**: Store branches with client isolation
- **`CounterMaster`**: Display counters within branches

### 💎 **Core Product & RFID Tables**
- **`ProductDetails`**: Main product information with all specifications
- **`Rfid`**: RFID tag information with EPC values
- **`ProductRfidAssignment`**: Links products to RFID tags
- **`ProductImage`**: Product images with metadata

### 📊 **Stock Verification System**
- **`StockVerification`**: Stock verification sessions
- **`StockVerificationDetail`**: Individual items in verification sessions

### 🧾 **Invoice & Sales Management**
- **`Invoice`**: Sales invoices with customer details

### 📈 **Stock Tracking & Movement**
- **`StockMovement`**: All stock movements (additions, sales, transfers)
- **`DailyStockBalance`**: Daily opening/closing stock balances

---

## 🔑 Key Features

### 🏗️ **Multi-Tenant Architecture**
- Each client has isolated data through `ClientCode`
- Separate database per client for complete isolation

### 🏷️ **RFID Integration**
- EPC value tracking for each RFID tag
- Product-RFID assignment management
- RFID scanning and verification support

### 📊 **Comprehensive Stock Management**
- Real-time stock tracking
- Movement history
- Daily balance calculations
- Stock verification workflows

### 🔐 **Role-Based Access Control**
- Granular permissions system
- User role management
- Secure authentication

### 📸 **Media Management**
- Product image storage
- Multiple image types support
- Organized display ordering

---

## 🎯 **Business Logic Flow**

1. **Product Creation**: Products are created with master data references
2. **RFID Assignment**: RFID tags are assigned to products
3. **Stock Tracking**: All movements are tracked in real-time
4. **Sales Processing**: Invoices are generated for sales
5. **Stock Verification**: Regular verification sessions ensure accuracy
6. **Reporting**: Comprehensive reports from all data points

---

## 📝 **Notes**

- All tables include `IsActive` flag for soft deletion
- Timestamps (`CreatedOn`, `UpdatedOn`) for audit trails
- Foreign key relationships ensure data integrity
- Multi-tenant design allows complete client isolation
- RFID system supports both EPC and custom RFID codes
- Stock verification provides comprehensive inventory accuracy

---

*This ER diagram represents the complete database schema for the RFID Jewelry Inventory Management System.*
