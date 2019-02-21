using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WattchDB;
using WattchDog.Attributes;
using WattchDog.Extensions;
using WattchDog.Models;
using WattchDog.Utilities;

namespace WattchDog.Controllers
{
    [RequireHttpsInProd]
    public class LoginController : Controller
    {
        [HttpGet]
        [AnonymousOnly]
        public ActionResult Index()
        {
            ViewBag.Error = null;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AnonymousOnly]
        public ActionResult Index(LoginViewModel input)
        {
            if (!ModelState.IsValid)
            {
                input.Password = "";
                return View(input);
            }

            var repo = new TempRepo();
            var user = repo.GetUser(u => u.Username, input.Username).Result;

            var salt = user != null ? user.Salt : "";
            var checkPW = (input.Password + salt).ComputeSHA256();

            if (user?.Password != checkPW)
            {
                ViewBag.Error = "Invalid username and password combination.";
                input.Password = "";
                return View(input);
            }

            Session["UID"] = user.ID;
            Session["Email"] = user.Email;
            Session["FirstName"] = user.FirstName;
            Session["LastName"] = user.LastName;
            Session["Username"] = user.Username;

            return RedirectToAction("Index", "Device");
        }

        [HttpPost]
        [Route(Name = "logout")]
        [UserOnly]
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index");
        }
    }
}