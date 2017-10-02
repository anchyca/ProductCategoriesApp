using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductCatalogueAppDb;
using ProductCatalogueAppDb.ServiceImplementations;
using ProductCatalogueAppDb.ServiceInterfaces;
using ProductCatalogueModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueServiceTests
{
    [TestClass]
    public class ProductsServiceTest
    {
        private ProductCatalogueDbContext _context;
        private IProductsService productsService;

        public ProductsServiceTest()
        {
            this._context = GetDbContext();
            productsService = new ProductsService(_context);
        }

        [TestMethod]
        public async Task TestGetProductByIdProductExists()
        {
            Product product = await productsService.GetProductById(1);

            Assert.IsNotNull(product);
        }

        [TestMethod]
        public async Task TestGetProductByIdCorrectData()
        {
            Product product = await productsService.GetProductById(1);

            Assert.IsTrue(product.SKU == "1234");
        }

        [TestMethod]
        public async Task TestCreateProduct()
        {
            var products = await productsService.GetProductsByFilter("", "");
            products = products.OrderBy(x => x.ID);
            int id = products.Last().ID + 1;

            Product product = new Product
            {
                ID = id,
                SKU = "1235",
                Name = "just created " + id,
                DateCreated = DateTime.Now,
                IsActive = true
            };

            await productsService.CreateProduct(product);

            var prod = await productsService.GetProductById(id);
            Assert.IsNotNull(prod);
        }

        [TestMethod]
        public async Task TestGetProductWithCategories()
        {
            Product product = await productsService.GetProductWithCategories(1);
            Assert.IsTrue(product.Categories.Count() > 0);
        }

        [TestMethod]
        public async Task TestUpdateProduct()
        {
            Product product = await productsService.GetProductById(2);
            string newSku = "New sku " + DateTime.Now.ToString();
            product.SKU = newSku;
            product.DateModified = DateTime.Now;

            await productsService.UpdateProduct(product);

            var prod = await productsService.GetProductById(2);
            Assert.IsTrue(prod.SKU == newSku);
        }

        [TestMethod]
        public async Task TestGetProductByCategory()
        {
            var products = await productsService.GetProductByCategory(1);
            Assert.IsTrue(products.Count == 2);
        }

        [TestMethod]
        public async Task TestRemoveAllCategories()
        {
            // product with id 1 has 2 categories
            Product product = await productsService.GetProductById(1);

            productsService.UpdateProductCategories(product, null);

            Assert.IsTrue(product.Categories.Count == 0);
        }

        [TestMethod]
        public async Task TestAddCategory()
        {
            // product with id 1 has 2 categories
            Product product = await productsService.GetProductById(5);

            productsService.UpdateProductCategories(product, new string[] { "1" });

            Assert.IsTrue(product.Categories.Count == 1);
        }

        private ProductCatalogueDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ProductCatalogueDbContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
            var context = new ProductCatalogueDbContext(options);

            var beerCategory = new Category { ID = 1, Name = "Beers" };
            var wineCategory = new Category { ID = 2, Name = "Wines" };
            context.Category.Add(beerCategory);
            context.Category.Add(wineCategory);

            context.Product.Add(new Product
            {
                ID = 1,
                IsActive = true,
                SKU = "1234",
                Name = "Product 1",
                Categories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        ProductId = 1,
                        CategoryId = 1
                    },
                    new ProductCategory
                    {
                        ProductId = 1,
                        CategoryId = 2
                    }
                }
            });
            context.Product.Add(new Product
            {
                ID = 2,
                IsActive = true,
                SKU = "12342",
                Name = "Product 2",
                Categories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        ProductId = 2,
                        CategoryId = 2
                    }
                }
            });
            context.Product.Add(new Product
            {
                ID = 3,
                IsActive = true,
                SKU = "12343",
                Name = "Product 3",
                Categories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        ProductId = 3,
                        CategoryId = 1
                    },
                    new ProductCategory
                    {
                        ProductId = 3,
                        CategoryId = 2
                    }
                }
            });
            context.Product.Add(new Product
            {
                ID = 4,
                IsActive = true,
                SKU = "12344",
                Name = "Product 4",
                Categories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        ProductId = 4,
                        CategoryId = 2
                    }
                }
            });
            context.Product.Add(new Product
            {
                ID = 5,
                IsActive = true,
                SKU = "12345",
                Name = "Product 5"
            });
            context.Product.Add(new Product
            {
                ID = 6,
                IsActive = true,
                SKU = "12346",
                Name = "Product 6"
            });

            context.SaveChanges();
            return context;
        }
    }
}
