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
        }

        #region Device Table Interactions

        public async Task<Device> GetDevice(string column, object value)
        {
            Device result = null;

            await _connection.OpenAsync();

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
                        result.Status = (bool)reader["status"];

                        reader.Close();
                    }
                }
            }

            await _connection.CloseAsync();

            return result;
        }

        public async Task<IEnumerable<Device>> GetAllDevices()
        {
            List<Device> result = new List<Device>();

            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Devices";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null)
                    {
                        while (await reader.ReadAsync())
                        {
                            var device = new Device();

                            device.ID = (int)reader["id"];
                            device.Name = reader["name"] as string;
                            device.MacAddress = reader["mac_address"] as string;
                            device.Created = (DateTime)reader["created"];
                            device.Status = (bool)reader["status"];

                            result.Add(device);
                        }

                        reader.Close();
                    }
                }
            }

            await _connection.CloseAsync();

            return result;
        }

        public async Task UpdateDeviceName(string searchCol, string searchVal, string name)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE Devices SET name = @name WHERE " + searchCol + " = @searchVal";
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@searchVal", searchVal);
                await cmd.ExecuteNonQueryAsync();
            }

            await _connection.CloseAsync();
        }

        public async Task UpdateDeviceStatus(string searchCol, string searchVal, bool status)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE Devices SET status = @status WHERE " + searchCol + " = @searchVal";
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@searchVal", searchVal);
                await cmd.ExecuteNonQueryAsync();
            }

            await _connection.CloseAsync();
        }

        public async Task<int> InsertDevice(Device device)
        {
            int id;

            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Devices (mac_address) VALUES(@mac)";
                cmd.Parameters.AddWithValue("@mac", device.MacAddress);
                await cmd.ExecuteNonQueryAsync();
                id = (int)cmd.LastInsertedId;
            }

            await _connection.CloseAsync();

            return id;
        }

        #endregion

        #region Data Table Interactions

        public async Task<IEnumerable<Data>> GetData(string table, int deviceId, int numRecords)
        {
            var result = new List<Data>();

            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM " + table + " WHERE device_id=@devId ORDER BY date_recorded DESC LIMIT @limit";
                cmd.Parameters.AddWithValue("@devId", deviceId);
                cmd.Parameters.AddWithValue("@limit", numRecords);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null)
                    {
                        while (await reader.ReadAsync())
                        {
                            var datapoint = new Data();

                            datapoint.ID = (int)reader["id"];
                            datapoint.Value = (double)reader["value"];
                            datapoint.DeviceId = (int)reader["device_id"];
                            datapoint.TimeRecorded = (DateTime)reader["date_recorded"];
                            datapoint.Type = table;

                            result.Add(datapoint);
                        }

                        reader.Close();
                    }
                }
            }

            await _connection.CloseAsync();

            return result;
        }

        public async Task InsertData(string table, int deviceId, double value, DateTime timeRecorded)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO " + table + " (value, device_id, date_recorded) VALUES(@value, @devId, @time)";
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@devId", deviceId);
                cmd.Parameters.AddWithValue("@time", timeRecorded);

                await cmd.ExecuteNonQueryAsync();
            }

            await _connection.CloseAsync();
        }

        #endregion
    }
}
