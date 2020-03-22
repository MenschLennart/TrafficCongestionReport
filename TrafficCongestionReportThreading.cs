using ColossalFramework;
using ColossalFramework.UI;
using ICities;
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
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
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
        }

        public static void RoadStatus(ushort segmentID, ref NetSegment data)
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

                    int lengthDenominator = Mathf.RoundToInt(totalLength) << 4;
                    int trafficDensity = 0;
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

                    int trafficDensityAmountMode = 0;
                    if (lengthDenominator != 0)
                    {
                        trafficDensityAmountMode = (int)((byte)Mathf.Min((int)(MainDataStore.trafficBufferAmountMode[(int)segmentID] * 100) / lengthDenominator, 100));
                    }
                    MainDataStore.trafficBufferAmountMode[(int)segmentID] = 0;
                    if (trafficDensityAmountMode > (int)MainDataStore.trafficDensityAmountMode[(int)segmentID])
                    {
                        MainDataStore.trafficDensityAmountMode[(int)segmentID] = (byte)Mathf.Min((int)(MainDataStore.trafficDensityAmountMode[(int)segmentID] + 5), trafficDensityAmountMode);
                    }
                    else if (trafficDensityAmountMode < (int)MainDataStore.trafficDensityAmountMode[(int)segmentID])
                    {
                        MainDataStore.trafficDensityAmountMode[(int)segmentID] = (byte)Mathf.Max((int)(MainDataStore.trafficDensityAmountMode[(int)segmentID] - 5), trafficDensityAmountMode);
                    }

                    if (TrafficCongestionReport.AmountMode)
                    {
                        data.m_trafficDensity = MainDataStore.trafficDensityAmountMode[(int)segmentID];
                    }
                    else
                    {
                        data.m_trafficDensity = MainDataStore.trafficDensity[(int)segmentID];
                    }
                }
            }
        }

        public void CheckDetour()
        {
            if (isFirstTime && Loader.HarmonyDetourInited)
            {
                isFirstTime = false;
                if (Loader.HarmonyDetourFailed)
                {
                    string error = $"Error: Send TrafficCongestionReport.txt to Author.";
                    DebugLog.LogToFileOnly(error);
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                }
            }
        }
    }
}