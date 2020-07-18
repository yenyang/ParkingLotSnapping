using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using ColossalFramework.Globalization;
using HarmonyLib;
using System;

namespace ParkingLotSnapping
{
    [HarmonyPatch(typeof(BuildingInfo), nameof(BuildingInfo.InitializePrefab))]
    public class BuildingInfoInitializePrefabPatch
    {
        public static bool Prefix(BuildingInfo __instance)
        {
            //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch Prefix");
            var oldAI = __instance.gameObject.GetComponent<PrefabAI>();
            Debug.Log("[PLS]BuildingInfoInitializePrefabPatch oldAI.GetType().fullname = " + oldAI.GetType().FullName);
            if (oldAI.GetType() == typeof(DummyBuildingAI))
            {
                Debug.Log("[PLS]BuildingInfoInitializePrefabPatch " + __instance.name +" is DummyBuildingAI");

                if (__instance.name == "1293870311.1x1 Accessible Parking - RHD_Data")
                {
                    
                    UnityEngine.Object.DestroyImmediate(oldAI);

                    // add new ai
                    __instance.gameObject.AddComponent<ParkingSpaceAssetAI>();
                }
            }
          
           return true;
        }
    }
}
