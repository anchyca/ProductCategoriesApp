using ProductCatalogueModels;

namespace ProductCatalogueApp.Models.ProductViewModels
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
                ImageName = productCategoriesViewModel.Image.FileName
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

            return viewModel;
        }
    }
}
