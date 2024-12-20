using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DPowerAPI.Data;
using Microsoft.AspNetCore.Authorization;

namespace DPowerApp.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ConfigMenuController : Controller
{
    private readonly DPowerAPIContext _context;

    public ConfigMenuController(DPowerAPIContext context)
    {
        _context = context;
    }

    // ดึงข้อมูลผู้ใช้ทั้งหมดที่มีเมนูที่มอบหมาย
    [HttpGet]
    public async Task<IActionResult> GetUserAll()
    {
        try
        {
            var usersWithRoles = await (from user in _context.User
                                        join userRole in _context.UserRoles on user.ID equals userRole.UserId into userRoles
                                        from userRole in userRoles.DefaultIfEmpty()  // left join
                                        join role in _context.Roles on userRole.RoleId equals role.Id into roles
                                        from role in roles.DefaultIfEmpty()  // left join
                                        where user.Status != "InActive"
                                        select new
                                        {
                                            UserId = user.ID,
                                            UserName = user.UserName,
                                            RoleName = role != null ? role.Name : "No Role Assigned" // เปลี่ยนจาก MenuName เป็น RoleName
                                        }).ToListAsync();

            return Ok(usersWithRoles);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching users with menus: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching users and menus. Please try again later." });
        }
    }

    // เพิ่ม API ที่รับ userID เป็น Query Parameter
    [HttpGet("GetUserMenus")]
    public async Task<IActionResult> GetUserMenus([FromQuery] int userID)
    {
        try
        {
            var userMenusPermissions = await (from user in _context.User
                                              join userMenu in _context.UserMenu on user.ID equals userMenu.UserId into userMenus
                                              from userMenu in userMenus.DefaultIfEmpty()  // left join
                                              join menu in _context.Menu on userMenu.MenuId equals menu.Id into menus
                                              from menu in menus.DefaultIfEmpty()  // left join
                                              where user.ID == userID && (menu == null || menu.Status != "InActive")
                                              select new
                                              {
                                                  userName = user.UserName,
                                                  menuId = menu != null ? menu.Id : (int?)null,
                                                  menuName = menu != null ? menu.Name : null
                                              }).ToListAsync();


            var allMenus = await _context.Menu.ToListAsync();

            return Ok(new
            {
                userMenusPermissions,
                allMenus
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching user menus and permissions: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching user menus and permissions. Please try again later." });
        }
    }

}