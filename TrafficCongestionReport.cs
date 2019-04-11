using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrafficCongestionReport
{
    public class TrafficCongestionReport : IUserMod
    {
        public static bool IsEnabled = false;
        public static bool AmountMode = false;

        public string Name
        {
            get { return "Traffic Congestion Report"; }
        }

        public string Description
        {
            get { return "More reality traffic congestion report based on traffic speed"; }
        }

        public void OnEnabled()
        {
            IsEnabled = true;
            FileStream fs = File.Create("TrafficCongestionReport.txt");
            fs.Close();
        }

        public void OnDisabled()
        {
            IsEnabled = false;
        }
    }
}