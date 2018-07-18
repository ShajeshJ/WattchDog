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
            var devices = repo.GetAllDevices().Result.Select(x => (DeviceViewModel)x);

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

        //// GET: Device/Details/5
        //public ActionResult Details(string macaddress)
        //{
        //    return View();
        //}

        //// POST: Device/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
