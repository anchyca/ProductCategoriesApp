using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ProductCatalogueModels;
using Microsoft.Extensions.Configuration;
using ProductCatalogueAppDb;
using ProductCatalogueAppDb.ServiceInterfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProductCatalogueWebApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductsService _productsService;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public ProductsController(IProductsService productsService, ILogger<ProductsController> logger, IConfiguration configuration)
        {
            this._productsService = productsService;
            this._logger = logger;
            this._configuration = configuration;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllProducts(string currentFilter, string searchString, int page)
        {
            try
            {
                int pageSize = 3; // _configuration.GetValue<int>("ProductsPageSize");

                var products = await _productsService.GetProductsPageByFilter(currentFilter, searchString, page, pageSize);

                page++;

                return new JsonResult(new
                {
                    products = products,
                    page = page
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Greška prilikom dohvaćanja produkata.");
                return null;
            }
        }

        //[HttpGet("{categoryId}")]
        [Route("GetByCategory/{categoryId}")]
        public async Task<IEnumerable<Product>> GetProductByCategory(int categoryId)
        {
            try
            {
                var products = await _productsService.GetProductByCategory(categoryId);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Greška prilikom dohvaćanja produkata prema ID-u kategorije");
                return null;
            }
        }
    }
}
