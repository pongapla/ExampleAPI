using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DPowerAPI.Data;
using DPowerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPowerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly DPowerAPIContext _context;

        public ColorController(DPowerAPIContext context)
        {
            _context = context;
        }

        #region CREATE - Add Color
        [HttpPost]
        public async Task<IActionResult> CreateColor([FromBody] Colors color)
        {
           
            if (color == null)
            {
                return BadRequest("Invalid color data.");
            }

            try
            {
               
                var existingColor = await _context.Colors
                    .FirstOrDefaultAsync(c => c.Color_Name == color.Color_Name || c.Code == color.Code);

                if (existingColor != null)
                {
                   
                    return BadRequest("Color name is already taken.");
                }

                
                _context.Colors.Add(color);
                await _context.SaveChangesAsync();

                
                return CreatedAtAction(nameof(GetColorById), new { id = color.ID }, color);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region READ - Get All Colors
        [HttpGet]
        public async Task<IActionResult> GetAllColors()
        {
            var activeColors = await _context.Colors.Where(c => c.Status == "IsActive").ToListAsync();

            if (activeColors == null || activeColors.Count == 0)
            {
                return NotFound("No colors found.");
            }

            return Ok(activeColors);
        }
        #endregion

        #region READ - Get Color by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetColorById(int id)
        {
            var color = await _context.Colors.FindAsync(id);
            if (color == null)
            {
                return NotFound($"Color with ID {id} not found.");
            }

            return Ok(color);
        }
        #endregion

        #region UPDATE - Update Color
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateColor(int id, [FromBody] Colors updatedColor)
        {
            if (updatedColor == null)
            {
                return BadRequest("Invalid color data.");
            }

            var color = await _context.Colors.FindAsync(id);
            if (color == null)
            {
                return NotFound($"Color with ID {id} not found.");
            }

            var existingColor = await _context.Colors.FirstOrDefaultAsync(c => c.Color_Name == updatedColor.Color_Name && c.ID != id);
            if (existingColor != null)
            {
                return BadRequest("Color name is already taken.");
            }

            color.Code = updatedColor.Code;
            color.Color_Name = updatedColor.Color_Name;
            color.Status = "IsActive";
            color.CreatedAt = DateTime.UtcNow;

            _context.Colors.Update(color);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region DELETE - Delete Color (Set InActive)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColor(int id)
        {
            var color = await _context.Colors.FindAsync(id);
            if (color == null)
            {
                return NotFound($"Color with ID {id} not found.");
            }

            color.Status = "InActive";

            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
