using ColossalFramework.UI;
using ICities;
using TrafficCongestionReport.Util;
using TrafficCongestionReport.UI;

namespace TrafficCongestionReport
{
    public class Loader : LoadingExtensionBase
    {
        public static LoadMode CurrentLoadMode;
        public static bool isLoaded = false;
        public static bool HarmonyDetourInited = false;
        public static bool HarmonyDetourFailed = true;
        public static bool isGuiRunning = false;
        public static SwitchUI guiPanel;
        public static UIView parentGuiView;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Loader.CurrentLoadMode = mode;
            if (TrafficCongestionReport.IsEnabled)
            {
                if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
                {
                    DebugLog.LogToFileOnly("OnLevelLoaded");
                    SetupGui();
                    HarmonyInitDetour();
                    if (mode == LoadMode.NewGame)
                    {
                        DebugLog.LogToFileOnly("New Game");
                    }
                    isLoaded = true;
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (isGuiRunning)
                {
                    RemoveGui();
                }
                HarmonyRevertDetour();
                TrafficCongestionReportThreading.isFirstTime = true;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }

        public void SetupGui()
        {
            Loader.parentGuiView = null;
            Loader.parentGuiView = UIView.GetAView();

            if (Loader.guiPanel == null)
            {
                Loader.guiPanel = (SwitchUI)Loader.parentGuiView.AddUIComponent(typeof(SwitchUI));
            }
            isGuiRunning = true;
        }

        public void RemoveGui()
        {
            if (parentGuiView != null)
            {
                parentGuiView = null;
                UnityEngine.Object.Destroy(guiPanel);
                guiPanel = null;
            }
            isGuiRunning = false;
        }

        public void HarmonyInitDetour()
        {
            if (!HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Init harmony detours");
                HarmonyDetours.Apply();
                HarmonyDetourInited = true;
                HarmonyDetourFailed = false;
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Revert harmony detours");
                HarmonyDetours.DeApply();
                HarmonyDetourInited = false;
                HarmonyDetourFailed = true;
            }
        }
    }
}

