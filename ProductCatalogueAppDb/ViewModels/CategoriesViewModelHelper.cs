using ProductCatalogueModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalogueAppDb.ViewModels
{
    public static class CategoriesViewModelHelper
    {
        public static CategoryViewModel ToViewModel(this Category category)
        {
            var viewModel = new CategoryViewModel
            {
                ID = category.ID,
                Name = category.Name
            };

            return viewModel;
        }

        public static Category ToModel(this CategoryViewModel viewModel, string userName)
        {
            var category = new Category
            {
                ID = viewModel.ID,
                Name = viewModel.Name,
                DateModified = DateTime.Now,
                UserModified = userName
            };

            return category;
        }

        public static List<CategoryViewModel> ToViewModels(this List<Category> categories)
        {
            List<CategoryViewModel> viewModels = new List<CategoryViewModel>();

            foreach(var category in categories)
            {
                viewModels.Add(category.ToViewModel());
            }

            return viewModels;
        }
    }
}
