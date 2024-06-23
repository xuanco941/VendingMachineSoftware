using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleX.DTOs;
using StyleX.Models;

namespace StyleX.Controllers
{
    [AllowAnonymous]
    public class ProductController : Controller
    {
        private readonly DatabaseContext _dbContext;
        public ProductController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewBag.pageName = "Sản phẩm";
            return View();
        }

        [HttpGet("product/detail/{productID}")]
        public IActionResult Detail(int productID)
        {
            ViewBag.pageName = "Sản phẩm";

            Product? product = _dbContext.Products.Find(productID);

            ViewBag.product = product;


            return View();
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                var query2 = _dbContext.Products;
                return new OkObjectResult(new { status = 1, message = "success", data = query2.ToList() });

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { status = -99, message = e.Message, data = new List<Product>() });
            }

        }

    }
}
