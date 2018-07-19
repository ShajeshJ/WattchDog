using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WattchDB;
using WattchDog.Models;

namespace WattchDog.Controllers
{
    public class DeviceController : Controller
    {
        // GET: Device
        public ActionResult Index()
        {
            ViewBag.Title = "WattchDog - Devices";

            var repo = new TempRepo();
            var devices = repo.GetAllDevices().Result.Select(d => (DeviceViewModel)d);

            return View(devices);
        }
        
        public ActionResult EditName(string macaddress, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["error"] = "New name cannot be empty or only contain whitespaces.";
            }
            else
            {
                var repo = new TempRepo();
                repo.UpdateDeviceName("mac_address", macaddress, name).Wait();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(string macaddress)
        {
            ViewBag.Title = "WattchDog - Device Data";

            var repo = new TempRepo();

            var deviceId = repo.GetDevice("mac_address", macaddress).Result.ID;

            var data = repo.GetData("RealPowers", deviceId, 10).Result.Select(d => (DataViewModel)d);

            return View(data);
        }
    }
}
