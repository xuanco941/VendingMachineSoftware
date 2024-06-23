using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StyleX.Models;
using StyleX.Utils;
using StyleX.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace StyleX.Controllers
{
    public class AccessController : Controller
    {
        private readonly DatabaseContext _dbContext;

        public AccessController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? keyActive)
        {
            await HttpContext.SignOutAsync(Common.CookieAuthUser);

            if (!string.IsNullOrEmpty(keyActive))
            {
                Account? user = _dbContext.Accounts.FirstOrDefault(u => u.keyActive == keyActive && u.isActive == false && u.Role == Common.RoleUser);
                if (user != null)
                {
                    user.isActive = true;
                    _dbContext.SaveChanges();
                    ViewBag.IsActive = 1;
                    ViewBag.email = user.Email;
                    ViewBag.password = user.Password;
                }
                else
                {
                    ViewBag.errorActive = "Link kích hoạt không khả dụng.";
                }
            }
            //ClaimsPrincipal claimsPrincipal = HttpContext.User;
            //if (claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.Identity.AuthenticationType == Common.CookieAuthUser)
            //{
            //    return RedirectToAction("Index", "Home");
            //}

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
                Account? user = _dbContext.Accounts.SingleOrDefault(u => u.Email == loginDTO.email && u.Password == loginDTO.password && u.Role == Common.RoleUser);

                if (user != null)
                {
                    if (user.isActive == true)
                    {
                        List<Claim> claims = new List<Claim>() { new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.NameIdentifier, user.AccountID.ToString()), new Claim(ClaimTypes.Role, user.Role) };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        AuthenticationProperties properties = new AuthenticationProperties() { AllowRefresh = true, IsPersistent = true };
                        await HttpContext.SignInAsync(Common.CookieAuthUser, new ClaimsPrincipal(claimsIdentity), properties);
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
        [AllowAnonymous]
        [HttpPost]
        public IActionResult SignUp([FromBody] LoginModel sigupDto)
        {
            try
            {
                sigupDto.email = sigupDto.email.Trim();
                sigupDto.password = sigupDto.password.Trim();
                if (string.IsNullOrEmpty(sigupDto.email) || string.IsNullOrEmpty(sigupDto.password))
                {
                    return new OkObjectResult(new { status = -1, message = "Tài khoản hoặc mật khẩu không được để trống." });
                }

                Account? user = _dbContext.Accounts.FirstOrDefault(u => u.Email == sigupDto.email);
                if (user != null)
                {
                    if (user.isActive == false)
                    {
                        return new OkObjectResult(new { status = -2, message = "Tài khoản đã đăng ký nhưng chưa được kích hoạt." });

                    }
                    return new OkObjectResult(new { status = -3, message = "Tài khoản này đã tồn tại." });

                }
                else
                {
                    string keyActive = Guid.NewGuid().ToString();
                    string linkActive = $"<a href=\"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/Access/Login/?keyActive={keyActive}\">StyleX - Nhấn vào đây để kích hoạt tài khoản của bạn.</a>";

                    _dbContext.Accounts.Add(new Models.Account() { Email = sigupDto.email, Password = sigupDto.password, isActive = false, keyActive = keyActive, Role = Common.RoleUser });
                    if (_dbContext.SaveChanges() > 0)
                    {
                        new SendMail().SendEmailByGmail(sigupDto.email, "Kích hoạt tài khoản", "<html><body>" + linkActive + "</body></html>");
                        return new OkObjectResult(new { status = 1, message = "Đã gửi link kích hoạt về email của bạn." });

                    }
                    else
                    {
                        return new OkObjectResult(new { status = -4, message = "Lỗi hệ thống vui lòng thử lại sau." });
                    }
                }
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Common.CookieAuthUser);

            return RedirectToAction("Login", "Access");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        public async Task<IActionResult> CheckLogin()
        {
            try
            {
                if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
                {
                    var accountID = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(accountID))
                    {
                        return new OkObjectResult(new { status = -1, message = "", data = 0 });
                    }
                    //đếm số item trong cart
                    var countCart = _dbContext.CartItems.Where(e => e.AccountID == Convert.ToInt32(accountID) && e.Status==0).Count();
                    return new OkObjectResult(new { status = 1, message = "success", data = countCart });
                }
                else
                {
                    await HttpContext.SignOutAsync(Common.CookieAuthUser);

                    return new OkObjectResult(new { status = -1, message = "Vui lòng đăng nhập lại.", data = 0 });
                }
            }
            catch (Exception e)
            {
                return new OkObjectResult(new { status = -2, message = e.Message, data = 0 });

            }
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult ForgotPassword([FromBody] LoginModel loginDTO)
        {
            try
            {
                Account? user = _dbContext.Accounts.SingleOrDefault(u => u.Email == loginDTO.email && u.Role == Common.RoleUser);

                if (user != null)
                {
                    if (user.isActive == true)
                    {

                        user.Password = Guid.NewGuid().ToString().Substring(0, 5);
                        _dbContext.SaveChanges();
                        new SendMail().SendEmailByGmail(user.Email, "Đặt lại mật khẩu", $"StyleX - Mật khẩu mới của bạn là: {user.Password}");

                        return new OkObjectResult(new { status = 1, message = "Mật khẩu mới đã được gửi về email của bạn." });
                    }
                    else
                    {
                        return new OkObjectResult(new { status = -1, message = "Tài khoản của bạn chưa được kích hoạt." });
                    }
                }
                else
                {
                    return new OkObjectResult(new { status = -2, message = "Tài khoản không tồn tại." });
                }
            }
            catch (Exception e)
            {
                return new OkObjectResult(new { status = -3, message = e.Message });

            }
        }
    }
}
