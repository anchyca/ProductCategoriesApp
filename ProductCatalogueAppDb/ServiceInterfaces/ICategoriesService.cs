using Microsoft.EntityFrameworkCore;
using ProductCatalogueModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueAppDb.ServiceInterfaces
{
    public interface ICategoriesService
    {
        DbSet<Category> GetAllCategories();
        Task<List<Category>> GetAllCategoriesList();
        Task<Category> GetCategoryById(int id);
        Task CreateCategory(Category category);
        Task UpdateCategory(Category category);
        Task DeleteCategory(int categoryID);
        Task<bool> CategoryExists(int id);
        Task<List<Category>> GetCategoriesPageByFilter(string searchString, int page, int pageSize);
    }
}
