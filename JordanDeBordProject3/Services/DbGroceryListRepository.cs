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

        public async Task<GroceryItem> AddItemAsync(int groceryListId, GroceryItem item)
        {
            var list = await ReadAsync(groceryListId);

            if (list != null) 
            {
                list.GroceryItems.Add(item);
                item.GroceryList = list;

                await _database.SaveChangesAsync();
            }
            return item;
        }

        public async Task<int?> GrantPermissionAsync(int listId, string email)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.Email == email);

            var list = await ReadAsync(listId);

            if (user != null && list != null)
            {
                var listAccess = await _database.GroceryListUsers.FirstOrDefaultAsync(u => u.GroceryListId == list.Id && u.ApplicationUserId == user.Id);

                if (listAccess == null)
                {
                    var userAccess = new GroceryListUser
                    {
                        ApplicationUser = user,
                        GroceryList = list,
                        Owner = false
                    };

                    user.GroceryListUsers.Add(userAccess);
                    list.GroceryListUsers.Add(userAccess);

                    await _database.SaveChangesAsync();

                    return userAccess.Id;
                }
            }
            return null;
        }

        public async Task<GroceryList> CreateAsync(string userName, GroceryList groceryList)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user != null) 
            {
                var userAccess = new GroceryListUser
                {
                    ApplicationUser = user,
                    GroceryList = groceryList,
                    Owner = true
                };
                
                groceryList.GroceryListUsers.Add(userAccess);

                user.GroceryListUsers.Add(userAccess);

                groceryList.OwnerEmail = user.Email;

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
            var list = await _database.GroceryLists
                .Include(i => i.GroceryItems)
                .FirstOrDefaultAsync(l => l.Id == id);

            return list;
        }

        public async Task<bool> RemoveItemAsync(int groceryListId, int groceryItemId)
        {
            var list = await ReadAsync(groceryListId);

            var item = await GetItemAsync(groceryItemId);

            if (list != null && item != null) 
            {
                _database.GroceryItems.Remove(item);

                await _database.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> RemoveUserAsync(int userAccessId)
        {
            var userAccess = await _database.GroceryListUsers.FirstOrDefaultAsync(u => u.Id == userAccessId);

            if (userAccess != null) 
            {
                _database.GroceryListUsers.Remove(userAccess);

                await _database.SaveChangesAsync();
                return true;
            }
            return false;
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

        public async Task<GroceryItem> GetItemAsync(int itemId) 
        {
            var item = await _database.GroceryItems.FirstOrDefaultAsync(i => i.Id == itemId);

            return item;
        }

        public async Task<ICollection<GroceryListUser>> GetAdditionalUsersAsync(int id) 
        {
            // var list = await ReadAsync(id);
            var userAccess = await _database.GroceryListUsers.Include(l => l.GroceryList).Include(u => u.ApplicationUser).Where(li => li.GroceryListId == id).ToListAsync();
            var additionalUsers = new List<GroceryListUser>();

            foreach (var u in userAccess)
            {
                if (u.Owner == false)
                {
                    additionalUsers.Add(u);
                }
            }
            return additionalUsers;
        }

        public async Task<GroceryListUser> GetPermissionAsync(int id) 
        {
            var access = await _database.GroceryListUsers
                .Include(u => u.ApplicationUser)
                .Include(li => li.GroceryList)
                .FirstOrDefaultAsync(l => l.Id == id);

            return access;
        }

        public async Task<GroceryListUser> GetPermissionAsync(int listId, string userId) 
        { 
            var permission = await _database.GroceryListUsers
                .Include(u => u.ApplicationUser)
                .Include(li => li.GroceryList)
                .FirstOrDefaultAsync(l => l.ApplicationUserId == userId && l.GroceryListId == listId);

            return permission;
        }

        public async Task<string> GetOwnerAsync(int id) 
        {
            var owner = await _database.GroceryListUsers.Where(u => u.Owner == true).FirstOrDefaultAsync(l => l.GroceryListId == id);

            return owner.ApplicationUser.Email;
        }

        public async Task<bool> UpdateStatusAsync(GroceryItem item)
        {
            var itemToUpdate = await GetItemAsync(item.Id);

            if (item != null) 
            {
                itemToUpdate.Shopped = item.Shopped;

                await _database.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
