using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Common.DTO
{
    public class Item : BaseDTO
    {
        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime Year { get; set; }
        public string Origin { get; set; }
        public string Condition { get; set; }
    }
}
