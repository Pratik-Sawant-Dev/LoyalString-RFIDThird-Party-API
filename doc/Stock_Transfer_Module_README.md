# Stock Transfer Module - RFID Jewelry Inventory System

## Overview

The Stock Transfer Module is a comprehensive solution for managing stock movements between different locations within the RFID Jewelry Inventory System. It enables users to easily transfer products between branches, counters, and boxes with full tracking and approval workflows.

## Features

### 🚀 Core Functionality
- **Multi-level Transfers**: Transfer products between branches, counters, and boxes
- **Workflow Management**: Complete approval and rejection workflow
- **Bulk Operations**: Handle multiple transfers simultaneously
- **Real-time Tracking**: Monitor transfer status and progress
- **Audit Trail**: Complete history of all stock movements

### 📍 Transfer Types
1. **Branch Transfer**: Move products between different branches
2. **Counter Transfer**: Move products between counters within the same branch
3. **Box Transfer**: Move products between boxes within the same counter
4. **Mixed Transfer**: Complex transfers involving multiple location changes

### 🔄 Transfer Statuses
- **Pending**: Transfer request created, awaiting approval
- **In Transit**: Transfer approved, product in movement
- **Completed**: Transfer finished, product at destination
- **Cancelled**: Transfer cancelled by user
- **Rejected**: Transfer rejected with reason

## API Endpoints

### Transfer Management

#### Create Transfer
```http
POST /api/stocktransfer
Content-Type: application/json

{
  "productId": 123,
  "rfidCode": "RFID001",
  "transferType": "Branch",
  "sourceBranchId": 1,
  "sourceCounterId": 5,
  "sourceBoxId": 10,
  "destinationBranchId": 2,
  "destinationCounterId": 8,
  "destinationBoxId": 15,
  "reason": "Inventory rebalancing",
  "remarks": "Moving to high-demand location"
}
```

#### Bulk Transfer
```http
POST /api/stocktransfer/bulk
Content-Type: application/json

{
  "transfers": [
    {
      "productId": 123,
      "sourceBranchId": 1,
      "sourceCounterId": 5,
      "destinationBranchId": 2,
      "destinationCounterId": 8
    }
  ],
  "commonReason": "Monthly inventory rebalancing",
  "commonRemarks": "Standard monthly transfer"
}
```

#### Get Transfers
```http
GET /api/stocktransfer?status=Pending&page=1&pageSize=20
```

#### Get Transfer by ID
```http
GET /api/stocktransfer/123
```

### Transfer Operations

#### Approve Transfer
```http
PUT /api/stocktransfer/123/approve
Content-Type: application/json

{
  "approvedBy": "manager@company.com",
  "remarks": "Approved for inventory rebalancing"
}
```

#### Reject Transfer
```http
PUT /api/stocktransfer/123/reject
Content-Type: application/json

{
  "rejectedBy": "manager@company.com",
  "rejectionReason": "Insufficient stock at source",
  "remarks": "Source location has low inventory"
}
```

#### Complete Transfer
```http
PUT /api/stocktransfer/123/complete?completedBy=staff@company.com
```

#### Cancel Transfer
```http
PUT /api/stocktransfer/123/cancel?cancelledBy=user@company.com
```

### Reporting & Analytics

#### Transfer Summary
```http
GET /api/stocktransfer/summary?fromDate=2024-01-01&toDate=2024-01-31
```

#### Transfers by Product
```http
GET /api/stocktransfer/product/123
```

#### Transfers by RFID
```http
GET /api/stocktransfer/rfid/RFID001
```

#### Pending Transfers by Location
```http
GET /api/stocktransfer/pending?branchId=1&counterId=5&boxId=10
```

### Utility Endpoints

#### Validate Transfer
```http
POST /api/stocktransfer/validate
Content-Type: application/json

{
  "productId": 123,
  "sourceBranchId": 1,
  "sourceCounterId": 5,
  "destinationBranchId": 2,
  "destinationCounterId": 8
}
```

#### Get Transfer Types
```http
GET /api/stocktransfer/types
```

#### Get Transfer Statuses
```http
GET /api/stocktransfer/statuses
```

## Data Models

