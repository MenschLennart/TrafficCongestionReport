using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficCongestionReport.Util
{
    public class MainDataStore
    {
        //2*36864 + 1*36864
        public static ushort[] trafficBuffer = new ushort[36864];         //1
        public static byte[] trafficDensity = new byte[36864];           //1      //1
        public static byte[] saveData = new byte[110592];
        //private void ProcessLeftWaiting(ushort vehicleID, ushort nodeId, ushort fromSegmentId, byte fromLaneIndex, ushort toSegmentId, uint laneID, byte offset)

        public static void DataInit()
        {
            for (int i = 0; i < MainDataStore.trafficBuffer.Length; i++)
            {
                trafficBuffer[i] = 0;
                trafficDensity[i] = 0;
            }
        }

        public static void save()
        {
            int i = 0;
            SaveAndRestore.save_ushorts(ref i, trafficBuffer, ref saveData);
            SaveAndRestore.save_bytes(ref i, trafficDensity, ref saveData);
        }

        public static void load()
        {
            int i = 0;
            trafficBuffer = SaveAndRestore.load_ushorts(ref i, saveData, trafficBuffer.Length);
            trafficDensity = SaveAndRestore.load_bytes(ref i, saveData, trafficDensity.Length);

        }
    }
}
