using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Models
{
    public class AggregatedData
    {
        public long NumSamples { get; set; }
        public double AvgValue { get; set; }
        public string Type { get; set; }
        public int DeviceId { get; set; }
        public DateTime GroupedDate { get; set; }
        public DateGrouping GroupedType { get; set; }
    }

    public enum DateGrouping
    {
        Hourly = 1,
        Daily = 2,
        Monthly = 3
    }
}
