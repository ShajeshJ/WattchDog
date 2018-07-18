using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DbTableAttribute : Attribute
    {
        public string TableName { get; private set; }

        public DbTableAttribute(string table)
        {

        }
    }
}
