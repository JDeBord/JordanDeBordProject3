﻿using JordanDeBordProject3.Models.Entities;
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

        Task<int?> DeleteAsync(int id, string userId);

        Task<int?> GrantPermissionAsync(int listId, string userName);

        Task<bool> RemoveUserAsync(int accessId);

        Task<GroceryItem> AddItemAsync(int groceryListId, GroceryItem item);

        Task<bool> RemoveItemAsync(int groceryListId, int groceryItemId);

        Task<GroceryItem> GetItemAsync(int itemId);

        Task<ICollection<GroceryListUser>> GetAdditionalUsersAsync(int id);

        Task<GroceryListUser> GetPermissionAsync(int id);

        Task<GroceryListUser> GetPermissionAsync(int listId, string userId);

        Task<string> GetOwnerAsync(int id);

        Task<bool> UpdateStatusAsync(GroceryItem item);
    }
}
