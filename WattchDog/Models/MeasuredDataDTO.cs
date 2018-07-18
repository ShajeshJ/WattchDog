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
        public string MacAddress { get; set; }

        [Required]
        public double RmsVoltage { get; set; }

        [Required]
        public double RmsCurrent { get; set; }

        [Required]
        public double RealPower { get; set; }

        [Required]
        public double ApparentPower { get; set; }

        [Required]
        public double Frequency { get; set; }

        [Required]
        public double PowerFactor { get; set; }

        [Required]
        public double EnergyUsage { get; set; }
    }

    public class MeasuredDataResponse
    {
        public string DeviceStatus { get; set; }
        public Device Device { get; set; }
        public int Result { get; set; }
    }
}