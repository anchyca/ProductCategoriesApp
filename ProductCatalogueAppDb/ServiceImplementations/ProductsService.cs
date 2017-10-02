using System.Collections.Generic;
using ProductCatalogueAppDb.ServiceInterfaces;
using ProductCatalogueModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ProductCatalogueAppDb.ViewModels;
using System;

namespace ProductCatalogueAppDb.ServiceImplementations
{
    public class ProductsService : IProductsService
    {
        private readonly ProductCatalogueDbContext _context;

        public ProductsService(ProductCatalogueDbContext context)
        {
            _context = context;
        }        

        public async Task<List<ProductCategoriesViewModel>> GetProductsPageByFilter(string currentFilter, string searchString, int page, int pageSize)
        {
            var products = await GetProductsByFilter(currentFilter, searchString);

            return await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<IQueryable<ProductCategoriesViewModel>> GetProductsByFilter(string currentFilter, string searchString)
        {
            var products = _context.Product
                       .Include(x => x.Categories)
                       .ThenInclude(x => x.Category).Where(x => x.IsActive == true);

            if (searchString == null)
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

            var productViewModels = products.AsNoTracking().ToList().ToProductViewModels();

            return productViewModels.AsQueryable().AsNoTracking();
        }

        public async Task<ProductViewModel> GetProductById(int id)
        {
            var product = await _context.Product
                    .SingleOrDefaultAsync(m => m.ID == id);

            return product.ToViewModel();
        }

        public async Task CreateProduct(ProductCategoriesViewModel productCategoriesViewModel, string userName, string[] selectedCategories)
        {
            
            var product = productCategoriesViewModel.ToProductModel();

            product.DateCreated = DateTime.Now;
            product.UserCreated = userName;
            product.DateModified = DateTime.Now;
            product.UserModified = userName;

            AddCategoriesToProduct(product, selectedCategories);

            _context.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductCategoriesViewModel> GetProductWithCategories(int id)
        {
            var product = await _context.Product
                       .Include(x => x.Categories)
                       .SingleOrDefaultAsync(m => m.ID == id);
            var productViewModel = product.ToProductViewModel();
            await PopulateAssignedCategories(productViewModel, product.Categories);

            return productViewModel;
        }

        public async Task UpdateProduct(ProductCategoriesViewModel productCategoriesViewModel, string fileName, string[] selectedCategories)
        {
            var productToUpdate = await _context.Product.Where(x => x.ID == productCategoriesViewModel.ProductID)
                .Include(x => x.Categories).SingleOrDefaultAsync();

            productToUpdate.Name = productCategoriesViewModel.Name;
            productToUpdate.SKU = productCategoriesViewModel.SKU;

            productToUpdate.ImageName = fileName;

            UpdateProductCategories(productToUpdate, selectedCategories);
            await PopulateAssignedCategories(productCategoriesViewModel, productToUpdate.Categories);

            _context.Update(productToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ProductExists(int id)
        {
            return await _context.Product.AnyAsync(e => e.ID == id);
        }

        public void UpdateProductCategories(Product product, string[] selectedCategories)
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

        public async Task<List<Product>> GetProductByCategory(int categoryId)
        {
            var products = await _context.Product
                                .Include(x => x.Categories)
                                    .ThenInclude(x => x.Category)
                                .Where(x => x.Categories.Select(y => y.CategoryId).Contains(categoryId)).ToListAsync();
            return products;
        }

        public async Task DeleteProduct(int id, string userName)
        {
            var product = await _context.Product.Where(x => x.ID == id).FirstOrDefaultAsync();
            if (product != null)
            {
                product.IsActive = false;
                product.DateModified = DateTime.Now;
                product.UserModified = userName;
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
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

        private async Task PopulateAssignedCategories(ProductCategoriesViewModel productViewModel, ICollection<ProductCategory> productCategories)
        {
            var categoriyList = await _context.Category.ToListAsync();
            var allCategories = categoriyList.ToViewModels();
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
}
