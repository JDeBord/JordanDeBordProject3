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
    /// <summary>
    /// Home Controller which handles requests by clients, and directs them to the appropriate action method. It then
    /// sends the resposne to the user. It handles requests from clients to /home/{action} where action is the name of
    /// a method below. Non-logged in users are sent to log in. 
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IGroceryListRepository _groceryListRepository;

        /// <summary>
        /// Constructor for the Home Controller, where we inject our needed repositories and configuration.
        /// </summary>
        /// <param name="logger">Default ILogger provided.</param>
        /// <param name="userRepository">User repository for user related CRUD access to the database.</param>
        /// <param name="groceryListRepository">Grocery List Repository for grocery list CRUD access to the database.</param>
        public HomeController(ILogger<HomeController> logger, IUserRepository userRepository, IGroceryListRepository groceryListRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _groceryListRepository = groceryListRepository;
        }

        /// <summary>
        /// Index action method, which returns a view containing all the lists the user has access to. It also
        /// includes a modal to create a list. In includes links to delete lists, change user permissions, 
        /// to edit the list, and to go shopping on the list.
        /// </summary>
        /// <returns>A View containing information regarding the lists the logged in user has access to.</returns>
        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name;

            // Get all the lists the user has access to.
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

        /// <summary>
        /// Default Error IActionResult method. 
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
