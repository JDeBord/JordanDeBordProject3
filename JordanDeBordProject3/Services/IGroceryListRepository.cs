using JordanDeBordProject3.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Services
{
    public interface IGroceryListRepository
    {
        Task<GroceryList> ReadAsync(int id);

        Task<GroceryList> CreateAsync(string userName, GroceryList groceryList);

        Task UpdateAsync(GroceryList groceryList);

        Task DeleteAsync(int id);

        Task AddUserAsync(int id, string userId);

        Task RemoveUserAsync(int id, string userId);

        Task AddItemAsync(int groceryListId, GroceryItem item);

        Task RemoveItemAsync(int groceryListId, int groceryItemId);
    }
}
