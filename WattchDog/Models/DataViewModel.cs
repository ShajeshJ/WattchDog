using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WattchDB.Models;

namespace WattchDog.Models
{
    public class DataViewModel
    {
        public double Value { get; set; }

        public DateTime Time { get; set; }

        public static explicit operator DataViewModel(Data obj)
        {
            return new DataViewModel { Value = obj.Value, Time = obj.TimeRecorded };
        }
    }
}