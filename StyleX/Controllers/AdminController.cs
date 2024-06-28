using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StyleX.DTOs;
using StyleX.Models;
using StyleX.Utils;
using System.Diagnostics;

namespace StyleX.Controllers
{
    [Authorize(AuthenticationSchemes = Common.CookieAuthAdmin)]
    public class AdminController : Controller
    {
        private readonly DatabaseContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public AdminController(DatabaseContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }
        [HttpGet]
        [Route("/admin")]
        public IActionResult Index()
        {
            DateTime currentDate = DateTime.Now;

            // Lấy ngày đầu tiên của tháng
            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
            var endDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var orders = _dbContext.Orders.Where(e => e.CreateAt >= firstDayOfMonth && e.CreateAt < endDayOfMonth).ToList();
            var orders2 = _dbContext.Orders.Where(e => e.CreateAt >= firstDayOfMonth.AddMonths(-1) && e.CreateAt < endDayOfMonth.AddMonths(-1)).ToList();


            ViewBag.date1 = "[]";
            ViewBag.value1 = "[]";
            ViewBag.value2 = "[]";
            ViewBag.date3 = "[]";
            ViewBag.value3 = "[]";
            ViewBag.value4 = "[]";
            ViewBag.count = 0;
            ViewBag.money = 0;

            if (orders != null)
            {
                var orderSuccess = orders.Where(e => e.Status == 2).ToList();
                if (orderSuccess != null && orderSuccess.Count > 0)
                {
                    ViewBag.count = orderSuccess.Count;
                    ViewBag.money = orderSuccess.Sum(e => e.NetPrice).ToString("#,##0");
                }
                else
                {
                    ViewBag.count = 0;
                    ViewBag.money = 0;
                }

                List<string> dateList = new List<string>();
                List<int> valueList = new List<int>();
                List<int> valueList2 = new List<int>();

                valueList2.Add(orders.Count(e => e.Status == 0));
                valueList2.Add(orders.Count(e => e.Status == 1));
                valueList2.Add(orders.Count(e => e.Status == 2));
                valueList2.Add(orders.Count(e => e.Status == 3));


                for (DateTime i = firstDayOfMonth; i <= endDayOfMonth; i = i.AddDays(1))
                {
                    var value = orders.Count(e => e.CreateAt >= i && e.CreateAt <= i.AddDays(1));

                    dateList.Add(i.ToString("dd/MM"));
                    valueList.Add(value);
                }

                ViewBag.date1 = "[" + string.Join(", ", dateList.Select(d => '"' + d + '"')) + "]";
                ViewBag.value1 = "[" + string.Join(", ", valueList) + "]";
                ViewBag.value2 = "[" + string.Join(", ", valueList2) + "]";

            }

            if (orders2 != null)
            {
                List<string> dateList3 = new List<string>();
                List<int> valueList3 = new List<int>();
                List<int> valueList4 = new List<int>();

                valueList4.Add(orders2.Count(e => e.Status == 0));
                valueList4.Add(orders2.Count(e => e.Status == 1));
                valueList4.Add(orders2.Count(e => e.Status == 2));
                valueList4.Add(orders2.Count(e => e.Status == 3));


                for (DateTime i = firstDayOfMonth.AddMonths(-1); i <= endDayOfMonth.AddMonths(-1); i = i.AddDays(1))
                {
                    var value = orders2.Count(e => e.CreateAt >= i && e.CreateAt <= i.AddDays(1));

                    dateList3.Add(i.ToString("dd/MM"));
                    valueList3.Add(value);
                }

                ViewBag.date3 = "[" + string.Join(", ", dateList3.Select(d => '"' + d + '"')) + "]";
                ViewBag.value3 = "[" + string.Join(", ", valueList3) + "]";
                ViewBag.value4 = "[" + string.Join(", ", valueList4) + "]";

            }

            return View();
        }

