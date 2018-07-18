using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Attributes;

namespace WattchDB.Models
{
    public abstract class MeasuredData
    {
        [DbKey("id")]
        public int ID { get; set; }

        [DbCol("value")]
        public double Value { get; set; }

        [DbCol("device_id")]
        public int DeviceId { get; set; }

        [DbCol("date_recorded")]
        public DateTime DateRecorded { get; set; }
    }
}
