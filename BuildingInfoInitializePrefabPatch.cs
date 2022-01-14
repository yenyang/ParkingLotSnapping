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
            if (oldAI.GetType() == typeof(DummyBuildingAI) && __instance.name.Length > 17)
            {
               // Debug.Log("[PLS]BuildingInfoInitializePrefabPatch " + __instance.name +" is DummyBuildingAI");
                
                string assetName = __instance.name.Remove(__instance.name.Length - 5).Substring(11);
                //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch assetName = " + assetName);
                bool flag = ModSettings.PSACustomProperties.ContainsKey(assetName);
                //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch ModSettings.PSACustomProperties.ContainsKey is " + flag);

                if (flag)
                {
                    ModSettings.PSACustomPropertiesStruct psaCustomProperties = ModSettings.PSACustomProperties.GetValueSafe<String, ModSettings.PSACustomPropertiesStruct>(assetName);
                    //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch " + psa.Key + " found!");
                    UnityEngine.Object.DestroyImmediate(oldAI);

                    // add new ai
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ParkingSpaceAssetAI>();

                    var newAIFields = newAI.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);


                    var newAIFieldDic = new Dictionary<string, FieldInfo>(newAIFields.Length);
                    foreach (var field in newAIFields)
                    {
                        //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch Found Field named " + field.Name);
                        newAIFieldDic.Add(field.Name, field);
                    }

                    foreach (var fieldInfo in newAIFields)
                    {
                        // do not copy attributes marked NonSerialized
                        bool copyField = !fieldInfo.IsDefined(typeof(NonSerializedAttribute), true);

                        if (!fieldInfo.IsDefined(typeof(CustomizablePropertyAttribute), true)) copyField = false;

                        if (copyField)
                        {
                            FieldInfo newAIField;
                            newAIFieldDic.TryGetValue(fieldInfo.Name, out newAIField);
                            try
                            {
                                if (newAIField != null && newAIField.GetType().Equals(fieldInfo.GetType()))
                                {
                                    if (fieldInfo.Name == "m_offset")
                                    {
                                        newAIField.SetValue(newAI, psaCustomProperties.Offset);
                                        //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch Set " + fieldInfo.Name + " to " + psa.Value.Offset);
                                    }
                                    else if (fieldInfo.Name == "m_parkingWidth")
                                    {
                                        newAIField.SetValue(newAI, psaCustomProperties.ParkingWidth);
                                    }
                                    //Debug.Log("[PLS]BuildingInfoInitializePrefabPatch Ready to set fieldInfo Named " + fieldInfo.Name);
                                    //newAIField.SetValue(newAI, fieldInfo.GetValue(src));
                                }
                            }
                            catch (NullReferenceException)
                            {
                            }
                        }
                    }
                }
                
                
            }
          
           return true;
        }
        
    }
}
