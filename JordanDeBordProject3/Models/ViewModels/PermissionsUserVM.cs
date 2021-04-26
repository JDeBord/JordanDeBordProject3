using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Models.ViewModels
{
    public class PermissionsUserVM
    {
        public int Id { get; set; }

        public int ListId { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
