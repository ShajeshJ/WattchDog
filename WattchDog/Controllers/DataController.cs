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
        private IDeviceRepository _deviceRepo;

        public DataController(IDeviceRepository deviceRepo)
        {
            _deviceRepo = deviceRepo;
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> SendData(MeasuredDataDTO input)
        {
            return Ok(new MeasuredDataResponse() { DeviceStatus = "on" });
        }
    }
}
