# Stock Movement Guide - How to Add Stock and See Daily Balances

## 🚨 **Issue Identified**
When you add products through the product creation API, it doesn't automatically create stock movements. This is why you're getting zeros in the daily balance report.

## ✅ **Solution: Create Stock Movements Manually**

### Step 1: Add Products (Already Working)
```
POST /api/Product/create
{
  "itemCode": "JWL001",
  "productName": "Gold Ring",
  "categoryName": "Rings",
  "branchName": "Main Branch",
  "counterName": "Counter 1",
  "rfidCode": "RFID001",
  "designName": "Classic",
  "purityName": "18K",
  "boxDetails": "Box A",
  "weight": 5.5,
  "makingCharges": 1000,
  "wastageCharges": 50,
  "totalPrice": 15000
}
```

### Step 2: Create Stock Movement for Addition
```
POST /api/Reporting/stock-movements
{
  "productId": 1,
  "rfidCode": "RFID001",
  "movementType": "Addition",
  "quantity": 20,
  "unitPrice": 15000,
  "totalAmount": 300000,
  "referenceNumber": "ADD-001",
  "referenceType": "Purchase",
  "remarks": "Initial stock addition",
  "movementDate": "2024-01-16T10:00:00Z"
}
```

### Step 3: Check Daily Balance
```
GET /api/Reporting/daily-balances/1/2024-01-16
```

## 📋 **Stock Movement Types**

| Movement Type | Description | Effect on Stock |
|---------------|-------------|-----------------|
| `Addition` | Adding new stock | + (Increases) |
| `Sale` | Selling to customer | - (Decreases) |
| `Return` | Customer return | + (Increases) |
| `TransferIn` | Received from other location | + (Increases) |
| `TransferOut` | Sent to other location | - (Decreases) |

## 🔄 **Complete Workflow Example**

### Day 1: Add 20 Products
1. **Create Product** (if not exists):
   ```json
   POST /api/Product/create
   {
     "itemCode": "JWL001",
     "productName": "Gold Ring",
     "categoryName": "Rings",
     "branchName": "Main Branch",
     "counterName": "Counter 1",
     "rfidCode": "RFID001"
   }
   ```

2. **Create Addition Movement**:
   ```json
   POST /api/Reporting/stock-movements
   {
     "productId": 1,
     "movementType": "Addition",
     "quantity": 20,
     "unitPrice": 15000,
     "totalAmount": 300000,
     "movementDate": "2024-01-16T10:00:00Z"
   }
   ```

3. **Check Daily Balance**:
   ```json
   GET /api/Reporting/daily-balances/1/2024-01-16
   ```
   **Expected Result**:
   ```json
   {
     "openingQuantity": 0,
     "addedQuantity": 20,
     "soldQuantity": 0,
     "closingQuantity": 20
   }
   ```

### Day 2: Sell 10 Products
1. **Create Sale Movement**:
   ```json
   POST /api/Reporting/stock-movements
   {
     "productId": 1,
     "movementType": "Sale",
     "quantity": 10,
     "unitPrice": 15000,
     "totalAmount": 150000,
     "movementDate": "2024-01-17T14:00:00Z"
   }
   ```

2. **Check Daily Balance**:
   ```json
   GET /api/Reporting/daily-balances/1/2024-01-17
   ```
   **Expected Result**:
   ```json
   {
     "openingQuantity": 20,
     "addedQuantity": 0,
     "soldQuantity": 10,
     "closingQuantity": 10
   }
   ```

## 🛠️ **Bulk Stock Movement Creation**

For multiple products at once:
```json
POST /api/Reporting/stock-movements/bulk
{
  "movements": [
    {
      "productId": 1,
      "movementType": "Addition",
      "quantity": 20,
      "unitPrice": 15000,
      "totalAmount": 300000,
      "movementDate": "2024-01-16T10:00:00Z"
    },
    {
      "productId": 2,
      "movementType": "Addition",
      "quantity": 15,
      "unitPrice": 20000,
      "totalAmount": 300000,
      "movementDate": "2024-01-16T10:00:00Z"
    }
  ]
}
```

## 🔍 **Troubleshooting**

### Problem: Getting Zeros in Daily Balance
**Cause**: No stock movements created for that date
**Solution**: Create stock movements using the API above

### Problem: Wrong Opening Quantity
**Cause**: Previous day's balance not calculated
**Solution**: Calculate previous day's balance first:
```json
POST /api/Reporting/daily-balances/calculate/1/2024-01-15
```

### Problem: Missing Product
**Cause**: Product doesn't exist
**Solution**: Create product first, then create stock movement

## 📊 **API Endpoints Summary**

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/Product/create` | POST | Create new product |
| `/api/Reporting/stock-movements` | POST | Create single stock movement |
| `/api/Reporting/stock-movements/bulk` | POST | Create multiple stock movements |
| `/api/Reporting/daily-balances/{productId}/{date}` | GET | Get daily balance |
| `/api/Reporting/daily-balances/calculate/{productId}/{date}` | POST | Calculate daily balance |

## 🎯 **Quick Test Steps**

1. **Create a product** (if not exists)
2. **Create an Addition movement** for that product
3. **Check daily balance** for that date
4. **Create a Sale movement** for the next day
5. **Check daily balance** for the next day

This will show you the complete flow working correctly! 🚀
