using ColossalFramework.UI;
using ICities;
using UnityEngine;
using System.IO;
using ColossalFramework;
using System.Reflection;
using System;
using System.Linq;
using ColossalFramework.Math;
using TrafficCongestionReport.Util;
using System.Collections.Generic;
using TrafficCongestionReport.UI;

namespace TrafficCongestionReport
{
    public class Loader : LoadingExtensionBase
    {
        public static LoadMode CurrentLoadMode;
        public static bool isAdvancedJunctionRuleRunning = false;
        public static bool isLoaded = false;
        public static bool HarmonyDetourInited = false;
        public static bool isGuiRunning = false;
        public class Detour
        {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod)
            {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }

        public static List<Detour> Detours { get; set; }
        public static bool DetourInited = false;
        public static SwitchUI guiPanel;
        public static UIView parentGuiView;

        public override void OnCreated(ILoading loading)
        {
            Detours = new List<Detour>();
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
                    isAdvancedJunctionRuleRunning = Check3rdPartyModLoaded("AdvancedJunctionRule", true);
                    HarmonyInitDetour();
                    Loader.DetourInited = true;
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
                RevertDetour();
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
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Revert harmony detours");
                HarmonyDetours.DeApply();
                HarmonyDetourInited = false;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogToFileOnly("Revert detours");
                Detours.Reverse();
                foreach (Detour d in Detours)
                {
                    RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
                }
                DetourInited = false;
                Detours.Clear();
                DebugLog.LogToFileOnly("Reverting detours finished.");
            }
        }

        private bool Check3rdPartyModLoaded(string namespaceStr, bool printAll = false)
        {
            bool thirdPartyModLoaded = false;

            var loadingWrapperLoadingExtensionsField = typeof(LoadingWrapper).GetField("m_LoadingExtensions", BindingFlags.NonPublic | BindingFlags.Instance);
            List<ILoadingExtension> loadingExtensions = (List<ILoadingExtension>)loadingWrapperLoadingExtensionsField.GetValue(Singleton<LoadingManager>.instance.m_LoadingWrapper);

            if (loadingExtensions != null)
            {
                foreach (ILoadingExtension extension in loadingExtensions)
                {
                    if (printAll)
                        DebugLog.LogToFileOnly($"Detected extension: {extension.GetType().Name} in namespace {extension.GetType().Namespace}");
                    if (extension.GetType().Namespace == null)
                        continue;

                    var nsStr = extension.GetType().Namespace.ToString();
                    if (namespaceStr.Equals(nsStr))
                    {
                        DebugLog.LogToFileOnly($"The mod '{namespaceStr}' has been detected.");
                        thirdPartyModLoaded = true;
                        break;
                    }
                }
            }
            else
            {
                DebugLog.LogToFileOnly("Could not get loading extensions");
            }

            return thirdPartyModLoaded;
        }
    }
}

