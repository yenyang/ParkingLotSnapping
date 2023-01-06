using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ColossalFramework;
using System.Xml;
using System.Xml.Serialization;
using static ColossalFramework.Threading.ContextSwitch;
using System.IO;
using Epic.OnlineServices.Presence;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace ParkingLotSnapping
{
    public static class ModSettings
    {
        // KEYBIND: key initialization section
        // As part of the settings, I define the Keybinds
        // Keybind is part of Colossal's "Savedinputkey" class
        // Which is immediately saved and loaded from a file when modified (if file exists), no need to do it ourselves
        // Next initialization will have the saved value instead of the old one
        // To initializ: Binding Name Reference, Filename, Default initial value and AutoUpdate = True
        // After that, we just need to use them in the mod with SavedInputKey.IsPressed()
        // Change bindings by editing SavedInputKey.value (InputKey class)
        public const string settingsFileName = "ParkingLotSnapping"; // File to save to
        private static InputKey UnlockDefaultKey = SavedInputKey.Encode(KeyCode.X, control: true, shift: false, alt: false);
        public static readonly SavedInputKey UnlockKeybind = new SavedInputKey("unlockKeybind", settingsFileName, UnlockDefaultKey, autoUpdate: true);
        private static InputKey ReturnLockDefaultKey = SavedInputKey.Encode(KeyCode.R, control: true, shift: false, alt: false);
        public static readonly SavedInputKey ReturnLockKeybind = new SavedInputKey("returnLockKeybind", settingsFileName, ReturnLockDefaultKey, autoUpdate: true);
        private static InputKey UndoDefaultKey = SavedInputKey.Encode(KeyCode.Z, control: true, shift: false, alt: false);
        public static readonly SavedInputKey UndoKeybind = new SavedInputKey("undoKeybind", settingsFileName, UndoDefaultKey, autoUpdate: true);
        private static InputKey LockOnToPSADefaultKey = SavedInputKey.Encode(KeyCode.G, control: false, shift: false, alt: false);
        public static readonly SavedInputKey LockOnToPSAKeybind = new SavedInputKey("LockToPSAKeybind", settingsFileName, LockOnToPSADefaultKey, autoUpdate: true);

        private static bool _PassElectricity;
        private static int? _PassElectricityInt;
        public static bool PassElectricity
        {
            get
            {
                if (_PassElectricityInt == null)
                {
                    _PassElectricityInt = PlayerPrefs.GetInt("PLS_PassElectricity", 1);
                }
                if (_PassElectricityInt == 1)
                {
                    _PassElectricity = true;
                } else
                {
                    _PassElectricity = false;
                }
                return _PassElectricity;
            }
            set
            {
                if (value == true)
                {
                    _PassElectricityInt = 1;
                } else
                {
                    _PassElectricityInt = 0;
                }
                PlayerPrefs.SetInt("PLS_PassElectricity", (int)_PassElectricityInt);


            }
        }
       

        private static bool _AllowSnapping;
        private static int? _AllowSnappingInt;
        public static bool AllowSnapping
        {
            get
            {
                if (_AllowSnappingInt == null)
                {
                    _AllowSnappingInt = PlayerPrefs.GetInt("PLS_AllowSnapping", 1);
                }
                if (_AllowSnappingInt == 1)
                {
                    _AllowSnapping = true;
                }
                else
                {
                    _AllowSnapping = false;
                }
                return _AllowSnapping;
            }
            set
            {
                if (value == true)
                {
                    _AllowSnappingInt = 1;
                }
                else
                {
                    _AllowSnappingInt = 0;
                }
                PlayerPrefs.SetInt("PLS_AllowSnapping", (int)_AllowSnappingInt);


            }
        }
        private static bool _AllowLocking;
        private static int? _AllowLockingInt;
        public static bool AllowLocking
        {
            get
            {
                if (_AllowLockingInt == null)
                {
                    _AllowLockingInt = PlayerPrefs.GetInt("PLS_AllowLocking", 1);
                }
                if (_AllowLockingInt == 1)
                {
                    _AllowLocking = true;
                }
                else
                {
                    _AllowLocking = false;
                }
                return _AllowLocking;
            }
            set
            {
                if (value == true)
                {
                    _AllowLockingInt = 1;
                }
                else
                {
                    _AllowLockingInt = 0;
                }
                PlayerPrefs.SetInt("PLS_AllowLocking", (int)_AllowLockingInt);


            }
        }
        private static bool _OverrideOverlapping;
        private static int? _OverrideOverlappingInt;
        public static bool OverrideOverlapping
        {
            get
            {
                if (_OverrideOverlappingInt == null)
                {
                    _OverrideOverlappingInt = PlayerPrefs.GetInt("PLS_OverrideOverlapping", 1);
                }
                if (_OverrideOverlappingInt == 1)
                {
                    _OverrideOverlapping = true;
                }
                else
                {
                    _OverrideOverlapping = false;
                }
                return _OverrideOverlapping;
            }
            set
            {
                if (value == true)
                {
                    _OverrideOverlappingInt = 1;
                }
                else
                {
                    _OverrideOverlappingInt = 0;
                }
                PlayerPrefs.SetInt("PLS_OverrideOverlapping", (int)_OverrideOverlappingInt);


            }
        }

        private static bool _UndoDisengagesLock;
        private static int? _UndoDisengagesLockInt;
        public static bool UndoDisengagesLock
        {
            get
            {
                if (_UndoDisengagesLockInt == null)
                {
                    _UndoDisengagesLockInt = PlayerPrefs.GetInt("PLS_UndoDisengagesLock", 1);
                }
                if (_UndoDisengagesLockInt == 1)
                {
                    _UndoDisengagesLock = true;
                }
                else
                {
                    _UndoDisengagesLock = false;
                }
                return _UndoDisengagesLock;
            }
            set
            {
                if (value == true)
                {
                    _UndoDisengagesLockInt = 1;
                }
                else
                {
                    _UndoDisengagesLockInt = 0;
                }
                PlayerPrefs.SetInt("PLS_UndoDisengagesLock", (int)_UndoDisengagesLockInt);


            }
        }

        private static int? _snappingDistance;
        public const float _maxSnappingDistance = 110f;
        public const float _minSnappingDistance = 10f;
        public const float _snappingStep = 10f;
        public const float _defaultSnappingDistance = 20f;

        public static int SnappingDistance
        {
            get
            {
                if (!_snappingDistance.HasValue)
                {
                    _snappingDistance = PlayerPrefs.GetInt("PLS_SnappingDistance", (int)_defaultSnappingDistance);
                }
                return _snappingDistance.Value;
            }
            set
            {
                if (value > _maxSnappingDistance || value < _minSnappingDistance)
                    throw new ArgumentOutOfRangeException();
                if (value == _snappingDistance)
                {
                    return;
                }
                PlayerPrefs.SetInt("PLS_SnappingDistance", value);
                _snappingDistance = value;
            }
        }
        private static int? _unlockingDistance;
        public const float _maxUnlockingDistance = 220f;
        public const float _minUnlockingDistance = 20f;
        public const float _unlockingStep = 20f;
        public const float _defaultUnlockingDistance = 100f;

        public static int UnlockingDistance
        {
            get
            {
                if (!_unlockingDistance.HasValue)
                {
                    _unlockingDistance = PlayerPrefs.GetInt("PLS_UnlockingDistance", (int)_defaultUnlockingDistance);
                }
                return _unlockingDistance.Value;
            }
            set
            {
                if (value > _maxUnlockingDistance || value < _minUnlockingDistance)
                    throw new ArgumentOutOfRangeException();
                if (value == _unlockingDistance)
                {
                    return;
                }
                PlayerPrefs.SetInt("PLS_UnlockingDistance", value);
                _unlockingDistance = value;
            }
        }

        private static bool _KeyboardShortcutHints;
        private static int? _KeyboardShortcutHintsInt;
        public static bool KeyboardShortcutHints
        {
            get
            {
                if (_KeyboardShortcutHintsInt == null)
                {
                    _KeyboardShortcutHintsInt = PlayerPrefs.GetInt("PLS_KeyboardShortcutHints", 1);
                }
                if (_KeyboardShortcutHintsInt == 1)
                {
                    _KeyboardShortcutHints = true;
                }
                else
                {
                    _KeyboardShortcutHints = false;
                }
                return _KeyboardShortcutHints;
            }
            set
            {
                if (value == true)
                {
                    _KeyboardShortcutHintsInt = 1;
                }
                else
                {
                    _KeyboardShortcutHintsInt = 0;
                }
                PlayerPrefs.SetInt("PLS_KeyboardShortcutHints", (int)_KeyboardShortcutHintsInt);


            }
        }
        private static bool _OverrideWarning;
        private static int? _OverrideWarningInt;
        public static bool OverrideWarning
        {
            get
            {
                if (_OverrideWarningInt == null)
                {
                    _OverrideWarningInt = PlayerPrefs.GetInt("PLS_OverrideWarning", 1);
                }
                if (_OverrideWarningInt == 1)
                {
                    _OverrideWarning = true;
                }
                else
                {
                    _OverrideWarning = false;
                }
                return _OverrideWarning;
            }
            set
            {
                if (value == true)
                {
                    _OverrideWarningInt = 1;
                }
                else
                {
                    _OverrideWarningInt = 0;
                }
                PlayerPrefs.SetInt("PLS_OverrideWarning", (int)_OverrideWarningInt);


            }
        }
        private static bool _DisableTooltips;
        private static int? _DisableTooltipsInt;
        public static bool DisableTooltips
        {
            get
            {
                if (_DisableTooltipsInt == null)
                {
                    _DisableTooltipsInt = PlayerPrefs.GetInt("PLS_DisableTooltips", 0);
                }
                if (_DisableTooltipsInt == 1)
                {
                    _DisableTooltips = true;
                }
                else
                {
                    _DisableTooltips = false;
                }
                return _DisableTooltips;
            }
            set
            {
                if (value == true)
                {
                    _DisableTooltipsInt = 1;
                }
                else
                {
                    _DisableTooltipsInt = 0;
                }
                PlayerPrefs.SetInt("PLS_DisableTooltips", (int)_DisableTooltipsInt);


            }
        }

        private static bool _BulldozeEffect;
        private static int? _BulldozeEffectInt;
        public static bool BulldozeEffect
        {
            get
            {
                if (_BulldozeEffectInt == null)
                {
                    _BulldozeEffectInt = PlayerPrefs.GetInt("PLS_BulldozeEffect", 0);
                }
                if (_BulldozeEffectInt == 1)
                {
                    _BulldozeEffect = true;
                }
                else
                {
                    _BulldozeEffect = false;
                }
                return _BulldozeEffect;
            }
            set
            {
                _BulldozeEffectInt = value ? 1 : 0;
                
                PlayerPrefs.SetInt("PLS_BulldozeEffect", (int)_BulldozeEffectInt);


            }
        }

        public static void resetModSettings()
        {

            ModSettings.PassElectricity = true;
            ModSettings.AllowLocking = true;
            ModSettings.AllowSnapping = true;
            ModSettings.OverrideOverlapping = true;
            ModSettings.UnlockingDistance = (int)ModSettings._defaultUnlockingDistance;
            ModSettings.SnappingDistance = (int)ModSettings._defaultSnappingDistance;
            ModSettings.DisableTooltips = false;
            ModSettings.KeyboardShortcutHints = true;
            ModSettings.OverrideWarning = true;
            ModSettings.BulldozeEffect = true;
            ModSettings.UnlockKeybind.value = UnlockDefaultKey;
            ModSettings.ReturnLockKeybind.value = ReturnLockDefaultKey;
            ModSettings.UndoKeybind.value = UndoDefaultKey;
            ModSettings.LockOnToPSAKeybind.value = LockOnToPSADefaultKey;
            ModSettings.UndoDisengagesLock = true;
            ModSettings.AssetCreatorMode = false;
            ModSettings.FolderPath = ModSettings._defaultFolderPath;
            ModSettings.ImportXMLonStartUp = false;
        }

        public class PSACustomPropertiesClass
        {
            private float m_offset;
            private float m_parkingWidth;
            private bool m_parallel;
            private float m_rotation;
            private bool m_raisable;
            private string m_name;
            private string m_filename;

            public PSACustomPropertiesClass() { } // Need in order to make de-serializer work
            public PSACustomPropertiesClass(float offset, float parkingWidth, bool parallel, float rotation, bool raisable, string name, string filename)
            {
                this.m_offset = offset;
                this.m_parkingWidth = parkingWidth;
                this.m_parallel = parallel;
                this.m_rotation = rotation;
                this.m_raisable = raisable;
                this.m_name = name;
                this.m_filename = filename;
            }

            
            
            public float Offset
            {
                get { return this.m_offset; }
                set { this.m_offset = value; }
            }


            public float ParkingWidth
            {
                get { return this.m_parkingWidth; }
                set { this.m_parkingWidth = value; }
            }
            public bool Parallel
            {
                get { return this.m_parallel; }
                set { this.m_parallel = value; }
            }
            public float Rotation
            {
                get { return this.m_rotation; }
                set { this.m_rotation = value; }
            }
            public bool Raisable
            { 
                get { return this.m_raisable; } 
                set { this.m_raisable = value; }
            }
            public string Name
            {
                get { return this.m_name; }
                set { this.m_name = value; }
            }
            public string Filename
            {
                get { return this.m_filename; }
                set { this.m_filename = value; }
            }

        }
        public static readonly float perpendicularAsymmetricOffset = 5.1f;
        public static readonly float perpendicularAsymmetricOffsetPLR2 = 6.05f;
        public static readonly float perpendicularAsymmetricOffsetRCP  = 4f;
        public static readonly float angledAsymmetricOffset = 6.6f;
        public static readonly float angledAsymmetricOffsetPLR2 = 6.05f;
        public static readonly float parallelAsymmetricOffset = 4.5f;
        public static Dictionary<string, PSACustomPropertiesClass> PSACustomProperties = new Dictionary<string, PSACustomPropertiesClass>()
        {
            {"1285201733.1x1 Electric Vehicle Parking_Data",        new PSACustomPropertiesClass(perpendicularAsymmetricOffset,         7.72f,      false,  0f,      false  , "1285201733.1x1 Electric Vehicle Parking_Data",    "1285201733-1x1 Electric Vehicle Parking_Data") },
            {"1285201733.1x1 Accessible Parking_Data",              new PSACustomPropertiesClass(perpendicularAsymmetricOffset,         9.07f,      false,  0f,      false  , "1285201733.1x1 Accessible Parking_Data",          "1285201733-1x1 Accessible Parking_Data") },
            {"1285201733.1x1 Parking Lot_Data",                     new PSACustomPropertiesClass(perpendicularAsymmetricOffset,         7.72f,      false,  0f,      false  , "1285201733.1x1 Parking Lot_Data",                 "1285201733-1x1 Parking Lot_Data") },
            {"1285201733.1x1 Single Space Parking_Data",            new PSACustomPropertiesClass(perpendicularAsymmetricOffset,         2.58f,      false,  0f,      false  , "1285201733.1x1 Single Space Parking_Data",        "1285201733-1x1 Single Space Parking_Data") },
            {"1285201733.3x2 Parking Lot_Data",                     new PSACustomPropertiesClass(0f,                                   23.14f,     false,  0f,      false  , "1285201733.3x2 Parking Lot_Data",                 "1285201733-3x2 Parking Lot_Data") },
            {"1285201733.4x1 Parking Row_Data",                     new PSACustomPropertiesClass(perpendicularAsymmetricOffset,         33.46f,     false,  0f,      false  , "1285201733.4x1 Parking Row_Data",                 "1285201733-4x1 Parking Row_Data") },
            {"1285201733.4x2 Parking Lot_Data",                     new PSACustomPropertiesClass(0f,                                   33.46f,     false,  0f,      false  , "1285201733.4x2 Parking Lot_Data",                 "1285201733-4x2 Parking Lot_Data") },
            {"1285201733.8x2 Parking Lot_Data",                     new PSACustomPropertiesClass(0f,                                   59.14f,     false,  0f,      false ,  "1285201733.8x2 Parking Lot_Data",                 "1285201733-8x2 Parking Lot_Data") },
            {"1293869603.1x1 Parking Lot - LHD_Data",               new PSACustomPropertiesClass(angledAsymmetricOffset,                5.72f,      false,  0f,      false ,  "1293869603.1x1 Parking Lot - LHD_Data",           "1293869603-1x1 Parking Lot - LHD_Data") },
            {"1293869603.1x1 End Side Left - LHD_Data",             new PSACustomPropertiesClass(angledAsymmetricOffset,                3.25f,      false,  0f,      false ,  "1293869603.1x1 End Side Left - LHD_Data",         "1293869603-1x1 End Side Left - LHD_Data") },
            {"1293869603.1x1 Parking Right End - LHD_Data",         new PSACustomPropertiesClass(angledAsymmetricOffset,                3.25f,      false,  0f,      false ,  "1293869603.1x1 Parking Right End - LHD_Data",     "1293869603-1x1 Parking Right End - LHD_Data") },
            {"1293869603.1x1 Accessible Parking - LHD_Data",        new PSACustomPropertiesClass(angledAsymmetricOffset,                7.28f,      false,  0f,      false ,  "1293869603.1x1 Accessible Parking - LHD_Data",    "1293869603-1x1 Accessible Parking - LHD_Data") },
            {"1293869603.1x1 Single Parking Space - LHD_Data",      new PSACustomPropertiesClass(angledAsymmetricOffset,                2.89f,      false,  0f,      false  , "1293869603.1x1 Single Parking Space - LHD_Data",  "1293869603-1x1 Single Parking Space - LHD_Data") },
            {"1293869603.3x2 Parking Lot 60 deg - LHD_Data",        new PSACustomPropertiesClass(0f,                                   20.05f,     false,  0f,      false ,  "1293869603.3x2 Parking Lot 60 deg - LHD_Data",    "1293869603-3x2 Parking Lot 60 deg - LHD_Data") },
            {"1293869603.4x1 Parking Lot 60 Degree - LHD_Data",     new PSACustomPropertiesClass(angledAsymmetricOffset,                34.39f,     false,  0f,      false ,  "1293869603.4x1 Parking Lot 60 Degree - LHD_Data", "1293869603-4x1 Parking Lot 60 Degree - LHD_Data") },
            {"1293870311.1x1 Parking Lot - RHD_Data",               new PSACustomPropertiesClass(angledAsymmetricOffset,                5.72f,      false,  0f,      false  , "1293870311.1x1 Parking Lot - RHD_Data",           "1293870311-1x1 Parking Lot - RHD_Data") },
            {"1293870311.1x1 End Side Left - RHD_Data",             new PSACustomPropertiesClass(angledAsymmetricOffset,                3.25f,      false,  0f,      false  , "1293870311.1x1 End Side Left - RHD_Data",         "1293870311-1x1 End Side Left - RHD_Data") },
            {"1293870311.1x1 Parking Right End - RHD_Data",         new PSACustomPropertiesClass(angledAsymmetricOffset,                3.25f,      false,  0f,      false  , "1293870311.1x1 Parking Right End - RHD_Data",     "1293870311-1x1 Parking Right End - RHD_Data") },
            {"1293870311.1x1 Accessible Parking - RHD_Data",        new PSACustomPropertiesClass(angledAsymmetricOffset,                7.28f,      false,  0f,      false  , "1293870311.1x1 Accessible Parking - RHD_Data",    "1293870311-1x1 Accessible Parking - RHD_Data") },
            {"1293870311.1x1 Single Parking Space - RHD_Data",      new PSACustomPropertiesClass(angledAsymmetricOffset,                2.89f,      false,  0f,      false  , "1293870311.1x1 Single Parking Space - RHD_Data",  "1293870311-1x1 Single Parking Space - RHD_Data") },
            {"1293870311.3x2 Parking Lot 60 degree - RHD_Data",     new PSACustomPropertiesClass(0f,                                   20.05f,     false,  0f,      false ,  "1293870311.3x2 Parking Lot 60 degree - RHD_Data", "1293870311-3x2 Parking Lot 60 degree - RHD_Data") },
            {"1293870311.4x1 Parking Lot 60 Degree - RHD_Data",     new PSACustomPropertiesClass(angledAsymmetricOffset,                34.39f,     false,  0f,      false ,  "1293870311.4x1 Parking Lot 60 Degree - RHD_Data", "1293870311-4x1 Parking Lot 60 Degree - RHD_Data") },
            {"1969111282.2 Spots with Planters (Mirrored)_Data",    new PSACustomPropertiesClass(parallelAsymmetricOffset,              24.43f,     true,   90f,     false , "1969111282.2 Spots with Planters (Mirrored)_Data","1969111282-2 Spots with Planters (Mirrored)_Data" )},
            {"1969111282.4 Spots with Planters (Mirrored)_Data",    new PSACustomPropertiesClass(parallelAsymmetricOffset,              36.80f,     true,   90f,     false , "1969111282.4 Spots with Planters (Mirrored)_Data","1969111282-4 Spots with Planters (Mirrored)_Data")},
            {"1969111282.6 Spots with Planters (Mirrored)_Data",    new PSACustomPropertiesClass(parallelAsymmetricOffset,              50.22f,     true,   90f,     false , "1969111282.6 Spots with Planters (Mirrored)_Data","1969111282-6 Spots with Planters (Mirrored)_Data")},
            {"1969111282.8 Spots with Planters (Mirrored)_Data",    new PSACustomPropertiesClass(parallelAsymmetricOffset,              62.66f,     true,   90f,     false , "1969111282.8 Spots with Planters (Mirrored)_Data","1969111282-8 Spots with Planters (Mirrored)_Data")},
            {"1969111282.10 Spts wth Plntrs EC (Mirrored)_Data",    new PSACustomPropertiesClass(parallelAsymmetricOffset,              79.85f,     true,   90f,     false , "1969111282.10 Spts wth Plntrs EC (Mirrored)_Data","1969111282-10 Spts wth Plntrs EC (Mirrored)_Data")},
            {"1969111282.10 Spts wth Planters (Mirrored)_Data",     new PSACustomPropertiesClass(parallelAsymmetricOffset,              75.03f,     true,   90f,     false , "1969111282.10 Spts wth Planters (Mirrored)_Data", "1969111282-10 Spts wth Planters (Mirrored)_Data")},
            {"1969111282.PLR II - 2 Spots_Data",                    new PSACustomPropertiesClass(parallelAsymmetricOffset,              13.43f,     true,   90f,     false , "1969111282.PLR II - 2 Spots_Data",                "1969111282-PLR II - 2 Spots_Data")},
            {"1969111282.PLR II - 2 Spots (Mirrored)_Data",         new PSACustomPropertiesClass(parallelAsymmetricOffset,              13.43f,     true,   90f,     false , "1969111282.PLR II - 2 Spots (Mirrored)_Data",     "1969111282-PLR II - 2 Spots (Mirrored)_Data")},
            {"1969111282.PLR II - 2 Spots with Planters_Data",      new PSACustomPropertiesClass(parallelAsymmetricOffset,              24.43f,     true,   90f,     false , "1969111282.PLR II - 2 Spots with Planters_Data",  "1969111282-PLR II - 2 Spots with Planters_Data")},
            {"1969111282.PLR II - 4 Spots _Data",                   new PSACustomPropertiesClass(parallelAsymmetricOffset,              25.80f,     true,   90f,     false , "1969111282.PLR II - 4 Spots _Data",               "1969111282-PLR II - 4 Spots _Data")},
            {"1969111282.PLR II - 4 Spots (Mirrored)_Data",         new PSACustomPropertiesClass(parallelAsymmetricOffset,              25.80f,     true,   90f,     false , "1969111282.PLR II - 4 Spots (Mirrored)_Data",     "1969111282-PLR II - 4 Spots (Mirrored)_Data")},
            {"1969111282.PLR II - 4 Spots with Planters_Data",      new PSACustomPropertiesClass(parallelAsymmetricOffset,              36.80f,     true,   90f,     false , "1969111282.PLR II - 4 Spots with Planters_Data",  "1969111282-PLR II - 4 Spots with Planters_Data")},
            {"1969111282.PLR II - 6 Spots with Planters_Data",      new PSACustomPropertiesClass(parallelAsymmetricOffset,              50.22f,     true,   90f,     false , "1969111282.PLR II - 6 Spots with Planters_Data",  "1969111282-PLR II - 6 Spots with Planters_Data")},
            {"1969111282.PLR II - 8 Spots_Data",                    new PSACustomPropertiesClass(parallelAsymmetricOffset,              50.65f,     true,   90f,     false , "1969111282.PLR II - 8 Spots_Data",                "1969111282-PLR II - 8 Spots_Data")},
            {"1969111282.PLR II - 8 Spots (mirrored)_Data",         new PSACustomPropertiesClass(parallelAsymmetricOffset,              50.65f,     true,   90f,     false , "1969111282.PLR II - 8 Spots (mirrored)_Data",     "1969111282-PLR II - 8 Spots (mirrored)_Data")},
            {"1969111282.PLR II - 8 Spots with Planters_Data",      new PSACustomPropertiesClass(parallelAsymmetricOffset,              62.66f,     true,   90f,     false , "1969111282.PLR II - 8 Spots with Planters_Data",  "1969111282-PLR II - 8 Spots with Planters_Data")},
            {"1969111282.PLR II - 10 Spots with Planters_Data",     new PSACustomPropertiesClass(parallelAsymmetricOffset,              79.85f,     true,   90f,     false , "1969111282.PLR II - 10 Spots with Planters_Data", "1969111282-PLR II - 10 Spots with Planters_Data")},
            {"1969111282.PLR II - 10 Spts wth Plntrs NEC_Data",     new PSACustomPropertiesClass(parallelAsymmetricOffset,              75.06f,     true,   90f,     false , "1969111282.PLR II - 10 Spts wth Plntrs NEC_Data", "1969111282-PLR II - 10 Spts wth Plntrs NEC_Data")},
            {"1969111282.PLR II - 60° Planters #03_Data",           new PSACustomPropertiesClass(angledAsymmetricOffsetPLR2,            12.05f,     false,  270f,    false , "1969111282.PLR II - 60° Planters #03_Data",       "1969111282-PLR II - 60° Planters #03_Data")},
            {"1969111282.PLR II - 60° Planters #04_Data",           new PSACustomPropertiesClass(angledAsymmetricOffsetPLR2,            24.03f,     false,  270f,    false , "1969111282.PLR II - 60° Planters #04_Data",       "1969111282-PLR II - 60° Planters #04_Data")},
            {"1969111282.PLR II - 60° Planters #02_Data",           new PSACustomPropertiesClass(angledAsymmetricOffsetPLR2,            33.92f,     false,  270f,    false , "1969111282.PLR II - 60° Planters #02_Data",       "1969111282-PLR II - 60° Planters #02_Data")},
            {"1969111282.PLR II - 60° Plain #01_Data",              new PSACustomPropertiesClass(angledAsymmetricOffsetPLR2,            27.50f,     false,  270f,    false , "1969111282.PLR II - 60° Plain #01_Data",          "1969111282-PLR II - 60° Plain #01_Data")},
            {"1969111282.PLR II - 90° Planters #01_Data",           new PSACustomPropertiesClass(perpendicularAsymmetricOffsetPLR2,     33.25f,     false,  270f,    false , "1969111282.PLR II - 90° Planters #01_Data",       "1969111282-PLR II - 90° Planters #01_Data")},
            {"1969111282.PLR II - 90° - Planters #02_Data",         new PSACustomPropertiesClass(perpendicularAsymmetricOffsetPLR2,     61.28f,     false,  270f,    false , "1969111282.PLR II - 90° - Planters #02_Data",     "1969111282-PLR II - 90° - Planters #02_Data")},
            {"1969111282.PLR II - 90° Planters #03_Data",           new PSACustomPropertiesClass(perpendicularAsymmetricOffsetPLR2,     11.22f,     false,  270f,    false , "1969111282.PLR II - 90° Planters #03_Data",       "1969111282-PLR II - 90° Planters #03_Data")},
            {"1969111282.PLR II - 90° Planters #04_Data",           new PSACustomPropertiesClass(perpendicularAsymmetricOffsetPLR2,     22.44f,     false,  270f,    false , "1969111282.PLR II - 90° Planters #04_Data",       "1969111282-PLR II - 90° Planters #04_Data")},
            {"1385628544.Raisable Parking 1x1_Data",                new PSACustomPropertiesClass(3.9f,                                  7.72f,      false,  0f,      true  ,  "1385628544.Raisable Parking 1x1_Data",            "1385628544-Raisable Parking 1x1_Data") },
            {"2174647682.Raisable Parking Access 1x1 LHS_Data",     new PSACustomPropertiesClass(perpendicularAsymmetricOffsetRCP,      5.19f,      false,  0f,      true  ,  "2174647682.Raisable Parking Access 1x1 LHS_Data", "2174647682-Raisable Parking Access 1x1 LHS_Data") },
            {"2174647682.Raisable Parking Access 1x1 RHS_Data",     new PSACustomPropertiesClass(perpendicularAsymmetricOffsetRCP,      5.19f,      false,  0f,      true  ,  "2174647682.Raisable Parking Access 1x1 RHS_Data", "2174647682-Raisable Parking Access 1x1 RHS_Data"  ) },
            {"2174647682.Raisable Parking 1x1_Data",                new PSACustomPropertiesClass(3.9f,                                 2.66f,      false,  0f,      true   , "2174647682.Raisable Parking 1x1_Data",            "2174647682-Raisable Parking 1x1_Data") },
            {"2174647682.Raisable Parking 1x1 Corner_Data",         new PSACustomPropertiesClass(3.8f,                                 7.91f,      false,  0f,      true    , "2174647682.Raisable Parking 1x1 Corner_Data",    "2174647682-Raisable Parking 1x1 Corner_Data") },
            {"2174647682.Raisable Parking EE 1x1 LHS_Data",         new PSACustomPropertiesClass(perpendicularAsymmetricOffsetRCP,      7.85f,      false,  0f,      true    , "2174647682.Raisable Parking EE 1x1 LHS_Data",    "2174647682-Raisable Parking EE 1x1 LHS_Data") },
            {"2174647682.Raisable Parking EE 1x1 RHS_Data",         new PSACustomPropertiesClass(perpendicularAsymmetricOffsetRCP,      7.80f,      false,  0f,      true   , "2174647682.Raisable Parking EE 1x1 RHS_Data",     "2174647682-Raisable Parking EE 1x1 RHS_Data" ) },
            {"2174647682.Raisable Parking Divider 1x3_Data",        new PSACustomPropertiesClass(perpendicularAsymmetricOffsetRCP,      7.86f,      false,  0f,      true   , "2174647682.Raisable Parking Divider 1x3_Data",    "2174647682-Raisable Parking Divider 1x3_Data" ) },
            {"2174647682.Raisable Parking 1x3_Data",                new PSACustomPropertiesClass(perpendicularAsymmetricOffsetRCP,      7.86f,      false,  0f,      true   , "2174647682.Raisable Parking 1x3_Data",            "2174647682-Raisable Parking 1x3_Data" ) },
            {"2174647682.Raisable Parking 4x1_Data",                new PSACustomPropertiesClass(perpendicularAsymmetricOffsetRCP,      31.5f,      false,  0f,      true   , "2174647682.Raisable Parking 4x1_Data",            "2174647682-Raisable Parking 4x1_Data" ) },
            {"2174647682.Raisable parking 4x2_Data",                new PSACustomPropertiesClass(9.4f,                                 31.5f,      false,  0f,      true   , "2174647682.Raisable parking 4x2_Data",            "2174647682-Raisable parking 4x2_Data" ) },
            {"2176019958.Raisable Parking A30 1x1_Data",            new PSACustomPropertiesClass(3.95f,                                2.66f,      false,  0f,      true,    "2176019958.Raisable Parking A30 1x1_Data",        "2176019958-Raisable Parking A30 1x1_Data") },
            {"2176019958.Raisable Parking A30 1x2_Data",            new PSACustomPropertiesClass(3.95f,                                5.37f,      false,  0f,      true,    "2176019958.Raisable Parking A30 1x2_Data",        "2176019958-Raisable Parking A30 1x2_Data") },
            {"2176019958.Raisable Parking A30 4x1_Data",            new PSACustomPropertiesClass(4.45f,                                21.85f,      false,  0f,      true,    "2176019958.Raisable Parking A30 4x1_Data",        "2176019958-Raisable Parking A30 4x1_Data") },
            {"2176019958.Raisable Parking A30 Access LHS_Data",     new PSACustomPropertiesClass(3.95f,                                5.33f,      false,  0f,      true,    "2176019958.Raisable Parking A30 Access LHS_Data", "2176019958-Raisable Parking A30 Access LHS_Data") },
            {"2176019958.Raisable Parking A30 Access RHS_Data",     new PSACustomPropertiesClass(3.95f,                                5.33f,      false,  0f,      true,    "2176019958.Raisable Parking A30 Access RHS_Data", "2176019958-Raisable Parking A30 Access RHS_Data") },
            {"2176019958.Raisable Parking A30 End LHS_Data",        new PSACustomPropertiesClass(3.95f,                                5.46f,      false,  0f,      true,    "2176019958.Raisable Parking A30 End LHS_Data",    "2176019958-Raisable Parking A30 End LHS_Data") },
            {"2176019958.Raisable Parking A30 End RHS_Data",        new PSACustomPropertiesClass(3.95f,                                5.46f,      false,  0f,      true,    "2176019958.Raisable Parking A30 End RHS_Data",    "2176019958-Raisable Parking A30 End RHS_Data") },
            {"2176019958.Raisable Parking A60 1x1_Data",            new PSACustomPropertiesClass(3.95f,                                2.66f,      false,  0f,      true,    "2176019958.Raisable Parking A60 1x1_Data",        "2176019958-Raisable Parking A60 1x1_Data") },
            {"2176019958.Raisable Parking A60 1x2_Data",            new PSACustomPropertiesClass(3.95f,                                5.37f,      false,  0f,      true,    "2176019958.Raisable Parking A60 1x2_Data",        "2176019958-Raisable Parking A60 1x2_Data") },
            {"2176019958.Raisable Parking A60 4x1_Data",            new PSACustomPropertiesClass(4.45f,                                21.85f,      false,  0f,      true,    "2176019958.Raisable Parking A60 4x1_Data",        "2176019958-Raisable Parking A60 4x1_Data") },
            {"2176019958.Raisable Parking A60 Access LHS_Data",     new PSACustomPropertiesClass(3.95f,                                5.33f,      false,  0f,      true,    "2176019958.Raisable Parking A60 Access LHS_Data", "2176019958-Raisable Parking A60 Access LHS_Data") },
            {"2176019958.Raisable Parking A60 Access RHS_Data",     new PSACustomPropertiesClass(3.95f,                                5.33f,      false,  0f,      true,    "2176019958.Raisable Parking A60 Access RHS_Data", "2176019958-Raisable Parking A60 Access RHS_Data") },
            {"2176019958.Raisable Parking A60 End LHS_Data",        new PSACustomPropertiesClass(3.95f,                                5.81f,      false,  0f,      true,    "2176019958.Raisable Parking A60 End LHS_Data",    "2176019958-Raisable Parking A60 End LHS_Data") },
            {"2176019958.Raisable Parking A60 End RHS_Data",        new PSACustomPropertiesClass(3.95f,                                5.81f,      false,  0f,      true,    "2176019958.Raisable Parking A60 End RHS_Data",    "2176019958-Raisable Parking A60 End RHS_Data") },

        };

        public static Dictionary<string, PLRCustomPropertiesClass> PLRCustomProperties = new Dictionary<string, PLRCustomPropertiesClass>()
        {
            {"1285230481.16m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 3f,-3f },         true,   true,   false,  "1285230481.16m Parking Lot_Data",                  "1285230481-16m Parking Lot_Data",                          0f) },
            {"1303766506.16m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 3f,-3f },         true,   true,   false,  "1303766506.16m Poorly Maintained Parking_Data",    "1303766506-16m Poorly Maintained Parking_Data",            0f) },
            {"1285201733.22m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "1285201733.22m Parking Lot_Data",                  "1285201733-22m Parking Lot_Data",                          0f) },
            {"1303772884.22m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "1303772884.22m Poorly Maintained Parking_Data",    "1303772884-22m Poorly Maintained Parking_Data",            0f) },
            {"1285201733.40m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {9f,-9f},            new List<float> { 9f, -9f },        false,  false,  false,  "1285201733.40m Parking Lot_Data",                  "1285201733-40m Parking Lot_Data",                          0f) },
            {"1303772884.40m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {9f,-9f},            new List<float> { 9f, -9f },        false,  false,  false,  "1303772884.40m Poorly Maintained Parking_Data",    "1303772884-40m Poorly Maintained Parking_Data",            0f) },
            {"1285201733.58m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {0f, 18f,-18f},      new List<float> { 0f,18f,-18f},     false,  false,  false,  "1285201733.58m Parking Lot_Data",                  "1285201733-58m Parking Lot_Data",                          0f) },
            {"1303772884.58m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {0f, 18f,-18f},      new List<float> { 0f,18f,-18f},     false,  false,  false,  "1303772884.58m Poorly Maintained Parking_Data",    "1303772884-58m Poorly Maintained Parking_Data",            0f) },
            {"1578348250.Tram Road #01_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 4.5f, -4.5f },    true,   false,  false,  "1578348250.Tram Road #01_Data",                    "1578348250-Tram Road #01_Data",                            0f) },
            {"1581742834.Tram Road #02_Data",                   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float>{ 0f },              false,  false,  false,  "1581742834.Tram Road #02_Data",                    "1581742834-Tram Road #02_Data",                            0f) },
            {"1423812793.60°/90° Parking Road 2L Urban_Data",   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 1.65f,-1.65f },   true,   false,  false,  "1423812793.60°/90° Parking Road 2L Urban_Data",    "1423812793-60 deg or 90 deg Parking Road 2L Urban_Data",   0f) },
            {"1426090117.60°/90° Parking Road 2L Suburban_Data",new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 1.65f, -1.65f },  true,   false,  false,  "1426090117.60°/90° Parking Road 2L Suburban_Data", "1426090117-60 deg or 90 deg Parking Road 2L Suburban_Data",0f) },
            {"1608297735.Tram Road#02 - 90° Parking Lots_Data", new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float>{ 0f },              false,  false,  false,  "1608297735.Tram Road#02 - 90° Parking Lots_Data",  "1608297735-Tram Road#02 - 90° Parking Lots_Data",          0f) },
            {"1608293777.Tram Road#01 - 90° Parking Lots_Data", new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 4.5f, -4.5f },    true,   false,  false,  "1608293777.Tram Road#01 - 90° Parking Lots_Data",  "1608293777-Tram Road#01 - 90° Parking Lots_Data",          0f) },
            {"2409968332.US 2L 2W Parking Asym Concr Tree_Data",new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409968332.US 2L 2W Parking Asym Concr Tree_Data", "2409968332-US 2L 2W Parking Asym Concr Tree_Data",         0f) },
            {"2409968332.US 2L 2W Parking Asym Concrete_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409968332.US 2L 2W Parking Asym Concrete_Data",   "2409968332-US 2L 2W Parking Asym Concrete_Data",           0f) },
            {"2409968332.US 2L 2W Parking Asym Red_Data",       new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409968332.US 2L 2W Parking Asym Red_Data",        "2409968332-US 2L 2W Parking Asym Red_Data",                0f) },
            {"2409968332.US 2L 2W Parking Asym Red Tree_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409968332.US 2L 2W Parking Asym Red Tree_Data",   "2409968332-US 2L 2W Parking Asym Red Tree_Data",           0f) },
            {"2409968332.US 2L 2W Parking Sym Concrete_Data",   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2409968332.US 2L 2W Parking Sym Concrete_Data",    "2409968332-US 2L 2W Parking Sym Concrete_Data",            0f) },
            {"2409968332.US 2L 2W Parking Sym Red_Data",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2409968332.US 2L 2W Parking Sym Red_Data",         "2409968332-US 2L 2W Parking Sym Red_Data",                 0f) },
            {"Parking Lot 01",                                  new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "Parking Lot 01",                                   "Parking Lot 01",                                           0f) },
            {"1969112396.PL - Road #01_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0f },             false,   false,  true,  "1969112396.PL - Road #01_Data",                    "1969112396-PL - Road #01_Data",                            0f) },
            {"1969113204.PL - Road #02_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0f },             false,   false,  true,  "1969113204.PL - Road #02_Data",                    "1969113204-PL - Road #02_Data",                            0f) },
            {"1971236400.PL - Road #03_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0f },             false,   false,  true,  "1971236400.PL - Road #03_Data",                    "1971236400-PL - Road #03_Data",                            0f) },
            {"2409941439.US 2L 2W Parking Asym Concr Tree_Data",new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409941439.US 2L 2W Parking Asym Concr Tree_Data", "2409941439-US 2L 2W Parking Asym Concr Tree_Data",         0f) },
            {"2409946015.US 2L 2W Parking Asym Concrete_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409946015.US 2L 2W Parking Asym Concrete_Data",   "2409946015-US 2L 2W Parking Asym Concrete_Data",           0f) },
            {"2409955382.US 2L 2W Parking Asym Red_Data",       new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409955382.US 2L 2W Parking Asym Red_Data",        "2409955382-US 2L 2W Parking Asym Red_Data",                0f) },
            {"2409958009.US 2L 2W Parking Asym Red Tree_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false,  "2409958009.US 2L 2W Parking Asym Red Tree_Data",   "2409958009-US 2L 2W Parking Asym Red Tree_Data",           0f) },
            {"2409960665.US 2L 2W Parking Sym Concrete_Data",   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2409960665.US 2L 2W Parking Sym Concrete_Data",    "2409960665-US 2L 2W Parking Sym Concrete_Data",            0f) },
            {"2409962948.US 2L 2W Parking Sym Red_Data",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2409962948.US 2L 2W Parking Sym Red_Data",         "2409962948-US 2L 2W Parking Sym Red_Data",                 0f) },
            {"2541224199.BIG Angled Parking Road 2 Lane_Data",  new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2541224199.BIG Angled Parking Road 2 Lane_Data",   "2541224199-BIG Angled Parking Road 2 Lane_Data",           0f) },
            {"2174913158.ms 20m garage elevator road_Data",     new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage elevator road_Data",      "2174913158-ms 20m garage elevator road_Data",             -0.25f) },
            {"2174913158.ms 20m garage inner road_Data",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage inner road_Data",         "2174913158-ms 20m garage inner road_Data",                -0.25f) },
            {"2174913158.ms 20m garage outer nopave road_Data", new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage outer nopave road_Data",  "2174913158-ms 20m garage outer nopave road_Data",         -0.25f) },
            {"2174913158.ms 20m garage outer road_Data",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage outer road_Data",         "2174913158-ms 20m garage outer road_Data",                -0.25f) },
            {"2174913158.ms 36m garage elevator road_Data",     new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m garage elevator road_Data",      "2174913158-ms 36m garage elevator road_Data",             -0.25f) },
            {"2174913158.ms 36m garage outer_Data",             new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m garage outer_Data",              "2174913158-ms 36m garage outer_Data",                     -0.25f) },
            {"2174913158.ms 36m inner garage road_Data",        new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m inner garage road_Data",         "2174913158-ms 36m inner garage road_Data",                -0.25f) },
            {"2174913158.ms edge garage road_Data",             new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms edge garage road_Data",              "2174913158-ms edge garage road_Data",                     -0.25f) },
            {"2174913158.ms 20m garage outer nopave road E",    new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage outer nopave road E",     "2174913158-ms 20m garage outer nopave road E",            -0.25f) },
            {"2174913158.ms 20m garage elevator road E",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage elevator road E",         "2174913158-ms 20m garage elevator road E",                -0.25f) },
            {"2174913158.ms 20m garage inner road E",           new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage inner road E",            "2174913158-ms 20m garage inner road E",                   -0.25f) },
            {"2174913158.ms 20m garage outer road E",           new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage outer road E",            "2174913158-ms 20m garage outer road E",                   -0.25f) },
            {"2174913158.ms 36m garage elevator road E",        new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m garage elevator road E",         "2174913158-ms 36m garage elevator road E",                -0.25f) },
            {"2174913158.ms 36m garage outer E",                new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m garage outer E",                 "2174913158-ms 36m garage outer E",                        -0.25f) },
            {"2174913158.ms 36m inner garage road E",           new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m inner garage road E",            "2174913158-ms 36m inner garage road E",                   -0.25f) },
            {"2174913158.ms edge garage road E",                new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms edge garage road E",                 "2174913158-ms edge garage road E",                        -0.25f) },
            {"2174913158.ms 20m garage outer nopave road B",    new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage outer nopave road B",     "2174913158-ms 20m garage outer nopave road B",            -0.25f) },
            {"2174913158.ms 20m garage elevator road B",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage elevator road B",         "2174913158-ms 20m garage elevator road B",                -0.25f) },
            {"2174913158.ms 20m garage inner road B",           new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage inner road B",            "2174913158-ms 20m garage inner road B",                   -0.25f) },
            {"2174913158.ms 20m garage outer road B",           new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms 20m garage outer road B",            "2174913158-ms 20m garage outer road B",                   -0.25f) },
            {"2174913158.ms 36m garage elevator road B",        new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m garage elevator road B",         "2174913158-ms 36m garage elevator road B",                -0.25f) },
            {"2174913158.ms 36m garage outer B",                new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m garage outer B",                 "2174913158-ms 36m garage outer B",                        -0.25f) },
            {"2174913158.ms 36m inner garage road B",           new PLRCustomPropertiesClass(new List<float>() {8f, -8f},           new List<float> {8f, -8f},          false,  false,  false,  "2174913158.ms 36m inner garage road B",            "2174913158-ms 36m inner garage road B",                   -0.25f) },
            {"2174913158.ms edge garage road B",                new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false,  "2174913158.ms edge garage road B",                 "2174913158-ms edge garage road B",                        -0.25f) },

        };


        public class PLRCustomPropertiesClass
        {
            private List<float> m_symmetricAisleOffsets;
            private List<float> m_asymmetricAisleOffsets;
            private bool m_onesided;
            private bool m_invertOffset;
            private bool m_parallel;
            private string m_name;
            private string m_filename;
            private float m_verticalOffset;
            public PLRCustomPropertiesClass() { } // Need in order to make de-serializer work
            public PLRCustomPropertiesClass(List<float> symmetricAisleOffsets, List<float> asymmetricAisleOffsets, bool onesided, bool invertOffset, bool parallel, string name, string filename, float verticalOffset)
            {
                this.m_symmetricAisleOffsets = symmetricAisleOffsets;
                this.m_asymmetricAisleOffsets = asymmetricAisleOffsets;
                this.m_onesided = onesided;
                this.m_invertOffset = invertOffset;
                this.m_parallel = parallel;
                this.m_name = name;
                this.m_filename = filename;
                this.m_verticalOffset = verticalOffset;
            }
            public List<float> SymmetricAisleOffsets
            {
                get { return this.m_symmetricAisleOffsets; }
                set { this.m_symmetricAisleOffsets = value; }
            }
            public List<float> AsymmetricAisleOffsets
            {
                get { return this.m_asymmetricAisleOffsets; }
                set { this.m_asymmetricAisleOffsets = value; }
            }
            public bool Onesided
            {
                get { return this.m_onesided; }
                set { this.m_onesided = value; }
            }

            public bool InvertOffset
            {
                get { return this.m_invertOffset; }
                set { this.m_invertOffset = value; }
            }

            public bool Parallel
            {
                get { return this.m_parallel; }
                set { this.m_parallel = value; }
            }
            public string Name
            {
                get { return this.m_name; }
                set { this.m_name = value; }
            }
            public string Filename
            {
                get { return this.m_filename; }
                set { this.m_filename = value; }
            }
            public float VerticalOffset
            {
                get { return this.m_verticalOffset; }
                set { this.m_verticalOffset = value; }
            }
        }

        private static bool _assetCreatorMode = false;
        private static int? _assetCreatorModeInt;
        public static bool AssetCreatorMode
        {
            get
            {
                if (_assetCreatorModeInt == null)
                {
                    _assetCreatorModeInt = PlayerPrefs.GetInt("PLS_AssetCreatorMode", 0);
                }
                if (_assetCreatorModeInt == 1)
                {
                    _assetCreatorMode = true;
                }
                else
                {
                    _assetCreatorMode = false;
                }
                return _assetCreatorMode;
            }
            set
            {
                _assetCreatorModeInt = value ? 1 : 0;

                PlayerPrefs.SetInt("PLS_AssetCreatorMode", (int)_assetCreatorModeInt);


            }
        }

        private static bool _importXMLonStartUp = false;
        private static int? _importXMLonStartUpInt;
        public static bool ImportXMLonStartUp
        {
            get
            {
                if (_importXMLonStartUpInt == null)
                {
                    _importXMLonStartUpInt = PlayerPrefs.GetInt("PLS_ImportXMLonStartUp", 0);
                }
                if (_importXMLonStartUpInt == 1)
                {
                    _importXMLonStartUp = true;
                }
                else
                {
                    _importXMLonStartUp = false;
                }
                return _importXMLonStartUp;
            }
            set
            {
                _importXMLonStartUpInt = value ? 1 : 0;

                PlayerPrefs.SetInt("PLS_ImportXMLonStartUp", (int)_importXMLonStartUpInt);


            }
        }


        public static List<string> unacceptableInfoNames = new List<string>() { "Highway", "Ship Path", "Airplane Path", "Airplane Connection Path", "Castle Wall 3", "HighwayRamp" , "HighwayRampElevated" };
        public static string lastAssetCreatorModeInfo = "";

        private static string _defaultFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Colossal Order\\Cities_Skylines\\PLS";
        private static string _FolderPath;
        public static string FolderPath
        {
            get
            {
                if (_FolderPath == null)
                {
                    _FolderPath = PlayerPrefs.GetString("PLS_FolderPath", _defaultFolderPath);
                }
                return _FolderPath;
            }
            set
            {
                _FolderPath = value;
                PlayerPrefs.SetString("PLS_FolderPath", _FolderPath);
            }
        }
        public static string SerializeXML() 
        {
            //Debug.Log("[PLS].modsettings.SerializePLRProperties Environment.CurrentDirectory = " + Environment.CurrentDirectory);
            //Debug.Log("[PLS].modsettings.SerializePLRProperties Environment.Environment.SpecialFolder.ApplicationData = " + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Local\\Colossal Order\\Cities_Skylines\\PLSAssetCreatorMode.xml");
            int PSAcount = 0;
            string folderPath = FolderPath;
            System.IO.Directory.CreateDirectory(FolderPath);
            int serializeErrorCount = 0;
            foreach (KeyValuePair<string, PSACustomPropertiesClass> currentTarget in PSACustomProperties)
            {
                try
                {
                    XmlSerializer serTool = new XmlSerializer(currentTarget.Value.GetType()); // Create serializer
                    var path = folderPath + "\\PSA-" + currentTarget.Value.Filename + ".xml"; // Find path
                    System.IO.FileStream file = System.IO.File.Create(path); // Create file

                    serTool.Serialize(file, currentTarget.Value); // Serialize whole properties
                    file.Close(); // Close file
                    PSAcount++;
                } catch (Exception e)
                {
                    Debug.Log("[PLS]Modsettings.SerializeXML Error! Encountered exception " + e.ToString() + " while trying to serialize PSA " + currentTarget.Key);
                    serializeErrorCount++;
                }
            }
            int PLRcount = 0;
            foreach (KeyValuePair<string, PLRCustomPropertiesClass> currentTarget in PLRCustomProperties)
            {
                try
                {


                    XmlSerializer serTool = new XmlSerializer(currentTarget.Value.GetType()); // Create serializer
                    var path = folderPath + "\\PLR-" + currentTarget.Value.Filename + ".xml"; // Find path
                    System.IO.FileStream file = System.IO.File.Create(path); // Create file

                    serTool.Serialize(file, currentTarget.Value); // Serialize whole properties
                    file.Close(); // Close file
                    PLRcount++;
                }
                catch (Exception e)
                {
                    serializeErrorCount++;
                    Debug.Log("[PLS]Modsettings.SerializeXML Error! Encountered exception " + e.ToString() + " while trying to serialize PLR " + currentTarget.Key);
                }
            }
            
            string exportLog = PSAcount.ToString() + " PSAs and " + PLRcount.ToString() + " PLRs.";
            if (serializeErrorCount > 0)
            {
                exportLog += " Error Count = " + serializeErrorCount.ToString() + ". See log file.";
            } else
            {
                exportLog += " No errors.";
            }
            exportLog += " | " + DateTime.Now.ToString("HH:mm:ss");
            Debug.Log("[PLS].Modsettings.DeserializeXML SerializeXML log: " + exportLog + " | " + DateTime.Now.ToString("HH:mm:ss"));
            return exportLog;
        }


        public static string DeserializeXML()
        {
            string folderPath = FolderPath;
            String[] fileNames = Directory.GetFiles(folderPath);
            int PSAcount = 0;
            int PLRcount = 0;
            int deserializeErrorCount = 0;
            bool logging = false;

            foreach (String file in fileNames)
            {
                try
                { 
                    if (file.Length > 3)
                    {
                        string currentPrefix = file.Substring(folderPath.Length+1, 3);
                        if (currentPrefix == "PSA")
                        {
                            XmlSerializer serTool = new XmlSerializer(typeof(PSACustomPropertiesClass)); // Create serializer
                            System.IO.FileStream readStream = new System.IO.FileStream(file, System.IO.FileMode.Open); // Open file
                            PSACustomPropertiesClass result = (PSACustomPropertiesClass)serTool.Deserialize(readStream); // Des-serialize to new Properties
                            if (logging) Debug.Log("[PLS].Modsettings.DeserializeXML result.Name = " + result.Name);
                            if (!PSACustomProperties.ContainsKey(result.Name))
                            {
                                PSACustomProperties.Add(result.Name, result); //Adds a completely new PSA into dictionary
                            } else
                            {
                                if (logging) Debug.Log("[PLS].Modsettings.DeserializeXML PSAcp[result.Name].Offset = " + PSACustomProperties[result.Name].Offset.ToString());
                                if (logging) Debug.Log("[PLS].Modsettings.DeserializeXML result.Offset = " + result.Offset.ToString());
                                PSACustomProperties.Remove(result.Name);
                                PSACustomProperties.Add(result.Name, result); //Overrides a already existing PSA in dictionary
                                if (logging) Debug.Log("[PLS].Modsettings.DeserializeXML PSAcp[result.Name].Offset = " + PSACustomProperties[result.Name].Offset.ToString());
                            }
                            PSAcount++;
                        }
                        else if (currentPrefix == "PLR")
                        {
                            XmlSerializer serTool = new XmlSerializer(typeof(PLRCustomPropertiesClass)); // Create serializer
                            System.IO.FileStream readStream = new System.IO.FileStream(file, System.IO.FileMode.Open); // Open file
                            PLRCustomPropertiesClass result = (PLRCustomPropertiesClass)serTool.Deserialize(readStream); // Des-serialize to new Properties
                            if (!PLRCustomProperties.ContainsKey(result.Name))
                            {
                                PLRCustomProperties.Add(result.Name, result); //Adds a completely new PLR into dictionary
                            }
                            else
                            {
                                PLRCustomProperties.Remove(result.Name);
                                PLRCustomProperties.Add(result.Name, result); //Overrides a already existing PLR in dictionary
                            }
                            PLRcount++;
                        } else
                        {
                            //Debug.Log("[PLS].ModSettings.DeserializeXML file is " + file);
                            Debug.Log("[PLS].ModSettings.DeserializeXML Invaled Prefix for file: " + file + ". currentPrefix is " + currentPrefix);
                        }
                    }
                       
                } catch (Exception e)
                {
                    Debug.Log("[PLS].ModSettings.DeserializeXML Error! Encountered Exception " + e.ToString());
                    deserializeErrorCount++;
                }
               
            }
            string importLog = PSAcount.ToString() + " PSAs and " + PLRcount.ToString() + " PLRs.";
            if (deserializeErrorCount > 0)
            {
                importLog += " Error Count = " + deserializeErrorCount.ToString() + ". See log file.";
            }
            else
            {
                importLog += " No errors.";
            }
            importLog += " | " + DateTime.Now.ToString("HH:mm:ss");
            Debug.Log("[PLS].Modsettings.DeserializeXML Import log: " + importLog + " | " + DateTime.Now.ToString("HH:mm:ss"));
            return importLog;
        }

        public static bool SerializeOnePLR (ModSettings.PLRCustomPropertiesClass target)
        {
            bool success = false;
            string folderPath = FolderPath;
            System.IO.Directory.CreateDirectory(FolderPath);

            try
            {

                XmlSerializer serTool = new XmlSerializer(target.GetType()); // Create serializer
                var path = folderPath + "\\PLR-" + target.Filename + ".xml"; // Find path
                System.IO.FileStream file = System.IO.File.Create(path); // Create file

                serTool.Serialize(file, target); // Serialize whole properties
                file.Close(); // Close file
                success = true;
            }
            catch (Exception e)
            {
                
                Debug.Log("[PLS]Modsettings.SerializeOnePLR Error! Encountered exception " + e.ToString() + " while trying to serialize PLR " + target.Name);
            }

            Debug.Log("[PLS]Modsettings.SerializeOnePLR Success = " + success.ToString());

            return success;
        }
        public static bool DeserializeOnePLR(ModSettings.PLRCustomPropertiesClass target)
        {
            bool success = false;
            string folderPath = FolderPath;
            var path = folderPath + "\\PLR-" + target.Filename + ".xml"; // Find path
            try
            {
                if (path.Length > 3)
                {
                    string currentPrefix = path.Substring(folderPath.Length + 1, 3);
                    if (currentPrefix == "PLR")
                    {
                        XmlSerializer serTool = new XmlSerializer(typeof(PLRCustomPropertiesClass)); // Create serializer
                        System.IO.FileStream readStream = new System.IO.FileStream(path, System.IO.FileMode.Open); // Open file
                        PLRCustomPropertiesClass result = (PLRCustomPropertiesClass)serTool.Deserialize(readStream); // Des-serialize to new Properties
                        if (!PLRCustomProperties.ContainsKey(result.Name))
                        {
                            PLRCustomProperties.Add(result.Name, result); //Adds a completely new PLR into dictionary
                        }
                        else
                        {
                            PLRCustomProperties.Remove(result.Name);
                            PLRCustomProperties.Add(result.Name, result); //Overrides a already existing PLR in dictionary
                        }
                        success = true;
                    }
                    else
                    {
                        //Debug.Log("[PLS].ModSettings.DeserializeXML file is " + file);
                        Debug.Log("[PLS].ModSettings.DeserializeOnePLR Invaled Prefix for file: " + path + ". currentPrefix is " + currentPrefix);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log("[PLS].ModSettings.DeserializeOnePLR Error! Encountered Exception " + e.ToString());

            }

            Debug.Log("[PLS]Modsettings.DeserializeOnePLR Success = " + success.ToString());

            return success;
        }
        
    }
}
