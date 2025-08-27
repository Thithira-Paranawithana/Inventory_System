using Inventory_System.Data;
using Inventory_System.DTOs;
using Inventory_System.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Inventory_System.Services
{
    public class ReorderService
    {
        private readonly InventoryDbContext _context;

        public ReorderService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReorderDto>> GetReorderRecommendations(int storeId)
        {
            
            var recommendations = new List<ReorderDto>();

            var lastThirtyDays = DateTime.UtcNow.AddDays(-30);

            // get all products for the store
            var inventories = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.StoreId == storeId)
                .ToListAsync();

            foreach(var inventory in inventories)
            {
                // get sales data for last 30 days
                var salesData = await _context.SalesTransactions
                    .Where(s => s.StoreId == storeId && s.ProductId == inventory.ProductId && s.SaleDate >= lastThirtyDays)
                    .ToListAsync();

                // check if sales data exists
                if (!salesData.Any())
                    continue;

                var recommendation = CalculateReorderRecommendation(inventory, salesData);

                if (recommendation != null)
                {
                    recommendations.Add(recommendation);
                }

            }

            return recommendations;

        }

        private ReorderDto? CalculateReorderRecommendation(Inventory inventory, List<SalesTransaction> salesData)
        {
            // average daily sales
            var totalSales = salesData.Sum(s => s.Quantity);
            var avgDailySales = (double)totalSales / 30;

            // seasonality factor
            var weekdayCount = salesData.Count(s => s.SaleDate.DayOfWeek != DayOfWeek.Saturday && s.SaleDate.DayOfWeek != DayOfWeek.Sunday);
            var weekendCount = salesData.Count(s => s.SaleDate.DayOfWeek == DayOfWeek.Saturday || s.SaleDate.DayOfWeek == DayOfWeek.Sunday);
            var seasonalityFactor = weekdayCount > 0 || weekendCount > 0 ? (weekdayCount * 0.8 + weekendCount * 1.4) / 7 : 1.0;

            //adjusted sales
            var adjustedSales = avgDailySales * seasonalityFactor;

            // safety stock
            var safetyStock = adjustedSales * 2;

            // reorder point
            var reorderPoint = adjustedSales * inventory.Product.LeadTimeDays - safetyStock;

            // reorder quantity
            var reorderQty = reorderPoint - inventory.CurrentStock;

            if (reorderQty <= 0)
                return null;    // no reorder needed

            // round upto nearest 10 units
            var recommendedQty = (int)(Math.Ceiling(reorderQty / 10.0) * 10);

            // check for max storage quantity
            if(recommendedQty + inventory.CurrentStock > inventory.Product.MaxStorageQty)
            {
                recommendedQty = Math.Max(0, inventory.Product.MaxStorageQty - inventory.CurrentStock);
            }

            if (recommendedQty <= 0)
                return null;

            // priority
            string priority;

            if (inventory.CurrentStock <= inventory.Product.MinStockLevel)
                priority = "High";
            else if (recommendedQty > 50)
                priority = "Medium";
            else
                priority = "Low";


            // Reason
            var reason = $"Avg daily sales: {avgDailySales}, Seasonality factor: {seasonalityFactor}, Current stock: { inventory.CurrentStock}, Reorder point: {reorderPoint}";

            return new ReorderDto
            {
                ProductId = inventory.Product.Id,
                ProductName = inventory.Product.Name,
                SKU = inventory.Product.SKU,
                Category = inventory.Product.Category,
                CurrentStock = inventory.CurrentStock,
                MinStockLevel = inventory.Product.MinStockLevel,
                RecommendedQuantity = recommendedQty,
                Reason = reason,
                EstimatedCost = recommendedQty * inventory.Product.Price,
                Priority = priority,
                GeneratedDate = DateTime.UtcNow
            };
        }

        // check user has access
        public bool ValidateReorderAccess(string userRole, string? userStoreId, int requestedStoreId)
        {
            // only managers can access 
            if (userRole != "StoreManager")
                return false;

            if (userStoreId == null)
                return false;

            // managers can only access their store
            return int.Parse(userStoreId) == requestedStoreId;
        }
    }
}
