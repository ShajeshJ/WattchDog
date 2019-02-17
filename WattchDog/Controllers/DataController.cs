using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WattchDB;
using WattchDB.Models;
using WattchDog.Constants;
using WattchDog.Extensions;
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
            var tempRepo = new TempRepo();
            var device = await tempRepo.GetDevice("mac_address", input.MacAddress);

            if (device == null)
                return BadRequest("Invalid MAC Address");

            var checkStr = device.Secret + input.Timestamp.ToString(DateTimeConstants.InterfaceFormat);
            var checkHash = checkStr.ComputeSHA256();

            if (checkHash != input.HashedPW)
                return Content(HttpStatusCode.Forbidden, "Failed to validate device");

            var powerFactor = (input.Irms == 0 || input.Vrms == 0) ? 0.0 : 0.9;
            var energyUsage = (input.RealPower * input.SampleDuration) / (1000 * 3600);
            
            var status = device.Status ? "on" : "off";

            var totalEnergyObj = await tempRepo.GetTotalEnergy(device.ID);
            var totalEnergy = totalEnergyObj != null ? totalEnergyObj.Value + energyUsage : energyUsage;

            DeviceHub.SendData(input.MacAddress, input.RealPower, energyUsage, powerFactor, input.Vrms, input.Irms, totalEnergy, input.Timestamp);
            
            await tempRepo.InsertData("EnergyUsages", device.ID, energyUsage, input.Timestamp);
            await tempRepo.InsertData("PowerFactors", device.ID, powerFactor, input.Timestamp);
            await tempRepo.InsertData("RealPowers", device.ID, input.RealPower, input.Timestamp);
            await tempRepo.InsertData("RmsCurrents", device.ID, input.Irms, input.Timestamp);
            await tempRepo.InsertData("RmsVoltages", device.ID, input.Vrms, input.Timestamp);

            if (totalEnergyObj == null)
            {
                await tempRepo.InsertTotalEnergy(device.ID, totalEnergy);
            }
            else
            {
                await tempRepo.UpdateTotalEnergy(device.ID, totalEnergy);
            }

            return Ok(new MeasuredDataResponse() { DeviceStatus = status });
        }
    }
}
