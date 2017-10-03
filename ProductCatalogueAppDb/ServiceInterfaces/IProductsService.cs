using ProductCatalogueAppDb.ViewModels;
using ProductCatalogueModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueAppDb.ServiceInterfaces
{
    public interface IProductsService
    {

        Task<List<ProductCategoriesViewModel>> GetProductsPageByFilter(string currentFilter, string searchString, int page, int pageSize);
        Task<IQueryable<ProductCategoriesViewModel>> GetProductsByFilter(string currentFilter, string searchString);
        Task<ProductViewModel> GetProductById(int id);
        Task CreateProduct(ProductCategoriesViewModel productCategoriesViewModel, string userName, string[] selectedCategories);
        Task<ProductCategoriesViewModel> GetProductWithCategories(int id);
        Task UpdateProduct(ProductCategoriesViewModel productCategoriesViewModel, string fileName, string[] selectedCategories);
        Task<bool> ProductExists(int id);
        void UpdateProductCategories(Product product, string[] selectedCategories);
        Task<List<ProductViewModel>> GetProductByCategory(int categoryId);
        Task DeleteProduct(int id, string userName);
    }
}
