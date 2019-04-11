using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.Collections;
using TrafficCongestionReport.Util;

namespace TrafficCongestionReport.UI
{
    public class SwitchUI : UIPanel
    {
        public override void Update()
        {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.K))
            {
                DebugLog.LogToFileOnly("Shift+R found!");
                TrafficCongestionReport.AmountMode = !TrafficCongestionReport.AmountMode;
                for (int j = 0; j <= 36863; j++)
                {
                    TrafficCongestionReportThreading.RoadStatus((ushort)j, ref Singleton<NetManager>.instance.m_segments.m_buffer[j]);
                }
            }
            base.Update();
        }

        public override void Start()
        {
            base.Start();
            base.Hide();
        }
    }
}
