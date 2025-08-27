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
    [Authorize("ApiPolicy")]
    public class AlgorithmsController : ControllerBase
    {
        private readonly ReorderService _reorderService;
        private readonly InventoryDbContext _context;

        public AlgorithmsController(ReorderService reorderService, InventoryDbContext context)
        {
            _reorderService = reorderService;
            _context = context;
        }

        [HttpGet("reorder-recommendations/{storeId}")]
        [Authorize(Roles = "StoreManager")]   // only managers can access
        public async Task<ActionResult<List<ReorderDto>>> GetReorderRecommendation(int storeId)
        {
            try
            {
                // Check if store exists 
                var storeExists = await _context.Stores.AnyAsync(s => s.Id == storeId);
                if (!storeExists)
                {
                    return NotFound(new { success = false, message = "Store not found" });
                }

                // get user info from token
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var userStoreId = User.FindFirst("StoreId")?.Value;

                // validate access permission
                if (!_reorderService.ValidateReorderAccess(userRole, userStoreId, storeId))
                {
                    return Forbid();
                }

                var recommendations = await _reorderService.GetReorderRecommendations(storeId);

                return Ok(new
                {
                    success = true,
                    storeId = storeId,
                    generatedDate = DateTime.UtcNow,
                    estimatedTotalCost = recommendations.Sum(r => r.EstimatedCost),
                    data = recommendations
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Rrror occurred while getting recommendations", error = ex.Message });
            }

        }
    }
}
