using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Attributes;

namespace WattchDB.Models
{
    [SqlTable("Users")]
    public class User
    {
        [SqlColumn("id")]
        public int ID { get; set; }

        [SqlColumn("username")]
        public string Username { get; set; }

        [SqlColumn("first_name")]
        public string FirstName { get; set; }

        [SqlColumn("last_name")]
        public string LastName { get; set; }

        [SqlColumn("email")]
        public string Email { get; set; }

        [SqlColumn("password")]
        public string Password { get; set; }

        [SqlColumn("salt")]
        public string Salt { get; set; }

        [SqlColumn("created")]
        public DateTime Created { get; set; }
    }
}
