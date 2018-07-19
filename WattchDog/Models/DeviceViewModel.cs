using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WattchDB.Models;

namespace WattchDog.Models
{
    public class DeviceViewModel
    {
        public string Name { get; set; }
        
        public string MacAddress { get; set; }

        public bool Status { get; set; }

        public static explicit operator DeviceViewModel(Device obj)
        {
            return new DeviceViewModel { MacAddress = obj.MacAddress, Name = obj.Name, Status = obj.Status };
        }
    }
}