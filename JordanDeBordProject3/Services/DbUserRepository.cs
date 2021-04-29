using JordanDeBordProject3.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Services
{
    /// <summary>
    /// Db User Repository, which adds abstraction to our application. This class is the instantiation of our
    /// interface. We provide methods related to users in this repository. This uses the ApplicationDbContext and
    /// DbSets to access the database.
    /// </summary>
    public class DbUserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _database;

        /// <summary>
        /// Constructor for the repository, in which we inject our ApplicationDbContext.
        /// </summary>
        /// <param name="database">ApplicationDbContext with which we interact with the databse.</param>
        public DbUserRepository(ApplicationDbContext database)
        {
            _database = database;
        }

        /// <summary>
        /// Check if a user has access to a grocery list. If they do we return true, if not we return false. 
        /// </summary>
        /// <param name="userName">User Name of the user to check permission for.</param>
        /// <param name="list">Grocery List to check access to.</param>
        /// <returns>A bool representing whether or not the user has access.</returns>
        public async Task<bool> CheckPermissionAsync(string userName, GroceryList list)
        {
            var userLists = await ReadAllListsAsync(userName);

            if (userLists.Contains(list))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method to get all the lists in the database that a user has access to. This allows us
        /// to return a list of just the grocery lists, instead of a list of the grocery list users.
        /// </summary>
        /// <param name="userName">UserName of user to get lists for.</param>
        /// <returns>A collection of grocery lists the user has access to.</returns>
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

        /// <summary>
        /// Method to return all grocery list users (which represents the user access) that a user has.
        /// This makes sure to include the grocery lists as well. 
        /// </summary>
        /// <param name="userName">Username of person to get GroceryListUsers for.</param>
        /// <returns>List of Grocery List Users (representing their access to the list).</returns>
        public async Task<ICollection<GroceryListUser>> ReadListAccessAsync(string userName) 
        {
            var user = await ReadAsync(userName);

            var listAccess = await _database.GroceryListUsers
                                    .Include(u => u.ApplicationUser)
                                    .Include(l => l.GroceryList)
                                    .Where(us => us.ApplicationUser.UserName == userName).ToListAsync();

            return listAccess;
        }

        /// <summary>
        /// Read the user from the database with this user name. Also includes the GroceryListUsers,
        /// GroceryLists, and GroceryItems associated with that user. 
        /// </summary>
        /// <param name="userName">Username of user to read from the database.</param>
        /// <returns>Application User with that username.</returns>
        public async Task<ApplicationUser> ReadAsync(string userName)
        {
            var user = await _database.Users.Include(gl => gl.GroceryListUsers)
                                .ThenInclude(l => l.GroceryList)
                                .ThenInclude(i => i.GroceryItems)
                                .FirstOrDefaultAsync(u => u.UserName == userName);

            return user;
        }

        /// <summary>
        /// Method to return the name of the user with the provided email.
        /// </summary>
        /// <param name="email">Email of the user to get the name for.</param>
        /// <returns>First and Last name of the user.</returns>
        public async Task<string> GetNameAsync(string email)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.Email == email);
            var name = $"{user.FirstName} {user.LastName}";

            return name;
        }
    }
}
