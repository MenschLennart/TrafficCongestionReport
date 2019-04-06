using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrafficCongestionReport.Util;
using UnityEngine;

namespace TrafficCongestionReport.CustomAI
{
    public class CustomCarAI
    {
        public static void CarAISimulationStepPreFix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            VehicleStatus(vehicleID);
        }
        //For detour AdvancedJuctionRule
        public static void CarAICustomSimulationStepPreFix(ushort vehicleID, ref Vehicle vehicleData)
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
                if (vehicleID < 16384)
                {
                    Vehicle vehicle = instance.m_vehicles.m_buffer[vehicleID];
                    uint pathId = vehicle.m_path;
                    if (pathId >= 262144)
                    {
                        DebugLog.LogToFileOnly("Invaid pathId = " + pathId.ToString());
                        pathId = 0;
                    }

                    if (pathId != 0)
                    {
                        byte finePathPosIndex = vehicle.m_pathPositionIndex;
                        byte lastPathOffset = vehicle.m_lastPathOffset;
                        PathUnit.Position currentPosition;
                        if (!pathMan.m_pathUnits.m_buffer[pathId].GetPosition(finePathPosIndex >> 1, out currentPosition))
                        {
                            //DebugLog.LogToFileOnly("Error: no currentPosition");
                            return;
                        }

                        if (currentPosition.m_segment >= 36864)
                        {
                            DebugLog.LogToFileOnly("Invaid m_segment = " + currentPosition.m_segment.ToString());
                            currentPosition.m_segment = 0;
                        }

                        if (currentPosition.m_segment != 0)
                        {
                            if (currentPosition.m_lane < netManager.m_segments.m_buffer[currentPosition.m_segment].Info.m_lanes.Length)
                            {
                                float speedLimit = netManager.m_segments.m_buffer[currentPosition.m_segment].Info.m_lanes[currentPosition.m_lane].m_speedLimit;
                                float realSpeed = (float)Math.Sqrt(vehicle.GetLastFrameVelocity().x * vehicle.GetLastFrameVelocity().x + vehicle.GetLastFrameVelocity().y * vehicle.GetLastFrameVelocity().y + vehicle.GetLastFrameVelocity().z * vehicle.GetLastFrameVelocity().z);
                                float tempNum = 1f;
                                if (speedLimit != 0)
                                {
                                    tempNum = (realSpeed / speedLimit * 8f);
                                }

                                if (tempNum > 0.5f)
                                {
                                    tempNum = 0.5f;
                                }

                                tempNum = (0.5f - tempNum) * 64f;
                                int noise;
                                float num9 = 1f + vehicle.CalculateTotalLength(vehicleID, out noise);
                                MainDataStore.trafficBuffer[currentPosition.m_segment] = (ushort)Mathf.Min((int)MainDataStore.trafficBuffer[currentPosition.m_segment] + (Mathf.RoundToInt(num9 * 2.5f) * tempNum), 65535);
                            }
                            else
                            {
                                DebugLog.LogToFileOnly("Error: invalid currentPosition.m_lane = " + currentPosition.m_lane.ToString() + "Length = " + netManager.m_segments.m_buffer[currentPosition.m_segment].Info.m_lanes.Length.ToString());
                            }
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
