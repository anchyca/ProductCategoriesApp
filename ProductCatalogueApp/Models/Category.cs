using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueApp.Models
{
    public class Category
    {
        //public Category()
        //{
        //    this.Products = new List<Product>();
        //}
        public int ID { get; set; }
        [DisplayName("Naziv")]
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string UserModified { get; set; }

        public virtual ICollection<ProductCategory> Products { get; set; }
    }
}
