using JordanDeBordProject3.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Services
{
    public class DbGroceryListRepository : IGroceryListRepository
    {
        private readonly ApplicationDbContext _database;

        public DbGroceryListRepository(ApplicationDbContext database)
        {
            _database = database;
        }

        public async Task AddItemAsync(int groceryListId, GroceryItem item)
        {
            var list = await ReadAsync(groceryListId);

            if (list != null) 
            {
                list.GroceryItems.Add(item);
                // CHECK HOW WE DID FOR ORCHESTRA
            }

        }

        public async Task AddUserAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<GroceryList> CreateAsync(string userName, GroceryList groceryList)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user != null) 
            {
                groceryList.User = user;
                user.GroceryLists.Add(groceryList);
                await _database.GroceryLists.AddAsync(groceryList);

                await _database.SaveChangesAsync();
            }

            return groceryList;
        }

        public async Task DeleteAsync(int id)
        {
            var list = await ReadAsync(id);

            if (list != null)
            {
                _database.Remove(list);
                await _database.SaveChangesAsync();
            }
        }

        public async Task<GroceryList> ReadAsync(int id)
        {
            var list = await _database.GroceryLists.Include(i => i.GroceryItems).FirstOrDefaultAsync(l => l.Id == id);

            return list;
        }

        public Task RemoveItemAsync(int groceryListId, int groceryItemId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveUserAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(GroceryList groceryList)
        {
            var listToUpdate = await ReadAsync(groceryList.Id);

            if (listToUpdate != null)
            {
                listToUpdate.Name = groceryList.Name;

                await _database.SaveChangesAsync();
            }
        }
    }
}
