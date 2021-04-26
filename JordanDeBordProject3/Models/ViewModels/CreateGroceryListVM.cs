using JordanDeBordProject3.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.ViewModels
{
    public class CreateGroceryListVM
    {
        [Required]
        [Display(Name ="Grocery List Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string Name { get; set; }


        public GroceryList GetGroceryListInstance() 
        {
            return new GroceryList
            {
                Id = 0,
                Name = this.Name
            };
        }
    }
}
