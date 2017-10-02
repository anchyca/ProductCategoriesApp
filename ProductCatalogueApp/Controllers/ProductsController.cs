using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogueAppDb.ViewModels;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using ProductCatalogueAppDb.ServiceInterfaces;
using ProductCatalogueModels;
using ProductCatalogueApp.Models;
using ProductCatalogueApp.Services;

namespace ProductCatalogueApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ILogger _logger;
        private readonly IProductsService _productsService;
        private readonly ICategoriesService _categoriesService;
        private readonly IStorageService _storageService;
        private readonly PageSizeConfig pageSizeConfig;

        public ProductsController(ILogger<ProductsController> logger, IProductsService productsService, ICategoriesService categoriesService, IStorageService storageService, IOptions<PageSizeConfig> options)
        {
            this._logger = logger;
            _categoriesService = categoriesService;
            _storageService = storageService;
            _productsService = productsService;
            pageSizeConfig = options.Value;
        }

        // GET: Products
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;

            int pageSize = pageSizeConfig.ProductsPageSize;
            var products = await _productsService.GetProductsByFilter(currentFilter, searchString);

            return View(PaginatedList<ProductCategoriesViewModel>.Create(products, page ?? 1, pageSize));
        }


        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = await _productsService.GetProductById(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageName))
            {
                product.ImagePath = _storageService.GetImagePath(product.ImageName);
            }

            return View(product);

        }

        [Authorize(Roles = "Admin")]
        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var productCategoryViewModel = new ProductCategoriesViewModel { Categories = await FetchCategories() };
            return View(productCategoryViewModel);
        }


        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductCategoriesViewModel productCategoriesViewModel, IFormFile file, string[] selectedCategories)
        {
            if (ModelState.IsValid)
            {
                productCategoriesViewModel.Image = file;
                if (file != null)
                {
                    await _storageService.UploadImageToStorage(file);
                }             
                await _productsService.CreateProduct(productCategoriesViewModel, User.Identity.Name, selectedCategories);

                return RedirectToAction(nameof(Index));

            }
            return View(productCategoriesViewModel);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productViewModel = await _productsService.GetProductWithCategories(id.Value);

            if (productViewModel == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(productViewModel.ImageName))
            {
                productViewModel.ImagePath = _storageService.GetImagePath(productViewModel.ImageName);
            }

            return View(productViewModel);
        }


        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,SKU,Name,ImagePath")] ProductCategoriesViewModel productCategoriesViewModel, IFormFile file, string[] selectedCategories)
        {
            if (id != productCategoriesViewModel.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null)
                    {
                        await _storageService.DeleteImageFromStorage(file);
                        await _storageService.UploadImageToStorage(file);
                    }
                    await _productsService.UpdateProduct(productCategoriesViewModel, file == null ? "": file.FileName, selectedCategories);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(await _productsService.ProductExists(productCategoriesViewModel.ProductID)))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productCategoriesViewModel);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productsService.GetProductById(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productsService.DeleteProduct(id, User.Identity.Name);

            return RedirectToAction(nameof(Index));

        }


        #region privateFunctions
        private async Task<ICollection<AssignedProductCategory>> FetchCategories()
        {
            var categories = await _categoriesService.GetAllCategoriesList();
            var assignedCategories = new List<AssignedProductCategory>();

            foreach (var item in categories)
            {
                assignedCategories.Add(new AssignedProductCategory
                {
                    CategoryID = item.ID,
                    CategoryName = item.Name,
                    Assigned = false
                });
            }

            return assignedCategories;
        }
    }
    #endregion
}
