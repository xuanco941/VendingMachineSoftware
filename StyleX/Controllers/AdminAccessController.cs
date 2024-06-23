using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StyleX.Models;
using StyleX.Utils;
using StyleX.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace StyleX.Controllers
{
    public class AdminAccessController : Controller
    {
        private readonly DatabaseContext _dbContext;

        public AdminAccessController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(Common.CookieAuthAdmin);

            ViewBag.Title = "CMS - Đăng nhập";

            return View();

        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] LoginModel loginDTO)
        {
            if (string.IsNullOrEmpty(loginDTO.email) || string.IsNullOrEmpty(loginDTO.password))
            {
                return new OkObjectResult(new { status = -4, message = "Tài khoản hoặc mật khẩu không được để trống." });
            }
            try
            {
                Account? user = _dbContext.Accounts.SingleOrDefault(u => u.Email == loginDTO.email && u.Password == loginDTO.password && u.Role == Common.RoleAdmin);

                if (user != null)
                {
                    if (user.isActive == true)
                    {
                        List<Claim> claims = new List<Claim>() { new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.NameIdentifier, user.AccountID.ToString()), new Claim(ClaimTypes.Role, user.Role) };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        AuthenticationProperties properties = new AuthenticationProperties() { AllowRefresh = true, IsPersistent = true };
                        await HttpContext.SignInAsync(Common.CookieAuthAdmin, new ClaimsPrincipal(claimsIdentity), properties);
                        return new OkObjectResult(new { status = 1, message = "/" });

                    }
                    else
                    {
                        return new OkObjectResult(new { status = -1, message = "Tài khoản của bạn chưa được kích hoạt." });
                    }
                }
                else
                {
                    return new OkObjectResult(new { status = -2, message = "Tài khoản hoặc mật khẩu không chính xác." });
                }
            }
            catch (Exception e)
            {
                return new OkObjectResult(new { status = -3, message = e.Message });

            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Common.CookieAuthAdmin);

            return RedirectToAction("Login", "AdminAccess");
        }
    }
}
