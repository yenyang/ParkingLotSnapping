
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
        public string Description { get { return "Allows for quick production of functional parking lots without anarchy. Mod By [SSU]yenyang. Parking lots by others"; } }

        public UIButton DeleteAssetsButton;
        public UIButton ExportXMLButton;
        public UIButton ResetButton;

        public UIButton ResetSnappingGrid;
        public UICheckBox PassElectricityCheckBox;
        public UICheckBox AllowSnappingCheckBox;
        public UICheckBox AllowLockingCheckBox;
        public UICheckBox OverrideOverlappingCheckBox;
        public UISlider SnappingDistanceSlider;
        public UISlider UnlockDistanceSlider;
        public UICheckBox KeyboardShortcutsHintsCheckBox;
        public UICheckBox OverrideWarningCheckBox;
        public UICheckBox UndoDisengagesLockCheckBox;
        public UICheckBox DisableTooltipsCheckBox;
        public UICheckBox BulldozeEffectCheckBox;
        public UIButton UnlockSnappingKeybindButton;
        public UIButton ReturnLastSnappingKeybindButton;
        public UIButton UndoKeybindButton;
        public UIButton LockOnToPSAKeybindButton;

        public UICheckBox AssetCreatorModeCheckBox;
        static public UITextField ExportErrorTextField;
        static public UITextField ImportErrorTextField;
        public UITextField FolderPathTextField;
        public UIButton ImportXMLButton;
        public UICheckBox ImportXMLOnGameStartCheckBox;


        public ParkingLotSnap()
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
            
            AllowSnappingCheckBox = group.AddCheckbox("Allow Snapping", ModSettings.AllowSnapping, OnAllowSnappingCheckBoxChanged) as UICheckBox;
            AllowLockingCheckBox = group.AddCheckbox("Allow Locking", ModSettings.AllowLocking, OnAllowLockingCheckBoxChanged) as UICheckBox;
            OverrideOverlappingCheckBox = group.AddCheckbox("Override Overlapping Parking Space Assets", ModSettings.OverrideOverlapping, OnOverrideOverlappingCheckBox) as UICheckBox;
            BulldozeEffectCheckBox = group.AddCheckbox("Play In-Game Bulldoze Effect when Undoing", ModSettings.BulldozeEffect, OnBulldozeEffectCheckBoxChanged) as UICheckBox;
            PassElectricityCheckBox = group.AddCheckbox("Pass Electricity", ModSettings.PassElectricity, OnPassElectricityCheckBoxChanged) as UICheckBox;
            UndoDisengagesLockCheckBox = group.AddCheckbox("Undo Disengages Lock", ModSettings.UndoDisengagesLock, OnUndoDisengagesLockCheckBoxChanged) as UICheckBox;

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

            UIHelperBase XMLGroup = helper.AddGroup("Advanced Options");
            AssetCreatorModeCheckBox = XMLGroup.AddCheckbox("Asset Creator Mode", ModSettings.AssetCreatorMode, OnAssetCreatorModeCheckBoxChanged) as UICheckBox;

            FolderPathTextField = XMLGroup.AddTextfield("Folder Path", ModSettings.FolderPath, OnFolderPathChanged) as UITextField;
            FolderPathTextField.width += 475;

            ExportXMLButton = XMLGroup.AddButton("Export PSA/PLR properties", ExportXML) as UIButton;
            ExportErrorTextField = XMLGroup.AddTextfield("Export Errors", "", OnErrorTextChanged) as UITextField;
            ExportErrorTextField.width += 250;
            ExportErrorTextField.isInteractive = false;

            ImportXMLButton = XMLGroup.AddButton("Import PSA/PLR properties", ImportXML) as UIButton;
            ImportErrorTextField = XMLGroup.AddTextfield("Import Errors", "", OnErrorTextChanged) as UITextField;
            ImportErrorTextField.width += 250;
            ImportErrorTextField.isInteractive = false;

            ImportXMLOnGameStartCheckBox = XMLGroup.AddCheckbox("Import On Game Start", ModSettings.ImportXMLonStartUp, OnImportXMLonStartUpCheckBoxChanged) as UICheckBox; //Need to implement on startup routine.
            
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
        private void OnImportXMLonStartUpCheckBoxChanged(bool val)
        {
            ModSettings.ImportXMLonStartUp = (bool)val;
        }
        private void OnUndoDisengagesLockCheckBoxChanged(bool val)
        {
            ModSettings.UndoDisengagesLock = (bool)val;
        }
        private void OnOverrideOverlappingCheckBox(bool val)
        {
            ModSettings.OverrideOverlapping = (bool)val;
        }
        private void OnBulldozeEffectCheckBoxChanged(bool val)
        {
            ModSettings.BulldozeEffect = (bool)val;
        }
        private void OnAssetCreatorModeCheckBoxChanged(bool val)
        {
            ModSettings.AssetCreatorMode = (bool)val;
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
        private void OnFolderPathChanged(string text)
        {
            ModSettings.FolderPath = text;
        }
        private void resetSettings()
        {

            ModSettings.resetModSettings();
            PassElectricityCheckBox.isChecked = ModSettings.PassElectricity;
            AllowLockingCheckBox.isChecked = ModSettings.AllowLocking;
            AllowSnappingCheckBox.isChecked = ModSettings.AllowSnapping;
            OverrideOverlappingCheckBox.isChecked = ModSettings.OverrideOverlapping;
            BulldozeEffectCheckBox.isChecked = ModSettings.BulldozeEffect;
            SnappingDistanceSlider.value = ModSettings.SnappingDistance;
            UnlockDistanceSlider.value = ModSettings.UnlockingDistance;
            KeyboardShortcutsHintsCheckBox.isChecked = ModSettings.KeyboardShortcutHints;
            OverrideWarningCheckBox.isChecked = ModSettings.OverrideWarning;
            UndoDisengagesLockCheckBox.isChecked = ModSettings.UndoDisengagesLock;
            DisableTooltipsCheckBox.isChecked = ModSettings.DisableTooltips;
            UnlockSnappingKeybindButton.text = ModSettings.UnlockKeybind.ToLocalizedString("KEYNAME");
            ReturnLastSnappingKeybindButton.text = ModSettings.ReturnLockKeybind.ToLocalizedString("KEYNAME");
            UndoKeybindButton.text = ModSettings.UndoKeybind.ToLocalizedString("KEYNAME");
            LockOnToPSAKeybindButton.text = ModSettings.LockOnToPSAKeybind.ToLocalizedString("KEYNAME");
            AssetCreatorModeCheckBox.isChecked = ModSettings.AssetCreatorMode;
            FolderPathTextField.text = ModSettings.FolderPath;
            ImportXMLOnGameStartCheckBox.isChecked = ModSettings.ImportXMLonStartUp;
        }
        private static void resetSnappingGrid()
        {
            if (ParkingSpaceGrid.areYouAwake() == true)
            {
                ParkingSpaceGrid.Awake();
            }
        }
        private static void deleteAllAssets()
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
                        else
                        {
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
        private static void ExportXML()
        {
            ExportErrorTextField.text = ModSettings.SerializeXML();
        }
        private static void ImportXML()
        {
            ImportErrorTextField.text = ModSettings.DeserializeXML();
        }

        private void OnErrorTextChanged(string text)
        {

        }

    }
}
