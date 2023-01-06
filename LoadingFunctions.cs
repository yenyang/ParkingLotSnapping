
using ICities;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.PlatformServices;

namespace ParkingLotSnapping
{
    public class LoadingFunctions : LoadingExtensionBase
    {
        private LoadMode _mode;
        public static bool Loaded = false;
       
        public override void OnCreated(ILoading loading)
        {

            base.OnCreated(loading);
            if (ModSettings.ImportXMLonStartUp && ModSettings.PSACustomProperties != null && ModSettings.PLRCustomProperties != null)
            {
                string results = ModSettings.DeserializeXML();
                Debug.Log("[PLS]LoadingFunction.onCreated " + results);
            }
            
            if (CitiesHarmony.API.HarmonyHelper.IsHarmonyInstalled) Patcher.PatchAll();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {

            _mode = mode;
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
                return;
            
            Debug.Log("[PLS] Level Loaded!");
            Loaded = true;
            base.OnLevelLoaded(mode);
        }



        public override void OnLevelUnloading()
        {
            if (_mode != LoadMode.LoadGame && _mode != LoadMode.NewGame)
                return;
            Loaded = false;
            Debug.Log("[PLS] Level Unloaded!");
            base.OnLevelUnloading();
        }


        public override void OnReleased()
        {
            Loaded = false;
            if (CitiesHarmony.API.HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
            base.OnReleased();
        }
        public void OnEnabled()
        {
            CitiesHarmony.API.HarmonyHelper.EnsureHarmonyInstalled();
        }



    }
}
