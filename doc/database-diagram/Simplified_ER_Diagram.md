# 🏗️ RFID Jewelry Inventory System - Simplified ER Diagram

## 📊 Simplified Database Schema

This is a simplified version of the Entity Relationship diagram focusing on the core business entities and their relationships.

---

## 🔗 Simplified Mermaid ER Diagram

```mermaid
erDiagram
    %% ========================================
    %% CORE BUSINESS ENTITIES
    %% ========================================
    
    %% Master Data (Lookup Tables)
    CategoryMaster {
        int CategoryId PK
        string CategoryName
    }
    
    ProductMaster {
        int ProductId PK
        string ProductName
    }
    
    DesignMaster {
        int DesignId PK
        string DesignName
    }
    
    PurityMaster {
        int PurityId PK
        string PurityName
    }
    
    BranchMaster {
        int BranchId PK
        string BranchName
        string ClientCode
    }
    
    CounterMaster {
        int CounterId PK
        string CounterName
        int BranchId FK
        string ClientCode
    }
    
    %% Core Business Tables
    ProductDetails {
        int Id PK
        string ClientCode
        string ItemCode
        int CategoryId FK
        int ProductId FK
        int DesignId FK
        int PurityId FK
        int BranchId FK
        int CounterId FK
        float GrossWeight
        float NetWeight
        decimal Mrp
        string Status
    }
    
    Rfid {
        string RFIDCode PK
        string EPCValue
        string ClientCode
        bool IsActive
    }
    
    ProductRfidAssignment {
        int Id PK
        int ProductId FK
        string RFIDCode FK
        bool IsActive
    }
    
    %% Stock Management
    StockVerification {
        int Id PK
        string ClientCode
        string VerificationSessionName
        int BranchId FK
        int CounterId FK
        int CategoryId FK
        string Status
        int TotalItemsScanned
        int MatchedItemsCount
        int UnmatchedItemsCount
    }
    
    StockVerificationDetail {
        int Id PK
        int StockVerificationId FK
        string ItemCode
        string RfidCode
        string VerificationStatus
    }
    
    %% Sales & Movement
    Invoice {
        int Id PK
        string ClientCode
        string InvoiceNumber
        int ProductId FK
        string RfidCode
        decimal FinalAmount
        string CustomerName
        datetime SoldOn
    }
    
    StockMovement {
        int Id PK
        string ClientCode
        int ProductId FK
        string MovementType
        int Quantity
        decimal TotalAmount
        int BranchId FK
        int CounterId FK
    }
    
    %% User Management
    User {
        int UserId PK
        string UserName
        string Email
        string ClientCode
        string OrganisationName
    }
    
    Role {
        int RoleId PK
        string RoleName
    }
    
    UserRole {
        int UserRoleId PK
        int UserId FK
        int RoleId FK
    }
    
    %% ========================================
    %% KEY RELATIONSHIPS
    %% ========================================
    
    %% Master Data Relationships
    BranchMaster ||--o{ CounterMaster : "contains"
    CategoryMaster ||--o{ ProductDetails : "categorizes"
    ProductMaster ||--o{ ProductDetails : "defines_type"
    DesignMaster ||--o{ ProductDetails : "defines_design"
    PurityMaster ||--o{ ProductDetails : "defines_purity"
    BranchMaster ||--o{ ProductDetails : "located_at"
    CounterMaster ||--o{ ProductDetails : "displayed_at"
    
    %% RFID Relationships
    ProductDetails ||--o{ ProductRfidAssignment : "assigned_to"
    Rfid ||--o{ ProductRfidAssignment : "assigned_from"
    
    %% Stock Verification
    StockVerification ||--o{ StockVerificationDetail : "contains"
    BranchMaster ||--o{ StockVerification : "verified_at"
    CounterMaster ||--o{ StockVerification : "verified_at"
    CategoryMaster ||--o{ StockVerification : "verified_for"
    
    %% Sales & Movement
    ProductDetails ||--o{ Invoice : "sold_in"
    ProductDetails ||--o{ StockMovement : "tracked_in"
    BranchMaster ||--o{ StockMovement : "moved_from"
    CounterMaster ||--o{ StockMovement : "moved_from"
    
    %% User Management
    User ||--o{ UserRole : "has"
    Role ||--o{ UserRole : "assigned_to"
    
    %% Multi-tenancy
    User ||--o{ BranchMaster : "owns"
    User ||--o{ CounterMaster : "owns"
    User ||--o{ ProductDetails : "owns"
    User ||--o{ Rfid : "owns"
    User ||--o{ StockVerification : "owns"
    User ||--o{ Invoice : "owns"
    User ||--o{ StockMovement : "owns"
```

---

## 🎯 **Core Business Flow**

### 1. **Product Management**
```
CategoryMaster → ProductDetails ← ProductMaster
     ↓                    ↓              ↓
DesignMaster         BranchMaster   PurityMaster
     ↓                    ↓              ↓
CounterMaster ← BranchMaster
```

### 2. **RFID Assignment**
```
Rfid → ProductRfidAssignment ← ProductDetails
```

### 3. **Stock Verification**
```
StockVerification → StockVerificationDetail
       ↓                    ↓
BranchMaster/CounterMaster/CategoryMaster
```

### 4. **Sales & Movement**
```
ProductDetails → Invoice
       ↓           ↓
StockMovement ← BranchMaster/CounterMaster
```

---

## 🔑 **Key Design Principles**

1. **Multi-Tenant**: All business tables include `ClientCode` for isolation
2. **Master Data**: Lookup tables for consistent categorization
3. **RFID Integration**: Complete RFID lifecycle management
4. **Stock Tracking**: Comprehensive movement and balance tracking
5. **Verification**: Built-in stock verification workflows
6. **Audit Trail**: Timestamps and status tracking throughout

---

## 📝 **Simplified View Benefits**

- **Easier to understand** the core business relationships
- **Focus on main entities** without overwhelming detail
- **Clear data flow** from master data to business operations
- **Quick overview** for stakeholders and developers
- **Foundation for understanding** the complete system

---

*This simplified diagram shows the essential structure while the full diagram provides complete technical details.*
