using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ProductCatalogueAppDb.ViewModels
{
    public class ProductViewModel
    {
        public int ID { get; set; }
        public string SKU { get; set; }
        [DisplayName("Naziv")]
        public string Name { get; set; }
        public string ImagePath { get; set; }
        [DisplayName("Slika")]
        public string ImageName { get; set; }
    }
}
