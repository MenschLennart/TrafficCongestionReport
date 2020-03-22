using Harmony;
using System.Reflection;
using System;
using UnityEngine;
using TrafficCongestionReport.CustomAI;

namespace TrafficCongestionReport.Util
{
    public static class HarmonyDetours
    {
        private static HarmonyInstance harmony = null;
        private static void ConditionalPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix, HarmonyMethod postfix)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (harmony.GetPatchInfo(method)?.Owners?.Contains(harmony.Id) == true)
            {
                DebugLog.LogToFileOnly("Harmony patches already present for {0}" + fullMethodName.ToString());
            }
            else
            {
                DebugLog.LogToFileOnly("Patching {0}..." + fullMethodName.ToString());
                harmony.Patch(method, prefix, postfix);
            }
        }

        private static void ConditionalUnPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (prefix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Prefix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Prefix);
            }
            if (postfix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Postfix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Postfix);
            }
        }

        public static void Apply()
        {
            harmony = HarmonyInstance.Create("TrafficCongestionReport");
            //1
            var carAISimulationStep = typeof(CarAI).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(Vehicle.Frame).MakeByRefType(),
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(int)}, null);
            var carAISimulationStepPreFix = typeof(CustomCarAI).GetMethod("CarAISimulationStepPreFix");
            harmony.ConditionalPatch(carAISimulationStep,
                new HarmonyMethod(carAISimulationStepPreFix),
                null);
        }

        public static void DeApply()
        {
            //1
            var carAISimulationStep = typeof(CarAI).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(Vehicle.Frame).MakeByRefType(),
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(int)}, null);
            var carAISimulationStepPreFix = typeof(CustomCarAI).GetMethod("CarAISimulationStepPreFix");
            harmony.ConditionalUnPatch(carAISimulationStep,
                new HarmonyMethod(carAISimulationStepPreFix),
                null);
        }
    }
}
