using Microsoft.EntityFrameworkCore;
using ProductCatalogueAppDb.ViewModels;
using ProductCatalogueModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueAppDb.ServiceInterfaces
{
    public interface ICategoriesService
    {
        Task<List<CategoryViewModel>> GetAllCategoriesList();
        Task<CategoryViewModel> GetCategoryById(int id);
        Task CreateCategory(CategoryViewModel category, string userName);
        Task UpdateCategory(CategoryViewModel category, string userName);
        Task DeleteCategory(int categoryID);
        Task<bool> CategoryExists(int id);
        Task<List<CategoryViewModel>> GetCategoriesPageByFilter(string searchString, int page, int pageSize);
    }
}
