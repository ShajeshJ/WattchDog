using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Models
{
    [Table("Devices", Schema = "ebdb")]
    public class Device
    {
        [Key]
        [Column("id")]
        public int ID { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("mac_address")]
        public string MacAddress { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }
    }
}
