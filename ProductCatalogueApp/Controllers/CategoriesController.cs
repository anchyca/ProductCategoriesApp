using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductCatalogueApp.Data;
using ProductCatalogueApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ProductCatalogueApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.Category.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Greška prilikom dohvata kategorija.");
                return View("Error");
            }
        }

        public async Task<JsonResult> GetCategoriesBySearch(string currentFilter, string searchString, int? page)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;

                var categories = _context.Category.Select(x => x);

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
                    categories = categories
                        .Where(x => x.Name.Contains(searchString));
                }

                int pageSize = _configuration.GetValue<int>("CategoriesPageSize");

                return new JsonResult(new
                {
                    categories = await PaginatedList<Category>.CreateAsync(categories.AsNoTracking(), page ?? 1, pageSize),
                    hasErrors = false,
                    errorMessage = ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Greška prilikom dohvata kategorija.");
                return new JsonResult(new
                {
                    hasErrors = true,
                    errorMessage = "Greška prilikom dohvata kategorija."
                });
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .SingleOrDefaultAsync(m => m.ID == id);
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
        public async Task<IActionResult> Create([Bind("ID,Name")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    category.DateCreated = DateTime.Now;
                    category.UserCreated = User.Identity.Name;
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Greška prilikom stvaranja kategorije.");
                return View("Error");
            }
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.SingleOrDefaultAsync(m => m.ID == id);
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
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] Category category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.DateModified = DateTime.Now;
                    category.UserModified = User.Identity.Name;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Greška prilikom editiranja kategorije.");
                        return View("Error");
                    }
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

            var category = await _context.Category
                .SingleOrDefaultAsync(m => m.ID == id);
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
            try
            {
                var category = await _context.Category.SingleOrDefaultAsync(m => m.ID == id);
                _context.Category.Remove(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Greška prilikom brisanja kategorije.");
                return View("Error");
            }
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.ID == id);
        }
    }
}
