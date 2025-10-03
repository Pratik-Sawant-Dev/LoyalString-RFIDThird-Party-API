# Stock Movement Test Examples for Postman

## 🧪 **Test Scenario: Add 20 Products on Jan 16, Sell 10 on Jan 17**

### Step 1: Create Product (if not exists)
```http
POST {{baseUrl}}/api/Product/create
Authorization: Bearer {{authToken}}
Content-Type: application/json

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

### Step 2: Create Addition Movement (Jan 16)
```http
POST {{baseUrl}}/api/Reporting/stock-movements
Authorization: Bearer {{authToken}}
Content-Type: application/json

{
  "productId": 1,
  "rfidCode": "RFID001",
  "movementType": "Addition",
  "quantity": 20,
  "unitPrice": 15000,
  "totalAmount": 300000,
  "referenceNumber": "ADD-001",
  "referenceType": "Purchase",
  "remarks": "Initial stock addition - 20 gold rings",
  "movementDate": "2024-01-16T10:00:00Z"
}
```

### Step 3: Check Daily Balance (Jan 16)
```http
GET {{baseUrl}}/api/Reporting/daily-balances/1/2024-01-16
Authorization: Bearer {{authToken}}
```

**Expected Response:**
```json
{
  "id": 1,
  "productId": 1,
  "openingQuantity": 0,
  "addedQuantity": 20,
  "soldQuantity": 0,
  "closingQuantity": 20,
  "openingValue": 0.00,
  "closingValue": 300000.00
}
```

### Step 4: Create Sale Movement (Jan 17)
```http
POST {{baseUrl}}/api/Reporting/stock-movements
Authorization: Bearer {{authToken}}
Content-Type: application/json

{
  "productId": 1,
  "rfidCode": "RFID001",
  "movementType": "Sale",
  "quantity": 10,
  "unitPrice": 15000,
  "totalAmount": 150000,
  "referenceNumber": "SALE-001",
  "referenceType": "Customer Sale",
  "remarks": "Sold 10 gold rings to customer",
  "movementDate": "2024-01-17T14:00:00Z"
}
```

### Step 5: Check Daily Balance (Jan 17)
```http
GET {{baseUrl}}/api/Reporting/daily-balances/1/2024-01-17
Authorization: Bearer {{authToken}}
```

**Expected Response:**
```json
{
  "id": 2,
  "productId": 1,
  "openingQuantity": 20,
  "addedQuantity": 0,
  "soldQuantity": 10,
  "closingQuantity": 10,
  "openingValue": 300000.00,
  "closingValue": 150000.00
}
```

### Step 6: Check Daily Balance (Jan 18)
```http
GET {{baseUrl}}/api/Reporting/daily-balances/1/2024-01-18
Authorization: Bearer {{authToken}}
```

**Expected Response:**
```json
{
  "id": 3,
  "productId": 1,
  "openingQuantity": 10,
  "addedQuantity": 0,
  "soldQuantity": 0,
  "closingQuantity": 10,
  "openingValue": 150000.00,
  "closingValue": 150000.00
}
```

## 🔄 **Alternative: Bulk Movement Creation**

### Create Multiple Movements at Once
```http
POST {{baseUrl}}/api/Reporting/stock-movements/bulk
Authorization: Bearer {{authToken}}
Content-Type: application/json

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
      "productId": 1,
      "movementType": "Sale",
      "quantity": 10,
      "unitPrice": 15000,
      "totalAmount": 150000,
      "movementDate": "2024-01-17T14:00:00Z"
    }
  ]
}
```

## 🛠️ **Calculate Missing Balances**

If you need to calculate balances for missing days:
```http
POST {{baseUrl}}/api/Reporting/daily-balances/calculate/1/2024-01-16
Authorization: Bearer {{authToken}}
```

## 📊 **View All Stock Movements**

```http
GET {{baseUrl}}/api/Reporting/stock-movements?productId=1&startDate=2024-01-16&endDate=2024-01-18
Authorization: Bearer {{authToken}}
```

## 🎯 **Quick Test Checklist**

- [ ] Create product (if not exists)
- [ ] Create Addition movement for Jan 16
- [ ] Check daily balance for Jan 16 (should show 20 closing)
- [ ] Create Sale movement for Jan 17
- [ ] Check daily balance for Jan 17 (should show 10 closing)
- [ ] Check daily balance for Jan 18 (should show 10 opening)

## 🚨 **Common Issues & Solutions**

### Issue: "Product not found"
**Solution**: Create the product first using the Product API

### Issue: "Movement type not valid"
**Solution**: Use exact values: "Addition", "Sale", "Return", "TransferIn", "TransferOut"

### Issue: "Date format error"
**Solution**: Use ISO 8601 format: "2024-01-16T10:00:00Z"

### Issue: "Still getting zeros"
**Solution**: Check if the movement date matches the balance date you're querying

This should solve your zero values issue! 🎉
