using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WattchDB;
using WattchDB.Models;
using WattchDog.Hubs;
using WattchDog.Models;

namespace WattchDog.Controllers
{
    [RoutePrefix("api/data")]
    public class DataController : ApiController
    {
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> SendData(MeasuredDataDTO input)
        {
            double irms;
            var powerFactor = 0.9;

            if (input.MaxCurrent - input.MinCurrent < 0.5)
            {
                irms = 0;
                powerFactor = 0;
            }
            else
            {
                irms = 0.512 * ((input.MaxCurrent - input.MinCurrent) / (2 * Math.Sqrt(2)));
            }

            var vrms = 1001 * 0.512 * ((input.MaxVoltage - input.MinVoltage) / (5 * 2 * Math.Sqrt(2)));

            var realPower = vrms * irms;
            var energyUsage = realPower * input.SampleDuration;

            var tempRepo = new TempRepo();

            var device = await tempRepo.GetDevice("mac_address", input.MacAddress);

            int deviceId;
            var status = "on";

            if (device == null)
            {
                deviceId = await tempRepo.InsertDevice(new Device() { MacAddress = input.MacAddress });
            }
            else
            {
                deviceId = device.ID;
                status = device.Status ? "on" : "off";
            }

            DeviceHub.SendData(input.MacAddress, realPower, energyUsage, powerFactor, vrms, irms, input.Timestamp);

            //await tempRepo.InsertData("ApparentPowers", deviceId, input.ApparentPower, input.Timestamp);
            await tempRepo.InsertData("EnergyUsages", deviceId, energyUsage, input.Timestamp);
            //await tempRepo.InsertData("Frequencies", deviceId, input.Frequency, input.Timestamp);
            await tempRepo.InsertData("PowerFactors", deviceId, powerFactor, input.Timestamp);
            await tempRepo.InsertData("RealPowers", deviceId, realPower, input.Timestamp);
            await tempRepo.InsertData("RmsCurrents", deviceId, irms, input.Timestamp);
            await tempRepo.InsertData("RmsVoltages", deviceId, vrms, input.Timestamp);

            return Ok(new MeasuredDataResponse() { DeviceStatus = status });
        }
    }
}
