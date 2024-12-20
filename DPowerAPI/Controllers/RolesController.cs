using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DPowerAPI.models;
using DPowerAPI.Data;
using Microsoft.AspNetCore.Authorization;

namespace DPowerAPI.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly DPowerAPIContext _context;

    public RolesController(DPowerAPIContext context)
    {
        _context = context;
    }

    #region CREATE
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] Roles role)
    {
        if (role == null)
        {
            return BadRequest("Invalid role data.");
        }

        var existingRoles = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role.Name);
        if (existingRoles != null)
        {
            return BadRequest("Role name is already taken.");
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
    }
    #endregion

    #region READ - Get All Roles
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var activeRoles = await _context.Roles.Where(r => r.Status == "IsActive").ToListAsync();

        if (activeRoles == null || activeRoles.Count == 0)
        {
            return NotFound("No roles found.");
        }
        return Ok(activeRoles);
    }
    #endregion

    #region READ - Get Role by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound($"Role with ID {id} not found.");
        }
        return Ok(role);
    }
    #endregion

    #region UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] Roles updatedRole)
    {
        
        if (updatedRole == null)
        {
            return BadRequest("Invalid role data.");
        }

        
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound($"Role with ID {id} not found.");
        }

        
        var existingRoles = await _context.Roles.FirstOrDefaultAsync(r => r.Name == updatedRole.Name && r.Id != id);
        if (existingRoles != null)
        {
            return BadRequest("Role name is already taken.");
        }

       
        role.Name = updatedRole.Name;
        role.Description = updatedRole.Description;
        role.Status = "IsActive";
        role.CreatedAt = DateTime.UtcNow;

        
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound($"Role with ID {id} not found.");
        }

        role.Status = "InActive";
        
        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion
}
