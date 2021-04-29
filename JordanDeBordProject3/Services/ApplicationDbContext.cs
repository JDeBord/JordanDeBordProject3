using JordanDeBordProject3.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JordanDeBordProject3.Services
{
    /// <summary>
    /// ApplicationDbContext class acts as the bridge between our application and the database.
    /// We us this class to interact with our database.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Constructor for ApplicationDbContext, which passes the options to the constructor for the super class.
        /// </summary>
        /// <param name="options">Options to be passed to the super constructor. These are used to link to the 
        ///     database using the ConnectionString from our appsettings.json file. </param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Represents the associated table for GroceryLists in our databse.
        /// </summary>
        public DbSet<GroceryList> GroceryLists { get; set; }

        /// <summary>
        /// Represents the associated table for Grocery Items in our database.
        /// </summary>
        public DbSet<GroceryItem> GroceryItems { get; set; }

        /// <summary>
        /// Represents the Grocery List Users table in the database. This is the associative entity
        /// between Users and Lists, as a user can have access to multiple lists, and a list can have access granted
        /// for multiple users.
        /// </summary>
        public DbSet<GroceryListUser> GroceryListUsers { get; set; }
    }
}
