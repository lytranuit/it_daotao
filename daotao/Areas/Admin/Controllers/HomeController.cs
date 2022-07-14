
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using it.Areas.Admin.Models;
using it.Data;
using Microsoft.AspNetCore.Identity;

namespace it.Areas.Admin.Controllers
{

    public class HomeController : BaseController
    {
        private UserManager<UserModel> UserManager;


        public HomeController(ItContext context, UserManager<UserModel> UserMgr) : base(context)
        {
            UserManager = UserMgr;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }


    }
}
