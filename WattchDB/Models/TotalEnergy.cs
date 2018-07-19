using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Models
{
    public class TotalEnergy
    {
        public int ID { get; set; }
        public int DeviceId { get; set; }
        public double Value { get; set; }
    }
}
