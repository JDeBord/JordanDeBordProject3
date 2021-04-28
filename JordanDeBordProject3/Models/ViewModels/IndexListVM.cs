using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.ViewModels
{
    public class IndexListVM
    {
        public int Id { get; set; }

        public int GroceryListUserId { get; set; }

        public string Name { get; set; }

        [Display(Name="Owner's Email")]
        public string OwnerEmail { get; set; }

        [Display(Name="Number of Items")]
        public int NumberItems { get; set; }
    }
}
