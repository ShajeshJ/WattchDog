using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WattchDB.Models;

namespace WattchDog.Models
{
    public class DeviceDataViewModel
    {
        public DeviceViewModel Device { get; set; }

        public DataType Type { get; set; }

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

    public enum DataType
    {
        RealPower = 1, 
        EnergyUsage = 2, 
        PowerFactor = 3, 
        Vrms = 4, 
        Irms = 5
    }
}