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

        private static string _hourlyQuery = @"SELECT COUNT(*) as numSamples, AVG(value) as average, DATE(date_recorded) as date, HOUR(date_recorded) as hour
                                                FROM {0} WHERE device_id=@devId
                                                AND date_recorded >= @mindate AND date_recorded < @maxdate
                                                GROUP BY date, hour
                                                ORDER BY date DESC, hour DESC";

        private static string _dailyQuery = @"SELECT COUNT(*) as numSamples, AVG(value) as average, DATE(date_recorded) as date
                                                FROM {0} WHERE device_id=@devId
                                                AND date_recorded >= @mindate AND date_recorded < @maxdate
                                                GROUP BY date
                                                ORDER BY date DESC";

        private static string _monthlyQuery = @"SELECT COUNT(*) as numSamples, AVG(value) as average, YEAR(date_recorded) as year, MONTH(date_recorded) as month
                                                FROM {0} WHERE device_id=@devId
                                                AND date_recorded >= @mindate AND date_recorded < @maxdate
                                                GROUP BY year, month
                                                ORDER BY year DESC, month DESC";

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
                        result.Secret = reader["secret"] as string;

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
                            device.Secret = reader["secret"] as string;

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

        //public async Task<int> InsertDevice(Device device)
        //{
        //    int id;

        //    await _connection.OpenAsync();

        //    using (var cmd = _connection.CreateCommand())
        //    {
        //        cmd.CommandText = "INSERT INTO Devices (mac_address) VALUES(@mac)";
        //        cmd.Parameters.AddWithValue("@mac", device.MacAddress);
        //        await cmd.ExecuteNonQueryAsync();
        //        id = (int)cmd.LastInsertedId;
        //    }

        //    await _connection.CloseAsync();

        //    return id;
        //}

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

        public async Task<IEnumerable<AggregatedData>> GetAggregatedData(string table, int deviceId, DateTime now, DateGrouping groupType = DateGrouping.Hourly)
        {
            var result = new List<AggregatedData>();

            string query, minDate, maxDate;

            switch(groupType)
            {
                case DateGrouping.Monthly:
                    query = string.Format(_monthlyQuery, table);
                    minDate = (new DateTime(now.Year, now.Month, 1)).AddYears(-1).ToString("yyyy-MM-dd");
                    maxDate = (new DateTime(now.Year, now.Month, 1)).ToString("yyyy-MM-dd");
                    break;
                case DateGrouping.Daily:
                    query = string.Format(_dailyQuery, table);
                    minDate = (new DateTime(now.Year, now.Month, now.Day)).AddMonths(-1).ToString("yyyy-MM-dd");
                    maxDate = (new DateTime(now.Year, now.Month, now.Day)).ToString("yyyy-MM-dd");
                    break;
                default:
                    query = string.Format(_hourlyQuery, table);
                    minDate = (new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0)).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
                    maxDate = (new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss");
                    break;
            }

            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@devId", deviceId);
                cmd.Parameters.AddWithValue("@mindate", minDate);
                cmd.Parameters.AddWithValue("@maxdate", maxDate);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null)
                    {
                        while (await reader.ReadAsync())
                        {
                            var datapoint = new AggregatedData();

                            datapoint.NumSamples = (long)reader["numSamples"];
                            datapoint.AvgValue = (double)reader["average"];
                            datapoint.DeviceId = deviceId;
                            datapoint.GroupedType = groupType;
                            datapoint.Type = table;

                            switch (groupType)
                            {
                                case DateGrouping.Monthly:
                                    datapoint.GroupedDate = new DateTime((int)reader["year"], (int)reader["month"], 1);
                                    break;
                                case DateGrouping.Daily:
                                    datapoint.GroupedDate = (DateTime)reader["date"];
                                    break;
                                default:
                                    datapoint.GroupedDate = ((DateTime)reader["date"]).AddHours((int)reader["hour"]);
                                    break;
                            }

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

        #region Total Energy Table Interactions

        public async Task<TotalEnergy> GetTotalEnergy(int deviceId)
        {
            TotalEnergy result = null;

            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM TotalEnergy WHERE device_id=@devId";
                cmd.Parameters.AddWithValue("@devId", deviceId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        result = new TotalEnergy();

                        result.ID = (int)reader["id"];
                        result.Value = (double)reader["value"];
                        result.DeviceId = (int)reader["device_id"];

                        reader.Close();
                    }
                }
            }

            await _connection.CloseAsync();

            return result;
        }

        public async Task InsertTotalEnergy(int deviceId, double value)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO TotalEnergy (value, device_id) VALUES(@value, @devId)";
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@devId", deviceId);

                await cmd.ExecuteNonQueryAsync();
            }

            await _connection.CloseAsync();
        }

        public async Task UpdateTotalEnergy(int deviceId, double value)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE TotalEnergy SET value=@value WHERE device_id=@devId";
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@devId", deviceId);

                await cmd.ExecuteNonQueryAsync();
            }

            await _connection.CloseAsync();
        }

        #endregion
    }
}
