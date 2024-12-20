using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DPowerAPI.Data;
using DPowerAPI.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DPowerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceInventoriesController : ControllerBase
    {
        private readonly DPowerAPIContext _context;

        public BalanceInventoriesController(DPowerAPIContext context)
        {
            _context = context;
        }

        // GET: api/BalanceInventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BalanceInventory>>> GetBalanceInventory()
        {
            try
            {
                await _context.CallspGetBalanceInventory();

                var balanceInventory = await _context.BalanceInventory.ToListAsync();

                return Ok(balanceInventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/BalanceInventories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BalanceInventory>> GetBalanceInventory(int id)
        {
            var balanceInventory = await _context.BalanceInventory.FindAsync(id);

            if (balanceInventory == null)
            {
                return NotFound();
            }

            return balanceInventory;
        }

        // PUT: api/BalanceInventories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBalanceInventory(int id, BalanceInventory balanceInventory)
        {
            if (id != balanceInventory.Id)
            {
                return BadRequest();
            }

            _context.Entry(balanceInventory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BalanceInventoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BalanceInventories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BalanceInventory>> PostBalanceInventory(BalanceInventory balanceInventory)
        {
            _context.BalanceInventory.Add(balanceInventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBalanceInventory", new { id = balanceInventory.Id }, balanceInventory);
        }

        // DELETE: api/BalanceInventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBalanceInventory(int id)
        {
            var balanceInventory = await _context.BalanceInventory.FindAsync(id);
            if (balanceInventory == null)
            {
                return NotFound();
            }

            _context.BalanceInventory.Remove(balanceInventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BalanceInventoryExists(int id)
        {
            return _context.BalanceInventory.Any(e => e.Id == id);
        }
    }
}
