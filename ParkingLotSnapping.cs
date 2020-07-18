
using ICities;
using System.Collections.Generic;
using ColossalFramework.UI;
using System.Reflection;
using UnityEngine;
using ColossalFramework;
using System;

namespace ParkingLotSnapping
{
    public class ParkingLotSnap : IUserMod
    {
        public string Name { get { return "Parking Lot Snapping"; } }
        public string Description { get { return "Allows for quick production of functional parking lots without anarchy. Mod By [SSU]yenyang. Parking lots by Badi_Dea"; } }
        public UIButton DeleteAssetsButton;
        public UIButton ResetButton;
        public UICheckBox PassElectricityCheckBox;
        public UISlider DistanceFromCurbSlider;
        public UISlider DistanceBetweenParkingStallsSlider;
        public void onEnabled()
        {
            //CitiesHarmony.API.HarmonyHelper.EnsureHarmonyInstalled();
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("General Options");
            PassElectricityCheckBox = group.AddCheckbox("Pass Electricity", ModSettings.PassElectricity, OnPassElectricityCheckBoxChanged) as UICheckBox;

            UIHelperBase OneSideGroup = helper.AddGroup("One Sided Parking Options");
            DistanceFromCurbSlider = OneSideGroup.AddSlider("Distance From Curb", (float)ModSettings.minDistanceFromCurb, (float)ModSettings.maxDistanceFromCurb, ModSettings.stepDistanceFromCurb, (float)ModSettings.DistanceFromCurb, OnDistanceFromCurbSliderChanged) as UISlider;
            DistanceFromCurbSlider.tooltip = (((float)ModSettings.DistanceFromCurb-(float)ModSettings.minDistanceFromCurb) / ModSettings.rangeDistanceFromCurb).ToString() + " units";
            DistanceFromCurbSlider.width += 100;

            DistanceBetweenParkingStallsSlider = OneSideGroup.AddSlider("Distance Between Rows", (float)ModSettings.minDistanceBetweenParkingStalls, (float)ModSettings.maxDistanceBetweenParkingStalls, ModSettings.stepDistanceBetweenParkingStalls, (float)ModSettings.DistanceBetweenParkingStalls, OnDistanceBetweenParkingStallsSliderChanged) as UISlider;
            DistanceBetweenParkingStallsSlider.tooltip = (((float)ModSettings.DistanceBetweenParkingStalls-(float)ModSettings.minDistanceBetweenParkingStalls) / ModSettings.rangeDistanceBetweenParkingStalls).ToString() + " units";
            DistanceBetweenParkingStallsSlider.width += 100;

            UIHelperBase resetGroup = helper.AddGroup("Reset");
            ResetButton = resetGroup.AddButton("Reset Settings", resetSettings) as UIButton;

            UIHelperBase SafelyRemoveAutoParkingLotsGroup = helper.AddGroup("Safely Remove Parking Lot Snapping");
            DeleteAssetsButton = SafelyRemoveAutoParkingLotsGroup.AddButton("Delete Parking Lot Snapping Assets", deleteAllAssets) as UIButton;

           

        }
        private void OnPassElectricityCheckBoxChanged(bool val)
        {
            ModSettings.PassElectricity = (bool)val;
        }
        private void OnDistanceFromCurbSliderChanged(float val)
        {
            ModSettings.DistanceFromCurb = (int)val;
            DistanceFromCurbSlider.tooltip = (((float)ModSettings.DistanceFromCurb - (float)ModSettings.minDistanceFromCurb) / ModSettings.rangeDistanceFromCurb).ToString() + " units";
            DistanceFromCurbSlider.tooltipBox.Show();
            DistanceFromCurbSlider.RefreshTooltip();
        }
        private void OnDistanceBetweenParkingStallsSliderChanged(float val)
        {
            ModSettings.DistanceBetweenParkingStalls = (int)val;
            DistanceBetweenParkingStallsSlider.tooltip = (((float)ModSettings.DistanceBetweenParkingStalls - (float)ModSettings.minDistanceBetweenParkingStalls) / ModSettings.rangeDistanceBetweenParkingStalls).ToString() + " units";
            DistanceBetweenParkingStallsSlider.tooltipBox.Show();
            DistanceBetweenParkingStallsSlider.RefreshTooltip();
        }
        private void resetSettings()
        {
            
            ModSettings.resetModSettings();
            DistanceFromCurbSlider.value = ModSettings.DistanceFromCurb;
            DistanceBetweenParkingStallsSlider.value = ModSettings.DistanceBetweenParkingStalls;
        }
        private void deleteAllAssets()
        {
            if (LoadingFunctions.Loaded == true)
            {
                try
                {
                    BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
                    int _capacity = _buildingManager.m_buildings.m_buffer.Length;
                    int id;
                    for (id = 0; id < _capacity; id++)
                    {
                        if ((_buildingManager.m_buildings.m_buffer[id].m_flags & Building.Flags.Created) == Building.Flags.None || (_buildingManager.m_buildings.m_buffer[id].m_flags & Building.Flags.Untouchable) != Building.Flags.None || (_buildingManager.m_buildings.m_buffer[id].m_flags & Building.Flags.BurnedDown) != Building.Flags.None || (_buildingManager.m_buildings.m_buffer[id].m_flags & Building.Flags.Demolishing) != Building.Flags.None || (_buildingManager.m_buildings.m_buffer[id].m_flags & Building.Flags.Deleted) != Building.Flags.None)
                        {
                            // Debug.Log("[RF].Hydrology  Failed Flag Test: " + _buildingManager.m_buildings.m_buffer[id].m_flags.ToString());

                        }
                        else {
                            BuildingAI ai = _buildingManager.m_buildings.m_buffer[id].Info.m_buildingAI;
                            if (ai is ParkingSpaceAssetAI)
                            {
                                // Debug.Log("[RF].Hydrology  Failed AI Test: " + ai.ToString());
                                _buildingManager.ReleaseBuilding((ushort)id);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("[PLS].deleteAllAssets Encountered Exception " + e);
                }
            }

        }
    }
}