### StockTransfer Entity
```csharp
public class StockTransfer
{
    public int Id { get; set; }
    public string ClientCode { get; set; }
    public string TransferNumber { get; set; }
    public int ProductId { get; set; }
    public string? RfidCode { get; set; }
    public string TransferType { get; set; }
    
    // Source Location
    public int SourceBranchId { get; set; }
    public int SourceCounterId { get; set; }
    public int? SourceBoxId { get; set; }
    
    // Destination Location
    public int DestinationBranchId { get; set; }
    public int DestinationCounterId { get; set; }
    public int? DestinationBoxId { get; set; }
    
    public string Status { get; set; }
    public DateTime TransferDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    
    // Approval/Rejection
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedOn { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedOn { get; set; }
    public string? RejectionReason { get; set; }
    
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public bool IsActive { get; set; }
}
```

## Business Rules

### Transfer Validation
1. **Product Existence**: Product must exist and be active
2. **Location Match**: Source location must match current product location
3. **No Pending Transfers**: Product cannot have multiple pending transfers
4. **Valid Destinations**: Destination locations must be valid and accessible

### Workflow Rules
1. **Approval Required**: All transfers require approval before execution
2. **Status Progression**: Status can only progress in specific order
3. **Completion**: Only transfers in "In Transit" status can be completed
4. **Cancellation**: Only pending or in-transit transfers can be cancelled

### Security & Access Control
1. **Client Isolation**: Transfers are isolated by client code
2. **Audit Trail**: All actions are logged with user information
3. **Validation**: Comprehensive validation at every step

## Database Schema

### Table: tblStockTransfer
```sql
CREATE TABLE tblStockTransfer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClientCode NVARCHAR(50) NOT NULL,
    TransferNumber NVARCHAR(50) NOT NULL UNIQUE,
    ProductId INT NOT NULL,
    RfidCode NVARCHAR(50) NULL,
    TransferType NVARCHAR(20) NOT NULL,
    
    -- Source Location
    SourceBranchId INT NOT NULL,
    SourceCounterId INT NOT NULL,
    SourceBoxId INT NULL,
    
    -- Destination Location
    DestinationBranchId INT NOT NULL,
    DestinationCounterId INT NOT NULL,
    DestinationBoxId INT NULL,
    
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    TransferDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedDate DATETIME2 NULL,
    Reason NVARCHAR(500) NULL,
    Remarks NVARCHAR(500) NULL,
    
    -- Approval/Rejection
    ApprovedBy NVARCHAR(100) NULL,
    ApprovedOn DATETIME2 NULL,
    RejectedBy NVARCHAR(100) NULL,
    RejectedOn DATETIME2 NULL,
    RejectionReason NVARCHAR(500) NULL,
    
    CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedOn DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
```

### Indexes for Performance
```sql
-- Primary and Unique Indexes
CREATE UNIQUE INDEX IX_StockTransfer_TransferNumber ON tblStockTransfer(TransferNumber);

-- Single Column Indexes
CREATE INDEX IX_StockTransfer_ProductId ON tblStockTransfer(ProductId);
CREATE INDEX IX_StockTransfer_RfidCode ON tblStockTransfer(RfidCode);
CREATE INDEX IX_StockTransfer_Status ON tblStockTransfer(Status);
CREATE INDEX IX_StockTransfer_TransferDate ON tblStockTransfer(TransferDate);
CREATE INDEX IX_StockTransfer_SourceBranchId ON tblStockTransfer(SourceBranchId);
CREATE INDEX IX_StockTransfer_SourceCounterId ON tblStockTransfer(SourceCounterId);
CREATE INDEX IX_StockTransfer_DestinationBranchId ON tblStockTransfer(DestinationBranchId);
CREATE INDEX IX_StockTransfer_DestinationCounterId ON tblStockTransfer(DestinationCounterId);

-- Composite Indexes for Complex Queries
CREATE INDEX IX_StockTransfer_Status_TransferDate ON tblStockTransfer(Status, TransferDate);
CREATE INDEX IX_StockTransfer_SourceLocation_Status ON tblStockTransfer(SourceBranchId, SourceCounterId, Status);
CREATE INDEX IX_StockTransfer_DestinationLocation_Status ON tblStockTransfer(DestinationBranchId, DestinationCounterId, Status);
CREATE INDEX IX_StockTransfer_Product_Status ON tblStockTransfer(ProductId, Status);
CREATE INDEX IX_StockTransfer_Rfid_Status ON tblStockTransfer(RfidCode, Status);
```

## Integration Points

### Stock Movement Tracking
- Automatically creates stock movement records for audit trail
- Updates movement records when transfer status changes
- Integrates with existing stock movement system

### Product Location Updates
- Updates product location when transfer is completed
- Maintains data consistency across the system
- Triggers location-based queries and reports

