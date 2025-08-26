using Inventory_System.DTOs;
using Inventory_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiPolicy")] // Requires JWT authentication
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("{storeId}")]
        public async Task<ActionResult<List<InventoryDto>>> GetInventory(int storeId)
        {
            try
            {
                
                // fetch service for data
                var inventory = await _inventoryService.GetInventoryByStoreId(storeId);

                return Ok(new
                {
                    success = true,
                    storeId = storeId,
                    data = inventory
                });
            }
            catch(ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error occurred" });
            }


        }



    }
}
