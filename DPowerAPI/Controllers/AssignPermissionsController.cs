using DPowerAPI.Data;
using DPowerAPI.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AssignPermissionsController : ControllerBase
{
    private readonly DPowerAPIContext _context;

    // Constructor ที่รับ DPowerAPIContext ผ่าน DI (Dependency Injection)
    public AssignPermissionsController(DPowerAPIContext context)
    {
        _context = context;
    }

    // POST: api/AssignPermissions
    [HttpPost]
    public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsRequest request)
    {
        if (request == null || request.RoleID <= 0 || request.UserID <= 0)
        {
            return BadRequest("ข้อมูลไม่ครบถ้วน");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            
            var existingUserRoles = _context.UserRoles
                .Where(ur => ur.UserId == request.UserID)
                .ToList();
            _context.UserRoles.RemoveRange(existingUserRoles);

            
            var existingRolePermissions = _context.RolePermissions
                .Where(rp => rp.RoleId == request.RoleID)
                .ToList();
            _context.RolePermissions.RemoveRange(existingRolePermissions);

            
            var userRole = new UserRoles
            {
                UserId = request.UserID,
                RoleId = request.RoleID 
            };
            _context.UserRoles.Add(userRole);

            
            if (request.Permissions != null && request.Permissions.Count > 0)
            {
                foreach (var permissionId in request.Permissions)
                {
                    var rolePermission = new RolePermissions
                    {
                        RoleId = request.RoleID,
                        PermissionId = permissionId
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "มอบสิทธิ์สำเร็จ" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
        }
    }

    // POST: api/AssignMenuPermissions
    [HttpPost("AssignMenuPermissions")]
    public async Task<IActionResult> AssignMenuPermissions([FromBody] AssignMenuPermissionsRequest request)
    {
        if (request == null || request.UserID <= 0)
        {
            return BadRequest("ข้อมูลไม่ครบถ้วน");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            
            var existingUserMenus = _context.UserMenu
                .Where(um => um.UserId == request.UserID)
                .ToList();
            _context.UserMenu.RemoveRange(existingUserMenus);

            
            foreach (var menuId in request?.MenuIDs)
            {
                var userMenu = new UserMenu
                {
                    UserId = request.UserID,
                    MenuId = menuId
                };
                _context.UserMenu.Add(userMenu);
            }

            // บันทึกการเปลี่ยนแปลงลงฐานข้อมูล
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "มอบสิทธิ์เมนูสำเร็จ" });
        }
        catch (Exception ex)
        {
          
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
        }
    }

}
