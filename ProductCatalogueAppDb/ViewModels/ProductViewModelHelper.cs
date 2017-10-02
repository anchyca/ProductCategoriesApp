using ProductCatalogueModels;
using System.Collections.Generic;

namespace ProductCatalogueAppDb.ViewModels
{
    public static class ProductViewModelHelper
    {
        public static Product ToProductModel(this ProductCategoriesViewModel productCategoriesViewModel)
        {
            Product product = new Product
            {
                ID = productCategoriesViewModel.ProductID,
                Name = productCategoriesViewModel.Name,
                SKU = productCategoriesViewModel.SKU,
                ImageName = productCategoriesViewModel.Image != null ? productCategoriesViewModel.Image.FileName : ""
            };

            return product;
        }

        public static ProductCategoriesViewModel ToProductViewModel(this Product product)
        {
            ProductCategoriesViewModel viewModel = new ProductCategoriesViewModel
            {
                ProductID = product.ID,
                Name = product.Name,
                SKU = product.SKU,
                ImageName = product.ImageName
            };

            if (product.Categories != null && product.Categories.Count > 0 )
            {
                foreach(var category in product.Categories)
                {
                    viewModel.Categories.Add(new AssignedProductCategory
                    {
                        Assigned = true,
                        CategoryID = category.CategoryId,
                        CategoryName = category.Category== null ? "" : category.Category.Name
                    });
                }
            }

            return viewModel;
        }

        public static List<ProductCategoriesViewModel> ToProductViewModels(this List<Product> products)
        {
            List<ProductCategoriesViewModel> viewModels = new List<ProductCategoriesViewModel>();

            foreach(var product in products)
            {
                viewModels.Add(product.ToProductViewModel());
            }

            return viewModels;
        }

        public static ProductViewModel ToViewModel(this Product product)
        {
            ProductViewModel model = new ProductViewModel
            {
                ID = product.ID,
                Name = product.Name,
                SKU = product.SKU,
                ImageName = product.ImageName,
                ImagePath = product.ImagePath
            };

            return model;
        }

        public static Product ToModel(this ProductViewModel productViewModel)
        {
            Product product = new Product
            {
                ID = productViewModel.ID,
                Name = productViewModel.Name,
                SKU = productViewModel.SKU,
                ImageName = productViewModel.ImageName,
                ImagePath = productViewModel.ImagePath
            };

            return product;
        }
    }
}
