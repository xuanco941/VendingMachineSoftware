using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StyleX.DTOs;
using StyleX.Models;
using StyleX.Utils;
using System.Diagnostics;
using static StyleX.DTOs.WarehouseDTO;

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

        #region Material
        [HttpGet]
        public IActionResult Material()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddMaterial([FromForm] AddMaterialModel addMaterialModel)
        {
            try
            {
                if (addMaterialModel.file != null && addMaterialModel.aoMap != null && addMaterialModel.normalMap != null && addMaterialModel.metalnessMap != null && addMaterialModel.roughnessMap != null)
                {
                    string folderName = Guid.NewGuid().ToString();
                    string fileNameImagePreview = "preview" + Path.GetExtension(addMaterialModel.file.FileName);
                    string fileNameImageAoMap = "aoMap" + Path.GetExtension(addMaterialModel.aoMap.FileName);
                    string fileNameImageNormalMap = "normalMap" + Path.GetExtension(addMaterialModel.normalMap.FileName);
                    string fileNameImageMetalnessMap = "metalnessMap" + Path.GetExtension(addMaterialModel.metalnessMap.FileName);
                    string fileNameImageRoughnessMap = "roughnessMap" + Path.GetExtension(addMaterialModel.roughnessMap.FileName);


                    var filePath1 = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileNameImagePreview);
                    var filePath2 = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileNameImageAoMap);
                    var filePath3 = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileNameImageNormalMap);
                    var filePath4 = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileNameImageMetalnessMap);
                    var filePath5 = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileNameImageRoughnessMap);


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
                        addMaterialModel.file.CopyTo(stream);
                    }
                    using (var stream = new FileStream(filePath2, FileMode.Create))
                    {
                        addMaterialModel.aoMap.CopyTo(stream);
                    }
                    using (var stream = new FileStream(filePath3, FileMode.Create))
                    {
                        addMaterialModel.normalMap.CopyTo(stream);
                    }
                    using (var stream = new FileStream(filePath4, FileMode.Create))
                    {
                        addMaterialModel.metalnessMap.CopyTo(stream);
                    }
                    using (var stream = new FileStream(filePath5, FileMode.Create))
                    {
                        addMaterialModel.roughnessMap.CopyTo(stream);
                    }

                    string pathSave = $"/{Common.FolderImageMaterials}/{folderName}/";

                    _dbContext.Materials.Add(new Material()
                    {
                        Name = addMaterialModel.name,
                        Status = addMaterialModel.status,
                        Url = pathSave + fileNameImagePreview,
                        AoMap = pathSave + fileNameImageAoMap,
                        NormalMap = pathSave + fileNameImageNormalMap,
                        MetalnessMap = pathSave + fileNameImageMetalnessMap,
                        RoughnessMap = pathSave + fileNameImageRoughnessMap,
                        IsDecal = addMaterialModel.isDecal
                    });
                    _dbContext.SaveChanges();
                    return new OkObjectResult(new { status = 1, message = "Tải lên chất liệu mới thành công." });

                }
                else
                {
                    return new OkObjectResult(new { status = -1, message = "File không khả dụng." });
                }

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }
        [HttpPost]
        public IActionResult UpdateMaterial([FromForm] UpdateMaterialModel md)
        {
            try
            {
                var mat = _dbContext.Materials.Find(md.materialID);
                if (mat == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Chất liệu này không khả dụng." });
                }
                mat.Name = md.name;
                mat.Status = md.status;
                mat.IsDecal = md.isDecal;

                string[] cacPhan = mat.Url.Split('/');
                string folderName = cacPhan[cacPhan.Length - 2];
                string pathSave = $"/{Common.FolderImageMaterials}/{folderName}/";

                if (md.file != null && md.file.Length > 0)
                {

                    // Xóa file cũ
                    var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.Url.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath1))
                    {
                        System.IO.File.Delete(oldFilePath1);

                    }
                    string fileName = "preview" + Path.GetExtension(md.file.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.file.CopyTo(stream);
                    }
                    mat.Url = pathSave + fileName;
                }
                if (md.aoMap != null && md.aoMap.Length > 0)
                {

                    // Xóa file cũ
                    var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.AoMap.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath1))
                    {
                        System.IO.File.Delete(oldFilePath1);

                    }
                    string fileName = "aoMap" + Path.GetExtension(md.aoMap.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.aoMap.CopyTo(stream);
                    }
                    mat.AoMap = pathSave + fileName;
                }
                if (md.normalMap != null && md.normalMap.Length > 0)
                {

                    // Xóa file cũ
                    var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.NormalMap.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath1))
                    {
                        System.IO.File.Delete(oldFilePath1);

                    }
                    string fileName = "normalMap" + Path.GetExtension(md.normalMap.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.normalMap.CopyTo(stream);
                    }
                    mat.NormalMap = pathSave + fileName;
                }
                if (md.roughnessMap != null && md.roughnessMap.Length > 0)
                {

                    // Xóa file cũ
                    var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.RoughnessMap.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath1))
                    {
                        System.IO.File.Delete(oldFilePath1);

                    }
                    string fileName = "roughnessMap" + Path.GetExtension(md.roughnessMap.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.roughnessMap.CopyTo(stream);
                    }
                    mat.RoughnessMap = pathSave + fileName;
                }
                if (md.metalnessMap != null && md.metalnessMap.Length > 0)
                {

                    // Xóa file cũ
                    var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.MetalnessMap.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath1))
                    {
                        System.IO.File.Delete(oldFilePath1);

                    }
                    string fileName = "metalnessMap" + Path.GetExtension(md.metalnessMap.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderImageMaterials, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.metalnessMap.CopyTo(stream);
                    }
                    mat.MetalnessMap = pathSave + fileName;
                }
                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật chất liệu thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }
        [HttpPost]
        public IActionResult GetMaterials()
        {
            try
            {
                var result = _dbContext.Materials.ToList();
                return new OkObjectResult(new { status = 1, message = "success", data = result });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }

        #endregion
        #region Account
        public IActionResult Account()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetAccounts([FromBody] SearchAccountModel model)
        {
            try
            {
                //var data = _dbContext.Accounts.Where(a => (model.isActive == null || a.isActive == model.isActive) && (string.IsNullOrEmpty(model.accountName) || a.Email.Contains(model.accountName))).ToList();

                var data = from account in _dbContext.Accounts
                           where account.Role == Common.RoleUser
                           && (model.isActive == 0 || (model.isActive == 1 && account.isActive == true) || (model.isActive == 2 && account.isActive == false))
                           && (string.IsNullOrEmpty(model.accountName) || account.Email.Contains(model.accountName))
                           select account;
                return new OkObjectResult(new { status = 1, message = "success", data = data.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = new List<Account>() });
            }

        }
        public IActionResult UpdateAccount([FromBody] UpdateAccountModel md)
        {
            if (md == null)
            {
                return new NotFoundObjectResult(new { status = -99, message = "Vui lòng nhập lại thông tin." });
            }
            try
            {
                var account = _dbContext.Accounts.Find(md.accountID);
                if (account == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Không tìm thấy tài khoản này." });
                }
                account.FullName = md.fullName;
                account.NumberPlayGame = md.numberPlayGame;
                account.Password = md.password;
                account.PhoneNumber = md.phoneNumber;
                account.Address = md.address;

                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật tài khoản thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }
        public IActionResult DeleteAccount([FromBody] IDModel md)
        {
            try
            {
                var account = _dbContext.Accounts.Find(md.ID);
                if (account == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Không tìm thấy tài khoản này." });
                }

                _dbContext.Accounts.Remove(account);

                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Xóa thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }

        #endregion
        #region Category
        public IActionResult Category()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddCategory([FromForm] AddCategoryModel model)
        {
            try
            {
                if (model.file != null)
                {
                    string folderName = Guid.NewGuid().ToString();
                    string fileNameImagePreview = "preview" + Path.GetExtension(model.file.FileName);


                    var filePath1 = Path.Combine(_environment.WebRootPath, Common.FolderImageCategories, folderName, fileNameImagePreview);

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

                    string pathSave = $"/{Common.FolderImageCategories}/{folderName}/";

                    _dbContext.Categories.Add(new Category()
                    {
                        Name = model.name,
                        Description = model.description,
                        Image = pathSave + fileNameImagePreview
                    });
                    _dbContext.SaveChanges();
                    return new OkObjectResult(new { status = 1, message = "Tạo danh mục mới thành công." });

                }
                else
                {
                    return new OkObjectResult(new { status = -1, message = "File không khả dụng." });
                }

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }
        [HttpPost]
        public IActionResult UpdateCategory([FromForm] UpdateCategoryModel model)
        {
            try
            {
                var mat = _dbContext.Categories.Find(model.categoryID);
                if (mat == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Chất liệu này không khả dụng." });
                }
                mat.Name = model.name;
                mat.Description = model.description;

                if (string.IsNullOrEmpty(mat.Image) == false && model.file != null && model.file.Length > 0)
                {
                    string[] cacPhan = mat.Image.Split('/');
                    string folderName = cacPhan[cacPhan.Length - 2];
                    string pathSave = $"/{Common.FolderImageCategories}/{folderName}/";
                    // Xóa file cũ
                    var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.Image.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath1))
                    {
                        System.IO.File.Delete(oldFilePath1);

                    }
                    string fileName = "preview" + Path.GetExtension(model.file.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderImageCategories, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.file.CopyTo(stream);
                    }
                    mat.Image = pathSave + fileName;
                }
                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật danh mục thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }
        [HttpPost]
        public IActionResult GetCategories()
        {
            try
            {
                var result = _dbContext.Categories.ToList();
                return new OkObjectResult(new { status = 1, message = "success", data = result });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }
        [HttpPost]
        public IActionResult DeleteCategory([FromBody] IDModel md)
        {
            try
            {
                var account = _dbContext.Categories.Find(md.ID);
                if (account == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Không tìm thấy danh mục này." });
                }

                _dbContext.Categories.Remove(account);

                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Xóa thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }

        #endregion
        #region Promotion
        public IActionResult Promotion()
        {
            return View();
        }
        public IActionResult GetPromotions([FromBody] SearchPromotionModel model)
        {
            try
            {
                //var data = from p in _dbContext.Promotions
                //           join a in _dbContext.Accounts on p.AccountID equals a.AccountID
                //           join o in _dbContext.Orders on p.OrderID equals o.OrderID into temp
                //           from t in temp.DefaultIfEmpty()
                //           where (model.status == 0 || (model.status == 1 && p.Status == true) || (model.status == 2 && p.Status == false))
                //           && (string.IsNullOrEmpty(model.accountName) || a.Email.Contains(model.accountName))
                //           select new
                //           {
                //               p.ResultSpin,
                //               p.Number,
                //               p.Status,
                //               a.Email,
                //               p.CreateAt,
                //               p.UsedAt,
                //               p.ExpiredAt,
                //               p.OrderID
                //           };
                var data = from p in _dbContext.Promotions
                           join a in _dbContext.Accounts on p.AccountID equals a.AccountID
                           where (model.status == 0 || (model.status == 1 && p.Status == true) || (model.status == 2 && p.Status == false))
                           && (string.IsNullOrEmpty(model.accountName) || a.Email.Contains(model.accountName))
                           select new
                           {
                               p.ResultSpin,
                               p.Number,
                               p.Status,
                               a.Email,
                               p.CreateAt,
                               p.UsedAt,
                               p.ExpiredAt,
                               p.OrderID
                           };
                return new OkObjectResult(new { status = 1, message = "success", data = data.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }

        #endregion
        #region Product
        public IActionResult Product()
        {
            return View();
        }
        public IActionResult GetProducts([FromBody] SearchProductModel model)
        {
            try
            {
                var query1 = from p in _dbContext.Products
                             join c in _dbContext.Categories on p.CategoryID equals c.CategoryID
                             where (model.status == 0 || (model.status == 1 && p.Status == true) || (model.status == 2 && p.Status == false))
                                   && (model.categoryID == 0 || (model.categoryID == c.CategoryID))
                             select new
                             {
                                 p.ProductID,
                                 p.PosterUrl,
                                 p.PosterDesignUrl1,
                                 p.PosterDesignUrl2,
                                 p.Name,
                                 p.Description,
                                 p.Price,
                                 p.Sale,
                                 p.SaleEndAt,
                                 p.Status,
                                 CategoryID = c.CategoryID,
                                 CategoryName = c.Name,
                                 p.ModelUrl,
                             };

                var result = query1.ToList(); // Execute the main query and get results

                // Now fetch related data for Warehouses and ProductSettings
                var productIds = result.Select(p => p.ProductID).ToList();
                var warehouses = _dbContext.Warehouses.Where(e => productIds.Contains(e.ProductID)).ToList();
                var productSettings = _dbContext.ProductSettings.Where(e => productIds.Contains(e.ProductID)).ToList();

                // Combine the related data with the main query results
                var finalResult = result.Select(p => new
                {
                    p.ProductID,
                    p.PosterUrl,
                    p.PosterDesignUrl1,
                    p.PosterDesignUrl2,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Sale,
                    p.SaleEndAt,
                    p.Status,
                    Warehouses = warehouses.Where(e => e.ProductID == p.ProductID).ToList(),
                    CategoryID = p.CategoryID,
                    CategoryName = p.CategoryName,
                    p.ModelUrl,
                    ProductSettings = productSettings.Where(e => e.ProductID == p.ProductID).ToList()
                }).ToList();


                return new OkObjectResult(new { status = 1, message = "success", data = finalResult });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }
        public IActionResult AddProduct([FromForm] AddProductModel model)
        {
            try
            {
                string folderName = Guid.NewGuid().ToString();

                string i1 = "";
                string filePath3 = "";

                if (model.img1 != null)
                {
                    i1 = "img1" + Path.GetExtension(model.img1.FileName);
                    filePath3 = Path.Combine(_environment.WebRootPath, Common.FolderProducts, folderName, i1);

                }
                string i2 = "";
                string filePath4 = "";
                if (model.img2 != null)
                {
                    i2 = "img2" + Path.GetExtension(model.img2.FileName);
                    filePath4 = Path.Combine(_environment.WebRootPath, Common.FolderProducts, folderName, i2);
                }

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
                    if (model.img1 != null)
                    {
                        using (var stream = new FileStream(filePath3, FileMode.Create))
                        {
                            model.img1.CopyTo(stream);
                        }
                    }
                    if (model.img2 != null)
                    {
                        using (var stream = new FileStream(filePath4, FileMode.Create))
                        {
                            model.img2.CopyTo(stream);
                        }
                    }


                    string pathSave = $"/{Common.FolderProducts}/{folderName}/";

                    string fileImg1 = string.IsNullOrEmpty(i1) == true ? "" : pathSave + i1;
                    string fileImg2 = string.IsNullOrEmpty(i2) == true ? "" : pathSave + i2;


                    var pro = new Product()
                    {
                        Name = model.name,
                        Status = model.status,
                        PosterUrl = pathSave + fileNameImagePreview,
                        ModelUrl = pathSave + fileNameModel,
                        PosterDesignUrl1 = fileImg1,
                        PosterDesignUrl2 = fileImg2,
                        Sale = model.sale,
                        SaleEndAt = model.saleEndAt,
                        Description = model.description,
                        Price = model.price,
                        CategoryID = model.categoryID,
                        CreateAt = DateTime.Now
                    };
                    _dbContext.Products.Add(pro);
                    _dbContext.SaveChanges();

                    var list = new List<ProductSetting>();


                    foreach (var n in model.productParts)
                    {
                        list.Add(new ProductSetting()
                        {
                            IsDefault = false,
                            ProductID = pro.ProductID,
                            ProductPartNameDefault = n,
                            ProductPartNameCustom = n,
                            AoMap = "",
                            NormalMap = "",
                            RoughnessMap = "",
                            MetalnessMap = ""
                        });
                    }
                    _dbContext.ProductSettings.AddRange(list);



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
        public IActionResult AddProductPart([FromForm] AddMatProductPart model)
        {
            try
            {
                var p = _dbContext.ProductSettings.FirstOrDefault(e => e.ProductID == model.productID && e.ProductPartNameDefault == model.name);
                if (p == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Bộ phận sản phẩm không khả dụng.", data = -1 });
                }


                string folderName = Guid.NewGuid().ToString();
                string fileNameAoMap = "aoMap.png";
                string fileNameNormalMap = "normalMap.png";
                string fileNameRoughnessMap = "roughnessMap.png";
                string fileNameMetalnessMap = "metalnessMap.png";
                var filePath1 = Path.Combine(_environment.WebRootPath, Common.FolderProductPartMaterialDefault, folderName, fileNameAoMap);
                var filePath2 = Path.Combine(_environment.WebRootPath, Common.FolderProductPartMaterialDefault, folderName, fileNameNormalMap);
                var filePath3 = Path.Combine(_environment.WebRootPath, Common.FolderProductPartMaterialDefault, folderName, fileNameRoughnessMap);
                var filePath4 = Path.Combine(_environment.WebRootPath, Common.FolderProductPartMaterialDefault, folderName, fileNameMetalnessMap);
                if (model.aoMap != null)
                {
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
                        model.aoMap.CopyTo(stream);
                    }
                    string pathSave = $"/{Common.FolderProductPartMaterialDefault}/{folderName}/";

                    p.AoMap = pathSave + fileNameAoMap;
                }
                if (model.normalMap != null)
                {
                    //tạo folder
                    if (!string.IsNullOrEmpty(filePath2))
                    {
                        string? directoryPath = Path.GetDirectoryName(filePath2);
                        if (directoryPath != null && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                    }
                    using (var stream = new FileStream(filePath2, FileMode.Create))
                    {
                        model.normalMap.CopyTo(stream);
                    }
                    string pathSave = $"/{Common.FolderProductPartMaterialDefault}/{folderName}/";

                    p.NormalMap = pathSave + fileNameNormalMap;
                }
                if (model.roughnessMap != null)
                {
                    //tạo folder
                    if (!string.IsNullOrEmpty(filePath3))
                    {
                        string? directoryPath = Path.GetDirectoryName(filePath3);
                        if (directoryPath != null && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                    }
                    using (var stream = new FileStream(filePath3, FileMode.Create))
                    {
                        model.roughnessMap.CopyTo(stream);
                    }
                    string pathSave = $"/{Common.FolderProductPartMaterialDefault}/{folderName}/";

                    p.RoughnessMap = pathSave + fileNameRoughnessMap;
                }
                if (model.metalnessMap != null)
                {
                    //tạo folder
                    if (!string.IsNullOrEmpty(filePath3))
                    {
                        string? directoryPath = Path.GetDirectoryName(filePath4);
                        if (directoryPath != null && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                    }
                    using (var stream = new FileStream(filePath4, FileMode.Create))
                    {
                        model.metalnessMap.CopyTo(stream);
                    }
                    string pathSave = $"/{Common.FolderProductPartMaterialDefault}/{folderName}/";

                    p.MetalnessMap = pathSave + fileNameMetalnessMap;
                }


                _dbContext.SaveChanges();

                return new OkObjectResult(new { status = 1, message = "Update chất liệu mặc định thành công.", data = p.ProductSettingID });



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
                mat.CategoryID = md.categoryID;
                mat.Price = md.price;
                mat.Description = md.description;
                mat.SaleEndAt = md.saleEndAt;

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
                if (md.img1 != null && md.img1.Length > 0)
                {

                    // Xóa file cũ
                    if (string.IsNullOrEmpty(mat.PosterDesignUrl1) == false)
                    {
                        var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.PosterDesignUrl1.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath1))
                        {
                            System.IO.File.Delete(oldFilePath1);

                        }
                    }

                    string fileName = "img1" + Path.GetExtension(md.img1.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderProducts, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.img1.CopyTo(stream);
                    }
                    mat.PosterDesignUrl1 = pathSave + fileName;
                }
                if (md.img2 != null && md.img2.Length > 0)
                {

                    // Xóa file cũ
                    if (string.IsNullOrEmpty(mat.PosterDesignUrl2) == false)
                    {
                        var oldFilePath1 = Path.Combine(_environment.WebRootPath, mat.PosterDesignUrl2.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath1))
                        {
                            System.IO.File.Delete(oldFilePath1);

                        }
                    }

                    string fileName = "img2" + Path.GetExtension(md.img2.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, Common.FolderProducts, folderName, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        md.img2.CopyTo(stream);
                    }
                    mat.PosterDesignUrl2 = pathSave + fileName;
                }


                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật sản phẩm thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }
        public IActionResult GetProductSettings([FromBody] IDModel model)
        {
            try
            {
                var query = _dbContext.ProductSettings.Find(model.ID);
                if (query == null)
                {
                    return new BadRequestObjectResult(new { status = -99, message = "Không tìm thấy bộ phận này.", data = DBNull.Value });

                }
                var mats = new List<Material>();
                var query2 = _dbContext.ProductSettingMaterials.Where(p => p.ProductSettingID == query.ProductSettingID).ToList();
                foreach (var j in query2)
                {
                    var amt = _dbContext.Materials.FirstOrDefault(m => m.MaterialID == j.MaterialID);
                    if (amt != null)
                    {
                        mats.Add(amt);
                    }
                }
                var p = new ProductSettingsWithMaterial()
                {
                    IsDefault = query.IsDefault,
                    materials = mats,
                    ProductPartNameCustom = query.ProductPartNameCustom,
                    ProductPartNameDefault = query.ProductPartNameDefault,
                    ProductSettingID = query.ProductSettingID,
                    NameMaterialDefault = query.NameMaterialDefault
                };
                return new OkObjectResult(new { status = 1, message = "success", data = p });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }
        public IActionResult SettingProduct([FromForm] SettingProductModel md)
        {
            try
            {
                if (string.IsNullOrEmpty(md.productPartNameCustom))
                {
                    return new OkObjectResult(new { status = -1, message = "Bạn chưa nhập tên hiển thị cho bộ phận này." });
                }
                var ps = _dbContext.ProductSettings.Find(md.productSettingID);
                if (ps == null)
                {
                    return new BadRequestObjectResult(new { status = -2, message = "Không khả dụng" });

                }
                ps.ProductPartNameCustom = md.productPartNameCustom;
                ps.IsDefault = md.isDefault;
                ps.NameMaterialDefault = md.nameMaterialDefault;

                var oldMat = _dbContext.ProductSettingMaterials.Where(a => a.ProductSettingID == md.productSettingID).ToList();
                if (oldMat != null && oldMat.Count() > 0)
                {
                    _dbContext.ProductSettingMaterials.RemoveRange(oldMat);
                }
                if (md.materials != null && md.materials.Count > 0)
                {
                    var newL = new List<ProductSettingMaterial>();
                    for (int i = 0; i < md.materials.Count; i++)
                    {
                        newL.Add(new ProductSettingMaterial() { ProductSettingID = md.productSettingID, MaterialID = md.materials[i] });
                    }
                    _dbContext.AddRange(newL);
                }

                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật bộ phận thiết kế thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }

        #endregion
        #region Warehouse
        public IActionResult Warehouse()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetListProducts()
        {
            try
            {
                var data = from p in _dbContext.Products
                           select new
                           {
                               productID = p.ProductID,
                               name = p.Name
                           };
                return new OkObjectResult(new { status = 1, message = "success", data = data.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }
        [HttpPost]
        public IActionResult GetWarehouses([FromBody] IDModel model)
        {
            try
            {
                var data = _dbContext.Warehouses.Include(e => e.Product).Where(e => model.ID == 0 || e.ProductID == model.ID).ToList();
                return new OkObjectResult(new { status = 1, message = "success", data = data.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }
        [HttpPost]
        public IActionResult AddWarehouse([FromBody] AddWarehouseModel model)
        {
            try
            {
                var whs = _dbContext.Warehouses.Where(e => e.ProductID == model.productID && e.Size == model.size).ToList();
                if (whs != null && whs.Count > 0)
                {
                    return new OkObjectResult(new { status = -1, message = "Sản phẩm với kích cỡ trên đã có trong kho." });
                }
                _dbContext.Warehouses.Add(new Models.Warehouse() { ProductID = model.productID, Amount = model.amount, Size = model.size });
                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Thêm thành công!" });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }
        }
        [HttpPost]
        public IActionResult UpdateWarehouse([FromBody] UpdateWarehouseModel model)
        {
            try
            {
                var w = _dbContext.Warehouses.Find(model.warehouseID);
                if (w == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Không khả dụng." });
                }
                var whs = _dbContext.Warehouses.Where(e => e.Size == model.size && e.Size != model.size).ToList();
                if (whs != null && whs.Count > 0)
                {
                    return new OkObjectResult(new { status = -2, message = "Sản phẩm với kích cỡ trên đã có trong kho." });
                }
                w.Amount = model.amount;
                w.Size = model.size;
                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật thành công!" });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }
        }
        public IActionResult DeleteWarehouse([FromBody] IDModel md)
        {
            try
            {
                var account = _dbContext.Warehouses.Find(md.ID);
                if (account == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Không tìm thấy sản phẩm này." });
                }

                _dbContext.Warehouses.Remove(account);

                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Xóa thành công." });

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
                List<Order>? list = _dbContext.Orders.Include(e => e.Account).Where(u => u.OrderID.ToString().Contains(model.orderID) && u.Name.Contains(model.accountName)).ToList();
                if (list == null)
                {
                    list = new List<Order>();
                }
                return new OkObjectResult(new { status = 1, message = "success.", data = list });

            }
            catch (Exception e)
            {
                return new OkObjectResult(new { status = -2, message = e.Message, data = "" });

            }
        }
        [HttpPost]
        public IActionResult GetOrderItems([FromBody] IDModel iDModel)
        {
            try
            {
                List<CartItem>? list = _dbContext.CartItems.Include(e => e.Product).Where(u => u.OrderID == iDModel.ID).ToList();
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
        [HttpPost]
        public IActionResult UpdateOrder([FromBody] UpdateOrderModel md)
        {
            if (md == null)
            {
                return new NotFoundObjectResult(new { status = -99, message = "Vui lòng nhập đủ thông tin thiết yếu." });
            }
            try
            {
                var obj = _dbContext.Orders.Find(md.orderID);
                if (obj == null)
                {
                    return new OkObjectResult(new { status = -1, message = "Không tìm thấy đơn hàng này." });
                }

                obj.Name = md.name;
                obj.NetPrice = md.netPrice;
                obj.PhoneNumber = md.phoneNumber;
                obj.Address = md.address;
                obj.Message = md.message;
                obj.UpdateAt = DateTime.Now;
                obj.Status = md.status;
                obj.TransportFee = md.transportFee;


                _dbContext.SaveChanges();
                return new OkObjectResult(new { status = 1, message = "Cập nhật đơn hàng thành công." });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message });
            }

        }

        #endregion
    }
}