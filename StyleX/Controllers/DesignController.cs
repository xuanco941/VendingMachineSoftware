using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleX.DTOs;
using StyleX.Models;
using System;
using System.Drawing;
using System.Security.Claims;

namespace StyleX.Controllers
{
    public class DesignController : Controller
    {
        private readonly DatabaseContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public DesignController(DatabaseContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }
        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        [HttpGet]
        public IActionResult Index(int? id)
        {
            if (id.HasValue == true)
            {
                string accountID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(accountID) == false)
                {
                    var cartItem = _dbContext.CartItems.Include(e => e.Product).FirstOrDefault(x => x.CartItemID == id.Value && x.Status==0 && x.AccountID == Convert.ToInt32(accountID));
                    if (cartItem != null)
                    {
                        ViewBag.id = id;
                        ViewBag.src = cartItem.Product.ModelUrl;
                        ViewBag.poster = cartItem.Product.PosterUrl;
                    }
                }

            }
            return View();
        }
        [HttpGet]
        public IActionResult ViewDesign(int? id)
        {
            if (id.HasValue == true)
            {
                var cartItem = _dbContext.CartItems.Include(e => e.Product).FirstOrDefault(x => x.CartItemID == id.Value);
                if (cartItem != null)
                {
                    ViewBag.id = id;
                    ViewBag.src = cartItem.Product.ModelUrl;
                    ViewBag.poster = cartItem.Product.PosterUrl;

                }
            }
            return View();
        }
        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        [HttpPost]
        public IActionResult GetCartItems()
        {
            List<CartItem> cartItems = new List<CartItem>();
            try
            {
                string accountID = User.FindFirstValue(ClaimTypes.NameIdentifier);

                //status = 0 là những đơn trong cart, design

                if (string.IsNullOrEmpty(accountID) == false)
                {
                    cartItems = _dbContext.CartItems.Include(e => e.Product).Where(e => e.AccountID == Convert.ToInt32(accountID) && e.Status == 0).ToList();
                }

                return new OkObjectResult(new { status = 1, message = "success", data = cartItems });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = cartItems });
            }
        }
        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        [HttpPost]
        public IActionResult GetProducts()
        {
            List<Product> products = new List<Product>();
            try
            {
                products = _dbContext.Products.Where(e => e.Status == true).ToList();

                return new OkObjectResult(new { status = 1, message = "success", data = products });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = products });
            }
        }
        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        [HttpPost]
        public IActionResult GetProductSetting([FromBody] IDModel model)
        {
            try
            {
                var cartItem = _dbContext.CartItems.FirstOrDefault(e => e.CartItemID == model.ID);

                if (cartItem == null)
                {
                    return new NotFoundObjectResult(new { status = -98, message = "Mẫu thiết kế này không tồn tại", data = DBNull.Value });
                }

                var query1 = from ps in _dbContext.ProductSettings
                             join psm in _dbContext.ProductSettingMaterials on ps.ProductSettingID equals psm.ProductSettingID into leftJoinT
                             from psml in leftJoinT.DefaultIfEmpty()
                             join m in _dbContext.Materials on psml.MaterialID equals m.MaterialID into leftJoinTable
                             from mat in leftJoinTable.DefaultIfEmpty()
                             where ps.ProductID == cartItem.ProductID
                             select new
                             {
                                 ps.ProductSettingID,
                                 ps.ProductPartNameDefault,
                                 ps.ProductPartNameCustom,
                                 ps.IsDefault,
                                 ps.NameMaterialDefault,
                                 ps.AoMap,
                                 ps.NormalMap,
                                 ps.RoughnessMap,
                                 ps.MetalnessMap,
                                 material = mat,

                             };

                var query2 = from q in query1
                             group q by q.ProductSettingID into g
                             select new
                             {
                                 productSettingID = g.Key,
                                 nameDefault = g.First().ProductPartNameDefault,
                                 nameCustom = g.First().ProductPartNameCustom,
                                 isDefault = g.First().IsDefault,
                                 nameMaterialDefault = g.First().NameMaterialDefault,
                                 aoMap = g.First().AoMap,
                                 normalMap = g.First().NormalMap,
                                 roughnessMap = g.First().RoughnessMap,
                                 metalnessMap = g.First().MetalnessMap,
                                 materials = g.Select(x => x.material),

                                 designInfo = _dbContext.DesignInfos.FirstOrDefault(e => e.DesignName == g.First().ProductPartNameDefault && e.CartItemID == model.ID)
                             };



                return new OkObjectResult(new { status = 1, message = "success", data = query2.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }
        }
        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        [HttpPost]
        public IActionResult GetMaterials([FromBody] IDModel model)
        {
            try
            {

                var query1 = _dbContext.ProductSettingMaterials.Include(e => e.Material).Where(e => e.ProductSettingID == model.ID).ToList();
                var result = from p in query1
                             where p.Material.Status == true
                             select new
                             { materialID = p.Material.MaterialID, name = p.Material.Name, preview = p.Material.Url, aoMap = p.Material.AoMap, normalMap = p.Material.NormalMap, roughnessMap = p.Material.RoughnessMap, metalnessMap = p.Material.MetalnessMap, isDecal = p.Material.IsDecal };
                return new OkObjectResult(new { status = 1, message = "success", data = result.ToList() });



            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }
        }
        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        [HttpPost]
        public IActionResult SaveDesign([FromForm] SaveDesignInfoModel model)
        {
            if (model == null)
            {
                return new BadRequestObjectResult(new { status = -99, message = "Tham số lưu thiếu.", data = DBNull.Value });
            }
            try
            {

                var cartItem = _dbContext.CartItems.FirstOrDefault(e => e.CartItemID == model.cartItemID);
                if (cartItem == null)
                {
                    return new NotFoundObjectResult(new { status = -99, message = "Sản phẩm thiết kế này không tồn tại.", data = DBNull.Value });
                }

                DesignInfo designInfoResutl = new DesignInfo();

                //chưa design lần nào
                if (model.designInfoID == 0)
                {
                    string folderName = Guid.NewGuid().ToString();

                    //thay ảnh preview
                    string fileNamePreview = "preview" + Guid.NewGuid() + Path.GetExtension(model.imageCartItem.FileName);
                    var filePathImageCartItem = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileNamePreview);
                    //tạo folder
                    if (!string.IsNullOrEmpty(filePathImageCartItem))
                    {
                        string? directoryPath = Path.GetDirectoryName(filePathImageCartItem);
                        if (directoryPath != null && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                    }
                    using (var stream = new FileStream(filePathImageCartItem, FileMode.Create))
                    {
                        model.imageCartItem.CopyTo(stream);
                    }

                    cartItem.PosterUrl = $"/{Common.FolderDesignInfo}/{folderName}/" + fileNamePreview;



                    DesignInfo designInfo = new DesignInfo();
                    designInfo.CartItemID = model.cartItemID;
                    designInfo.Color = model.color;
                    designInfo.TextureScale = model.textureScale;

                    designInfo.DesignName = model.designName;
                    designInfo.NameMaterial = model.nameMaterial ?? "";
                    designInfo.AoMap = "";
                    designInfo.NormalMap = "";
                    designInfo.MetalnessMap = "";
                    designInfo.RoughnessMap = "";
                    designInfo.ImageTexture = "";



                    if (model.imageTexture != null)
                    {
                        string fileNameImageTexture = "textureColor" + Guid.NewGuid() + Path.GetExtension(model.imageTexture.FileName);
                        var filePathImageTexture = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileNameImageTexture);
                        //tạo folder
                        if (!string.IsNullOrEmpty(filePathImageTexture))
                        {
                            string? directoryPath = Path.GetDirectoryName(filePathImageTexture);
                            if (directoryPath != null && !Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                        }
                        using (var stream = new FileStream(filePathImageTexture, FileMode.Create))
                        {
                            model.imageTexture.CopyTo(stream);
                        }

                        designInfo.ImageTexture = $"/{Common.FolderDesignInfo}/{folderName}/" + fileNameImageTexture;
                    }



                    if (model.aoMap != null)
                    {
                        string fileNameAoMap = "aoMap" + Guid.NewGuid() + Path.GetExtension(model.aoMap.FileName);
                        var filePathImageAoMap = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileNameAoMap);
                        //tạo folder
                        if (!string.IsNullOrEmpty(filePathImageAoMap))
                        {
                            string? directoryPath = Path.GetDirectoryName(filePathImageAoMap);
                            if (directoryPath != null && !Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                        }
                        using (var stream = new FileStream(filePathImageAoMap, FileMode.Create))
                        {
                            model.aoMap.CopyTo(stream);
                        }

                        designInfo.AoMap = $"/{Common.FolderDesignInfo}/{folderName}/" + fileNameAoMap;
                    }



                    if (model.normalMap != null)
                    {
                        string fileNameNormalMap = "normalMap" + Guid.NewGuid() + Path.GetExtension(model.normalMap.FileName);
                        var filePathImagenormalMap = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileNameNormalMap);
                        //tạo folder
                        if (!string.IsNullOrEmpty(filePathImagenormalMap))
                        {
                            string? directoryPath = Path.GetDirectoryName(filePathImagenormalMap);
                            if (directoryPath != null && !Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                        }
                        using (var stream = new FileStream(filePathImagenormalMap, FileMode.Create))
                        {
                            model.normalMap.CopyTo(stream);
                        }

                        designInfo.NormalMap = $"/{Common.FolderDesignInfo}/{folderName}/" + fileNameNormalMap;
                    }


                    if (model.roughnessMap != null)
                    {
                        string fileNameroughnessMap = "roughnessMap" + Path.GetExtension(model.roughnessMap.FileName);
                        var filePathImageroughnessMap = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileNameroughnessMap);
                        //tạo folder
                        if (!string.IsNullOrEmpty(filePathImageroughnessMap))
                        {
                            string? directoryPath = Path.GetDirectoryName(filePathImageroughnessMap);
                            if (directoryPath != null && !Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                        }
                        using (var stream = new FileStream(filePathImageroughnessMap, FileMode.Create))
                        {
                            model.roughnessMap.CopyTo(stream);
                        }

                        designInfo.RoughnessMap = $"/{Common.FolderDesignInfo}/{folderName}/" + fileNameroughnessMap;
                    }


                    if (model.metalnessMap != null)
                    {
                        string fileNamemetalnessMap = "metalnessMap" + Guid.NewGuid() + Path.GetExtension(model.metalnessMap.FileName);
                        var filePathImagemetalnessMap = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileNamemetalnessMap);
                        //tạo folder
                        if (!string.IsNullOrEmpty(filePathImagemetalnessMap))
                        {
                            string? directoryPath = Path.GetDirectoryName(filePathImagemetalnessMap);
                            if (directoryPath != null && !Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                        }
                        using (var stream = new FileStream(filePathImagemetalnessMap, FileMode.Create))
                        {
                            model.metalnessMap.CopyTo(stream);
                        }

                        designInfo.MetalnessMap = $"/{Common.FolderDesignInfo}/{folderName}/" + fileNamemetalnessMap;
                    }


                    _dbContext.DesignInfos.Add(designInfo);
                    _dbContext.SaveChanges();
                    designInfoResutl = designInfo;
                }
                else
                {
                    var designInfo = _dbContext.DesignInfos.FirstOrDefault(e => e.DesignInfoID == model.designInfoID);
                    if (designInfo == null)
                    {
                        return new NotFoundObjectResult(new { status = -99, message = "Bộ phận thiết kế này không tồn tại.", data = DBNull.Value });
                    }
                    designInfo.Color = model.color;
                    designInfo.TextureScale = model.textureScale;
                    designInfo.DesignName = model.designName;
                    designInfo.NameMaterial = model.nameMaterial ?? "";
                    designInfo.AoMap = "";
                    designInfo.NormalMap = "";
                    designInfo.MetalnessMap = "";
                    designInfo.RoughnessMap = "";

                    string[] cacPhan = cartItem.PosterUrl.Split('/');
                    string folderName = cacPhan[cacPhan.Length - 2];
                    string pathSave = $"/{Common.FolderDesignInfo}/{folderName}/";

                    if ((model.imageCartItem != null && string.IsNullOrEmpty(cartItem.PosterUrl) == false))
                    {
                        // Xóa file cũ
                        var oldFilePath = Path.Combine(_environment.WebRootPath, cartItem.PosterUrl?.TrimStart('/') ?? "");

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);

                        }
                        string fileName = "preview" + Guid.NewGuid() + Path.GetExtension(model.imageCartItem.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.imageCartItem.CopyTo(stream);
                        }
                        cartItem.PosterUrl = pathSave + fileName;
                    }


                    if (string.IsNullOrEmpty(designInfo.ImageTexture) == false)
                    {
                        // Xóa file cũ
                        var oldFilePath = Path.Combine(_environment.WebRootPath, designInfo.ImageTexture.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);

                        }
                        designInfo.ImageTexture = "";
                    }
                    if (model.imageTexture != null && model.imageTexture.Length > 0)
                    {

                        string fileName = "imageTexture" + Guid.NewGuid() + Path.GetExtension(model.imageTexture.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.imageTexture.CopyTo(stream);
                        }
                        designInfo.ImageTexture = pathSave + fileName;
                    }

                    if (string.IsNullOrEmpty(designInfo.AoMap) == false)
                    {
                        // Xóa file cũ
                        var oldFilePath = Path.Combine(_environment.WebRootPath, designInfo.AoMap.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        designInfo.AoMap = "";
                    }
                    if (model.aoMap != null && model.aoMap.Length > 0)
                    {

                        string fileName = "aoMap" + Guid.NewGuid() + Path.GetExtension(model.aoMap.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.aoMap.CopyTo(stream);
                        }
                        designInfo.AoMap = pathSave + fileName;
                    }
                    if (string.IsNullOrEmpty(designInfo.NormalMap) == false)
                    {
                        // Xóa file cũ
                        var oldFilePath = Path.Combine(_environment.WebRootPath, designInfo.NormalMap.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        designInfo.NormalMap = "";
                    }
                    if (model.normalMap != null && model.normalMap.Length > 0)
                    {

                        string fileName = "normalMap" + Guid.NewGuid() + Path.GetExtension(model.normalMap.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.normalMap.CopyTo(stream);
                        }
                        designInfo.NormalMap = pathSave + fileName;
                    }
                    if (string.IsNullOrEmpty(designInfo.RoughnessMap) == false)
                    {
                        // Xóa file cũ
                        var oldFilePath = Path.Combine(_environment.WebRootPath, designInfo.RoughnessMap.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);

                        }
                        designInfo.RoughnessMap = "";
                    }
                    if (model.roughnessMap != null && model.roughnessMap.Length > 0)
                    {

                        string fileName = "roughnessMap" + Guid.NewGuid() + Path.GetExtension(model.roughnessMap.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.roughnessMap.CopyTo(stream);
                        }
                        designInfo.RoughnessMap = pathSave + fileName;
                    }
                    if (string.IsNullOrEmpty(designInfo.MetalnessMap) == false)
                    {
                        // Xóa file cũ
                        var oldFilePath = Path.Combine(_environment.WebRootPath, designInfo.MetalnessMap.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);

                        }
                        designInfo.MetalnessMap = "";
                    }
                    if (model.metalnessMap != null && model.metalnessMap.Length > 0)
                    {

                        string fileName = "metalnessMap" + Guid.NewGuid() + Path.GetExtension(model.metalnessMap.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, Common.FolderDesignInfo, folderName, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.metalnessMap.CopyTo(stream);
                        }
                        designInfo.MetalnessMap = pathSave + fileName;
                    }

                    _dbContext.SaveChanges();
                    designInfoResutl = designInfo;
                }

                return new OkObjectResult(new { status = 1, message = "Lưu thành công", data = designInfoResutl });
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }
        }



        [Authorize(AuthenticationSchemes = Common.CookieAuthUser)]
        [HttpPost]
        public IActionResult SaveDecal([FromForm] SaveDecalInfoModel model)
        {
            if (model == null)
            {
                return new BadRequestObjectResult(new { status = -99, message = "Tham số lưu thiếu.", data = DBNull.Value });
            }
            try
            {

                var cartItem = _dbContext.CartItems.FirstOrDefault(e => e.CartItemID == model.cartItemID);
                if (cartItem == null)
                {
                    return new NotFoundObjectResult(new { status = -99, message = "Sản phẩm thiết kế này không tồn tại.", data = DBNull.Value });
                }

                DecalInfo decalInfo = new DecalInfo();

                string folderName = Guid.NewGuid().ToString();
                //ảnh 
                string fileNamePreview = "image" + Guid.NewGuid() + Path.GetExtension(model.image.FileName);
                var filePathImage = Path.Combine(_environment.WebRootPath, Common.FolderDecalInfo, folderName, fileNamePreview);
                //tạo folder
                if (!string.IsNullOrEmpty(filePathImage))
                {
                    string? directoryPath = Path.GetDirectoryName(filePathImage);
                    if (directoryPath != null && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }
                using (var stream = new FileStream(filePathImage, FileMode.Create))
                {
                    model.image.CopyTo(stream);
                }

                decalInfo.Image = $"/{Common.FolderDecalInfo}/{folderName}/" + fileNamePreview;

                decalInfo.PositionX = model.positionX;
                decalInfo.PositionZ = model.positionZ;
                decalInfo.PositionY = model.positionY;
                decalInfo.OrientationX = model.orientationX;
                decalInfo.OrientationZ = model.orientationZ;
                decalInfo.OrientationY = model.orientationY;
                decalInfo.Size = model.size;
                decalInfo.MeshUuid = model.meshUuid;
                decalInfo.RenderOrder = model.renderOrder;
                decalInfo.CartItemID = model.cartItemID;

                _dbContext.DecalInfos.Add(decalInfo);
                _dbContext.SaveChanges();


                return new OkObjectResult(new { status = 1, message = "Lưu thành công", data = decalInfo });
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }
        }

        [HttpPost]
        public IActionResult DeleteAllDecal([FromBody] IDModel model)
        {
            try
            {

                var decalInfos = _dbContext.DecalInfos.Where(e => e.CartItemID == model.ID);
                if (decalInfos != null && decalInfos.Count() > 0)
                {

                    _dbContext.DecalInfos.RemoveRange(decalInfos);
                    _dbContext.SaveChanges();
                }

                return new OkObjectResult(new { status = 1, message = "Xóa thành công", data = 1 });
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }
        }
        [HttpPost]
        public IActionResult GetDecals([FromBody] IDModel model)
        {
            List<DecalInfo> products = new List<DecalInfo>();
            try
            {
                products = _dbContext.DecalInfos.Where(e => e.CartItemID == model.ID).ToList();

                return new OkObjectResult(new { status = 1, message = "success", data = products });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = products });
            }
        }

    }
}
