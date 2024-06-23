using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleX.DTOs;
using StyleX.Models;
using System.Security.Claims;

namespace StyleX.Controllers
{
    [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
    public class PromotionController : Controller
    {
        private readonly DatabaseContext _dbContext;

        public PromotionController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewBag.pageName = "Quay thưởng";

            return View();
        }

        [HttpPost]
        public IActionResult GetNumberPlay()
        {
            try
            {
                string userEmail = User.FindFirstValue(ClaimTypes.Email);
                Account? user = _dbContext.Accounts.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    return new OkObjectResult(new { status = 1, message = "success", number = user.NumberPlayGame });
                }
                else
                {
                    return new OkObjectResult(new { status = -1, message = "Tài khoản của bạn không tồn tại.", number = 0 });
                }
            }
            catch
            {
                return new OkObjectResult(new { status = -2, message = "Có lỗi xảy ra, vui lòng thử lại sau.", number = 0 });
            }
        }

        [HttpPost]
        public IActionResult GetResult()
        {
            //kết quả của 1 lượt quay là 3 mảng gồm 7 số sắp xếp ngẫu nhiên,
            //trong 3 mảng ấy nếu phần tử thứ 3,4,5 của mỗi mảng mà bằng nhau thì win
            //1 dãy trùng nhau phiếu 10%
            //2 dãy trùng nhau phiếu 20%
            //3 dãy trùng nhau phiếu 30%
            //3 dãy trùng nhau và 1 trong 3 có phần tử là 1 thì nhận phiếu 40%

            try
            {
                string userEmail = User.FindFirstValue(ClaimTypes.Email);
                Account? user = _dbContext.Accounts.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    if (user.NumberPlayGame > 0)
                    {
                        user.NumberPlayGame = user.NumberPlayGame - 1;
                        var result1 = Utils.Utils.GenerateRandomArray(1, 7, 7);
                        var result2 = Utils.Utils.GenerateRandomArray(1, 7, 7);
                        var result3 = Utils.Utils.GenerateRandomArray(1, 7, 7);

                        int count = 0;
                        if (result1[2] == result2[2] && result2[2] == result3[2])
                        {
                            count++;
                        }
                        if (result1[3] == result2[3] && result2[3] == result3[3])
                        {
                            count++;
                        }
                        if (result1[4] == result2[4] && result2[4] == result3[4])
                        {
                            count++;
                        }
                        if (count == 3 && (result1[2] == 1 || result1[3] == 1 || result1[4] == 1))
                        {
                            count++;
                        }
                        if (count > 0)
                        {

                            string resultString = "";
                            for (int i = 2; i < 5; i++)
                            {
                                resultString = resultString + $"{result1[i]}-{result2[i]}-{result3[i]}  ";
                            }
                            DateTime createAt = DateTime.Now;
                            _dbContext.Promotions.Add(new Promotion() { AccountID = user.AccountID, Status = false, Number = count * 10, ResultSpin = resultString.Trim(), CreateAt = createAt, ExpiredAt = createAt.AddDays(30) });
                        }
                        _dbContext.SaveChanges();

                        return new OkObjectResult(new { status = 1, message = "success", result1, result2, result3, numberSale = count * 10, numberPlayGame = user.NumberPlayGame }); ;

                    }
                    else
                    {
                        return new OkObjectResult(new { status = -1, message = "Bạn không còn lượt quay nào." });
                    }
                }
                else
                {
                    return new OkObjectResult(new { status = -2, message = "Tài khoản của bạn không tồn tại." });
                }
            }
            catch
            {
                return new OkObjectResult(new { status = -3, message = "Có lỗi xảy ra, vui lòng thử lại sau." });
            }
        }
        [HttpPost]
        public IActionResult GetPromotionAccount()
        {
            try
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                List<Promotion> promotion = _dbContext.Promotions.Where(p => p.Status == false && p.ExpiredAt > DateTime.Now && p.AccountID == Convert.ToInt32(id)).ToList();
                return new OkObjectResult(new { status = 1, message = "success", data = promotion });
            }
            catch
            {
                return new OkObjectResult(new { status = -2, message = "Có lỗi xảy ra, vui lòng thử lại sau.", number = 0 });
            }
        }
    }

}

