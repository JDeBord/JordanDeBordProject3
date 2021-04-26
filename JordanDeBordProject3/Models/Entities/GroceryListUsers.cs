using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.Entities
{
    public class GroceryListUsers
    {
        public int Id { get; set; }

        public bool Owner { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        
        [Required]
        public int GroceryListId { get; set; }
        public GroceryList GroceryList { get; set; }
    }
}
