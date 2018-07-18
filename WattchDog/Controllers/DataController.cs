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
            Device result;
            int num;
            using (var context = new DatabaseContext())
            {
                result = context.Devices.FirstOrDefault();
                result.Name = "new name";
                num = context.SaveChanges();
            }

            return Ok(new MeasuredDataResponse() { DeviceStatus = "on", Device = result, Result = num });
        }
    }
}
