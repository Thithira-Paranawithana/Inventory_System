using Inventory_System.Entities;

namespace Inventory_System.Data
{
    public static class DbInitializer
    {
        public static void Initialize(InventoryDbContext context)
        {
          
            context.Database.EnsureCreated();

            if (context.Stores.Any())
            {
                return;   // means already seeded
            }

            //if not seed

            //seed stores
            var stores = new Store[]
            {
                new Store { Name = "7-Eleven_North", Code = "2025N", Address = "123 Main St, North" },
                new Store { Name = "7-Eleven_South", Code = "2025S", Address = "456 High St, South" },
                new Store { Name = "7-Eleven_West", Code = "2025W", Address = "789 Low st, West" }
            };

            context.Stores.AddRange(stores);
            context.SaveChanges();

            // seed products 
            var products = new List<Product>();
            var categories = new[] { "Beverages", "Snacks", "Dairy", "Bakery", "Frozen", "Personal Care" };
            var productNames = new[]
            {
                "Coca Cola", "Pepsi", "Water Bottle", "Orange Juice", "Coffee",
                "Chips", "Chocolate Bar", "Cookies", "Candy", "Nuts",
                "Milk", "Yogurt", "Cheese", "Butter", "Ice Cream",
                "Bread", "Croissant", "Muffin", "Bagel", "Pizza",
                "Frozen Vegetables", "Ice Cubes", "Frozen Meals", "Soap", "Shampoo",
                "Toothpaste", "Tissue", "Paper Towels", "Detergent", "Batteries",
                "Phone Charger", "Headphones", "Hand Sanitizer", "Face Mask", "Magazines"
            };

            for (int i = 0; i < 35; i++)
            {
                products.Add(new Product
                {
                    SKU = $"SKU{i + 1:D3}",
                    Name = productNames[i % productNames.Length] + $" {i + 1}",
                    Category = categories[i % categories.Length],
                    Price = Math.Round((decimal)(2 + i * 0.5), 2),
                    MinStockLevel = 5 + (i % 15),
                    LeadTimeDays = 1 + (i % 7),
                    MaxStorageQty = 50 + (i * 10)
                });
            }

            context.Products.AddRange(products);
            context.SaveChanges();

            // seed users 
            var users = new User[]
            {
                new User
                {
                    Username = "operator1",
                    Email = "operator1@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "StoreOperator",
                    StoreId = stores[0].Id
                },
                new User
                {
                    Username = "manager1",
                    Email = "manager1@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "StoreManager",
                    StoreId = stores[0].Id
                },
                new User
                {
                    Username = "operator2",
                    Email = "operator2@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "StoreOperator",
                    StoreId = stores[1].Id
                },
                new User
                {
                    Username = "manager2",
                    Email = "manager2@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "StoreManager",
                    StoreId = stores[1].Id
                },
                 new User
                {
                    Username = "operator3",
                    Email = "operator3@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "StoreOperator",
                    StoreId = stores[2].Id
                },
                new User
                {
                    Username = "manager3",
                    Email = "manager3@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "StoreManager",
                    StoreId = stores[2].Id
                },
                new User
                {
                    Username = "client1",
                    Email = "client1@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "Client",
                    StoreId = null
                },
                new User
                {
                    Username = "client2",
                    Email = "client2@2025.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    Role = "Client",
                    StoreId = null
                }

            };

            context.Users.AddRange(users);
            context.SaveChanges();

            // seed inventory
            var inventories = new List<Inventory>();
            var random = new Random(42); 

            foreach (var store in stores)
            {
                foreach (var product in products)
                {
                    inventories.Add(new Inventory
                    {
                        ProductId = product.Id,
                        StoreId = store.Id,
                        CurrentStock = random.Next(product.MinStockLevel - 5, product.MaxStorageQty / 3),
                        LastUpdated = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                    });
                }
            }

            context.Inventories.AddRange(inventories);
            context.SaveChanges();


            // seed 3 months of sales data 
            var salesTransactions = new List<SalesTransaction>();
            var startDate = DateTime.UtcNow.AddDays(-90);

            for (int day = 0; day < 90; day++)
            {
                var currentDate = startDate.AddDays(day);
                var isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;

                // more transactions on weekends
                var transactionsPerDay = isWeekend ? random.Next(15, 25) : random.Next(8, 15);

                for (int i = 0; i < transactionsPerDay; i++)
                {
                    var store = stores[random.Next(stores.Length)];
                    var product = products[random.Next(products.Count)];
                    var quantity = random.Next(1, 4);

                    salesTransactions.Add(new SalesTransaction
                    {
                        StoreId = store.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = Math.Round(product.Price * (decimal)(0.9 + random.NextDouble() * 0.2), 2),
                        SaleDate = currentDate.AddHours(random.Next(8, 22)).AddMinutes(random.Next(0, 59))
                    });
                }
            }

            context.SalesTransactions.AddRange(salesTransactions);
            context.SaveChanges();

        }
    }
}
