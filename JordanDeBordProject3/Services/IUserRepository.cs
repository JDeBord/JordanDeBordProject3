using JordanDeBordProject3.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Services
{
    public interface IUserRepository
    {
        Task<ApplicationUser> ReadAsync(string userName);

        Task<ICollection<GroceryList>> ReadAllListsAsync(string userName);

        Task<bool> CheckPermissionAsync(string userName, GroceryList list);

        Task<string> GetNameAsync(string email);
    }
}
