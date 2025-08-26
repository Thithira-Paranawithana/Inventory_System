using Inventory_System.Data;
using Inventory_System.DTOs;
using Inventory_System.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory_System.Services
{
    public class SalesService
    {
        private readonly InventoryDbContext _context;

        public SalesService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RecordSale(SalesTransactionDto salesDto)
        {
            // begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // check storeId exists
                var storeExist = await _context.Stores.AnyAsync(s => s.Id == salesDto.StoreId);

                if (!storeExist)
                {
                    throw new ArgumentException("Store not found");
                }

                // check product Id and get it to access price
                var product = await _context.Products.FindAsync(salesDto.ProductId);

                if(product == null)
                {
                    throw new ArgumentException("Product not found");
                }

                // get inventory
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.StoreId == salesDto.StoreId && i.ProductId == salesDto.ProductId);

                if (inventory == null)
                {
                    throw new ArgumentException("Inventory not found");
                }

                // check for stock availability
                if (inventory.CurrentStock < salesDto.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock");
                }

                var salesTransaction = new SalesTransaction
                {
                    StoreId = salesDto.StoreId,
                    ProductId = salesDto.ProductId,
                    Quantity = salesDto.Quantity,
                    UnitPrice = product.Price,  // get price from product table
                    SaleDate = salesDto.SaleDate
                };

                _context.SalesTransactions.Add(salesTransaction);

                // update inventory accordingly
                inventory.CurrentStock = inventory.CurrentStock - salesDto.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;

                // EF checks RowVersion for concurrency handling when saving changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();  // commit transaction

                return true;

            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();  // rollback transaction
                throw new InvalidOperationException("Failed. Concurrent stock update.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        // check user has access for the store
        public bool ValidateSaleAccess(string userRole, string? userStoreId, int requestedStoreId)
        {
            if (userRole == "Client") return true;
            if (userStoreId != null && int.Parse(userStoreId) == requestedStoreId) return true;
            return false;
        }


    }
}
