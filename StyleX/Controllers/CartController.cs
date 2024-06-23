using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleX.DTOs;
using StyleX.Models;
using System.Security.Claims;

namespace StyleX.Controllers
{
    [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
    public class CartController : Controller
    {
        private readonly DatabaseContext _dbContext;
        public CartController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewBag.pageName = "Giỏ hàng";

            return View();
        }
        [HttpPost]
        public IActionResult AddToCart([FromBody] AddToCartModel model)
        {
            try
            {
                string accountID = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var product = _dbContext.Products.Find(model.productID);
                if (string.IsNullOrEmpty(accountID) || product == null || product.Status == false)
                {
                    return new OkObjectResult(new { status = -1, message = "Sản phẩm không khả dụng." });
                }

                //int checkCart = _dbContext.CartItems.Where(e => e.ProductID == model.ID && e.AccountID == Convert.ToInt32(accountID)).Count();
                //if (checkCart > 0)
                //{
                //    return new OkObjectResult(new { status = 2, message = "Sản phẩm đã có trong giỏ hàng." });
                //}

                if (string.IsNullOrEmpty(model.size))
                {
                    model.size = "";
                }
                if (model.amount.HasValue == false || model.amount < 1)
                {
                    model.amount = 1;
                }

                var c = new CartItem()
                {
                    ProductID = model.productID,
                    AccountID = Convert.ToInt32(accountID),
                    Amount = (int)model.amount,
                    Size = model.size,
                    PosterUrl = product.PosterUrl,
                    Price = product.Price,
                    Sale = product.Sale,
                    Status = 0,
                    OrderID = null
                };

                _dbContext.CartItems.Add(c);
                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Thêm vào giỏ hàng thành công!", data = c.CartItemID });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }
        [HttpPost]
        public IActionResult DeleteCartItem([FromBody] IDModel model)
        {
            try
            {
                string accountID = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var cartItem = _dbContext.CartItems.FirstOrDefault(e => e.CartItemID == model.ID && e.AccountID == Convert.ToInt32(accountID));
                if (cartItem == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Không khả dụng." });
                }
                else
                {
                    _dbContext.CartItems.Remove(cartItem);
                    _dbContext.SaveChanges();
                    return new OkObjectResult(new { status = 1, message = "Xóa thành công." });
                }

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }


        [HttpPost]
        public IActionResult GetCarts()
        {
            try
            {
                string accountID = User.FindFirstValue(ClaimTypes.NameIdentifier);

                //status = 0 là những đơn trong cart, design
                var query = from c in _dbContext.CartItems.Include(c => c.Product)
                            join wh in _dbContext.Warehouses on c.ProductID equals wh.ProductID into leftJoinTableW
                            from w in leftJoinTableW.DefaultIfEmpty()
                            where c.AccountID == Convert.ToInt32(accountID) && c.Status == 0
                            select new
                            {
                                c.CartItemID,
                                c.ProductID,
                                c.PosterUrl,
                                c.Amount,
                                c.Size,
                                c.Status,
                                Warehouse = w,
                                c.Product
                            };

                var query2 = from c in query
                             group c by c.CartItemID into g
                             select new
                             {
                                 CartItemID = g.Key,
                                 ProductID = g.First().ProductID,
                                 PosterUrl = g.First().PosterUrl,
                                 Status = g.First().Status,
                                 Amount = g.First().Amount,
                                 Size = g.First().Size,

                                 Warehouses = g.Select(x => x.Warehouse).ToList(),
                                 Product = g.First().Product
                             };
                foreach (var item in query2)
                {
                    if (item.Product != null && item.Product.SaleEndAt != null && item.Product.SaleEndAt < DateTime.Now)
                    {
                        // Đã hết hạn giảm giá
                        item.Product.Sale = 0;
                    }
                }
                return new OkObjectResult(new { status = 1, message = "success", data = query2.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = new List<CartItem>() });
            }

        }

    }
}
