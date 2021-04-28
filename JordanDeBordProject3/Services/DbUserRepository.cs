﻿using JordanDeBordProject3.Models.Entities;
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

        public async Task<bool> CheckPermissionAsync(string userName, GroceryList list)
        {
            var userLists = await ReadAllListsAsync(userName);

            if (userLists.Contains(list))
            {
                return true;
            }
            return false;
        }

        public async Task<ICollection<GroceryList>> ReadAllListsAsync(string userName)
        {
            var user = await ReadAsync(userName);

            var userAccessLists = user.GroceryListUsers;

            var lists = new List<GroceryList>();

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
            return lists;
        }

        public async Task<ICollection<GroceryListUser>> ReadListAccessAsync(string userName) 
        {
            var user = await ReadAsync(userName);

            var listAccess = await _database.GroceryListUsers
                                    .Include(u => u.ApplicationUser)
                                    .Include(l => l.GroceryList)
                                    .Where(us => us.ApplicationUser.UserName == userName).ToListAsync();

            return listAccess;
        }

        public async Task<ApplicationUser> ReadAsync(string userName)
        {
            var user = await _database.Users.Include(gl => gl.GroceryListUsers)
                                .ThenInclude(l => l.GroceryList)
                                .ThenInclude(i => i.GroceryItems)
                                .FirstOrDefaultAsync(u => u.UserName == userName);

            return user;
        }

        public async Task<string> GetNameAsync(string email)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.Email == email);
            var name = $"{user.FirstName} {user.LastName}";

            return name;
        }
    }
}
