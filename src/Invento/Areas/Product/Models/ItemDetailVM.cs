using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Invento.Areas.Product.Models
{
    public class ItemDetailVM
    {
        public int ItemDetailVMID { get; set; }
        public string ItemName { get; set; }
        public string ItemPhoto { get; set; }
        public decimal Quantity { get; set; }
    }
}
