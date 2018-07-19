using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Models
{
    public class Data
    {
        public int ID { get; set; }
        public double Value { get; set; }
        public string Type { get; set; }
        public int DeviceId { get; set; }
        public DateTime TimeRecorded { get; set; }
    }
}
