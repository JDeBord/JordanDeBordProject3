using JordanDeBordProject3.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.ViewModels
{
    public class GoShoppingItemVM
    {
        public int Id { get; set; }

        public bool Shopped { get; set; }

        public string Name { get; set; }

        public string QuantityToBuy { get; set; }

        public GroceryItem GetItemInstance() 
        {
            return new GroceryItem
            {
                Id = this.Id,
                Shopped = this.Shopped
            };
        }
    }
}
