﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using WattchDB;
using WattchDB.Models;
using WattchDog.Attributes;
using WattchDog.Models;
using WattchDog.Models.Enums;

namespace WattchDog.Controllers
{
    [UserOnly]
    public class DeviceController : Controller
    {
        [HttpGet]
        public ActionResult Index(string search = null)
        {
            ViewBag.Title = "WattchDog - Devices";
            search = string.IsNullOrWhiteSpace(search) ? null : search;

            var repo = new TempRepo();
            var devices = repo.GetAllDevices(d => d.UserId, Session["UID"], search).Result.Select(d => (DeviceViewModel)d);

            return View(devices);
        }
        
        [HttpPost]
        public ActionResult EditName(string macaddress, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["error"] = "New name cannot be empty or only contain whitespaces.";
            }
            else
            {
                var repo = new TempRepo();
                var device = repo.GetDevice(d => d.MacAddress, macaddress).Result;

                if (device.UserId == Session["UID"] as int?)
                {
                    repo.UpdateDeviceName("mac_address", macaddress, name).Wait();
                }
            }

            return RedirectToAction("Index");
        }
        
        public ActionResult EditStatus(string macaddress, int status, string start = null, string end = null)
        {
            DeviceSchedule schedule = null;
            if (status == 2)
            {
                var startSuccess = DateTime.TryParseExact(start, "H:mm:ss", null, System.Globalization.DateTimeStyles.None, out var startTime);
                var endSuccess = DateTime.TryParseExact(end, "H:mm:ss", null, System.Globalization.DateTimeStyles.None, out var endTime);

                if (!startSuccess || !endSuccess)
                {
                    TempData["error"] = "Invalid start or end time format (must have format H:mm:ss).";
                    return RedirectToAction("Index");
                }
                else if ((endTime - startTime).TotalMinutes % TimeSpan.FromHours(24).TotalMinutes < 1)
                {
                    TempData["error"] = "The range from start time to end time must be greater than 1 minute.";
                    return RedirectToAction("Index");
                }
                else
                {
                    schedule = new DeviceSchedule()
                    {
                        StartTime = startTime.ToString("HH:mm:ss"),
                        EndTime = endTime.ToString("HH:mm:ss")
                    };
                }
            }

            var repo = new TempRepo();
            var device = repo.GetDevice(d => d.MacAddress, macaddress).Result;

            if (device.UserId == Session["UID"] as int?)
            {
                repo.UpdateDeviceStatus("mac_address", macaddress, status, schedule).Wait();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddDevice(string macaddress)
        {
            var repo = new TempRepo();
            var device = repo.GetDevice(d => d.MacAddress, macaddress).Result;

            if (device == null)
            {
                TempData["error"] = "Invalid MAC address";
            }
            else if (device.UserId != null)
            {
                TempData["error"] = "Device has already been registered to another account";
            }
            else
            {
                repo.UpdateDeviceOwner("mac_address", macaddress, (int)Session["UID"]).Wait();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Realtime(string macaddress, DeviceDataType type = DeviceDataType.EnergyUsage)
        {
            ViewBag.Title = "WattchDog - Realtime Device Data";
            TempData["data_type"] = type;

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

            if (device.UserId != Session["UID"] as int?)
            {
                return RedirectToAction("Index");
            }

            deviceData.Device = (DeviceViewModel)device;

            var data = repo.GetData(table, device.ID, 10).Result.Select(d => (DataViewModel)d);
            data = data.Reverse();
            deviceData.Type = type;
            deviceData.Data = data;

            return View(deviceData);
        }

        [HttpGet]
        public ActionResult Hourly(string macaddress, DeviceDataType type = DeviceDataType.EnergyUsage)
        {
            ViewBag.Title = "WattchDog - Hourly Device Data";
            TempData["data_type"] = type;

            string table = "";
            switch (type)
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
            var aggregatetdData = new HourlyDataViewModel();
            var device = repo.GetDevice("mac_address", macaddress).Result;

            if (device.UserId != Session["UID"] as int?)
            {
                return RedirectToAction("Index");
            }

            aggregatetdData.Device = (DeviceViewModel)device;

            var queryTime = TimeZoneInfo.ConvertTime(DateTime.Now, 
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            List<HourlyDatapointViewModel> outputData;
            var curTime = new DateTime(queryTime.Year, queryTime.Month, queryTime.Day, queryTime.Hour, 0, 0);
            var refTime = new DateTime(queryTime.Year, queryTime.Month, queryTime.Day, queryTime.Hour, 0, 0).AddDays(-1);
            
            var cache = MemoryCache.Default;
            var key = $"{macaddress.ToUpper()}-{type.ToString()}-{curTime.ToString()}-hourly";

            if (cache.Contains(key))
            {
                outputData = (List<HourlyDatapointViewModel>)cache[key];
            }
            else
            {
                outputData = new List<HourlyDatapointViewModel>();
                var data = repo.GetAggregatedData(table, device.ID, queryTime, DateGrouping.Hourly).Result.Reverse().ToList();

                while (refTime < curTime)
                {
                    if (data.ElementAtOrDefault(0)?.GroupedDate == refTime)
                    {
                        outputData.Add(new HourlyDatapointViewModel()
                        {
                            Hour = data[0].GroupedDate,
                            NumSamples = data[0].NumSamples,
                            Value = type == DeviceDataType.EnergyUsage ? Math.Round(data[0].AvgValue, 7) : Math.Round(data[0].AvgValue, 2)
                        });
                        data.RemoveAt(0);
                    }
                    else
                    {
                        outputData.Add(new HourlyDatapointViewModel()
                        {
                            Hour = refTime,
                            NumSamples = 0,
                            Value = 0
                        });
                    }

                    refTime = refTime.AddHours(1);
                }

                cache.Add(key, outputData, DateTimeOffset.Now.AddMinutes(5));
            }

            aggregatetdData.Data = outputData;
            aggregatetdData.Type = type;

            return View(aggregatetdData);
        }

        [HttpGet]
        public ActionResult Daily(string macaddress, DeviceDataType type = DeviceDataType.EnergyUsage)
        {
            ViewBag.Title = "WattchDog - Daily Device Data";
            TempData["data_type"] = type;

            string table = "";
            switch (type)
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
            var aggregatetdData = new DailyDataViewModel();
            var device = repo.GetDevice("mac_address", macaddress).Result;

            if (device.UserId != Session["UID"] as int?)
            {
                return RedirectToAction("Index");
            }

            aggregatetdData.Device = (DeviceViewModel)device;

            var queryTime = TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            List<DailyDatapointViewModel> outputData;
            var curTime = new DateTime(queryTime.Year, queryTime.Month, queryTime.Day);
            var refTime = new DateTime(queryTime.Year, queryTime.Month, queryTime.Day).AddMonths(-1);

            var cache = MemoryCache.Default;
            var key = $"{macaddress.ToUpper()}-{type.ToString()}-{curTime.ToString()}-daily";

            if (cache.Contains(key))
            {
                outputData = (List<DailyDatapointViewModel>)cache[key];
            }
            else
            {
                outputData = new List<DailyDatapointViewModel>();
                var data = repo.GetAggregatedData(table, device.ID, queryTime, DateGrouping.Daily).Result.Reverse().ToList();

                while (refTime < curTime)
                {
                    if (data.ElementAtOrDefault(0)?.GroupedDate == refTime)
                    {
                        outputData.Add(new DailyDatapointViewModel()
                        {
                            Date = data[0].GroupedDate,
                            NumSamples = data[0].NumSamples,
                            Value = type == DeviceDataType.EnergyUsage ? Math.Round(data[0].AvgValue, 7) : Math.Round(data[0].AvgValue, 2)
                        });
                        data.RemoveAt(0);
                    }
                    else
                    {
                        outputData.Add(new DailyDatapointViewModel()
                        {
                            Date = refTime,
                            NumSamples = 0,
                            Value = 0
                        });
                    }

                    refTime = refTime.AddDays(1);
                }

                cache.Add(key, outputData, DateTimeOffset.Now.AddMinutes(5));
            }

            aggregatetdData.Data = outputData;
            aggregatetdData.Type = type;

            return View(aggregatetdData);
        }

        [HttpGet]
        public ActionResult Monthly(string macaddress, DeviceDataType type = DeviceDataType.EnergyUsage)
        {
            ViewBag.Title = "WattchDog - Monthly Device Data";
            TempData["data_type"] = type;

            string table = "";
            switch (type)
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
            var aggregatetdData = new MonthlyDataViewModel();
            var device = repo.GetDevice("mac_address", macaddress).Result;

            if (device.UserId != Session["UID"] as int?)
            {
                return RedirectToAction("Index");
            }

            aggregatetdData.Device = (DeviceViewModel)device;

            var queryTime = TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            List<MonthlyDatapointViewModel> outputData;
            var curTime = new DateTime(queryTime.Year, queryTime.Month, 1);
            var refTime = new DateTime(queryTime.Year, queryTime.Month, 1).AddYears(-1);

            var cache = MemoryCache.Default;
            var key = $"{macaddress.ToUpper()}-{type.ToString()}-{curTime.ToString()}-monthly";

            if (cache.Contains(key))
            {
                outputData = (List<MonthlyDatapointViewModel>)cache[key];
            }
            else
            {
                outputData = new List<MonthlyDatapointViewModel>();
                var data = repo.GetAggregatedData(table, device.ID, queryTime, DateGrouping.Monthly).Result.Reverse().ToList();

                while (refTime < curTime)
                {
                    if (data.ElementAtOrDefault(0)?.GroupedDate == refTime)
                    {
                        outputData.Add(new MonthlyDatapointViewModel()
                        {
                            Month = data[0].GroupedDate,
                            NumSamples = data[0].NumSamples,
                            Value = type == DeviceDataType.EnergyUsage ? Math.Round(data[0].AvgValue, 7) : Math.Round(data[0].AvgValue, 2)
                        });
                        data.RemoveAt(0);
                    }
                    else
                    {
                        outputData.Add(new MonthlyDatapointViewModel()
                        {
                            Month = refTime,
                            NumSamples = 0,
                            Value = 0
                        });
                    }

                    refTime = refTime.AddMonths(1);
                }

                cache.Add(key, outputData, DateTimeOffset.Now.AddMinutes(5));
            }

            aggregatetdData.Data = outputData;
            aggregatetdData.Type = type;

            return View(aggregatetdData);
        }
    }
}
