using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Models;

namespace WattchDB
{
    public class DatabaseContext : DbContext
    {
        static DatabaseContext()
        {
            Database.SetInitializer(new MySqlInitializer());
        }

        public DbSet<Device> Devices { get; set; }

        public DatabaseContext()
            :base("WattchDB")
        {

        }
    }
}
