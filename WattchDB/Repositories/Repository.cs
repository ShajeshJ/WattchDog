using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Attributes;
using WattchDB.Models;

namespace WattchDB.Repositories
{
    public abstract class Repository<T>
    {
        private MySqlConnection _connection;
        private string _tableName;
        private IReadOnlyDictionary<string, PropertyInfo> _tableMappings;

        public Repository()
        {
            _connection = new MySqlConnection();
            _connection.ConnectionString = ConfigurationManager.ConnectionStrings["WattchDB"].ConnectionString;

            var mapping = new Dictionary<string, PropertyInfo>();

            var tableAttr = (SqlTableAttribute)typeof(T).GetCustomAttribute(typeof(SqlTableAttribute), false);
            _tableName = tableAttr.Table;

            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var colAttr = (SqlColumnAttribute)property.GetCustomAttributes(typeof(SqlColumnAttribute), false).FirstOrDefault();

                if (colAttr != null)
                {
                    try
                    {
                        mapping.Add(colAttr.Column, property);
                    }
                    catch (ArgumentException agx)
                    {
                        throw new Exception("Duplicate column names specified on properties within the " + typeof(T).Name + " class", agx);
                    }
                }
            }

            _tableMappings = mapping;
        }

        public async void Insert(T item)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                var cmdTxt = $"INSERT INTO {_tableName} ";

                var columns = "(";
                var values = "(";

                foreach (var mapping in _tableMappings)
                {
                    var propValue = mapping.Value.GetValue(item);

                    if (mapping.Value.PropertyType.IsValueType && propValue == Activator.CreateInstance(mapping.Value.PropertyType)
                        || propValue == null)
                    {

                    }
                }
            }
        }
    }
}
