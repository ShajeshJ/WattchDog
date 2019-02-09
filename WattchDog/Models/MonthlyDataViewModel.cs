using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WattchDB.Models;
using WattchDog.Models.Enums;

namespace WattchDog.Models
{
    public class MonthlyDataViewModel
    {
        public DeviceViewModel Device { get; set; }

        public DeviceDataType Type { get; set; }

        public IEnumerable<MonthlyDatapointViewModel> Data { get; set; }
    }

    public class MonthlyDatapointViewModel
    {
        public double Value { get; set; }
        public long NumSamples { get; set; }
        public DateTime Month { get; set; }
    }
}