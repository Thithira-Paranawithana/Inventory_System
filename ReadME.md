# 7-Eleven Smart Inventory System

## ⚡ Project Setup

### Requirements
- **.NET 9 SDK**
- **SQL Server**
- **Recommended IDE**: Visual Studio 2022 or Visual Studio Code

### Packages Used
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.AspNetCore.Authentication.JwtBearer`

### Configuration

1. Open `appsettings.json`
2. Update SQL Server connection string and JWT settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server={your-server-name};Database=InventoryDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Key": "ThisIsAVerySecretKeyForJWTTokenGenerationAndShouldBeAtLeast32CharactersLong",
    "Issuer": "InventorySystem",
    "Audience": "InventoryUsers"
  }
}
```

### Database Setup

Seeded data is available via `DbInitializer.cs`.

**steps:**
1. Run EF Core migration
2. Data will populate automatically

> **Note**: Some records were manually adjusted; a full SQL script is included in the `Database_Script` folder.

## 👤 Test Users

| Username  | Password     | Role           | Store   |
|-----------|--------------|----------------|---------|
| manager1  | Password@123 | StoreManager   | Store 1 |
| operator1 | Password@123 | StoreOperator  | Store 1 |
| manager2  | Password@123 | StoreManager   | Store 2 |
| operator2 | Password@123 | StoreOperator  | Store 2 |
| manager3  | Password@123 | StoreManager   | Store 3 |
| operator3 | Password@123 | StoreOperator  | Store 3 |
| client1   | Password@123 | Client         | None    |
| client2   | Password@123 | Client         | None    |

## 🔗 API Endpoints

### Authentication
- **POST** `/api/auth/login` – Login and get JWT token
- **POST** `/api/auth/logout` – Logout and clear cookies

### Inventory Management
- **GET** `/api/inventory/{storeId}` – Get inventory for a store
  - **Access**: All roles can view any store's inventory

### Sales Management
- **POST** `/api/sales/transaction` – Record a sales transaction
  - **Access**: 
    - StoreManager/StoreOperator → Only their store
    - ApiClient → Any store

### Smart Algorithms
- **GET** `/api/algorithms/reorder-recommendations/{storeId}` – Get smart reorder suggestions
  - **Access**: Only managers of the specified store
- **GET** `/api/algorithms/abc-analysis/{storeId}` – Get ABC product classification
  - **Access**: Only managers of the specified store

## 📂 API Testing

Postman collection is included in the `APIDoc` folder.

**To use:**
1. Import the JSON file into Postman
2. Test all available APIs

## ⚙ Running the Project

1. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

2. **Apply migrations:** 
   ```bash
   dotnet ef database update
   ```

3. **Build and run the project:**
   ```bash
   dotnet run
   ```

4. **Access APIs** via Postman using the provided collection