### RFID Integration
- Supports RFID-based transfers
- Maintains RFID-product associations
- Enables RFID-based tracking and reporting

## Usage Examples

### Simple Counter Transfer
```csharp
var transferDto = new CreateStockTransferDto
{
    ProductId = 123,
    TransferType = "Counter",
    SourceBranchId = 1,
    SourceCounterId = 5,
    DestinationBranchId = 1,  // Same branch
    DestinationCounterId = 8,  // Different counter
    Reason = "Display reorganization"
};

var transfer = await _stockTransferService.CreateTransferAsync(transferDto, "CLIENT001");
```

### Branch Transfer with Approval
```csharp
// 1. Create transfer
var transfer = await _stockTransferService.CreateTransferAsync(transferDto, "CLIENT001");

// 2. Approve transfer
var approveDto = new ApproveTransferDto
{
    ApprovedBy = "manager@company.com",
    Remarks = "Approved for inventory rebalancing"
};
var approvedTransfer = await _stockTransferService.ApproveTransferAsync(transfer.Id, approveDto, "CLIENT001");

// 3. Complete transfer
var completedTransfer = await _stockTransferService.CompleteTransferAsync(transfer.Id, "staff@company.com", "CLIENT001");
```

### Bulk Transfer Operations
```csharp
var bulkDto = new BulkStockTransferDto
{
    Transfers = new List<CreateStockTransferDto>
    {
        new CreateStockTransferDto { ProductId = 123, /* ... */ },
        new CreateStockTransferDto { ProductId = 124, /* ... */ },
        new CreateStockTransferDto { ProductId = 125, /* ... */ }
    },
    CommonReason = "Monthly inventory rebalancing",
    CommonRemarks = "Standard monthly transfer process"
};

var result = await _stockTransferService.CreateBulkTransfersAsync(bulkDto, "CLIENT001");
```

## Error Handling

### Common Error Scenarios
1. **Product Not Found**: Product ID doesn't exist
2. **Invalid Location**: Source/destination locations are invalid
3. **Status Conflict**: Attempting invalid status changes
4. **Duplicate Transfer**: Product already has pending transfer
5. **Permission Denied**: User lacks required permissions

### Error Response Format
```json
{
  "error": "Transfer validation failed",
  "details": "Product with ID 123 not found at specified source location"
}
```

## Performance Considerations

### Optimization Strategies
1. **Indexed Queries**: All common queries are indexed
2. **Pagination**: Large result sets are paginated
3. **Eager Loading**: Related data is loaded efficiently
4. **Batch Operations**: Bulk transfers use optimized batch processing

### Scalability Features
1. **Client Isolation**: Multi-tenant architecture
2. **Async Operations**: All operations are asynchronous
3. **Connection Pooling**: Efficient database connection management
4. **Caching**: Master data caching for repeated operations

## Security Features

### Authentication & Authorization
- JWT-based authentication
- Role-based access control
- Client code isolation
- Audit logging for all operations

### Data Protection
- Input validation and sanitization
- SQL injection prevention
- XSS protection
- CSRF protection

## Monitoring & Logging

### Audit Trail
- All transfer operations are logged
- User actions are tracked
- Status changes are recorded
- Complete history is maintained

### Performance Metrics
- Transfer processing times
- Success/failure rates
- Database query performance
- API response times

## Future Enhancements

### Planned Features
1. **Email Notifications**: Automated status notifications
2. **Mobile App Support**: Mobile-optimized interfaces
3. **Advanced Reporting**: Custom report builder
4. **Integration APIs**: Third-party system integration
5. **Real-time Updates**: WebSocket-based live updates

### Scalability Improvements
1. **Microservices Architecture**: Service decomposition
2. **Event Sourcing**: Event-driven architecture
3. **CQRS Pattern**: Command-Query Responsibility Segregation
4. **Distributed Caching**: Redis-based caching layer

## Support & Maintenance

### Troubleshooting
- Check transfer validation logs
- Verify product location consistency
- Review approval workflow status
- Monitor database performance

### Maintenance Tasks
- Regular index maintenance
- Audit log cleanup
- Performance monitoring
- Security updates

---

## Conclusion

The Stock Transfer Module provides a robust, scalable, and user-friendly solution for managing inventory movements within the RFID Jewelry Inventory System. With comprehensive validation, workflow management, and audit capabilities, it ensures data integrity while providing flexibility for various business scenarios.

For additional support or feature requests, please contact the development team.
