using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WattchDB.Models;

namespace WattchDog.Models
{
    public class MeasuredDataDTO
    {
        [Required]
        [JsonProperty("macaddress")]
        public string MacAddress { get; set; }

        [Required]
        [JsonProperty("power")]
        public double RealPower { get; set; }

        [Required]
        [JsonProperty("vrms")]
        public double Vrms { get; set; }

        [Required]
        [JsonProperty("irms")]
        public double Irms { get; set; }

        [Required]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [Required]
        [JsonProperty("sampletime")]
        public double SampleDuration { get; set; }

        [Required]
        [JsonProperty("hashed_pw")]
        public string HashedPW { get; set; }

        //[Required]
        //[JsonProperty("minvoltage")]
        //public double MinVoltage { get; set; }

        //[Required]
        //[JsonProperty("maxvoltage")]
        //public double MaxVoltage { get; set; }

        //[Required]
        //[JsonProperty("mincurrent")]
        //public double MinCurrent { get; set; }

        //[Required]
        //[JsonProperty("maxcurrent")]
        //public double MaxCurrent { get; set; }
    }

    public class MeasuredDataResponse
    {
        public string DeviceStatus { get; set; }

        public ScheduleResponse Schedule { get; set; }
    }

    public class ScheduleResponse
    {
        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        public static explicit operator ScheduleResponse(DeviceSchedule obj)
        {
            return new ScheduleResponse { StartTime = obj.StartTime, EndTime = obj.EndTime };
        }
    }
}