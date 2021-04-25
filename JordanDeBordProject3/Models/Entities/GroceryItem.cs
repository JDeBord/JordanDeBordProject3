using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.Entities
{
    public class GroceryItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string QuantityToBuy { get; set; }

        public bool Shopped { get; set; }

        public int GroceryListId { get; set; }

        [Required]
        public GroceryList GroceryList { get; set; }
    }
}
