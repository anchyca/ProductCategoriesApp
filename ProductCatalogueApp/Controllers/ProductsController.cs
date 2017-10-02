using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogueApp.Data;
using ProductCatalogueApp.Models.ProductViewModels;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using ProductCatalogueAppDb.ServiceInterfaces;
using ProductCatalogueModels;
using ProductCatalogueApp.Models;
using ProductCatalogueApp.Services;

namespace ProductCatalogueApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IProductsService _productsService;
        private readonly ICategoriesService _categoriesService;
        private readonly IStorageService _storageService;

        public ProductsController(ILogger<ProductsController> logger, IConfiguration configuration, IProductsService productsService, ICategoriesService categoriesService, IStorageService storageService)
        {
            this._logger = logger;
            _configuration = configuration;
            _categoriesService = categoriesService;
            _storageService = storageService;
            _productsService = productsService;
        }

        // GET: Products
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;

            int pageSize = _configuration.GetValue<int>("ProductsPageSize");

            var products = await _productsService.GetProductsByFilter(currentFilter, searchString);


            return View(await PaginatedList<Product>.CreateAsync(products, page ?? 1, pageSize));

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
        public IActionResult Create()
        {
            var productCategoryViewModel = new ProductCategoriesViewModel { Categories = FetchCategories() };
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
                var product = productCategoriesViewModel.ToProductModel();

                product.DateCreated = DateTime.Now;
                product.UserCreated = User.Identity.Name;

                await _storageService.UploadImageToStorage(file);

                AddCategoriesToProduct(product, selectedCategories);
                await _productsService.CreateProduct(product);

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

            var product = await _productsService.GetProductWithCategories(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            var productViewModel = product.ToProductViewModel();
            PopulateAssignedCategories(productViewModel, product.Categories);
            if (!string.IsNullOrEmpty(product.ImageName))
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
                    var productToUpdate = await _productsService.GetProductWithCategories(id);

                    productToUpdate.Name = productCategoriesViewModel.Name;
                    productToUpdate.SKU = productCategoriesViewModel.SKU;

                    if (file != null && file.FileName.CompareTo(productToUpdate.ImageName) != 0)
                    {
                        productToUpdate.ImageName = file.FileName;
                        await _storageService.DeleteImageFromStorage(file);
                        await _storageService.UploadImageToStorage(file);
                    }

                    productToUpdate.DateModified = DateTime.Now;
                    productToUpdate.UserModified = User.Identity.Name;

                    _productsService.UpdateProductCategories(productToUpdate, selectedCategories);
                    PopulateAssignedCategories(productCategoriesViewModel, productToUpdate.Categories);

                    await _productsService.UpdateProduct(productToUpdate);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(await _productsService.ProductExists(productCategoriesViewModel.ProductID)))
                    {
                        return NotFound();
                    }
                    //else
                    //{
                    //    throw el

                    //}
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

            var product = await _productsService.GetProductById(id);

            product.DateModified = DateTime.Now;
            product.UserModified = User.Identity.Name;
            product.IsActive = false;

            await _productsService.UpdateProduct(product);

            return RedirectToAction(nameof(Index));

        }


        #region privateFunctions
        private ICollection<AssignedProductCategory> FetchCategories()
        {
            var categories = _categoriesService.GetAllCategories();
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


        private void AddCategoriesToProduct(Product product, string[] categories)
        {
            if (categories == null) return;

            foreach (var item in categories)
            {
                var category = new ProductCategory { ProductId = product.ID, CategoryId = int.Parse(item) };
                product.Categories.Add(category);
            }
        }

        private void PopulateAssignedCategories(ProductCategoriesViewModel productViewModel, ICollection<ProductCategory> productCategories)
        {
            var allCategories = _categoriesService.GetAllCategories();
            var categories = productCategories.Select(x => x.CategoryId).ToList();
            var assignViewModel = new List<AssignedProductCategory>();
            foreach (var category in allCategories)
            {
                assignViewModel.Add(new AssignedProductCategory
                {
                    CategoryID = category.ID,
                    CategoryName = category.Name,
                    Assigned = categories.Contains(category.ID)
                });
            }

            productViewModel.Categories = assignViewModel;
        }
    }
    #endregion
}
