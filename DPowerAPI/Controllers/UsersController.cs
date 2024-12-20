
using Microsoft.AspNetCore.Mvc;
using DPowerAPI.Data;
using DPowerAPI.models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace DPowerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DPowerAPIContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        public UsersController(DPowerAPIContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }
        

        // POST: api/Login
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<User>> Login(LoginModel login)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserName == login.userName);
            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password!, login.password!);
                if (result != PasswordVerificationResult.Failed)
                {
                    // ดึงข้อมูล Roles
                    var roles = await (from ur in _context.UserRoles
                                       join r in _context.Roles on ur.RoleId equals r.Id into roleGroup
                                       from r in roleGroup.DefaultIfEmpty()
                                       where ur.UserId == user.ID
                                       select r.Name).ToListAsync();

                    // ดึงข้อมูล Permissions
                    var permissions = await (from ur in _context.UserRoles
                                             join r in _context.Roles on ur.RoleId equals r.Id
                                             join rp in _context.RolePermissions on r.Id equals rp.RoleId
                                             join p in _context.Permissions on rp.PermissionId equals p.Id
                                             where ur.UserId == user.ID
                                             select p.Name).Distinct().ToListAsync();

                    // ดึงข้อมูล menuPermissions
                    var menuPermissions = await (from um in _context.UserMenu
                                                 join m in _context.Menu on um.MenuId equals m.Id
                                                 where um.UserId == user.ID
                                                 select m.Name).ToListAsync();

                    // สร้าง claims สำหรับ JWT
                    var claims = new[]
                    {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserName", user.UserName!),
                new Claim("UserID", user.ID.ToString()),
                new Claim("Roles", string.Join(",", roles)),
                new Claim("Permissions", string.Join(",", permissions)),
                new Claim("MenuPermissions", string.Join(",", menuPermissions)) // เพิ่มข้อมูลเมนูใน claims
            };

                    // สร้างการเซ็นต์ Token
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddDays(1),
                        signingCredentials: signIn
                    );

                    // สร้าง Token เป็น string
                    string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                    // ส่งผลลัพธ์กลับไปพร้อมกับ Token และข้อมูล User
                    return Ok(new
                    {
                        Token = tokenValue,
                        User = user,
                        Roles = roles,
                        Permissions = permissions,
                        MenuPermissions = menuPermissions // ส่งเมนูที่ผู้ใช้สามารถเข้าถึงได้
                    });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }
            }

            return NoContent();
        }


        [Authorize]
        [HttpGet]
        // GET: api/Users
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            var activeUsers = await _context.User.Where(u => u.Status == "IsActive").ToListAsync();

            return Ok(activeUsers);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            var existingUser = await _context.User.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound($"User with id {id} not found.");
            }

            // ตรวจสอบว่ามีการเปลี่ยนแปลงใน userName หรือไม่
            if (existingUser.UserName != user.UserName)
            {
                var userNameExists = await _context.User.AnyAsync(u => u.UserName == user.UserName);
                if (userNameExists)
                {
                   
                    return BadRequest("Username is already taken.");
                }
            }

            
            existingUser.UserName = user.UserName;
            existingUser.Password = _passwordHasher.HashPassword(user, user.Password!);
            existingUser.Email = user.Email;
            existingUser.CreatedAt = DateTime.UtcNow;

            
            try
            {
                _context.Entry(existingUser).State = EntityState.Modified; 
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
                if (!UserExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            try
            {
                
                if (string.IsNullOrEmpty(user.UserName))
                {
                    return BadRequest("Username cannot be null or empty.");
                }

                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.UserName == user.UserName);
                if (existingUser != null)
                {
                   
                    return BadRequest("Username is already taken.");
                }

               
                if (string.IsNullOrEmpty(user.Password))
                {
                    return BadRequest("Password cannot be null or empty.");
                }

                user.Password = _passwordHasher.HashPassword(user, user.Password);

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetUser", new { id = user.ID }, user);
            }
            catch (Exception ex)
            {
                
                return BadRequest($"Error: {ex.Message}");
            }
        }



        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {

            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Status = "InActive";

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.ID == id);
        }
    }
}
