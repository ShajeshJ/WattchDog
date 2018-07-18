using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SqlTableAttribute : Attribute
    {
        public string Table { get; private set; }
        
        public SqlTableAttribute(string table)
        {
            Table = table;
        }
    }
}
