using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ProductCatalogueAppDb.ViewModels
{
    public class CategoryViewModel
    {
        public int ID { get; set; }
        [DisplayName("Naziv")]
        public string Name { get; set; }
    }
}
