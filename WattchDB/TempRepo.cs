using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Models;

namespace WattchDB
{
    public class TempRepo
    {
        private static string _connectionStr = ConfigurationManager.ConnectionStrings["WattchDB"].ConnectionString;
        MySqlConnection _connection;

        public TempRepo()
        {
            _connection = new MySqlConnection();
            _connection.ConnectionString = _connectionStr;
            _connection.Open();
        }

        public async Task<int> InsertDevice(Device device)
        {
            int id;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Devices (mac_address) VALUES(@mac)";
                cmd.Parameters.AddWithValue("@mac", device.MacAddress);
                await cmd.ExecuteNonQueryAsync();
                id = (int)cmd.LastInsertedId;
            }

            return id;
        }

        public async Task<Device> GetDevice(string column, object value)
        {
            Device result = null;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Devices WHERE " + column + " = @value";
                cmd.Parameters.AddWithValue("@value", value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        result = new Device();

                        result.ID = (int)reader["id"];
                        result.Name = reader["name"] as string;
                        result.MacAddress = reader["mac_address"] as string;
                        result.Created = (DateTime)reader["created"];

                        reader.Close();
                    }
                }
            }

            return result;
        }

        public async Task InsertData(string table, int deviceId, double value, DateTime timeRecorded)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO " + table + " (value, device_id, date_recorded) VALUES(@value, @devId, @time)";
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@devId", deviceId);
                cmd.Parameters.AddWithValue("@time", timeRecorded);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
