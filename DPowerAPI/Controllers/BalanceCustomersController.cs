using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DPowerAPI.Data;
using DPowerAPI.models;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

namespace DPowerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceCustomersController : ControllerBase
    {
        private readonly DPowerAPIContext _context;

        public BalanceCustomersController(DPowerAPIContext context)
        {
            _context = context;
        }

        // GET: api/BalanceCustomers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BalanceCustomer>>> GetBalanceCustomer([FromHeader] int? page, [FromHeader]  int? pageSize)
        {
            await _context.CallspGetBalanceCustomer();

            page = page ?? 1;
            pageSize = pageSize ?? 10;
            
            try
            {
                var skip = (page.Value - 1) * pageSize.Value;
                var take = pageSize.Value;

                var customers = await _context.BalanceCustomer
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        // GET: api/BalanceCustomers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BalanceCustomer>> GetBalanceCustomer(int id)
        {
            var balanceCustomer = await _context.BalanceCustomer.FindAsync(id);

            if (balanceCustomer == null)
            {
                return NotFound();
            }

            return balanceCustomer;
        }

        // PUT: api/BalanceCustomers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBalanceCustomer(int id, BalanceCustomer balanceCustomer)
        {
            if (id != balanceCustomer.ID)
            {
                return BadRequest();
            }

            _context.Entry(balanceCustomer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BalanceCustomerExists(id))
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

        // POST: api/BalanceCustomers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BalanceCustomer>> PostBalanceCustomer(BalanceCustomer balanceCustomer)
        {
            _context.BalanceCustomer.Add(balanceCustomer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBalanceCustomer", new { id = balanceCustomer.ID }, balanceCustomer);
        }

        // DELETE: api/BalanceCustomers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBalanceCustomer(int id)
        {
            var balanceCustomer = await _context.BalanceCustomer.FindAsync(id);
            if (balanceCustomer == null)
            {
                return NotFound();
            }

            _context.BalanceCustomer.Remove(balanceCustomer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BalanceCustomerExists(int id)
        {
            return _context.BalanceCustomer.Any(e => e.ID == id);
        }
    }
}
