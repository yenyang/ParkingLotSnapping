
using ICities;
using System.Collections.Generic;
using ColossalFramework.UI;
using System.Reflection;
using UnityEngine;
using ColossalFramework;
using System;

namespace ParkingLotSnapping
{
    public class ParkingLotSnapping : IUserMod
    {
        public string Name { get { return "Parking Lot Snapping"; } }
        public string Description { get { return "Allows for quick production of functional parking lots without anarchy. Mod By [SSU]yenyang and Superpancho. Parking lots by Badi_Dea"; } }
        public UIButton DeleteAssetsButton;
        public UIButton ResetButton;

        public UIButton ResetSnappingGrid;
        public UICheckBox PassElectricityCheckBox;
        public UICheckBox AllowSnappingCheckBox;
        public UICheckBox AllowLockingCheckBox;
        public UICheckBox OverrideOverlappingCheckBox;
        public UICheckBox UndoDisengagesLockCheckBox;
        public UISlider SnappingDistanceSlider;
        public UISlider UnlockDistanceSlider;
        public UICheckBox KeyboardShortcutsHintsCheckBox;
        public UICheckBox OverrideWarningCheckBox;
        public UICheckBox DisableTooltipsCheckBox;
        public UICheckBox BulldozeEffectCheckBox;
        public UIButton UnlockSnappingKeybindButton;
        public UIButton ReturnLastSnappingKeybindButton;
        public UIButton UndoKeybindButton;
        public UIButton LockOnToPSAKeybindButton;

        public ParkingLotSnapping()
        {
            if (GameSettings.FindSettingsFileByName(ModSettings.settingsFileName) == null) // If setting file does not exist...
            {
                GameSettings.AddSettingsFile(new SettingsFile { fileName = ModSettings.settingsFileName }); // we create it
            }

        }
        public void onEnabled()
        {
            CitiesHarmony.API.HarmonyHelper.EnsureHarmonyInstalled();
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("General Options");
            PassElectricityCheckBox = group.AddCheckbox("Pass Electricity (Applies individually to newly built PSAs)", ModSettings.PassElectricity, OnPassElectricityCheckBoxChanged) as UICheckBox;
            AllowSnappingCheckBox = group.AddCheckbox("Allow Snapping", ModSettings.AllowSnapping, OnAllowSnappingCheckBoxChanged) as UICheckBox;
            AllowLockingCheckBox = group.AddCheckbox("Allow Locking (Allow Snapping Required)", ModSettings.AllowLocking, OnAllowLockingCheckBoxChanged) as UICheckBox;
            OverrideOverlappingCheckBox = group.AddCheckbox("Override Overlapping Parking Space Assets", ModSettings.OverrideOverlapping, OnOverrideOverlappingCheckBox) as UICheckBox;
            UndoDisengagesLockCheckBox = group.AddCheckbox("Undo Disengages Lock", ModSettings.UndoDisengagesLock, OnUndoDisengagesLockCheckBoxChanged) as UICheckBox;
            BulldozeEffectCheckBox = group.AddCheckbox("Play In-Game Bulldoze Effect when Undoing", ModSettings.BulldozeEffect, OnBulldozeEffectCheckBoxChanged) as UICheckBox;

            SnappingDistanceSlider = group.AddSlider("Snapping Distance", (float)ModSettings._minSnappingDistance, (float)ModSettings._maxSnappingDistance, (float)ModSettings._snappingStep, (float)ModSettings._defaultSnappingDistance, OnSnappingDistanceChanged) as UISlider;
            SnappingDistanceSlider.tooltip = ((float)ModSettings.SnappingDistance).ToString() + " units";
            SnappingDistanceSlider.width += 100;
            UnlockDistanceSlider = group.AddSlider("Unlocking Distance", (float)ModSettings._minUnlockingDistance, (float)ModSettings._maxUnlockingDistance, (float)ModSettings._unlockingStep, (float)ModSettings._defaultUnlockingDistance, OnUnlockingDistanceChanged) as UISlider;
            UnlockDistanceSlider.tooltip = ((float)ModSettings.UnlockingDistance).ToString() + " units";
            UnlockDistanceSlider.width += 100;

            UIHelperBase tooltipgroup = helper.AddGroup("Tooltip Options");
            KeyboardShortcutsHintsCheckBox = tooltipgroup.AddCheckbox("Keyboard Shortcuts Hints", ModSettings.KeyboardShortcutHints, OnKeyboardShortcutsHintsCheckBoxChanged) as UICheckBox;
            OverrideWarningCheckBox = tooltipgroup.AddCheckbox("Overlapping/Overriding Warnings", ModSettings.OverrideWarning, OnOverrideWarningCheckBoxChanged) as UICheckBox;
            DisableTooltipsCheckBox = tooltipgroup.AddCheckbox("Disable Tooltips", ModSettings.DisableTooltips, OnDisableTooltipsCheckBoxChanged) as UICheckBox;

            // Key bindings section
            //Need UIhelper class, not UIHelperBase, it's ok because the game uses UIHelper anyway
            UIHelper keyBindingGroup = helper.AddGroup("Keybindings") as UIHelper;
            // Will use utility function in "ModKeyBindingFunctions" class. Addkeymapping creates an element to selected panel
            // Needs text description and SavedInputClass associated (in ModSettings)
            UnlockSnappingKeybindButton = ModKeyBindingFunctions.AddKeyMappingUI(keyBindingGroup, "Unlock From Current Snapping", ModSettings.UnlockKeybind);
            ReturnLastSnappingKeybindButton = ModKeyBindingFunctions.AddKeyMappingUI(keyBindingGroup, "Re-Lock to Last Locked Position", ModSettings.ReturnLockKeybind);
            UndoKeybindButton = ModKeyBindingFunctions.AddKeyMappingUI(keyBindingGroup, "Undo Last Placement", ModSettings.UndoKeybind);
            LockOnToPSAKeybindButton = ModKeyBindingFunctions.AddKeyMappingUI(keyBindingGroup, "Lock-on to Existing Parking Space Asset", ModSettings.LockOnToPSAKeybind);

            UIHelperBase resetGroup = helper.AddGroup("Reset");
            ResetSnappingGrid = resetGroup.AddButton("Reset Snapping Grid", resetSnappingGrid) as UIButton;
            ResetButton = resetGroup.AddButton("Reset Settings", resetSettings) as UIButton;

            UIHelperBase SafelyRemoveAutoParkingLotsGroup = helper.AddGroup("Safely Remove Parking Lot Snapping");
            DeleteAssetsButton = SafelyRemoveAutoParkingLotsGroup.AddButton("Delete Parking Lot Snapping Assets", deleteAllAssets) as UIButton;

        }
        private void OnPassElectricityCheckBoxChanged(bool val)
        {
            ModSettings.PassElectricity = (bool)val;
        }
        
