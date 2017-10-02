using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductCatalogueModels;
using ProductCatalogueAppDb;
using ProductCatalogueAppDb.ServiceInterfaces;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProductCatalogueWebApi.Controllers
{
    [Route("api/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly ICategoriesService _categoriesService;
        private readonly ILogger _logger;
        private readonly PageSizeConfig _configuration;

        public CategoriesController(ICategoriesService categoriesService, ILogger<ProductsController> logger, IOptions<PageSizeConfig> configuration)
        {
            this._categoriesService = categoriesService;
            this._logger = logger;
            this._configuration = configuration.Value;
        }
        // GET: api/values
        [HttpGet]
        public async Task<JsonResult> GetCategoriesBySearch(string searchString, int page)
        {
            try
            {
                int pageSize = _configuration.CategoriesPageSize;

                var categories = await _categoriesService.GetCategoriesPageByFilter(searchString, page, pageSize);

                page++;

                return new JsonResult(new
                {
                    categories = categories,
                    page = page
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Greška prilikom dohvata kategorija.");
                return null;
            }
        }
    }
}
