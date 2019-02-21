using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WattchDB;
using WattchDB.Models;
using WattchDog.Attributes;
using WattchDog.Extensions;
using WattchDog.Models;
using WattchDog.Utilities;

namespace WattchDog.Controllers
{
    [RequireHttpsInProd]
    public class RegisterController : Controller
    {
        [HttpGet]
        [AnonymousOnly]
        public ActionResult Index()
        {
            ViewBag.Success = null;
            ViewBag.Error = null;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AnonymousOnly]
        public ActionResult Index(RegisterViewModel input)
        {
            if (!ModelState.IsValid)
            {
                input.Password = "";
                return View(input);
            }

            var newUser = new User()
            {
                Username = input.Username,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                Salt = CryptoUtility.GetRandomStr(32)
            };

            newUser.Password = (input.Password + newUser.Salt).ComputeSHA256();

            var repo = new TempRepo();
            var success = repo.InsertUser(newUser).Result;
            
            if (!success)
            {
                ViewBag.Success = null;
                ViewBag.Error = "Failed to create the account. Please try again.";
                input.Password = "";
                return View(input);
            }
            else
            {
                ViewBag.Success = "Successfully Registered!";
                ViewBag.Error = null;
                return View(new RegisterViewModel());
            }
        }
    }
}