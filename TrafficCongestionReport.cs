using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TrafficCongestionReport.UI;
using TrafficCongestionReport.Util;

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
            get { return "More reality traffic congestion report based on traffic speed or traffic amount"; }
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

        public TrafficCongestionReport()
        {
            try
            {
                if (GameSettings.FindSettingsFileByName("TrafficCongestionReport_SETTING") == null)
                {
                    // Creating setting file 
                    GameSettings.AddSettingsFile(new SettingsFile { fileName = "TrafficCongestionReport_SETTING" });
                }
            }
            catch (Exception)
            {
                DebugLog.LogToFileOnly("Could not load/create the setting file.");
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            OptionUI.makeSettings(helper);
        }
    }
}