using JordanDeBordProject3.Models.ViewModels;
using JordanDeBordProject3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Controllers
{
    /// <summary>
    /// GroceryList Controller, which handles requests from clients and directs them to the appropriate action method.
    /// It then sends the response to the user. It handles requests to /grocerylist/{action} where action is the name of the 
    /// method below. Non-logged in users are sent to log in. 
    /// </summary>
    [Authorize]
    public class GroceryListController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IGroceryListRepository _groceryListRepository;

        /// <summary>
        /// Constructor for the GroceryList Controller, where we inject our needed repositories.
        /// </summary>
        /// <param name="userRepository">User repository for user related CRUD access to the database.</param>
        /// <param name="groceryRepository">Grocery list repository for grocery list CRUD access to the database.</param>
        public GroceryListController(IUserRepository userRepository, IGroceryListRepository groceryRepository)
        {
            _userRepository = userRepository;
            _groceryListRepository = groceryRepository;
        }

        /// <summary>
        /// Index action method to handle if a user sends a request to the Index for Grocery List Controller. Users shouldn't
        /// end up here, but if they do we redirect to home.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Create action method to be used with Ajax, to create a Grocery List and insert it into the database.
        /// </summary>
        /// <param name="groceryListVM">View Model for Create Grocery List containing information to create a list for.</param>
        /// <returns>Either returns JSON with the model state if the ModelState isn't valid
        ///     or the message "created" and the new Id for the list.</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(CreateGroceryListVM groceryListVM)
        {
            if (ModelState.IsValid)
            {
                var list = groceryListVM.GetGroceryListInstance();
                var user = User.Identity.Name;
                await _groceryListRepository.CreateAsync(user, list);

                return Json(new { id = list.Id, message = "created"});
            }
            return Json(ModelState);
        }

        /// <summary>
        /// DeleteAjax action method to remove the grocerylist from the database with Ajax.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>JSON containing information on if request was successful or not.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            
            if (list != null) 
            {

                var result = await _groceryListRepository.DeleteAsync(id, User.Identity.Name);
                // If we removed the grocery list, report it.
                if (result == id)
                {
                    return Json(new { id, message = "deleted" });
                }
                // If the user requesting this isn't the owner, report that they can not do that.
                else if (result == -1)
                {
                    return Json(new { id, message = "not-owner" });
                }
            }
            // If the list doesn't exist, or the user didn't exist (thus returning null for result), report that it was invalid.
            return Json(new { id, message = "invalid-request" });
        }

        /// <summary>
        /// UpdateAjax method, to update the name of the grocery list with Ajax.
        /// </summary>
        /// <param name="editListVM">View Model containing the new name of the grocery list.</param>
        /// <returns>JSON containing information on if request was successful or not.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateAjax(EditListVM editListVM) 
        {
            // If the model name is not valid, return JSON stating invalid name.
            if (editListVM.ListName == null)
            {
                ModelState.AddModelError("ListName", "The List must have a Name between 1 and 50 characters long.");
                return Json(new { message = "invalid-name" });
            }
            else if (editListVM.ListName.Length > 50)
            {
                ModelState.AddModelError("ListName", "The List must have a Name between 1 and 50 characters long.");
                return Json(new { message = "invalid-name" });
            }

            if (ModelState.IsValid) 
            {
                var list = editListVM.GetGroceryListInstance();

                // If the list exists, update the name, and send JSON with the list Id, updated list message, and the list Name.
                if (list != null)
                {
                    await _groceryListRepository.UpdateAsync(list);

                    return Json(new { id = list.Id, message = "updated-list", name=$"{list.Name}" });
                }

                // If the list doesn't exist, report that it is an invalid list.
                return Json(new { id = list.Id, message = "invalid-list" });
            }
            // Return the model state if it was not valid.
            return Json(ModelState);
        }

        /// <summary>
        /// AddItemAjax method to add an Item to a grocery list, and add it to the database. 
        /// </summary>
        /// <param name="groceryItemVM">View model containing information about the grocery item.</param>
        /// <returns>JSON containing information on if the request was valid or not.</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItemAjax(CreateGroceryItemVM groceryItemVM) 
        {
            // If the model state is valid, create the item, and report the item Id, added-item message, and the list Id
            //      which contains that item.
            if (ModelState.IsValid)
            {
                var item = groceryItemVM.GetGroceryItemInstance();

                await _groceryListRepository.AddItemAsync(item.GroceryListId, item);

                return Json(new { id = item.Id, message = "added-item", listId=item.GroceryListId });
            }
            // If the model state was not valid, return the model state.
            return Json(ModelState);
        }
        /// <summary>
        /// RemoveItemAjax, which uses Ajax to send a request to delete an item from the database.
        /// </summary>
        /// <param name="id">Id of the item to be removed.</param>
        /// <returns>JSON containing the result.</returns>
        [HttpPost]
        public async Task<IActionResult> RemoveItemAjax(int id) 
        {
            var item = await _groceryListRepository.GetItemAsync(id);
            if (item != null)
            {
                var listId = item.GroceryListId;

                var result = await _groceryListRepository.RemoveItemAsync(listId, item.Id);
                // If the item exists, and we were able to remove it, report such using JSON.
                if (result)
                {
                    return Json(new { id = id, message = "item-removed", listId = listId });
                }
            }
            // If the item doesn't exist, or if it does but we can't remove it, report that it wasn't valid.
            return Json(new { id, message = "not-valid" });
        }

        /// <summary>
        /// GrantPermissionAjax method, for owners of a list to grant permission for other users to access it.
        /// Returns JSON representing the result of the attempt to grant permission. This represents the create
        /// for our GroceryListUser table.
        /// </summary>
        /// <param name="grantPermissionsUserVM">View model containing information about the user and list to grant access to.</param>
        /// <returns>JSON containing result of our attempt to grant permission.</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPermissionAjax(GrantPermissionsUserVM grantPermissionsUserVM) 
        {
            if (ModelState.IsValid)
            {
                var list = await _groceryListRepository.ReadAsync(grantPermissionsUserVM.ListId);
                var email = grantPermissionsUserVM.EmailAddress;

                var result = await _groceryListRepository.GrantPermissionAsync(list.Id, email);

                // If the user and list both existed.
                if (result != null)
                {
                    // If the user already had access.
                    if (result == -1)
                    {
                        return Json(new { listId = list.Id, message = "previous-access" });
                    }
                    // If permission was granted.
                    return Json(new { id = result, message = "granted-permission", listId=list.Id});
                }

                else 
                {
                    return Json(new { listId = list.Id, message = "invalid-permission" });
                }
            }
            return Json(ModelState);
        }

        /// <summary>
        /// RevokeAccessAjax method, which uses Ajax to send a request to revoke a user's access to a list.
        /// This acts as the delete for our GroceryListUser table.
        /// </summary>
        /// <param name="id">Id of the GroceryListUser to remove.</param>
        /// <returns>JSON stating the result of our attempt to revoke access.</returns>
        [HttpPost]
        public async Task<IActionResult> RevokeAccessAjax(int id)
        {
            var permission = await _groceryListRepository.GetPermissionAsync(id);

            // If the user has permission. 
            if (permission != null)
            {

                var result = await _groceryListRepository.RemoveUserAsync(id);

                // If we revoked the user's access, return JSON stating such.
                if (result)
                {
                    return Json(new { id, message = "access-revoked"});
                }
                // If we can not revoke that user (if they are the owner).
                return Json(new { id, message = "revoke-declined" });
            }
            // If that access does not exist.
            return Json(new { id, message="no-access" });
        }

        /// <summary>
        /// CheckAjax, to update an item in our database to the "Checked" state (meaning that it was shopped). 
        /// This is used to send out notifications to clients to update the GoShopping page.
        /// </summary>
        /// <param name="itemVM">View model containing information for the item to be updated.</param>
        /// <returns>JSON containing result of the attempt to check off the item.</returns>
        [HttpPost]
        public async Task<IActionResult> CheckAjax(GoShoppingItemVM itemVM) 
        {
            var item = await _groceryListRepository.GetItemAsync(itemVM.Id);

            if (item != null)
            {
                var result = await _groceryListRepository.UpdateStatusAsync(itemVM.GetItemInstance());
                // If our attempt was a success, report such with JSON.
                if (result)
                {
                    return Json(new { id = item.Id, message = "checked"});
                }
            }
            // If the item doesn't exist, report that as well. 
            return Json(new { id = itemVM.Id, message = "no-item" });
        }

        /// <summary>
        /// UnCheckAjax method, to set an item in the database to unchecked (not shopped). This is the opposite
        /// of the above method. 
        /// </summary>
        /// <param name="itemVM">View model containin information about the item to be unchecked.</param>
        /// <returns>JSON containing the result of our attempt to uncheck the item.</returns>
        [HttpPost]
        public async Task<IActionResult> UncheckAjax(GoShoppingItemVM itemVM) 
        {
            var item = await _groceryListRepository.GetItemAsync(itemVM.Id);

            if (item != null)
            {
                var result = await _groceryListRepository.UpdateStatusAsync(itemVM.GetItemInstance());

                // If we were able to uncheck the item, send the message that we were.
                if (result)
                {
                    return Json(new { id = item.Id, message = "unchecked" });
                }
            }
            // If the item doesn't exist, report that as well.
            return Json(new { id = itemVM.Id, message = "no-item" });
        }

        /// <summary>
        /// ShopRow action method, which returns a partial view. This view will set the checkbox checked
        /// status, as well as strike through the item name if it has been checked. This is used to
        /// automatically update the GoShopping page when any user with access has checked or unchecked an item.
        /// </summary>
        /// <param name="id">Id of the item to get the new partial view for.</param>
        /// <returns>Partial view for the Item row that has been updated.</returns>
        public async Task<IActionResult> ShopRow(int id)
        {
            var item = await _groceryListRepository.GetItemAsync(id);
            if (item != null) 
            {
                var itemModel = new GoShoppingItemVM
                {
                    Id = item.Id,
                    Shopped = item.Shopped,
                    Name = item.Name,
                    QuantityToBuy = item.QuantityToBuy
                };
                return PartialView("Views/GroceryList/_UpdateShoppingItem.cshtml", itemModel);
            }

            // If the item no longer exists, return Ok. The item delete notification that is sent out after
            // deletion will notify all clients to remove this row. 
            return Ok();
        }

        /// <summary>
        /// Returns a partial view of the item on the grocery list, including its current shopped status.
        /// This is called when an item has been added to a Grocery List, to add onto the bottom of the Goshopping page.
        /// </summary>
        /// <param name="id">It of item to get the partial view for.</param>
        /// <param name="listId">List Id that the item belongs to.</param>
        /// <returns>Partial view containing the new goshopping item row.</returns>
        public async Task<IActionResult> AddShopRow(int id, int listId)
        {
            var item = await _groceryListRepository.GetItemAsync(id);
            var list = await _groceryListRepository.ReadAsync(listId);

            // If either do not exist, or if the list doesn't contain the item, return OK. No row will get appended.
            if (item != null && list != null)
            {
                if (!list.GroceryItems.Contains(item))
                {
                    return Ok();
                }
                // If the item exists and belongs to the list, get the partial view to append to the table. 
                var itemModel = new GoShoppingItemVM
                {
                    Id = item.Id,
                    Shopped = item.Shopped,
                    Name = item.Name,
                    QuantityToBuy = item.QuantityToBuy
                };
                return PartialView("Views/GroceryList/_AddShoppingItem.cshtml", itemModel);
            }

            return Ok();
        }

        /// <summary>
        /// ListRow returns a partial view of the grocery list to append to the Home Index if a user has access to the list.
        /// This is called after a list is created, or after access is granted.
        /// </summary>
        /// <param name="id">Id of the list that has been updated.</param>
        /// <returns>Partial view containing list information if the user has access.</returns>
        public async Task<IActionResult> ListRow(int id)
        {
            var user = await _userRepository.ReadAsync(User.Identity.Name);
            var list = await _groceryListRepository.ReadAsync(id);

            // If the user and the List both exist.
            if (user != null && list != null)
            {
                var permissionCheck = await _userRepository.CheckPermissionAsync(user.UserName, list);

                // If the user has access to the list, return the partial view.
                if (permissionCheck)
                {
                    var permission = await _groceryListRepository.GetPermissionAsync(list.Id, user.Id);
                    var listToShow = new IndexListVM
                    {
                        Id = list.Id,
                        GroceryListUserId = permission.Id,
                        Name = list.Name,
                        OwnerEmail = list.OwnerEmail,
                        NumberItems = list.NumberItems
                    };
                    return PartialView("Views/Home/_AddListRow.cshtml", listToShow);
                }
            }

            // If the list or user do not exist, or if the user does not have access, return null.
            return Ok();
        }

        /// <summary>
        /// PermissionRow method, which gets a partial view containing the new row for the Permission page
        /// after a user is granted access to a list.
        /// </summary>
        /// <param name="id">Id of the GroceryListUser (access permission) that has been created.</param>
        /// <returns>A partial view representing the access to the list the user has been granted.</returns>
        public async Task<IActionResult> PermissionRow(int id) 
        {
            var access = await _groceryListRepository.GetPermissionAsync(id);

            var list = access.GroceryList;

            var user = await _userRepository.ReadAsync(User.Identity.Name);

            // If the user is the owner, and the list and access exist.
            if (user == access.ApplicationUser && list != null && access != null) 
            {
                var permission = await _userRepository.CheckPermissionAsync(user.UserName, list);

                // If the user has permission to view the list, return the partial view.
                if (permission)
                {
                    var newPerm = new PermissionsUserVM
                    {
                        Id = access.Id,
                        EmailAddress = user.Email,
                        ListId = list.Id
                    };
                    return PartialView("Views/GroceryList/_AddPermissionRow.cshtml", newPerm);
                }
            }

            // Otherwise, return null.
            return Ok();
        }

        /// <summary>
        /// ListItemRow, which returns a partial view for the Edit page. This includes the row for a new item that has been added,
        /// if a user has access to the list.
        /// </summary>
        /// <param name="id">Id of the item for the list that the partial view is for.</param>
        /// <returns>A partial view containing the new row to add to the table.</returns>
        public async Task<IActionResult> ListItemRow(int id)
        {
            var user = await _userRepository.ReadAsync(User.Identity.Name);
            var item = await _groceryListRepository.GetItemAsync(id);
            var list = await _groceryListRepository.ReadAsync(item.GroceryListId); 

            // If the user, item, and list exist.
            if (user != null && list != null && item != null)
            {
                var permission = await _userRepository.CheckPermissionAsync(user.UserName, list);

                // If the user has permission to view the list, return the partial view.
                if (permission)
                {
                    var listToShow = new GroceryItemVM
                    {
                        Id = item.Id,
                        Name = item.Name,
                        QuantityToBuy = item.QuantityToBuy
                    };
                    return PartialView("Views/GroceryList/_AddItemListRow.cshtml", listToShow);
                }
            }

            // Return Ok, if the user doesn't have access, or the user, list, or item do not exist.
            return Ok();
        }

        /// <summary>
        /// Update List Row returns a partial view (if the user has access) containing the updated information
        /// for a row for the Home Index. This is called after a list name is changed, or the item count for a list changes.
        /// It only includes the information inside of the table row tag (not including the tag itself).
        /// </summary>
        /// <param name="id">Id of the list to return the new partial view for.</param>
        /// <returns>A partial view (if the user has access to the list) containing the new row information to overwrite
        ///     the current row with. </returns>
        public async Task<IActionResult> UpdateListRow(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            var user = await _userRepository.ReadAsync(User.Identity.Name);
            if (list != null && user != null)
            {
                var permissionCheck = await _userRepository.CheckPermissionAsync(user.UserName, list);

                // If the user has access to the list, return the partial view.
                if (permissionCheck)
                {
                    var permission = await _groceryListRepository.GetPermissionAsync(list.Id, user.Id);
                    var listToShow = new IndexListVM
                    {
                        Id = list.Id,
                        GroceryListUserId = permission.Id,
                        Name = list.Name,
                        OwnerEmail = list.OwnerEmail,
                        NumberItems = list.NumberItems
                    };
                    return PartialView("Views/Home/_ListRow.cshtml", listToShow);
                }
            }
            return Ok();
        }

        /// <summary>
        /// GoShopping action method, which returns a View which shows the Grocery List name, and a list
        /// of all items in the Grocery List. Each item has a checkbox, which can be checked or unchecked
        /// to represent an item's shopped status.
        /// </summary>
        /// <param name="id">Id of the Grocery List to display in the view.</param>
        /// <returns>A view containing the Grocery List shopping view.</returns>
        public async Task<IActionResult> GoShopping(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            var user = await _userRepository.ReadAsync(User.Identity.Name);

            // If the list doesn't exist, redirect to the Home Index.
            if (list == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var accessPerm = await _userRepository.CheckPermissionAsync(user.UserName, list);

            // If the User doesn't have access to the list, deny them.
            if (!accessPerm)
            {
                return Forbid();
            }
            var permission = await _groceryListRepository.GetPermissionAsync(list.Id, user.Id);

            // Get list of all items in list, and build a ViewModel then store in the other View Model to send to the View.
            var itemModel = list.GroceryItems.Select(i=>
                new GoShoppingItemVM 
                { 
                    Id = i.Id,
                    Shopped = i.Shopped,
                    Name = i.Name,
                    QuantityToBuy = i.QuantityToBuy
                }).ToList();

            var model = new GoShoppingListVM
            {
                Id = list.Id,
                Name = list.Name,
                GroceryItems = itemModel,
                GroceryListUserId = permission.Id
            };
            
            // Set the Title for the page.
            ViewData["Title"] = "Shopping";
            return View(model);
        }
        
        /// <summary>
        /// Edit action method, which returns a view containing a list of all items on the list. It also includes
        /// the ability for the owner of the list to update the name. Included also is a modal to add items to the list,
        /// and buttons to delete items.
        /// </summary>
        /// <param name="id">Id of the Grocery List to get the View of.</param>
        /// <returns>A view containing the items of the Grocery list and the abilit to add and delete items.</returns>
        public async Task<IActionResult> Edit(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            var user = await _userRepository.ReadAsync(User.Identity.Name);

            // If the list does not exist, redirect to the Home Index.
            if (list == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var accessPerm = await _userRepository.CheckPermissionAsync(user.UserName, list);

            // If the User doesn't have access to the list, deny them.
            if (!accessPerm)
            {
                return Forbid();
            }

            var permission = await _groceryListRepository.GetPermissionAsync(list.Id, user.Id);
            // Get list of all items from the list, build a View Model for each and store it in our other View Model.
            var items = list.GroceryItems.ToList();
            var itemModel = items.Select(item =>
                new GroceryItemVM
                {
                    Id = item.Id,
                    Name = item.Name,
                    QuantityToBuy = item.QuantityToBuy
                });

            var model = new EditListVM
            {
                Id = list.Id,
                GroceryListUserId = permission.Id,
                ListName = list.Name,
                GroceryItems = itemModel.ToList()
            };

            // Find out if the accessing user is the owner or not.
            var ownerEmail = list.OwnerEmail;
            var owner = false;

            if (ownerEmail == user.Email)
            {
                owner = true;
            }
            
            // Set the title, and store the name of the owner and if the accessing user is the owner in the ViewData.
            var ownerName = await _userRepository.GetNameAsync(ownerEmail);
            ViewData["Title"] = $"Edit Grocery List";
            ViewData["OwnerName"] = ownerName;
            ViewData["Owner"] = owner;
            
            return View(model);
        }
       
        /// <summary>
        /// Permissions action method, which returns a view containing information about additional users who have
        /// access to a list. Only the owner of the list may access this. It also provides a modal to grant a user access,
        /// and the ability to revoke access.
        /// </summary>
        /// <param name="id">Id of the grocery list to see the view for.</param>
        /// <returns>A View containing information about users who have access to a list.</returns>
        public async Task<IActionResult> Permissions(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            var user = await _userRepository.ReadAsync(User.Identity.Name);

            // If the list doesn't exist, redirect to the Home Index.
            if (list == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var owner = list.OwnerEmail;

            // If the user is not the Owner, forbid access.
            if (owner != user.Email) 
            {
                return Forbid();
            }

            // Get the list of all additional people granted permission to the list.
            var additionalUsers = await _groceryListRepository.GetAdditionalUsersAsync(id);

            var model = additionalUsers.Select(u =>
                new PermissionsUserVM
                {
                    Id = u.Id,
                    EmailAddress = u.ApplicationUser.Email
                });


            // Set Title and store the list name and Id in the ViewData.
            ViewData["Title"] = $"Grocery List Permissions";
            ViewData["ListName"] = list.Name;
            ViewData["ListId"] = list.Id;

            return View(model);
        }
    }
}
