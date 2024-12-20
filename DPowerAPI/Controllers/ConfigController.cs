using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DPowerAPI.Data;
using Microsoft.AspNetCore.Authorization;

namespace DPowerApp.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ConfigController : Controller
{
    private readonly DPowerAPIContext _context;

    public ConfigController(DPowerAPIContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAll()
    {
        try
        {
            var usersWithRoles = await (from user in _context.User
                                        join userRole in _context.UserRoles on user.ID equals userRole.UserId into userRoles
                                        from userRole in userRoles.DefaultIfEmpty()
                                        join role in _context.Roles on userRole.RoleId equals role.Id into roles
                                        from role in roles.DefaultIfEmpty()
                                        where user.Status != "InActive"
                                        select new
                                        {
                                            UserId = user.ID,
                                            UserName = user.UserName,
                                            RoleName = role != null ? role.Name : "No Role Assigned"
                                        }).ToListAsync();



            return Ok(usersWithRoles);
        }
        catch (Exception ex)
        {
            
            Console.Error.WriteLine($"Error fetching users with roles: {ex.Message}");

            
            return StatusCode(500, new { message = "An error occurred while fetching users and roles. Please try again later." });
        }
    }

    [HttpGet("GetUserRolesPermissions")]
    public async Task<IActionResult> GetUserRolesPermissions(int userID)
    {
        try
        {
            // ค้นหาข้อมูล roles และ permissions ของ user
            var userRolesPermissions = await (from user in _context.User
                                              join userRole in _context.UserRoles on user.ID equals userRole.UserId into userRoles
                                              from userRole in userRoles.DefaultIfEmpty()  // left join
                                              join role in _context.Roles on userRole.RoleId equals role.Id into roles
                                              from role in roles.DefaultIfEmpty()  // left join
                                              join rolePermission in _context.RolePermissions on role.Id equals rolePermission.RoleId into rolePermissions
                                              from rolePermission in rolePermissions.DefaultIfEmpty()  // left join
                                              join permission in _context.Permissions on rolePermission.PermissionId equals permission.Id into permissions
                                              from permission in permissions.DefaultIfEmpty()  // left join
                                              where user.ID == userID && (permission == null || permission.Status != "InActive")
                                              select new
                                              {
                                                  userName = user.UserName,
                                                  roleId = role != null ? role.Id : (int?)null,
                                                  roleName = role != null ? role.Name : null,
                                                  permissionId = permission != null ? permission.Id : (int?)null,
                                                  permissionName = permission != null ? permission.Name : null
                                              }).ToListAsync();

            
            var allRoles = await _context.Roles.ToListAsync();
            var allPermissions = await _context.Permissions.ToListAsync();

            return Ok(new
            {
                userRolesPermissions, 
                allRoles,
                allPermissions
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }



}
