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

        public List<GroceryItem> GroceryItems { get; set; }

        public List <GroceryListUsers> GroceryListUsers { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser User { get; set; }


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
                var owner = GroceryListUsers.FirstOrDefault(l => l.Owner == true);

                return owner.ApplicationUser.Email;
            }
        }

    }
}
