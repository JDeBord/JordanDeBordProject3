using JordanDeBordProject3.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.ViewModels
{
    public class CreateGroceryItemVM
    {
        [Required]
        [Display(Name="Item Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string Name { get; set; }

        [Display(Name="Quantity to Buy")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Required]
        public string QuantityToBuy { get; set; }

        public bool Shopped { get; set; }

        [Required]
        public int GroceryListId { get; set; }

        public GroceryItem GetGroceryItemInstance() 
        {
            return new GroceryItem
            {
                Id = 0,
                Name = this.Name,
                Shopped = false,
                QuantityToBuy = this.QuantityToBuy,
                GroceryListId = this.GroceryListId
            };
        }
    }
}