        #region Product
        public IActionResult Product()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetProducts()
        {
            try
            {
                var query1 = _dbContext.Products;


                return new OkObjectResult(new { status = 1, message = "success", data = query1.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = new List<Product>() });
            }

        }
        public IActionResult AddProduct([FromForm] AddProductModel model)
        {
            try
            {
                string folderName = Guid.NewGuid().ToString();

                if (model.file != null && model.fileModel != null)
                {
                    string fileNameImagePreview = "preview" + Path.GetExtension(model.file.FileName);
                    string fileNameModel = "model" + Path.GetExtension(model.fileModel.FileName);


                    var filePath1 = Path.Combine(_environment.WebRootPath, Common.FolderProducts, folderName, fileNameImagePreview);
                    var filePath2 = Path.Combine(_environment.WebRootPath, Common.FolderProducts, folderName, fileNameModel);


                    //tạo folder
                    if (!string.IsNullOrEmpty(filePath1))
                    {
                        string? directoryPath = Path.GetDirectoryName(filePath1);
                        if (directoryPath != null && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                    }


                    using (var stream = new FileStream(filePath1, FileMode.Create))
                    {
                        model.file.CopyTo(stream);
                    }
                    using (var stream = new FileStream(filePath2, FileMode.Create))
                    {
                        model.fileModel.CopyTo(stream);
                    }


                    string pathSave = $"/{Common.FolderProducts}/{folderName}/";


                    var pro = new Product()
                    {
                        Name = model.name,
                        Status = model.status,
                        PosterUrl = pathSave + fileNameImagePreview,
                        ModelUrl = pathSave + fileNameModel,
                        Sale = model.sale,
                        Description = model.description,
                        Price = model.price,
                        NumberAvailable = model.numberAvailable,
                        CreateAt = DateTime.Now
                    };
                    _dbContext.Products.Add(pro);
                    _dbContext.SaveChanges();

                    return new OkObjectResult(new { status = 1, message = "Tải lên sản phẩm mới thành công.", data = pro.ProductID });

                }
                else
                {
                    return new OkObjectResult(new { status = -1, message = "File không khả dụng.", data = -1 });
                }

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = -1 });
            }

        }

        public IActionResult UpdateProduct([FromForm] UpdateProductModel md)
        {
            try
            {
                var mat = _dbContext.Products.Find(md.productID);
                if (mat == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Sản phẩm này này không khả dụng." });
                }
                mat.Name = md.name;
                mat.Status = md.status;
                mat.Sale = md.sale;
                mat.Price = md.price;
                mat.Description = md.description;
                mat.NumberAvailable = md.numberAvailable;
                string[] cacPhan = mat.PosterUrl.Split('/');
                string folderName = cacPhan[cacPhan.Length - 2];
                string pathSave = $"/{Common.FolderProducts}/{folderName}/";

                if (md.file != null && md.file.Length > 0)
                {

                    // Xóa file cũ
                    var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.PosterUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath1))
                    {
                        System.IO.File.Delete(oldFilePath1);

                    }
                    string fileName = "preview" + Path.GetExtension(md.file.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderProducts, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.file.CopyTo(stream);
                    }
                    mat.PosterUrl = pathSave + fileName;
                }
               

                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật sản phẩm thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }

        public IActionResult DeleteProduct([FromBody] IDModel md)
        {
            try
            {
                var mat = _dbContext.Products.Find(md.ID);
                if (mat == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Sản phẩm này này không còn tồn tại." });
                }
                _dbContext.Products.Remove(mat);
                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Xóa sản phẩm thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }


        #endregion
        #region Order
        public IActionResult Order(int? orderID)
        {
            ViewBag.orderID = orderID;
            return View();
        }
        [HttpPost]
        public IActionResult GetOrders([FromBody] SearhOrderModel model)
        {
            try
            {
                List<Order>? list = _dbContext.Orders.ToList();
                if (list == null)
                {
                    list = new List<Order>();
                }
                return new OkObjectResult(new { status = 1, message = "success.", data = list });

            }
            catch (Exception e)
            {
                return new OkObjectResult(new { status = -2, message = e.Message, data = new List<Order>() });

            }
        }
        #endregion
    }
}