using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WattchDog.Models;

namespace WattchDog.Controllers
{
    public class DataController : ApiController
    {
        /// <summary>
        /// one more testjiu
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> SendData(MeasuredDataDTO input)
        {
            return Ok(new MeasuredDataResponse() { DeviceStatus = "on" });
        }
    }
}
