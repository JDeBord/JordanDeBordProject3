using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.Entities
{
    public class GroceryList
    {
        public int Id { get; set; }

        public string Name { get; set; }


        [Required]
        public string ApplicationUserId { get; set; }

        public ApplicationUser User { get; set; }

        public List<GroceryItem> GroceryItems { get; set; }
    }
}
