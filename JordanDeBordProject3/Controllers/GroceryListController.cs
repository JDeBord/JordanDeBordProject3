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
                Name = list.Name,
                GroceryItems = itemModel.ToList()
            };

            var ownerEmail = list.OwnerEmail;
            var owner = false;

            if (ownerEmail == user.Email)
            {
                owner = true;
            }
            // set title
            ViewData["Title"] = $"Editing {list.Name}";
            ViewData["OwnerName"] = $"{list.OwnerName}";
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

            // Get the list of all people granted permission.

            // New up a View Model and send to View.

            // set Title

            return View();
        }
    }
}
