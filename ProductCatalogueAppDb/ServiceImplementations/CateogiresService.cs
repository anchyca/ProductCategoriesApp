using ProductCatalogueAppDb.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ProductCatalogueModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using ProductCatalogueAppDb.ViewModels;

namespace ProductCatalogueAppDb.ServiceImplementations
{
    public class CateogiresService : ICategoriesService
    {
        private readonly ProductCatalogueDbContext _context;

        public CateogiresService(ProductCatalogueDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoriesList()
        {
            var categories = await _context.Category.ToListAsync();
            return categories.ToViewModels();
        }

        public async Task<CategoryViewModel> GetCategoryById(int id)
        {
            var category = await _context.Category
                .SingleOrDefaultAsync(m => m.ID == id);
            return category.ToViewModel();
        }

        public async Task CreateCategory(CategoryViewModel category, string userName)
        {
            var categoryToAdd = category.ToModel(userName);
            categoryToAdd.UserCreated = userName;
            categoryToAdd.DateCreated = DateTime.Now;

            _context.Add(categoryToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategory(CategoryViewModel category, string userName)
        {
            var categoryDb = category.ToModel(userName);
            _context.Update(categoryDb);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategory(int categoryID)
        {
            var category = await _context.Category.SingleOrDefaultAsync(m => m.ID == categoryID);
            _context.Category.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CategoryExists(int id)
        {
            return await _context.Category.AnyAsync(e => e.ID == id);
        }

        public async Task<List<CategoryViewModel>> GetCategoriesPageByFilter(string searchString, int page, int pageSize)
        {
            var categories = _context.Category.Select(x => x);

            if (searchString != null)
            {
                page = 1;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories
                    .Where(x => x.Name.Contains(searchString));
            }

            var categoriesPage = await categories.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return categoriesPage.ToViewModels();
        }
    }
}
