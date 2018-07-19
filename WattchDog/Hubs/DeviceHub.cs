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

        public static void SendData(string macAddress, double realPower, double energyUsage, double powerFactor, double vrms, double irms, double totalEnergy, DateTime date)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<DeviceHub>();
            context.Clients.Group(macAddress).addMeasurements(
                realPower.ToString("0.##"), 
                energyUsage.ToString("0.#######"), 
                powerFactor.ToString("0.##"), 
                vrms.ToString("0.##"), 
                irms.ToString("0.##"),
                totalEnergy.ToString("0.#####"), 
                date.ToString("hh:mm:ss tt dd/MMM/yyyy"));
        }
    }
}