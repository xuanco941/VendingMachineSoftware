using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleX.DTOs;
using StyleX.Models;
using System.Security.Claims;
using System.Collections.Generic;

namespace StyleX.Controllers
{
    [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
    public class OrderController : Controller
    {
        private readonly DatabaseContext _dbContext;

        public OrderController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpPost]
        public IActionResult GetOrderItems([FromBody] IDModel iDModel)
        {
            try
            {
                List<CartItem>? list = _dbContext.CartItems.Include(d => d.Product).Where(u => u.OrderID == iDModel.ID).ToList();
                if (list == null)
                {
                    list = new List<CartItem>();
                }
                return new OkObjectResult(new { status = 1, message = "success.", data = list });

            }
            catch (Exception e)
            {
                return new OkObjectResult(new { status = -2, message = e.Message, data = "" });

            }
        }
        public IActionResult CreateOrder([FromBody] AddOrderModel model)
        {
            var resultResponse= new OkObjectResult(new { status = -99, message = "Lỗi hệ thống." });
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            executionStrategy.Execute(() =>
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        string accountID = User.FindFirstValue(ClaimTypes.NameIdentifier);

                        var account = _dbContext.Accounts.Find(Convert.ToInt32(accountID));

                        if (model == null)
                        {
                            transaction.Rollback();

                            resultResponse = new OkObjectResult(new { status = -2, message = "Vui lòng nhập đẩy đủ các trường thiết yếu." });
                            return;
                        }

                        if (account == null)
                        {
                            transaction.Rollback();
                            resultResponse = new OkObjectResult(new { status = -1, message = "Tài khoản không khả dụng." });
                            return;
                        }

                        if (string.IsNullOrEmpty(model.name) || string.IsNullOrEmpty(model.address) || string.IsNullOrEmpty(model.phoneNumber))

                        {
                            transaction.Rollback();
                            resultResponse = new OkObjectResult(new { status = -2, message = "Vui lòng nhập đẩy đủ các trường thiết yếu." });
                            return;
                        }
                        if (model.itemOrders == null || model.itemOrders.Count < 1)
                        {
                            transaction.Rollback();
                            resultResponse = new OkObjectResult(new { status = -3, message = "Chưa có sản phẩm." });
                            return;
                        }
                        else
                        {
                            int check = model.itemOrders.Where(e => e.amount < 1 || string.IsNullOrEmpty(e.size) == true).Count();

                            if (check > 0)
                            {
                                transaction.Rollback();
                                resultResponse = new OkObjectResult(new { status = -4, message = "Tạo đơn thất bại, số lượng hoặc kích cỡ không phù hợp." });
                                return;
                            }
                        }
                        double percentSale = 0;
                        double tongTien = 0;

                        if (model.promotionID != null && model.promotionID > 0)
                        {
                            var promotion = _dbContext.Promotions.Find(model.promotionID);
                            if (promotion != null && promotion.Status == false && promotion.ExpiredAt > DateTime.Now)
                            {
                                percentSale = promotion.Number;
                            }
                            else
                            {
                                transaction.Rollback();
                                resultResponse = new OkObjectResult(new { status = -8, message = "Phiếu giảm giá không khả dụng." });
                                return;
                            }
                        }

                        DateTime now = DateTime.Now;

                        var order = new Order()
                        {
                            Status = 0,
                            AccountID = account.AccountID,
                            Address = model.address,
                            PhoneNumber = model.phoneNumber,
                            Name = model.name,
                            TransportFee = Common.TransportFee,
                            CreateAt = now,
                            UpdateAt = now,
                            Message = model.message ?? "",
                            PercentSale = percentSale,
                            BasePrice = tongTien,
                            NetPrice = tongTien - (tongTien * percentSale / 100) + Common.TransportFee
                        };
                        _dbContext.Orders.Add(order);
                        _dbContext.SaveChanges();



                        if (model.promotionID.HasValue)
                        {
                            var promotion = _dbContext.Promotions.Find(model.promotionID);
                            if (promotion != null)
                            {
                                promotion.OrderID = order.OrderID;
                                promotion.Status = true; // đánh dấu promotion đã được dùng
                                promotion.UsedAt = now;
                            }

                        }


                        foreach (var item in model.itemOrders)
                        {
                            var iCart = _dbContext.CartItems.Include(e => e.Product).FirstOrDefault(e => e.CartItemID == item.cartItemID);
                            var w = _dbContext.Warehouses.Find(item.warehouseID);

                            if (iCart != null && w != null)
                            {
                                if (iCart.Product.Status == false)
                                {
                                    transaction.Rollback();
                                    resultResponse = new OkObjectResult(new { status = -6, message = $"Tạo đơn thất bại, {iCart.Product.Name} đang ngừng bán." });
                                    return;
                                }
                                if (w.Amount < item.amount)
                                {
                                    transaction.Rollback();
                                    resultResponse = new OkObjectResult(new { status = -7, message = $"Tạo đơn thất bại, số dư sản phẩm {iCart.Product.Name} trong kho không đủ, tối đa {w.Amount}" });
                                    return;
                                }

                                double saleC = 0;
                                if (iCart.Product.SaleEndAt > DateTime.Now)
                                {
                                    saleC = iCart.Product.Sale;
                                }
                                tongTien += (iCart.Product.Price * item.amount) - (iCart.Product.Price * item.amount * saleC / 100);


                                iCart.Size = item.size;
                                iCart.Amount = item.amount;
                                iCart.Status = 1;
                                iCart.OrderID = order.OrderID;
                                iCart.Sale = saleC;
                                iCart.Price = iCart.Product.Price;

                                w.Amount = w.Amount - item.amount;

                            }
                            else
                            {
                                transaction.Rollback();
                                resultResponse = new OkObjectResult(new { status = -6, message = "Tạo đơn thất bại." });
                                return;

                            }
                        }

                        order.BasePrice = tongTien;
                        order.NetPrice = tongTien - (tongTien * percentSale / 100) + Common.TransportFee;






                        _dbContext.SaveChanges();

                        transaction.Commit();
                        resultResponse = new OkObjectResult(new { status = 1, message = "Đặt hàng thành công!" });
                        return;

                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();

                        resultResponse = new OkObjectResult(new { status = -99, message = e.Message });
                        return;
                    }

                }

            });

            return resultResponse;

        }
    }
}