using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Attributes;

namespace WattchDB.Models
{
    [DbTable("Devices")]
    public class Device
    {
        [DbKey("id")]
        public int ID { get; set; }

        [DbCol("name")]
        public string Name { get; set; }

        [DbCol("mac_address")]
        public string MacAddress { get; set; }

        [DbCol("created")]
        public DateTime Created { get; set; }
    }
}
