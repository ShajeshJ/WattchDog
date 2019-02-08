using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WattchDB.Models;
using WattchDog.Models.Enums;

namespace WattchDog.Models
{
    public class RealtimeDataViewModel
    {
        public DeviceViewModel Device { get; set; }

        public DeviceDataType Type { get; set; }

        public IEnumerable<DataViewModel> Data { get; set; }

        public TotalEnergyViewModel Energy { get; set; }
    }

    public class TotalEnergyViewModel
    {
        public double Value { get; set; }

        public static explicit operator TotalEnergyViewModel(TotalEnergy obj)
        {
            return new TotalEnergyViewModel { Value = obj.Value };
        }
    }

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