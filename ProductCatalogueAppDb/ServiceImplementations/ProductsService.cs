using System.Collections.Generic;
using ProductCatalogueAppDb.ServiceInterfaces;
using ProductCatalogueModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueAppDb.ServiceImplementations
{
    public class ProductsService : IProductsService
    {
        private readonly ProductCatalogueDbContext _context;

        public ProductsService(ProductCatalogueDbContext context)
        {
            _context = context;
        }        

        public async Task<List<Product>> GetProductsPageByFilter(string currentFilter, string searchString, int page, int pageSize)
        {
            var products = await GetProductsByFilter(currentFilter, searchString);

            return await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<IQueryable<Product>> GetProductsByFilter(string currentFilter, string searchString)
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

            return products.AsNoTracking();
        }

        public async Task<Product> GetProductById(int id)
        {
            var product = await _context.Product
                    .SingleOrDefaultAsync(m => m.ID == id);

            return product;
        }

        public async Task CreateProduct(Product product)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task<Product> GetProductWithCategories(int id)
        {
            var product = await _context.Product
                       .Include(x => x.Categories)
                       .SingleOrDefaultAsync(m => m.ID == id);

            return product;
        }

        public async Task UpdateProduct(Product product)
        {
            _context.Update(product);
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
    }
}
