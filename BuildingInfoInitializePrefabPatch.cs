using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using ColossalFramework.Globalization;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ParkingLotSnapping
{
    [HarmonyPatch(typeof(BuildingInfo), nameof(BuildingInfo.InitializePrefab))]
    public class BuildingInfoInitializePrefabPatch 
    {
        public static bool Prefix(BuildingInfo __instance)
        {
            //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch Prefix");

            var oldAI = __instance.gameObject.GetComponent<PrefabAI>();
            //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch oldAI.GetType().fullname = " + oldAI.GetType().FullName);
            if (oldAI.GetType() == typeof(DummyBuildingAI))
            {
                //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch " + __instance.name +" is DummyBuildingAI");
                
                if (ModSettings.AssetCreatorMode) Debug.Log("[PLS]BuildingInfoInitializePrefabPatch __instance.name = " + __instance.name);
                bool flag = ModSettings.PSACustomProperties.ContainsKey(__instance.name);
                //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch ModSettings.PSACustomProperties.ContainsKey is " + flag);

                if (flag)
                {
                    __instance.m_disableSnow = true;

                    //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch " + psa.Key + " found!");
                    UnityEngine.Object.DestroyImmediate(oldAI);

                    // add new ai
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ParkingSpaceAssetAI>();

                   
                }
                
                
            }
          
           return true;
        }
        
    }
}
