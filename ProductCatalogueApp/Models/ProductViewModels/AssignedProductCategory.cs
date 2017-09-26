using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueApp.Models.ProductViewModels
{
    public class AssignedProductCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public bool Assigned { get; set; }
    }
}
