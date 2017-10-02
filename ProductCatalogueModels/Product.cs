using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductCatalogueModels
{
    public class Product
    {
        public Product()
        {
            this.Categories = new List<ProductCategory>();
            this.IsActive = true;
        }

        public int ID { get; set; }
        public string SKU { get; set; }

        [DisplayName("Naziv")]
        public string Name { get; set; }
        [DisplayName("Slika")]
        public string ImageName { get; set; }
        [NotMapped]
        public string ImagePath { get; set; }

        public DateTime DateCreated { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string UserModified { get; set; }

        public bool IsActive { get; set; }

        [DisplayName("Kategorije")]
        public virtual List<ProductCategory> Categories { get; set; }
    }
}