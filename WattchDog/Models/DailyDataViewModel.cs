using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WattchDB.Models;
using WattchDog.Models.Enums;

namespace WattchDog.Models
{
    public class DailyDataViewModel
    {
        public DeviceViewModel Device { get; set; }

        public DeviceDataType Type { get; set; }

        public IEnumerable<DailyDatapointViewModel> Data { get; set; }
    }

    public class DailyDatapointViewModel
    {
        public double Value { get; set; }
        public long NumSamples { get; set; }
        public DateTime Date { get; set; }
    }
}