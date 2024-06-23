using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleX.Models;
using System.Diagnostics;

namespace StyleX.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _dbContext;
        public HomeController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewBag.pageName = "Trang chủ";
            return View();
        }
        public IActionResult GetProductHomes()
        {
            List<Product> listProducts = new List<Product>();
            List<Product> newProducts = new List<Product>();
            Product? saleProducts = new Product();
            List<Product> highlightProducts = new List<Product>();

            try
            {
                listProducts = _dbContext.Products.Include(e => e.Category).Where(e => e.Status==true).ToList();
                if (listProducts != null)
                {
                    DateTime now = DateTime.Now;
                    newProducts = listProducts.OrderByDescending(e => e.CreateAt).Take(2).ToList();
                    saleProducts = listProducts
                        .Where(product => product.Sale > 0 && product.SaleEndAt>now) // Lọc các sản phẩm có giảm giá
                        .OrderByDescending(product => product.Sale) // Sắp xếp giảm dần theo tỷ lệ giảm giá
                        .FirstOrDefault(); 
                    highlightProducts = listProducts.OrderByDescending(e => e.Price).Take(6).ToList();

                }
                return new OkObjectResult(new { status = 1, message = "success", data = new { newProducts, saleProducts, highlightProducts } });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = DBNull.Value });
            }

        }

        public IActionResult GetCategories()
        {
            try
            {
                var result = _dbContext.Categories.ToList();
                return new OkObjectResult(new { status = 1, message = "success", data = result });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = new List<Category>() });
            }

        }


    }
}