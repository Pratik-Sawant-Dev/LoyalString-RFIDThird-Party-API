# 📋 RFID Jewelry Inventory System - Table Summary

## 🗂️ Complete Database Tables Overview

This document provides a quick reference to all tables in the RFID Jewelry Inventory Management System.

---

## 🔐 **User Management & Authentication**

| Table | Purpose | Key Fields | Relationships |
|-------|---------|------------|---------------|
| **User** | Main user accounts | `UserId`, `UserName`, `Email`, `ClientCode`, `OrganisationName` | → UserRole, → BranchMaster, → CounterMaster |
| **Role** | User roles | `RoleId`, `RoleName`, `Description` | → UserRole, → Permission |
| **UserRole** | User-Role mapping | `UserRoleId`, `UserId`, `RoleId` | ← User, ← Role |
| **Permission** | Role permissions | `PermissionId`, `RoleId`, `PageName`, `CanView/Edit/Delete` | ← Role |

---

## 🏢 **Master Data Tables**

| Table | Purpose | Key Fields | Relationships |
|-------|---------|------------|---------------|
| **CategoryMaster** | Product categories | `CategoryId`, `CategoryName` | → ProductDetails, → StockVerification |
| **ProductMaster** | Product types | `ProductId`, `ProductName` | → ProductDetails |
| **DesignMaster** | Design patterns | `DesignId`, `DesignName` | → ProductDetails |
| **PurityMaster** | Purity levels | `PurityId`, `PurityName` | → ProductDetails |
| **BranchMaster** | Store branches | `BranchId`, `BranchName`, `ClientCode` | → CounterMaster, → ProductDetails, → StockVerification |
| **CounterMaster** | Display counters | `CounterId`, `CounterName`, `BranchId`, `ClientCode` | ← BranchMaster, → ProductDetails, → StockVerification |

---

## 💎 **Core Product & RFID Tables**

| Table | Purpose | Key Fields | Relationships |
|-------|---------|------------|---------------|
| **ProductDetails** | Main product info | `Id`, `ClientCode`, `ItemCode`, `CategoryId`, `ProductId`, `DesignId`, `PurityId`, `BranchId`, `CounterId`, `GrossWeight`, `NetWeight`, `Mrp` | ← CategoryMaster, ← ProductMaster, ← DesignMaster, ← PurityMaster, ← BranchMaster, ← CounterMaster, → ProductRfidAssignment, → ProductImage, → Invoice, → StockMovement, → DailyStockBalance |
| **Rfid** | RFID tag info | `RFIDCode`, `EPCValue`, `ClientCode`, `IsActive` | → ProductRfidAssignment |
| **ProductRfidAssignment** | Product-RFID links | `Id`, `ProductId`, `RFIDCode`, `AssignedOn`, `IsActive` | ← ProductDetails, ← Rfid |
| **ProductImage** | Product images | `Id`, `ClientCode`, `ProductId`, `FileName`, `FilePath`, `ImageType`, `DisplayOrder` | ← ProductDetails |

---

## 📊 **Stock Verification System**

| Table | Purpose | Key Fields | Relationships |
|-------|---------|------------|---------------|
| **StockVerification** | Verification sessions | `Id`, `ClientCode`, `VerificationSessionName`, `BranchId`, `CounterId`, `CategoryId`, `Status`, `TotalItemsScanned`, `MatchedItemsCount`, `UnmatchedItemsCount` | ← BranchMaster, ← CounterMaster, ← CategoryMaster, → StockVerificationDetail |
| **StockVerificationDetail** | Verification items | `Id`, `StockVerificationId`, `ClientCode`, `ItemCode`, `RfidCode`, `VerificationStatus` | ← StockVerification |

---

## 🧾 **Invoice & Sales Management**

| Table | Purpose | Key Fields | Relationships |
|-------|---------|------------|---------------|
| **Invoice** | Sales invoices | `Id`, `ClientCode`, `InvoiceNumber`, `ProductId`, `RfidCode`, `SellingPrice`, `FinalAmount`, `CustomerName`, `SoldOn` | ← ProductDetails |

