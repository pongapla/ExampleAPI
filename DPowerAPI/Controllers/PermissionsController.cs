using DPowerAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using DPowerAPI.models;

namespace DPowerAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PermissionsController : ControllerBase
{
    private readonly DPowerAPIContext _context;

    public PermissionsController(DPowerAPIContext context)
    {
        _context = context;
    }

    #region CREATE
    [HttpPost]
    public async Task<IActionResult> CreatePermissions([FromBody] Permissions permission)
    {
        if (permission == null)
        {
            return BadRequest("Invalid permission data.");
        }

        var existingPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permission.Name);

        if (existingPermission != null)
        {
            return BadRequest("Permission name is already taken.");
        }

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPermissionsById), new { id = permission.Id }, permission);
    }
    #endregion

    #region READ - Get All Permissions
    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        var activePermissions = await _context.Permissions
            .Where(p => p.Status == "IsActive")
            .ToListAsync();

        if (activePermissions == null || activePermissions.Count == 0)
        {
            return NotFound("No permissions found.");
        }

        return Ok(activePermissions);
    }
    #endregion

    #region READ - Get Permission by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissionsById(int id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return NotFound($"Permission with ID {id} not found.");
        }

        return Ok(permission);
    }
    #endregion

    #region UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePermissions(int id, [FromBody] Permissions updatedPermission)
    {
        if (updatedPermission == null)
        {
            return BadRequest("Invalid permission data.");
        }

        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return NotFound($"Permission with ID {id} not found.");
        }

        var existingPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == updatedPermission.Name && p.Id != id);

        if (existingPermission != null)
        {
            return BadRequest("Permission name is already taken.");
        }

        permission.Name = updatedPermission.Name;
        permission.Description = updatedPermission.Description;
        permission.Status = "IsActive"; // Assuming you want to set the status as active
        permission.CreatedAt = DateTime.UtcNow;

        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return NotFound($"Permission with ID {id} not found.");
        }

        permission.Status = "InActive"; // Mark as inactive instead of deleting
        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion
}
