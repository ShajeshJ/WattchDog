using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WattchDB;
using WattchDog.Models;
using WattchDog.Models.Enums;

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

        public ActionResult EditStatus(string macaddress, bool status)
        {
            var repo = new TempRepo();
            repo.UpdateDeviceStatus("mac_address", macaddress, status).Wait();

            return RedirectToAction("Index");
        }

        public ActionResult Realtime(string macaddress, DeviceDataType type = DeviceDataType.RealPower)
        {
            ViewBag.Title = "WattchDog - Device Data";

            string table = "";
            switch(type)
            {
                case DeviceDataType.EnergyUsage:
                    table = "EnergyUsages";
                    break;
                case DeviceDataType.Irms:
                    table = "RmsCurrents";
                    break;
                case DeviceDataType.PowerFactor:
                    table = "PowerFactors";
                    break;
                case DeviceDataType.RealPower:
                    table = "RealPowers";
                    break;
                default:
                    table = "RmsVoltages";
                    break;
            }

            var repo = new TempRepo();

            var deviceData = new RealtimeDataViewModel();

            var device = repo.GetDevice("mac_address", macaddress).Result;
            deviceData.Device = (DeviceViewModel)device;

            if (type == DeviceDataType.EnergyUsage)
            {
                deviceData.Energy = (TotalEnergyViewModel)repo.GetTotalEnergy(device.ID).Result;
            }

            var data = repo.GetData(table, device.ID, 10).Result.Select(d => (DataViewModel)d);
            data = data.Reverse();
            deviceData.Type = type;
            deviceData.Data = data;

            return View(deviceData);
        }
    }
}
