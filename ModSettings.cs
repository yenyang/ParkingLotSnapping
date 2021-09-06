using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ColossalFramework;

namespace ParkingLotSnapping
{
    internal static class ModSettings
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
        }

        public struct PSACustomPropertiesStruct
        {
            private float m_offset;
            private float m_parkingWidth;

            public PSACustomPropertiesStruct(float offset, float parkingWidth)
            {
                this.m_offset = offset;
                this.m_parkingWidth = parkingWidth;
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
        }
        public static readonly float perpendicularAsymetricOffset = 5.1f;
        public static readonly float angledAsymetricOffset = 6.6f;
        public static Dictionary<string, PSACustomPropertiesStruct> PSACustomProperties = new Dictionary<string, PSACustomPropertiesStruct>()
        {
            {"1x1 Electric Vehicle Parking",    new PSACustomPropertiesStruct(perpendicularAsymetricOffset,       7.72f  ) },
            {"1x1 Accessible Parking",          new PSACustomPropertiesStruct(perpendicularAsymetricOffset,       9.07f  ) },
            {"1x1 Parking Lot",                 new PSACustomPropertiesStruct(perpendicularAsymetricOffset,       7.72f  ) },
            {"1x1 Single Space Parking",        new PSACustomPropertiesStruct(perpendicularAsymetricOffset,       2.58f  ) },
            {"3x2 Parking Lot",                 new PSACustomPropertiesStruct(0f,                                 23.14f ) },
            {"4x1 Parking Row",                 new PSACustomPropertiesStruct(perpendicularAsymetricOffset,       33.46f ) },
            {"4x2 Parking Lot",                 new PSACustomPropertiesStruct(0f,                                 33.46f ) },
            {"8x2 Parking Lot",                 new PSACustomPropertiesStruct(0f,                                 59.14f ) },
            {"1x1 Parking Lot - LHD",           new PSACustomPropertiesStruct(angledAsymetricOffset,              5.72f  ) },
            {"1x1 End Side Left - LHD",         new PSACustomPropertiesStruct(angledAsymetricOffset,              3.25f  ) },
            {"1x1 Parking Right End - LHD",     new PSACustomPropertiesStruct(angledAsymetricOffset,              3.25f  ) },
            {"1x1 Accessible Parking - LHD",    new PSACustomPropertiesStruct(angledAsymetricOffset,              7.28f  ) },
            {"1x1 Single Parking Space - LHD",  new PSACustomPropertiesStruct(angledAsymetricOffset,              2.89f  ) },
            {"3x2 Parking Lot 60 deg - LHD",    new PSACustomPropertiesStruct(0f,                                 20.05f ) },
            {"4x1 Parking Lot 60 Degree - LHD", new PSACustomPropertiesStruct(angledAsymetricOffset,              34.39f ) },
            {"1x1 Parking Lot - RHD",           new PSACustomPropertiesStruct(angledAsymetricOffset,              5.72f  ) },
            {"1x1 End Side Left - RHD",         new PSACustomPropertiesStruct(angledAsymetricOffset,              3.25f  ) },
            {"1x1 Parking Right End - RHD",     new PSACustomPropertiesStruct(angledAsymetricOffset,              3.25f  ) },
            {"1x1 Accessible Parking - RHD",    new PSACustomPropertiesStruct(angledAsymetricOffset,              7.28f  ) },
            {"1x1 Single Parking Space - RHD",  new PSACustomPropertiesStruct(angledAsymetricOffset,              2.89f  ) },
            {"3x2 Parking Lot 60 degree - RHD", new PSACustomPropertiesStruct(0f,                                 20.05f ) },
            {"4x1 Parking Lot 60 Degree - RHD", new PSACustomPropertiesStruct(angledAsymetricOffset,              34.39f ) }
           
        };

        public static Dictionary<string, PLRCustomPropertiesClass> PLRCustomProperties = new Dictionary<string, PLRCustomPropertiesClass>()
        {
            {"1285230481.16m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 3f,-3f }, true) },
            {"1303766506.16m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 3f,-3f }, true) },
            {"1285201733.22m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f }, false) },
            {"1303772884.22m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f }, false) },
            {"1285201733.40m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {9f,-9f},            new List<float> { 9f, -9f }, false) },
            {"1303772884.40m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {9f,-9f},            new List<float> { 9f, -9f }, false) },
            {"1285201733.58m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {0f, 18f,-18f},      new List<float> { 0f,18f,-18f}, false) },
            {"1303772884.58m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {0f, 18f,-18f},      new List<float> { 0f,18f,-18f}, false) },
            {"1578348250.Tram Road #01_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 4.5f, -4.5f }, true) },
            {"1581742834.Tram Road #02_Data",                   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float>{ 0f }, false) },
            {"1423812793.60°/90° Parking Road 2L Urban_Data",   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 1.65f,-1.65f }, true) },
            {"1426090117.60°/90° Parking Road 2L Suburban_Data",new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 1.65f, -1.65f }, true) },
            {"1608297735.Tram Road#02 - 90° Parking Lots_Data", new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float>{ 0f }, false) },
            {"1608293777.Tram Road#01 - 90° Parking Lots_Data", new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 4.5f, -4.5f }, true) },


        };


        public class PLRCustomPropertiesClass
        {
            private List<float> m_symetricAisleOffsets;
            private List<float> m_asymetricAisleOffsets;
            private bool m_onesided;
            public PLRCustomPropertiesClass(List<float> symetricAisleOffsets, List<float> asymetricAisleOffsets, bool onesided)
            {
                this.m_symetricAisleOffsets = symetricAisleOffsets;
                this.m_asymetricAisleOffsets = asymetricAisleOffsets;
                this.m_onesided = onesided;
            }
            public List<float> SymetricAisleOffsets
            {
                get { return this.m_symetricAisleOffsets; }
                set { this.m_symetricAisleOffsets = value; }
            }
            public List<float> AsymetricAisleOffsets
            {
                get { return this.m_asymetricAisleOffsets; }
                set { this.m_asymetricAisleOffsets = value; }
            }
            public bool Onesided
            {
                get { return this.m_onesided; }
                set { this.m_onesided = value; }
            }
        }
       

    }
}
