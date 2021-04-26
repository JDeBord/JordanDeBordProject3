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

        public async Task<ICollection<GroceryList>> ReadAllListsAsync(string userName)
        {
            var user = await ReadAsync(userName);
            var lists = await _database.GroceryLists.Where(l => l.ApplicationUserId == user.Id).ToListAsync();

            var userAccessLists = user.GroceryListUsers;


            // For each one the user has been granted access, if they also don't own it, add to the list.
            if (userAccessLists != null)
            {
                foreach (var userList in userAccessLists)
                {
                    var list = userList.GroceryList;
                    if (!lists.Contains(list))
                    {
                        lists.Add(list);
                    }
                }
            }

            // New up our list that we will return after ordering.
            List<GroceryList> orderedList = null;

            // If the user has lists, order them by Id. Then return. 
            if (lists != null)
            {
                orderedList = lists.OrderBy(l => l.Id).ToList();
            }
                return orderedList;
        }

        public async Task<ApplicationUser> ReadAsync(string userName)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            return user;
        }
    }
}
