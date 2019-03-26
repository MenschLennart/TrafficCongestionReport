using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Reflection;
using TrafficCongestionReport.CustomAI;
using TrafficCongestionReport.Util;
using UnityEngine;

namespace TrafficCongestionReport
{
    public class TrafficCongestionReportThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (TrafficCongestionReport.IsEnabled)
            {
                CheckDetour();
                uint num2 = Singleton<SimulationManager>.instance.m_currentFrameIndex & 255u;
                int num3 = (int)(num2 * 144u);
                int num4 = (int)((num2 + 1u) * 144u - 1u);
                for (int j = num3; j <= num4; j++)
                {
                    RoadStatus((ushort)j, ref Singleton<NetManager>.instance.m_segments.m_buffer[j]);
                }
            }
        }

        public void RoadStatus(ushort segmentID, ref NetSegment data)
        {
            NetManager instance = Singleton<NetManager>.instance;
            if (data.m_flags.IsFlagSet(NetSegment.Flags.Created))
            {
                if (data.Info.m_laneTypes.IsFlagSet(NetInfo.LaneType.Vehicle) && data.Info.m_vehicleTypes.IsFlagSet(VehicleInfo.VehicleType.Car))
                {
                    uint curLaneId = data.m_lanes;
                    int laneIndex = 0;
                    float totalLength = 0f;
                    while (laneIndex < data.Info.m_lanes.Length && curLaneId != 0u)
                    {
                        NetInfo.Lane lane = data.Info.m_lanes[laneIndex];
                        if ((lane.m_laneType & (NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle)) != NetInfo.LaneType.None && (lane.m_vehicleType & ~VehicleInfo.VehicleType.Bicycle) != VehicleInfo.VehicleType.None)
                        {
                            totalLength += instance.m_lanes.m_buffer[(int)curLaneId].m_length;
                        }
                        curLaneId = instance.m_lanes.m_buffer[(int)curLaneId].m_nextLane;
                        laneIndex++;
                    }
                    int trafficDensity = 0;
                    int lengthDenominator = Mathf.RoundToInt(totalLength) << 4;
                    if (lengthDenominator != 0)
                    {
                        trafficDensity = (int)((byte)Mathf.Min((int)(MainDataStore.trafficBuffer[(int)segmentID] * 100) / lengthDenominator, 100));
                    }
                    MainDataStore.trafficBuffer[(int)segmentID] = 0;
                    if (trafficDensity > (int)MainDataStore.trafficDensity[(int)segmentID])
                    {
                        MainDataStore.trafficDensity[(int)segmentID] = (byte)Mathf.Min((int)(MainDataStore.trafficDensity[(int)segmentID] + 5), trafficDensity);
                    }
                    else if (trafficDensity < (int)MainDataStore.trafficDensity[(int)segmentID])
                    {
                        MainDataStore.trafficDensity[(int)segmentID] = (byte)Mathf.Max((int)(MainDataStore.trafficDensity[(int)segmentID] - 5), trafficDensity);
                    }
                    data.m_trafficDensity = MainDataStore.trafficDensity[(int)segmentID];
                }
            }
        }

        public void CheckDetour()
        {
            if (isFirstTime)
            {
                isFirstTime = false;
                DetourAfterLoad();
                if (Loader.DetourInited)
                {
                    DebugLog.LogToFileOnly("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Checking detours.");
                    List<string> list = new List<string>();
                    foreach (Loader.Detour current in Loader.Detours)
                    {
                        if (!RedirectionHelper.IsRedirected(current.OriginalMethod, current.CustomMethod))
                        {
                            list.Add(string.Format("{0}.{1} with {2} parameters ({3})", new object[]
                            {
                    current.OriginalMethod.DeclaringType.Name,
                    current.OriginalMethod.Name,
                    current.OriginalMethod.GetParameters().Length,
                    current.OriginalMethod.DeclaringType.AssemblyQualifiedName
                            }));
                        }
                    }
                    DebugLog.LogToFileOnly(string.Format("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Detours checked. Result: {0} missing detours", list.Count));
                    if (list.Count > 0)
                    {
                        string error = "TrafficCongestionReport detected an incompatibility with another mod! You can continue playing but it's NOT recommended. TrafficCongestionReport will not work as expected. See TrafficCongestionReport.txt for technical details.";
                        DebugLog.LogToFileOnly(error);
                        string text = "The following methods were overriden by another mod:";
                        foreach (string current2 in list)
                        {
                            text += string.Format("\n\t{0}", current2);
                        }
                        DebugLog.LogToFileOnly(text);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", text, true);
                    }
                }
            }
        }

        public void DetourAfterLoad()
        {
            //This is for Detour Other Mod method
            DebugLog.LogToFileOnly("Init DetourAfterLoad");
            bool detourFailed = false;

            DebugLog.LogToFileOnly("Detour AdvancedJunctionRule.NewCarAI::VehicleStatusForTrafficCongestionReport calls");
            if (Loader.isAdvancedJunctionRuleRunning)
            {
                try
                {
                    Assembly as1 = Assembly.Load("AdvancedJunctionRule");
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("AdvancedJunctionRule.CustomAI.NewCarAI").GetMethod("VehicleStatusForTrafficCongestionReport", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType()}, null),
                typeof(CustomCarAI).GetMethod("CustomCarAICustomSimulationStepPreFix", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType()}, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour AdvancedJunctionRule.NewCarAI::VehicleStatusForTrafficCongestionReport");
                    detourFailed = true;
                }
                Loader.DetourInited = true;
            }

            if (detourFailed)
            {
                DebugLog.LogToFileOnly("DetourAfterLoad failed");
            }
            else
            {
                DebugLog.LogToFileOnly("DetourAfterLoad successful");
            }
        }
    }
}