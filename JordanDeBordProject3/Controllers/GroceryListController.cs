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
    }
}
