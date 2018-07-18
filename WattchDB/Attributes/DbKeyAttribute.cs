using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DbKeyAttribute : DbColAttribute
    {
        public DbKeyAttribute(string column)
            :base(column, false)
        {
            
        }
    }
}
