using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WattchDB.Attributes;
using WattchDB.Models;

namespace WattchDB
{
    public class TempRepo
    {
        private static string _connectionStr = ConfigurationManager.ConnectionStrings["WattchDB"].ConnectionString;
        MySqlConnection _connection;

        private static string _hourlyQuery = @"SELECT COUNT(*) as numSamples, {0}(value) as average, DATE(date_recorded) as date, HOUR(date_recorded) as hour
                                                FROM {1} WHERE device_id=@devId
                                                AND date_recorded >= @mindate AND date_recorded < @maxdate
                                                GROUP BY date, hour
                                                ORDER BY date DESC, hour DESC";

        private static string _dailyQuery = @"SELECT COUNT(*) as numSamples, {0}(value) as average, DATE(date_recorded) as date
                                                FROM {1} WHERE device_id=@devId
                                                AND date_recorded >= @mindate AND date_recorded < @maxdate
                                                GROUP BY date
                                                ORDER BY date DESC";

        private static string _monthlyQuery = @"SELECT COUNT(*) as numSamples, {0}(value) as average, YEAR(date_recorded) as year, MONTH(date_recorded) as month
                                                FROM {1} WHERE device_id=@devId
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
                cmd.CommandText = @"SELECT d.*, e.connected FROM Devices d 
                                    LEFT JOIN(
                                        SELECT device_id,
                                            IF(MAX(date_recorded) >= DATE_SUB(CONVERT_TZ(NOW(), 'UTC', 'US/Eastern'), INTERVAL 20 SECOND),
                                                1,
                                                0) as connected
                                        FROM EnergyUsages
                                        GROUP BY device_id
                                    ) e ON d.id = e.device_id 
                                    WHERE d." + column + @" = @value";
                cmd.Parameters.AddWithValue("@value", value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        result = new Device();
                        var schedule = reader["schedule"] as string;

                        result.ID = (int)reader["id"];
                        result.Name = reader["name"] as string;
                        result.MacAddress = reader["mac_address"] as string;
                        result.Created = (DateTime)reader["created"];
                        result.Status = (int)reader["status"];
                        result.Secret = reader["secret"] as string;
                        result.UserId = reader["user_id"] as int?;
                        result.Schedule = schedule == null ? null : JsonConvert.DeserializeObject<DeviceSchedule>(schedule);
                        result.Connected = (reader["connected"] as int? ?? 0) == 1;

                        reader.Close();
                    }
                }
            }

            await _connection.CloseAsync();

            return result;
        }

        public async Task<Device> GetDevice<TProperty>(Expression<Func<Device, TProperty>> property, object value)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            var colAttr = (SqlColumnAttribute)propertyInfo.GetCustomAttribute(typeof(SqlColumnAttribute), false);
            var column = colAttr.Column;

            var result = await GetDevice(column, value);
            return result;
        }

        public async Task<IEnumerable<Device>> GetAllDevices(string column = null, object value = null, string search = null)
        {
            List<Device> result = new List<Device>();

            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT d.*, e.connected FROM Devices d
                                    LEFT JOIN(
                                        SELECT device_id,
                                            IF(MAX(date_recorded) >= DATE_SUB(CONVERT_TZ(NOW(), 'UTC', 'US/Eastern'), INTERVAL 20 SECOND),
                                                1,
                                                0) as connected
                                        FROM EnergyUsages
                                        GROUP BY device_id
                                    ) e ON d.id = e.device_id";

                if (column != null)
                {
                    cmd.CommandText += " WHERE " + column + " = @value";
                    cmd.Parameters.AddWithValue("@value", value);
                }

                if (search != null)
                {
                    cmd.CommandText += (column == null) ? " WHERE (" : " AND (";
                    cmd.CommandText += "mac_address LIKE @search OR name LIKE @search)";
                    cmd.Parameters.AddWithValue("@search", $"%{search.Replace("%", "\\%").Replace("_", "\\_")}%");
                }

                cmd.CommandText += " ORDER BY name ASC";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null)
                    {
                        while (await reader.ReadAsync())
                        {
                            var device = new Device();
                            var schedule = reader["schedule"] as string;

                            device.ID = (int)reader["id"];
                            device.Name = reader["name"] as string;
                            device.MacAddress = reader["mac_address"] as string;
                            device.Created = (DateTime)reader["created"];
                            device.Status = (int)reader["status"];
                            device.Secret = reader["secret"] as string;
                            device.UserId = reader["user_id"] as int?;
                            device.Schedule = schedule == null ? null : JsonConvert.DeserializeObject<DeviceSchedule>(schedule);
                            device.Connected = (reader["connected"] as int? ?? 0) == 1;

                            result.Add(device);
                        }

                        reader.Close();
                    }
                }
            }

            await _connection.CloseAsync();

            return result;
        }

        public async Task<IEnumerable<Device>> GetAllDevices<TProperty>(Expression<Func<Device, TProperty>> property, object value, string search)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            var colAttr = (SqlColumnAttribute)propertyInfo.GetCustomAttribute(typeof(SqlColumnAttribute), false);
            var column = colAttr.Column;

            var result = await GetAllDevices(column, value, search);
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

        public async Task UpdateDeviceStatus(string searchCol, string searchVal, int status, DeviceSchedule schedule = null)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE Devices SET status = @status, schedule = @schedule WHERE " + searchCol + " = @searchVal";
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@schedule", schedule == null ? null : JsonConvert.SerializeObject(schedule));
                cmd.Parameters.AddWithValue("@searchVal", searchVal);
                await cmd.ExecuteNonQueryAsync();
            }

            await _connection.CloseAsync();
        }

        public async Task UpdateDeviceOwner(string searchCol, string searchVal, int userId)
        {
            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE Devices SET user_id = @user_id WHERE " + searchCol + " = @searchVal";
                cmd.Parameters.AddWithValue("@user_id", userId);
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

            var aggregator = table == "EnergyUsages" ? "SUM" : "AVG";

            switch(groupType)
            {
                case DateGrouping.Monthly:
                    query = string.Format(_monthlyQuery, aggregator, table);
                    minDate = (new DateTime(now.Year, now.Month, 1)).AddYears(-1).ToString("yyyy-MM-dd");
                    maxDate = (new DateTime(now.Year, now.Month, 1)).ToString("yyyy-MM-dd");
                    break;
                case DateGrouping.Daily:
                    query = string.Format(_dailyQuery, aggregator, table);
                    minDate = (new DateTime(now.Year, now.Month, now.Day)).AddMonths(-1).ToString("yyyy-MM-dd");
                    maxDate = (new DateTime(now.Year, now.Month, now.Day)).ToString("yyyy-MM-dd");
                    break;
                default:
                    query = string.Format(_hourlyQuery, aggregator, table);
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

        #region User Table Interactions

        public async Task<User> GetUser(string column, object value)
        {
            User result = null;

            await _connection.OpenAsync();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users WHERE " + column + " = @value";
                cmd.Parameters.AddWithValue("@value", value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        result = new User();

                        result.ID = (int)reader["id"];
                        result.Username = reader["username"] as string;
                        result.FirstName = reader["first_name"] as string;
                        result.LastName = reader["last_name"] as string;
                        result.Email = reader["email"] as string;
                        result.Password = reader["password"] as string;
                        result.Salt = reader["salt"] as string;
                        result.Created = (DateTime)reader["created"];

                        reader.Close();
                    }
                }
            }

            await _connection.CloseAsync();

            return result;
        }

        public async Task<User> GetUser<TProperty>(Expression<Func<User, TProperty>> property, object value)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            var colAttr = (SqlColumnAttribute)propertyInfo.GetCustomAttribute(typeof(SqlColumnAttribute), false);
            var column = colAttr.Column;

            var result = await GetUser(column, value);
            return result;
        }

        public async Task<bool> InsertUser(User user)
        {
            await _connection.OpenAsync();
            int result;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Users (username, first_name, last_name, email, password, salt)
                                    VALUES(@username, @firstname, @lastname, @email, @password, @salt)";
                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@firstname", user.FirstName);
                cmd.Parameters.AddWithValue("@lastname", user.LastName);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@password", user.Password);
                cmd.Parameters.AddWithValue("@salt", user.Salt);

                result = await cmd.ExecuteNonQueryAsync();
            }

            return result > 0;
        }

        #endregion
    }
}
