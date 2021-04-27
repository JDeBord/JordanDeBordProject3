using JordanDeBordProject3.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.ViewModels
{
    public class GoShoppingListVM
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public List<GoShoppingItemVM> GroceryItems { get; set; }

        public int GroceryListUserId { get; set; }
    }
}