        private void OnKeyboardShortcutsHintsCheckBoxChanged(bool val)
        {
            ModSettings.KeyboardShortcutHints = (bool)val;
        }
        private void OnOverrideWarningCheckBoxChanged(bool val)
        {
            ModSettings.OverrideWarning = (bool)val;
        }
        private void OnUndoDisengagesLockCheckBoxChanged(bool val)
        {
            ModSettings.UndoDisengagesLock = (bool)val;
        }
        private void OnDisableTooltipsCheckBoxChanged(bool val)
        {
            ModSettings.DisableTooltips = (bool)val;
        }
        private void OnAllowSnappingCheckBoxChanged(bool val)
        {
            ModSettings.AllowSnapping = (bool)val;
        }
        private void OnAllowLockingCheckBoxChanged(bool val)
        {
            ModSettings.AllowLocking = (bool)val;
        }
        private void OnOverrideOverlappingCheckBox(bool val)
        {
            ModSettings.OverrideOverlapping = (bool)val;
        }
        private void OnBulldozeEffectCheckBoxChanged(bool val)
        {
            ModSettings.BulldozeEffect = (bool)val;
        }
        private void OnSnappingDistanceChanged(float val)
        {
            ModSettings.SnappingDistance = (int)val;
            SnappingDistanceSlider.tooltip = ModSettings.SnappingDistance.ToString() + " units";
            SnappingDistanceSlider.tooltipBox.Show();
            SnappingDistanceSlider.RefreshTooltip();
        }
        private void OnUnlockingDistanceChanged(float val)
        {
            ModSettings.UnlockingDistance = (int)val;
            UnlockDistanceSlider.tooltip = ModSettings.UnlockingDistance.ToString() + " units";
            UnlockDistanceSlider.tooltipBox.Show();
            UnlockDistanceSlider.RefreshTooltip();
        }
        private void resetSettings()
        {
            
            ModSettings.resetModSettings();
            PassElectricityCheckBox.isChecked = ModSettings.PassElectricity;
            AllowLockingCheckBox.isChecked = ModSettings.AllowLocking;
            AllowSnappingCheckBox.isChecked = ModSettings.AllowSnapping;
            OverrideOverlappingCheckBox.isChecked = ModSettings.OverrideOverlapping;
            UndoDisengagesLockCheckBox.isChecked = ModSettings.UndoDisengagesLock;
            BulldozeEffectCheckBox.isChecked = ModSettings.BulldozeEffect;
            SnappingDistanceSlider.value = ModSettings.SnappingDistance;
            UnlockDistanceSlider.value = ModSettings.UnlockingDistance;
            KeyboardShortcutsHintsCheckBox.isChecked = ModSettings.KeyboardShortcutHints;
            OverrideWarningCheckBox.isChecked = ModSettings.OverrideWarning;
            DisableTooltipsCheckBox.isChecked = ModSettings.DisableTooltips;
            UnlockSnappingKeybindButton.text = ModSettings.UnlockKeybind.ToLocalizedString("KEYNAME");
            ReturnLastSnappingKeybindButton.text = ModSettings.ReturnLockKeybind.ToLocalizedString("KEYNAME");
            UndoKeybindButton.text = ModSettings.UndoKeybind.ToLocalizedString("KEYNAME");
            LockOnToPSAKeybindButton.text = ModSettings.LockOnToPSAKeybind.ToLocalizedString("KEYNAME");
        }
        private void resetSnappingGrid()
        {
            if (ParkingSpaceGrid.areYouAwake() == true)
            {
                ParkingSpaceGrid.Awake();
            }
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
                          
                        }
                        else {
                            BuildingAI ai = _buildingManager.m_buildings.m_buffer[id].Info.m_buildingAI;
                            if (ai is ParkingSpaceAssetAI)
                            {
                               
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
