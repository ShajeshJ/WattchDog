﻿using System;
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

        public static void SendData(string macAddress, double realPower, double energyUsage, double powerFactor, double vrms, double irms, DateTime date)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<DeviceHub>();
            context.Clients.Group(macAddress).addMeasurements(
                Math.Round(realPower, 2),
                Math.Round(energyUsage, 7),
                Math.Round(powerFactor, 2),
                Math.Round(vrms, 2),
                Math.Round(irms, 2),
                date);
        }
    }
}