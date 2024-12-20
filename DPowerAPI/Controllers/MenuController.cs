using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DPowerAPI.Data;
using DPowerAPI.models;
using Microsoft.EntityFrameworkCore;


namespace DPowerAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MenuController : ControllerBase
{
    private readonly DPowerAPIContext _context;

    public MenuController(DPowerAPIContext context)
    {
        _context = context;
    }
    [HttpPost]
    public async Task<IActionResult> CreateMenu([FromBody] Menu menu)
    {
        if (menu == null)
        {
            return BadRequest("Invalid menu data.");
        }

        var existingRoles = await _context.Menu.FirstOrDefaultAsync(m => m.Name == menu.Name);
        if (existingRoles != null)
        {
            return BadRequest("Menu name is already taken.");
        }

        _context.Menu.Add(menu);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMenuById), new { id = menu.Id }, menu);
    }

    #region READ - Get All Menu
    [HttpGet]
    public async Task<IActionResult> GetAllMenu()
    {
        var activeMenu = await _context.Menu.Where(m => m.Status == "IsActive").ToListAsync();

        if (activeMenu == null || activeMenu.Count == 0)
        {
            return NotFound("No menu found.");
        }
        return Ok(activeMenu);
    }
    #endregion

    #region READ - Get Menu by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenuById(int id)
    {
        var menu = await _context.Menu.FindAsync(id);
        if (menu == null)
        {
            return NotFound($"Menu with ID {id} not found.");
        }
        return Ok(menu);
    }
    #endregion

    #region UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] Menu updatedMenu)
    {

        if (updatedMenu == null)
        {
            return BadRequest("Invalid menu data.");
        }


        var menu = await _context.Menu.FindAsync(id);
        if (menu == null)
        {
            return NotFound($"Mune with ID {id} not found.");
        }


        var existingMenu = await _context.Menu.FirstOrDefaultAsync(m => m.Name == updatedMenu.Name && m.Id != id);
        if (existingMenu != null)
        {
            return BadRequest("Menu name is already taken.");
        }


        menu.Name = updatedMenu.Name;
        menu.Description = updatedMenu.Description;
        menu.Status = "IsActive";
        menu.CreatedAt = DateTime.UtcNow;


        _context.Menu.Update(menu);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        var menu = await _context.Menu.FindAsync(id);
        if (menu == null)
        {
            return NotFound($"Menu with ID {id} not found.");
        }

        menu.Status = "InActive";

        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion
}
