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
    [Authorize]
    public class GroceryListController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IGroceryListRepository _groceryListRepository;

        public GroceryListController(IUserRepository userRepository, IGroceryListRepository groceryRepository)
        {
            _userRepository = userRepository;
            _groceryListRepository = groceryRepository;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

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

        [HttpPost]
        public async Task<IActionResult> UpdateAjax(EditListVM editListVM) 
        {
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

                if (list != null)
                {
                    await _groceryListRepository.UpdateAsync(list);

                    return Json(new { id = list.Id, message = "updated-list" });
                }

                return Json(new { id = list.Id, message = "invalid-list" });
            }
            return Json(ModelState);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItemAjax(CreateGroceryItemVM groceryItemVM) 
        {
            if (ModelState.IsValid)
            {
                var item = groceryItemVM.GetGroceryItemInstance();

                await _groceryListRepository.AddItemAsync(item.GroceryListId, item);

                return Json(new { id = item.Id, message = "added-item", listId=item.GroceryListId });
            }
            return Json(ModelState);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPermissionAjax(GrantPermissionsUserVM grantPermissionsUserVM) 
        {
            if (ModelState.IsValid)
            {
                var list = await _groceryListRepository.ReadAsync(grantPermissionsUserVM.ListId);
                var email = grantPermissionsUserVM.EmailAddress;

                var result = await _groceryListRepository.GrantPermissionAsync(list.Id, email);

                if (result != null)
                {
                    return Json(new { id = result, message = "granted-permission", listId=list.Id});
                }
                else 
                {
                    return Json(new { listId = list.Id, message = "invalid-permission" });
                }
            }
            return Json(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RevokeAccessAjax(int id)
        {
            var permission = await _groceryListRepository.GetPermissionAsync(id);
            if (permission != null)
            {
                var userId = permission.ApplicationUserId;
                var listId = permission.GroceryListId;

                var result = await _groceryListRepository.RemoveUserAsync(id);

                if (result)
                {
                    return Json(new { id = listId, message = "access-revoked", userId});
                }
                // If we can not revoke that user (owner)
                return Json(new { id, message = "revoke-declined" });
            }
            return Json(new { id, message="no-access" });
        }

        public async Task<IActionResult> ListRow(int id)
        {
            var user = await _userRepository.ReadAsync(User.Identity.Name);
            var list = await _groceryListRepository.ReadAsync(id);

            if (user != null && list != null)
            {
                var permission = await _userRepository.CheckPermissionAsync(user.UserName, list);

                // If the user has permission to view the list, return the partial view.
                if (permission)
                {
                    var listToShow = new IndexListVM
                    {
                        Id = list.Id,
                        Name = list.Name,
                        OwnerEmail = list.OwnerEmail,
                        NumberItems = list.NumberItems
                    };
                    return PartialView("Views/Home/_AddListRow.cshtml", listToShow);
                }
            }

            // Otherwise, return null.
            return Ok();
        }

        public async Task<IActionResult> PermissionRow(int id) 
        {
            var access = await _groceryListRepository.GetPermissionAsync(id);

            var list = access.GroceryList;

            var user = await _userRepository.ReadAsync(User.Identity.Name);

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

        public async Task<IActionResult> ListItemRow(int id)
        {
            var user = await _userRepository.ReadAsync(User.Identity.Name);
            var item = await _groceryListRepository.GetItemAsync(id);
            var list = await _groceryListRepository.ReadAsync(item.GroceryListId); 

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

            // Otherwise, return null.
            return Ok();
        }

        public async Task<IActionResult> GoShopping(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            var user = await _userRepository.ReadAsync(User.Identity.Name);

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

            // Set Title

            // Get list of all items in list, and build a VM then pass to view.

            return View();
        }
        
        public async Task<IActionResult> Edit(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            var user = await _userRepository.ReadAsync(User.Identity.Name);

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

            // Get list of all items from the list, build VM and pass.
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
                ListName = list.Name,
                GroceryItems = itemModel.ToList()
            };

            var ownerEmail = list.OwnerEmail;
            var owner = false;

            if (ownerEmail == user.Email)
            {
                owner = true;
            }
            // set title

            var ownerName = await _userRepository.GetNameAsync(ownerEmail);
            ViewData["Title"] = $"Editing {list.Name}";
            ViewData["OwnerName"] = ownerName;
            ViewData["Owner"] = owner;
            
            return View(model);
        }
       
        public async Task<IActionResult> Permissions(int id) 
        {
            var list = await _groceryListRepository.ReadAsync(id);
            var user = await _userRepository.ReadAsync(User.Identity.Name);

            if (list == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var owner = list.OwnerEmail;

            // If the user is not the Owner, forbid access
            if (owner != user.Email) 
            {
                return Forbid();
            }

            // Get the list of all additional people granted permission.
            var additionalUsers = await _groceryListRepository.GetAdditionalUsersAsync(id);

            var model = additionalUsers.Select(u =>
                new PermissionsUserVM
                {
                    Id = u.Id,
                    EmailAddress = u.ApplicationUser.Email
                });


            // set Title
            ViewData["Title"] = $"Editing Permissions for {list.Name}";
            ViewData["ListName"] = list.Name;
            ViewData["ListId"] = list.Id;

            return View(model);
        }
    }
}
