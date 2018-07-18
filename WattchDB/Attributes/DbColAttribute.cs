using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WattchDB.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DbColAttribute : Attribute
    {
        public string Column { get; private set; }
        public bool IsEditable { get; protected set; }

        public DbColAttribute(string column)
        {
            Column = column;
            IsEditable = true;
        }

        protected DbColAttribute(string column, bool editable)
        {
            Column = column;
            IsEditable = editable;
        }
    }
}