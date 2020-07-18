
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
        private ulong workshop_id = 0;
        public override void OnCreated(ILoading loading)
        {

            base.OnCreated(loading);
            foreach (PublishedFileId mod in PlatformService.workshop.GetSubscribedItems())
            {
                if (mod.AsUInt64 == workshop_id)
                {
                    ColossalFramework.Packaging.PackageManager.instance.LoadPackages(mod);
                }
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
