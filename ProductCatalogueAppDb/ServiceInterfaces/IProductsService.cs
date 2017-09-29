using ProductCatalogueModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueAppDb.ServiceInterfaces
{
    public interface IProductsService
    {

        Task<List<Product>> GetProductsPageByFilter(string currentFilter, string searchString, int page, int pageSize);
        Task<IQueryable<Product>> GetProductsByFilter(string currentFilter, string searchString);
        Task<Product> GetProductById(int id);
        Task CreateProduct(Product product);
        Task<Product> GetProductWithCategories(int id);
        Task UpdateProduct(Product product);
        Task<bool> ProductExists(int id);
        void UpdateProductCategories(Product product, string[] selectedCategories);
        Task<List<Product>> GetProductByCategory(int categoryId);
    }
}
