using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueAppDb.ViewModels
{
    public class ProductCategoriesViewModel
    {
        public ProductCategoriesViewModel()
        {
            Categories = new Collection<AssignedProductCategory>();
        }

        public int ProductID { get; set; }
        public string SKU { get; set; }
        [DisplayName("Naziv")]
        public string Name { get; set; }
        [DisplayName("Naziv slike")]
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        [DisplayName("Slika")]
        public IFormFile Image { get; set; }
        [DisplayName("Kategorije")]
        public virtual ICollection<AssignedProductCategory> Categories { get; set; }
    }
}
