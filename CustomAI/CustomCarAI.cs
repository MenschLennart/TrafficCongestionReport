using ColossalFramework;
using TrafficCongestionReport.Util;
using UnityEngine;

namespace TrafficCongestionReport.CustomAI
{
    public class CustomCarAI
    {
        public static void CarAISimulationStepPreFix(ushort vehicleID)
        {
            VehicleStatus(vehicleID);
        }

        public static void VehicleStatus(ushort vehicleID)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            PathManager pathMan = Singleton<PathManager>.instance;
            NetManager netManager = Singleton<NetManager>.instance;
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num4 = (int)(currentFrameIndex & 255u);
            if (((num4 >> 4) & 15u) == (vehicleID & 15u))
            {
                if (vehicleID < Singleton<VehicleManager>.instance.m_vehicles.m_size)
                {
                    Vehicle vehicle = instance.m_vehicles.m_buffer[vehicleID];
                    uint pathId = vehicle.m_path;

                    if (pathId != 0)
                    {
                        byte finePathPosIndex = vehicle.m_pathPositionIndex;
                        if (!pathMan.m_pathUnits.m_buffer[pathId].GetPosition(finePathPosIndex >> 1, out PathUnit.Position currentPosition))
                        {
                            return;
                        }

                        if (currentPosition.m_segment != 0)
                        {
                            float speedLimit = netManager.m_segments.m_buffer[currentPosition.m_segment].Info.m_lanes[currentPosition.m_lane].m_speedLimit;
                            float realSpeed = vehicle.GetLastFrameVelocity().magnitude;
                            float tempNum = 1f;
                            if (speedLimit != 0)
                            {
                                tempNum = 32f - ((realSpeed * 512f) / speedLimit);
                            }

                            if (tempNum < 0f)
                            {
                                tempNum = 0f;
                            }

                            float num9 = 1f + vehicle.CalculateTotalLength(vehicleID, out _);
                            MainDataStore.trafficBuffer[currentPosition.m_segment] = (ushort)Mathf.Min((int)MainDataStore.trafficBuffer[currentPosition.m_segment] + (Mathf.RoundToInt(num9 * 2.5f) * tempNum), 65535);
                            //2.5f * 16 = 40f
                            MainDataStore.trafficBufferAmountMode[currentPosition.m_segment] = (ushort)Mathf.Min((int)MainDataStore.trafficBufferAmountMode[currentPosition.m_segment] + (Mathf.RoundToInt(num9 * 40f)), 65535);
                        }
                    }
                }
                else
                {
                    DebugLog.LogToFileOnly("Error: invalid vehicleID = " + vehicleID.ToString());
                }
            }
        }
    }
}
