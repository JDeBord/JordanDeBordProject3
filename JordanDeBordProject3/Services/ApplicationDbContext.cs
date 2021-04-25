using JordanDeBordProject3.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JordanDeBordProject3.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GroceryList> GroceryLists { get; set; }

        public DbSet<GroceryItem> GroceryItems { get; set; }

        public DbSet<GroceryListUsers> GroceryListUsers { get; set; }
    }
}
