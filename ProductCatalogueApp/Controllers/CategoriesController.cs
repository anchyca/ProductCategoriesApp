using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductCatalogueAppDb.ServiceInterfaces;
using ProductCatalogueAppDb.ViewModels;
using System.Threading.Tasks;

namespace ProductCatalogueApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ICategoriesService _categoriesService;

        public CategoriesController(ILogger<CategoriesController> logger, IConfiguration configuration, ICategoriesService categoriesService)
        {
            _logger = logger;
            _configuration = configuration;
            _categoriesService = categoriesService;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _categoriesService.GetAllCategoriesList());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoriesService.GetCategoryById(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [Authorize(Roles = "Admin")]
        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ID,Name")] CategoryViewModel category)
        {
            if (ModelState.IsValid)
            {
                await _categoriesService.CreateCategory(category, User.Identity.Name);

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoriesService.GetCategoryById(id.Value);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] CategoryViewModel category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoriesService.UpdateCategory(category, User.Identity.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(await _categoriesService.CategoryExists(category.ID)))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoriesService.GetCategoryById(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoriesService.DeleteCategory(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
