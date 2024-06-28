using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using StyleX.Util;
using StyleX.DTOs;
using StyleX.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace StyleX.Controllers
{
    public class VNPayController : Controller
    {
        private readonly DatabaseContext _dbContext;
        public VNPayController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public string url = "http://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public string tmnCode = "VOGPH89L";
        public string hashSecret = "S0N09DFP70T7FY2TZFSENMXTWYN4FO8X";
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Payment([FromBody] List<ItemOrder> itemOrders)
        {
            try
            {
                var request = HttpContext.Request;
                var returnUrl = $"{request.Scheme}://{request.Host}/VNPay/PaymentConfirm";

                if (itemOrders == null || itemOrders.Count < 1)
                {
                    return new OkObjectResult(new { status = -1, message = "Vui lòng chọn sản phẩm trước khi thanh toán.", data = "" });
                }
                PayLib pay = new PayLib();


                var clientIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                double basePrice = 0;
                double netPrice = 0;

                string description = "Thanh toán hóa đơn mua nước giải khát";
                string descriptionInOrder = string.Empty;
                string productNotAvailable = string.Empty;
                for (int i = 0; i < itemOrders.Count; i++)
                {
                    if (itemOrders[i].amount < 1)
                    {
                        productNotAvailable = $"Vui lòng chọn số lượng hợp lệ.";
                        break;
                    }
                    var product = _dbContext.Products.Find(itemOrders[i].productID);
                    if (product == null)
                    {
                        productNotAvailable = $"Sản phẩm có mã {itemOrders[i].productID} đã hết, vui lòng chọn sản phẩm khác.";
                        break;
                    }

                    if (product.NumberAvailable < itemOrders[i].amount)
                    {
                        productNotAvailable = $"Sản phẩm {product.Name} không còn đủ số lượng.";
                        break;
                    }
                    descriptionInOrder = descriptionInOrder + product.Name + $"({product.Price} - {product.Sale}%)x{itemOrders[i].amount} | ";

                    netPrice = netPrice + (product.Price - product.Price * (product.Sale / 100)) * itemOrders[i].amount;
                    basePrice = basePrice + (product.Price) * itemOrders[i].amount;

                    product.NumberAvailable = product.NumberAvailable - itemOrders[i].amount;

                }
                if (!string.IsNullOrEmpty(productNotAvailable))
                {
                    return new OkObjectResult(new { status = -1, message = productNotAvailable, data = "" });
                }

                descriptionInOrder = descriptionInOrder.TrimEnd(' ');
                descriptionInOrder = descriptionInOrder.TrimEnd('|');




                Order order = new Order() { Status = 0, CreateAt = DateTime.Now, UpdateAt = DateTime.Now, BasePrice = basePrice, NetPrice = netPrice, Description = descriptionInOrder };

                _dbContext.Orders.Add(order);

                _dbContext.SaveChanges();




                pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
                pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
                pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
                pay.AddRequestData("vnp_Amount", (netPrice * 100).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
                pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
                pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
                pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
                pay.AddRequestData("vnp_IpAddr", clientIPAddress ?? "127.0.0.1"); //Địa chỉ IP của khách hàng thực hiện giao dịch
                pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
                pay.AddRequestData("vnp_OrderInfo", description); //Thông tin mô tả nội dung thanh toán
                pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
                pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
                pay.AddRequestData("vnp_TxnRef", order.OrderID.ToString()); //mã hóa đơn

                string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
                return new OkObjectResult(new { status = 1, message = "Chuyển trang thanh toán", data = paymentUrl });
            }
            catch
            {
                return new OkObjectResult(new { status = -1, message = "Có lỗi xảy ra, vui lòng thử lại sau", data = "" });

            }

        }
        public IActionResult PaymentConfirm()
        {
            try
            {
                if (Request.QueryString.HasValue)
                {
                    //lấy toàn bộ dữ liệu trả về
                    var queryString = Request.QueryString.Value;
                    var json = HttpUtility.ParseQueryString(queryString);

                    int orderId = Convert.ToInt32(json["vnp_TxnRef"]); //mã hóa đơn
                    string orderInfor = json["vnp_OrderInfo"].ToString(); //Thông tin giao dịch
                    long vnpayTranId = Convert.ToInt64(json["vnp_TransactionNo"]); //mã giao dịch tại hệ thống VNPAY
                    string vnp_ResponseCode = json["vnp_ResponseCode"].ToString(); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                    string vnp_SecureHash = json["vnp_SecureHash"].ToString(); //hash của dữ liệu trả về
                    var pos = Request.QueryString.Value.IndexOf("&vnp_SecureHash");

                    //return Ok(Request.QueryString.Value.Substring(1, pos-1) + "\n" + vnp_SecureHash + "\n"+ PayLib.HmacSHA512(hashSecret, Request.QueryString.Value.Substring(1, pos-1)));
                    bool checkSignature = ValidateSignature(Request.QueryString.Value.Substring(1, pos - 1), vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
                    if (checkSignature && tmnCode == json["vnp_TmnCode"].ToString())
                    {
                        if (vnp_ResponseCode == "00")
                        {
                            //Thanh toán thành công
                            var order = _dbContext.Orders.Find(orderId);
                            if (order != null)
                            {
                                order.Status = 1;
                                _dbContext.SaveChanges();
                            }
                            return RedirectToAction("Index", "Product");
                        }
                        else
                        {
                            //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                            var order = _dbContext.Orders.Find(orderId);
                            if (order != null)
                            {
                                order.Status = 2;
                                _dbContext.SaveChanges();
                            }
                            return RedirectToAction("Index", "Product");
                        }
                    }
                    else
                    {
                        //phản hồi không khớp với chữ ký
                        var order = _dbContext.Orders.Find(orderId);
                        if (order != null)
                        {
                            order.Status = 2;
                            _dbContext.SaveChanges();
                        }
                        return RedirectToAction("Index", "Product");
                    }
                }
                return RedirectToAction("Index", "Product");
            }
            catch
            {
                return RedirectToAction("Index", "Product");
            }

        }
        public bool ValidateSignature(string rspraw, string inputHash, string secretKey)
        {
            string myChecksum = PayLib.HmacSHA512(secretKey, rspraw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}