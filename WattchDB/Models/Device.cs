using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Attributes;

namespace WattchDB.Models
{
    [SqlTable("Devices")]
    public class Device
    {
        [SqlColumn("id")]
        public int ID { get; set; }

        [SqlColumn("name")]
        public string Name { get; set; }

        [SqlColumn("mac_address")]
        public string MacAddress { get; set; }

        [SqlColumn("created")]
        public DateTime Created { get; set; }

        [SqlColumn("status")]
        public bool Status { get; set; }

        [SqlColumn("secret")]
        public string Secret { get; set; }

        [SqlColumn("user_id")]
        public int? UserId { get; set; }
    }
}
