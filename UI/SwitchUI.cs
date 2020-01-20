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
        public void OnGUI()
        {
            var e = Event.current;
            // Checking key presses
            if (OptionsKeymappingFunction.amountMode.IsPressed(e))  TrafficCongestionReport.AmountMode = true;
            if (OptionsKeymappingFunction.speedMode.IsPressed(e))   TrafficCongestionReport.AmountMode = false;
        }

        public override void Start()
        {
            base.Start();
            base.Hide();
        }
    }
}
