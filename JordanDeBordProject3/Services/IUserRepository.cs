using JordanDeBordProject3.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Services
{
    /// <summary>
    /// Interface for our User Repository. It contains the method headers needed for some general
    /// operations regarding users, which are to be implemented in the repository.
    /// </summary>
    public interface IUserRepository
    {
        Task<ApplicationUser> ReadAsync(string userName);

        Task<ICollection<GroceryList>> ReadAllListsAsync(string userName);

        Task<bool> CheckPermissionAsync(string userName, GroceryList list);

        Task<string> GetNameAsync(string email);

        Task<ICollection<GroceryListUser>> ReadListAccessAsync(string userName);
    }
}
