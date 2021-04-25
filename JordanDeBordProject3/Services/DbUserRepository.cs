using JordanDeBordProject3.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Services
{
    public class DbUserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _database;

        public DbUserRepository(ApplicationDbContext database)
        {
            _database = database;
        }

        public async Task<ICollection<GroceryList>> ReadAllLists(string userName)
        {
            //var lists = await _database.GroceryLists.Where(l => l.ApplicationUserId == userId);

           // var userAccessLists = user.GroceryListUsers;
            

            // For each one the user has been granted access, if they also don't own it, add to the list.
            //foreach (var listAccess in userAccessLists)
            //{
            //    var list = listAccess.GroceryList;
            //    if (!userLists.Contains(list))
            //    {
            //        userLists.Add(list);
            //    }
            //}

            return null;
        }

        public async Task<ApplicationUser> ReadAsync(string userName)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            return user;
        }
    }
}
