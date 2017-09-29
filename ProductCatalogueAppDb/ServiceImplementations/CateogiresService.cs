using ProductCatalogueAppDb.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ProductCatalogueModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ProductCatalogueAppDb.ServiceImplementations
{
    public class CateogiresService : ICategoriesService
    {
        private readonly ProductCatalogueDbContext _context;

        public CateogiresService(ProductCatalogueDbContext context)
        {
            _context = context;
        }
        public DbSet<Category> GetAllCategories()
        {
            return _context.Category;
        }

        public async Task<List<Category>> GetAllCategoriesList()
        {
            return await _context.Category.ToListAsync();
        }

        public async Task<Category> GetCategoryById(int id)
        {
            var category = await _context.Category
                .SingleOrDefaultAsync(m => m.ID == id);
            return category;
        }

        public async Task CreateCategory(Category category)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategory(Category category)
        {
            _context.Update(category);
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

        public async Task<List<Category>> GetCategoriesPageByFilter(string searchString, int page, int pageSize)
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

            return await categories.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
    }
}
