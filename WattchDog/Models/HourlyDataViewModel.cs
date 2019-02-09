using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WattchDB.Models;
using WattchDog.Models.Enums;

namespace WattchDog.Models
{
    public class HourlyDataViewModel
    {
        public DeviceViewModel Device { get; set; }

        public DeviceDataType Type { get; set; }

        public IEnumerable<HourlyDatapointViewModel> Data { get; set; }
    }

    public class HourlyDatapointViewModel
    {
        public double Value { get; set; }
        public long NumSamples { get; set; }
        public DateTime HourRecorded { get; set; }
    }
}