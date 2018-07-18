using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WattchDB;
using WattchDB.Models;
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
            var tempRepo = new TempRepo();

            var device = await tempRepo.GetDevice("mac_address", input.MacAddress);

            int deviceId;

            if (device == null)
            {
                deviceId = await tempRepo.InsertDevice(new Device() { MacAddress = input.MacAddress });
            }
            else
            {
                deviceId = device.ID;
            }

            var time = input.Timestamp == default(DateTime) ? DateTime.Now : input.Timestamp;

            await tempRepo.InsertData("ApparentPowers", deviceId, input.ApparentPower, time);
            await tempRepo.InsertData("EnergyUsages", deviceId, input.EnergyUsage, time);
            await tempRepo.InsertData("Frequencies", deviceId, input.Frequency, time);
            await tempRepo.InsertData("PowerFactors", deviceId, input.PowerFactor, time);
            await tempRepo.InsertData("RealPowers", deviceId, input.RealPower, time);
            await tempRepo.InsertData("RmsCurrents", deviceId, input.RmsCurrent, time);
            await tempRepo.InsertData("RmsVoltages", deviceId, input.RmsVoltage, time);

            return Ok(new MeasuredDataResponse() { DeviceStatus = "on" });
        }
    }
}
