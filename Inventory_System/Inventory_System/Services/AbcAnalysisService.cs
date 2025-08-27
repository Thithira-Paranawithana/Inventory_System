using Inventory_System.Data;
using Inventory_System.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Inventory_System.Services
{
    public class AbcAnalysisService
    {
        private readonly InventoryDbContext _context;

        public AbcAnalysisService(InventoryDbContext context)
        {
            _context = context;
        }


        public async Task<List<AbcAnalysisDto>> getAbcAnalysis(int storeId)
        {

            // find revenue of products
            var productRevenues = await _context.SalesTransactions
                .Where(s => s.StoreId == storeId)
                .Include(s => s.Product)
                .GroupBy(s => new { s.ProductId, s.Product.Name, s.Product.SKU, s.Product.Category })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    SKU = g.Key.SKU,
                    Category = g.Key.Category,
                    TotalRevenue = g.Sum(s => s.Quantity * s.UnitPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)     // sort by revenue in descending order
                .ToListAsync();

            if (!productRevenues.Any())
            {
                return new List<AbcAnalysisDto>();
            }

            // whole revenue of all products
            var totalRevenue = productRevenues.Sum(p => p.TotalRevenue);

            var results = new List<AbcAnalysisDto>();

            decimal cumulativeRevenue = 0;

            // loop through sorted product revenue list
            foreach(var product in productRevenues)
            {
                // calculate cumulative revenue
                cumulativeRevenue += product.TotalRevenue;

                // revenue percentage of a single product from total revenue
                var revenuePercentage = totalRevenue > 0 ? (double)(product.TotalRevenue / totalRevenue) * 100 : 0;

                // cumulative revenue percentage up to the product from the sorted list from top
                var cumulativePercentage = totalRevenue > 0 ? (double)(cumulativeRevenue / totalRevenue) * 100 : 0;

                // ABC categorization
                char abcCategory;

                if (cumulativePercentage <= 80)
                    abcCategory = 'A';
                else if (cumulativePercentage <= 95)
                    abcCategory = 'B';
                else
                    abcCategory = 'C';

                // add to results to show to the manager
                results.Add(new AbcAnalysisDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    SKU = product.SKU,
                    Category = product.Category,
                    TotalRevenue = product.TotalRevenue,
                    RevenuePercentage = Math.Round(revenuePercentage, 2),
                    CumulativePercentage = Math.Round(cumulativePercentage, 2),
                    AbcCategory = abcCategory
                });

            }

            return results;

        }

       

    }
}
