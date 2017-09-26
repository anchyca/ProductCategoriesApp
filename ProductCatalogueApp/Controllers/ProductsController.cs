using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogueApp.Data;
using ProductCatalogueApp.Models;
using ProductCatalogueApp.Models.ProductViewModels;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Auth;

namespace ProductCatalogueApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureStorageConfig storageConfig;

        public ProductsController(ApplicationDbContext context, IOptions<AzureStorageConfig> config)
        {
            _context = context;
            storageConfig = config.Value;
        }

        // GET: Products
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;

            var products = _context.Product
                    .Include(x => x.Categories)
                    .ThenInclude(x => x.Category).Where(x => x.IsActive == true);

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                var productIds = await _context.Category
                                    .Include(x => x.Products)
                                    .Where(x => x.Name.Contains(searchString))
                                    .SelectMany(x => x.Products.Select(y => y.ProductId))
                                    .ToListAsync();
                products = products
                    .Where(x => x.IsActive == true &&
                            (x.SKU.Contains(searchString) || x.Name.Contains(searchString) || productIds.Contains(x.ID)));
            }

            int pageSize = 3;
            return View(await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), page ?? 1, pageSize));

        }

        public async Task<IActionResult> GetProductByCategory(int categoryId)
        {
            var products = await _context.Product
                            .Include(x => x.Categories)
                                .ThenInclude(x => x.Category)
                            .Where(x => x.Categories.Select(y => y.CategoryId).Contains(categoryId)).ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            product.ImagePath = GetImagePath(product.ImageName);

            return View(product);
        }

       
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
        public async Task<IActionResult> Create(ProductCategoriesViewModel productCategoriesViewModel, IFormFile file, string[] selectedCategories)
        {
            if (ModelState.IsValid)
            {
                productCategoriesViewModel.Image = file;
                var product = productCategoriesViewModel.ToProductModel();

                product.DateCreated = DateTime.Now;
                product.UserCreated = User.Identity.Name;

                await UploadImageToAzure(file);

                AddCategoriesToProduct(product, selectedCategories);
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productCategoriesViewModel);
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                        .Include(x => x.Categories)
                        .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            var productViewModel = product.ToProductViewModel();
            PopulateAssignedCategories(productViewModel, product.Categories);
            productViewModel.ImagePath = GetImagePath(productViewModel.ImageName);

            return View(productViewModel);
        }


        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    var productToUpdate = await _context.Product.Include(x => x.Categories).SingleOrDefaultAsync(x => x.ID == id);
                    productToUpdate.Name = productCategoriesViewModel.Name;
                    productToUpdate.SKU = productCategoriesViewModel.SKU;

                    if (file.FileName.CompareTo(productToUpdate.ImageName) != 0)
                    {
                        productToUpdate.ImageName = file.FileName;
                        await DeleteImageFromAzure(file);
                        await UploadImageToAzure(file);
                    }

                    productToUpdate.DateModified = DateTime.Now;
                    productToUpdate.UserModified = User.Identity.Name;

                    UpdateProductCategories(productToUpdate, selectedCategories);
                    PopulateAssignedCategories(productCategoriesViewModel, productToUpdate.Categories);

                    _context.Update(productToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(productCategoriesViewModel.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productCategoriesViewModel);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.SingleOrDefaultAsync(m => m.ID == id);

            product.DateModified = DateTime.Now;
            product.UserModified = User.Identity.Name;
            product.IsActive = false;

            _context.Product.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ID == id);
        }

        private ICollection<AssignedProductCategory> FetchCategories()
        {
            var categories = _context.Category;
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
            var allCategories = _context.Category;
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

        private void UpdateProductCategories(Product product, string[] selectedCategories)
        {
            if (selectedCategories == null)
            {
                product.Categories = new List<ProductCategory>();
                return;
            }

            var currentCategories = product.Categories.Select(x => x.CategoryId).ToList();
            var allCategories = _context.Category;

            foreach (var category in allCategories)
            {
                if (selectedCategories.Contains(category.ID.ToString()))
                {
                    if (!currentCategories.Contains(category.ID))
                    {
                        product.Categories.Add(new ProductCategory
                        {
                            CategoryId = category.ID,
                            ProductId = product.ID
                        });
                    }
                }
                else
                {
                    if (currentCategories.Contains(category.ID))
                    {
                        ProductCategory productCategoryToRemove = product.Categories.SingleOrDefault(x => x.CategoryId == category.ID);
                        _context.Remove(productCategoryToRemove);
                    }
                }
            }
        }

        private async Task<bool> UploadImageToAzure(IFormFile file)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);

            // Upload the file
            await blockBlob.UploadFromStreamAsync(file.OpenReadStream());

            return await Task.FromResult(true);
        }

        private async Task<bool> DeleteImageFromAzure(IFormFile file)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);

            // Upload the file
            await blockBlob.DeleteIfExistsAsync();

            return await Task.FromResult(true);
        }

        private string GetImagePath(string imageName)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageName);

            return blockBlob.Uri.ToString();
        }

    }
}