---

## 📈 **Stock Tracking & Movement**

| Table | Purpose | Key Fields | Relationships |
|-------|---------|------------|---------------|
| **StockMovement** | Stock movements | `Id`, `ClientCode`, `ProductId`, `RfidCode`, `MovementType`, `Quantity`, `TotalAmount`, `BranchId`, `CounterId`, `CategoryId`, `MovementDate` | ← ProductDetails, ← BranchMaster, ← CounterMaster, ← CategoryMaster |
| **DailyStockBalance** | Daily balances | `Id`, `ClientCode`, `ProductId`, `RfidCode`, `BranchId`, `CounterId`, `CategoryId`, `BalanceDate`, `OpeningQuantity`, `ClosingQuantity`, `AddedQuantity`, `SoldQuantity` | ← ProductDetails, ← BranchMaster, ← CounterMaster, ← CategoryMaster |

---

## 🔗 **Key Relationships Summary**

### **One-to-Many Relationships**
- **BranchMaster** → **CounterMaster** (1 branch contains many counters)
- **CategoryMaster** → **ProductDetails** (1 category has many products)
- **ProductMaster** → **ProductDetails** (1 product type has many instances)
- **DesignMaster** → **ProductDetails** (1 design has many products)
- **PurityMaster** → **ProductDetails** (1 purity level has many products)
- **BranchMaster** → **ProductDetails** (1 branch has many products)
- **CounterMaster** → **ProductDetails** (1 counter has many products)

### **Many-to-Many Relationships**
- **ProductDetails** ↔ **Rfid** (through ProductRfidAssignment)
- **User** ↔ **Role** (through UserRole)

### **Multi-Tenant Isolation**
- All business tables include `ClientCode` for complete data isolation
- Each client has their own database instance

---

## 📊 **Table Count Summary**

| Category | Table Count | Description |
|----------|-------------|-------------|
| **User Management** | 4 | Authentication, roles, permissions |
| **Master Data** | 6 | Lookup tables for categorization |
| **Core Product** | 4 | Products, RFID, images, assignments |
| **Stock Verification** | 2 | Verification sessions and details |
| **Sales & Movement** | 3 | Invoices, movements, daily balances |
| **Total** | **19** | Complete system coverage |

---

## 🎯 **Business Domains**

### **1. Product Management**
- **Tables**: ProductDetails, CategoryMaster, ProductMaster, DesignMaster, PurityMaster
- **Purpose**: Complete product lifecycle management

### **2. RFID Management**
- **Tables**: Rfid, ProductRfidAssignment
- **Purpose**: RFID tag assignment and tracking

### **3. Inventory Management**
- **Tables**: StockMovement, DailyStockBalance
- **Purpose**: Real-time stock tracking and reporting

### **4. Stock Verification**
- **Tables**: StockVerification, StockVerificationDetail
- **Purpose**: Regular inventory accuracy verification

### **5. Sales Management**
- **Tables**: Invoice
- **Purpose**: Sales tracking and customer management

### **6. User Management**
- **Tables**: User, Role, UserRole, Permission
- **Purpose**: Access control and user administration

---

## 📝 **Notes**

- **Primary Keys**: All tables have auto-incrementing integer primary keys except `Rfid` (uses `RFIDCode` string)
- **Foreign Keys**: Comprehensive referential integrity through foreign key relationships
- **Audit Fields**: Most tables include `CreatedOn`, `UpdatedOn`, `IsActive` for audit trails
- **Multi-Tenant**: `ClientCode` field ensures complete data isolation between clients
- **RFID Integration**: Complete RFID lifecycle from tag creation to product assignment
- **Stock Tracking**: Comprehensive movement tracking with daily balance calculations

---

*This table summary provides a quick reference to understand the complete database structure at a glance.*
