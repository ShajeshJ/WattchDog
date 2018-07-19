using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WattchDog.Hubs
{
    public class DeviceHub : Hub
    {
        public void Connect(string macAddress)
        {
            Groups.Add(Context.ConnectionId, macAddress);
        }

        public static void SendData(string macAddress, double realPower, DateTime date)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<DeviceHub>();
            context.Clients.Group(macAddress).addMeasurements(realPower, date.ToString("hh:mm:ss tt dd/MMM/yyyy"));
        }
    }
}