using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SqlColumnAttribute : Attribute
    {
        public string Column { get; private set; }

        public SqlColumnAttribute(string column)
        {
            if (string.IsNullOrEmpty(column))
                throw new ArgumentNullException("Column name specified cannot be empty or null.");

            Column = column;
        }
    }
}
