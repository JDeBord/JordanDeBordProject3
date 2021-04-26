using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.Entities
{
    public class GroceryList
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<GroceryItem> GroceryItems { get; set; } = new List<GroceryItem>();

        public List<GroceryListUsers> GroceryListUsers { get; set; } = new List<GroceryListUsers>();

        // [Required]
        // public string ApplicationUserId { get; set; }

        // public ApplicationUser User { get; set; }


        [NotMapped]
        public int NumberItems 
        { 
            get 
            {
                return GroceryItems.Count;
            }
        
        }

        [NotMapped]
        public string OwnerEmail
        {
            get 
            {
                var owner = GroceryListUsers.FirstOrDefault(u => u.Owner = true);
                if (owner != null)
                {
                    return owner.ApplicationUser.Email;
                }
                else 
                {
                    return null;
                }
            }
        }

    }
}
