using JordanDeBordProject3.Models;
using JordanDeBordProject3.Models.ViewModels;
using JordanDeBordProject3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IGroceryListRepository _groceryListRepository;

        public HomeController(ILogger<HomeController> logger, IUserRepository userRepository, IGroceryListRepository groceryListRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _groceryListRepository = groceryListRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name;

            //var groceryLists = await _userRepository.ReadAllListsAsync(userName);
            var listAccess = await _userRepository.ReadListAccessAsync(userName);

            ViewData["Title"] = "Grocery List Home Page";
            var model = listAccess.Select(list =>
                new IndexListVM
                {
                    Id = list.GroceryListId,
                    GroceryListUserId = list.Id,
                    Name = list.GroceryList.Name,
                    OwnerEmail = list.GroceryList.OwnerEmail,
                    NumberItems = list.GroceryList.NumberItems
                });
           
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
