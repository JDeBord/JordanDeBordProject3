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
    }
}
