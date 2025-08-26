using Inventory_System.Data;
using Inventory_System.DTOs;
using Inventory_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Inventory_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiPolicy")]
    public class SalesController : ControllerBase
    {
        private readonly SalesService _salesService;
        private readonly InventoryDbContext _context; 

        public SalesController(SalesService salesService, InventoryDbContext context)
        {
            _salesService = salesService;
            _context = context; 
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> RecordindSale([FromBody] SalesTransactionDto salesDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid input", errors = ModelState });
                }

                // check whether store exists 
                var storeExists = await _context.Stores.AnyAsync(s => s.Id == salesDto.StoreId);
                if (!storeExists)
                {
                    return NotFound(new { success = false, message = "Store not found" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var userStoreId = User.FindFirst("StoreId")?.Value;

                // check whether user has access to the store
                if (!_salesService.ValidateSaleAccess(userRole, userStoreId, salesDto.StoreId))
                {
                    return Forbid();
                }

                await _salesService.RecordSale(salesDto);

                return Ok(new
                {
                    success = true,
                    message = "Sale recorded successfully",
                    data = new
                    {
                        storeId = salesDto.StoreId,
                        productId = salesDto.ProductId,
                        quantity = salesDto.Quantity,
                        saleDate = salesDto.SaleDate
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error occurred while recording", error = ex.Message});
            }
        }

    }
}
