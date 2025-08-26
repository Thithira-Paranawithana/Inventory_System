using Inventory_System.Data;
using Inventory_System.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Inventory_System.Services
{
    public class InventoryService
    {
        private readonly InventoryDbContext _context;

        public InventoryService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryDto>> GetInventoryByStoreId(int storeId)
        {
            // validate store exists
            var storeExist = await _context.Stores.AnyAsync(s => s.Id == storeId);

            if (!storeExist)
            {
                throw new ArgumentException("Store not found");
            }

            // get inventory data
            var inventories = await _context.Inventories
                .Include(i => i.Product)    // get related product details (eager loading)
                .Where(i => i.StoreId == storeId)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    SKU = i.Product.SKU,
                    Category = i.Product.Category,
                    CurrentStock = i.CurrentStock,
                    MinStockLevel = i.Product.MinStockLevel,
                    Price = i.Product.Price,
                    LastUpdated = i.LastUpdated

                }).ToListAsync();

            return inventories;
        }



    }
}
