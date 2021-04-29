using JordanDeBordProject3.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Services
{
    /// <summary>
    /// Db Grocery List Repository, which adds abstraction to our application. The class implements the
    /// IGroceryListRepository, and allows us to access the database relating to needs regarding Grocery Lists.
    /// It uses the Application Db Context and Db Sets to access the database.
    /// </summary>
    public class DbGroceryListRepository : IGroceryListRepository
    {
        private readonly ApplicationDbContext _database;

        /// <summary>
        /// Constructor for the repository, in which we inject our ApplicationDbContext.
        /// </summary>
        /// <param name="database">ApplicationDbContext with which we interact with the databse.</param>
        public DbGroceryListRepository(ApplicationDbContext database)
        {
            _database = database;
        }

        /// <summary>
        /// Method to add an item to a grocery list. This creates the item and associates it with the list.
        /// </summary>
        /// <param name="groceryListId">Id of the grocery list to add the item to.</param>
        /// <param name="item">Item to be added to the database and list.</param>
        /// <returns>The grocery item that was added to the Database.</returns>
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

        /// <summary>
        /// Method to grant a user access to a list. This serves as the Create for our GroceryListUsers.
        /// </summary>
        /// <param name="listId">Id of the grocery list to grant the user access to.</param>
        /// <param name="email">Email address of the user to be granted access.</param>
        /// <returns>An int representing the Id for the new GroceryListUser, -1 if the user
        ///     already has access, or null if the user or list don't exist.</returns>
        public async Task<int?> GrantPermissionAsync(int listId, string email)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.Email == email);

            var list = await ReadAsync(listId);

            if (user != null && list != null)
            {
                // Check if the user already has access. If they do, don't create it again. 
                // As this deals with granting access, this person is not the owner, so we make sure it is set to false.
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

                return -1;
            }
            return null;
        }

        /// <summary>
        /// Method to create our grocery list in the database. We also set the owner's association with this list.
        /// </summary>
        /// <param name="userName">User Name of the person creating the list.</param>
        /// <param name="groceryList">Grocery List to be added to the database. </param>
        /// <returns>Grocery List that was added to the database.</returns>
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

        /// <summary>
        /// Method to delete a grocery list from the database, if the requesting user is the owner.
        /// </summary>
        /// <param name="id">Id of the grocery list to be deleted.</param>
        /// <param name="userName">UserName of the person requesting the deletion.</param>
        /// <returns>An int representing the result. The list Id if success, -1 if
        ///     the requesting user is not the owner, or null if the list or user doesn't exist.</returns>
        public async Task<int?> DeleteAsync(int id, string userName)
        {
            var list = await ReadAsync(id);

            var user = await _database.Users.FirstOrDefaultAsync(u => u.UserName == userName);


            if (list != null && user != null)
            {
                var listOwner = await GetOwnerAsync(id);

                if (listOwner == user.Email)
                {
                    _database.Remove(list);
                    await _database.SaveChangesAsync();

                    return id;
                }

                return -1;
            }
            return null;
        }

        /// <summary>
        /// Read the Grocery List from the database with the provided Id.
        /// </summary>
        /// <param name="id">Id of the Grocery List to return.</param>
        /// <returns>GroceryList with that Id.</returns>
        public async Task<GroceryList> ReadAsync(int id)
        {
            var list = await _database.GroceryLists
                .Include(i => i.GroceryItems)
                .FirstOrDefaultAsync(l => l.Id == id);

            return list;
        }

        /// <summary>
        /// Method to delete an item from the database.
        /// </summary>
        /// <param name="groceryListId">Id of the grocery list containing the item.</param>
        /// <param name="groceryItemId">Id of the grocery item to remove.</param>
        /// <returns>Boolean representing whether the removal was successful or not.</returns>
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

        /// <summary>
        /// Method to remove access to a GroceryList, deleting the associated GroceryListUser.
        /// </summary>
        /// <param name="userAccessId">Id representing the association entity in the database to be deleted.</param>
        /// <returns>Boolean representing if removal was successful.</returns>
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

        /// <summary>
        /// Method to update the name of a GroceryList in the database.
        /// </summary>
        /// <param name="groceryList">GroceryList containing the Id to change and the new Name.</param>
        public async Task UpdateAsync(GroceryList groceryList)
        {
            var listToUpdate = await ReadAsync(groceryList.Id);

            if (listToUpdate != null)
            {
                listToUpdate.Name = groceryList.Name;

                await _database.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Method to return a GroceryItem from the database with the provided Id.
        /// </summary>
        /// <param name="itemId">Id of the GroceryItem to return.</param>
        /// <returns>Grocery Item with that Id.</returns>
        public async Task<GroceryItem> GetItemAsync(int itemId) 
        {
            var item = await _database.GroceryItems.FirstOrDefaultAsync(i => i.Id == itemId);

            return item;
        }

        /// <summary>
        /// Method to return all additional (non-owner) users for a GroceryList.
        /// </summary>
        /// <param name="id">Id of the list to get users with access for.</param>
        /// <returns>Collection of GroceryListUsers who have access and are not the owner.</returns>
        public async Task<ICollection<GroceryListUser>> GetAdditionalUsersAsync(int id) 
        {

            var userAccess = await _database.GroceryListUsers.Include(l => l.GroceryList).Include(u => u.ApplicationUser).Where(li => li.GroceryListId == id).ToListAsync();
            var additionalUsers = new List<GroceryListUser>();

            foreach (var u in userAccess)
            {
                // If the Access is not for the owner, add it to the list.
                if (u.Owner == false)
                {
                    additionalUsers.Add(u);
                }
            }
            return additionalUsers;
        }

        /// <summary>
        /// Gets the GroceryListUser with the provided Id from the database and returns it.
        /// </summary>
        /// <param name="id">Id of the GroceryListUser (representing user access) to return.</param>
        /// <returns>GroceryListUser with that Id.</returns>
        public async Task<GroceryListUser> GetPermissionAsync(int id) 
        {
            var access = await _database.GroceryListUsers
                .Include(u => u.ApplicationUser)
                .Include(li => li.GroceryList)
                .FirstOrDefaultAsync(l => l.Id == id);

            return access;
        }

        /// <summary>
        /// Does the same as the above method, but instead of getting the GroceryListUser via its Id,
        /// we instead get it based on the Id of the list and Id of the user. 
        /// </summary>
        /// <param name="listId">Id of the List to get the GroceryListUser for.</param>
        /// <param name="userId">Id of the user to get the GroceryListUser for.</param>
        /// <returns>GroceryListUser representing the association between the user and list.</returns>
        public async Task<GroceryListUser> GetPermissionAsync(int listId, string userId) 
        { 
            var permission = await _database.GroceryListUsers
                .Include(u => u.ApplicationUser)
                .Include(li => li.GroceryList)
                .FirstOrDefaultAsync(l => l.ApplicationUserId == userId && l.GroceryListId == listId);

            return permission;
        }

        /// <summary>
        /// Method to get the owner's email for a grocery list with the provided Id.
        /// </summary>
        /// <param name="id">Id of the grocery list to return the owner for.</param>
        /// <returns>A string containing the email for the owner of the grocery list.</returns>
        public async Task<string> GetOwnerAsync(int id) 
        {
            var owner = await _database.GroceryListUsers.Include(a => a.ApplicationUser).Where(u => u.Owner == true).FirstOrDefaultAsync(l => l.GroceryListId == id);

            return owner.ApplicationUser.Email;
        }

        /// <summary>
        /// Updates the database to set the current status for a Grocery Item.
        /// </summary>
        /// <param name="item">Item containing the Id of the Item to be updated, and the new shopped status.</param>
        /// <returns>Boolean representing if the item exists, and if so that it was updated.</returns>
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
