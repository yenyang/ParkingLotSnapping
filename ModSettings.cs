﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ColossalFramework;
using System.Xml;
using System.Xml.Serialization;

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
        }

        public struct PSACustomPropertiesStruct
        {
            private float m_offset;
            private float m_parkingWidth;
            private bool m_parralel;
            private float m_rotation;

            public PSACustomPropertiesStruct(float offset, float parkingWidth, bool parralel, float rotation)
            {
                this.m_offset = offset;
                this.m_parkingWidth = parkingWidth;
                this.m_parralel = parralel;
                this.m_rotation = rotation;
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
            public bool Parralel
            {
                get { return this.m_parralel; }
                set { this.m_parralel = value; }
            }
            public float Rotation
            {
                get { return this.m_rotation; }
                set { this.m_rotation = value; }
            }
        }
        public static readonly float perpendicularAsymetricOffset = 5.1f;
        public static readonly float perpendicularAsymetricOffsetPLR2 = 6.05f;
        public static readonly float angledAsymetricOffset = 6.6f;
        public static readonly float angledAsymetricOffsetPLR2 = 6.05f;
        public static readonly float parralelAsymetricOffset = 4.5f;
        public static Dictionary<string, PSACustomPropertiesStruct> PSACustomProperties = new Dictionary<string, PSACustomPropertiesStruct>()
        {
            {"1x1 Electric Vehicle Parking",        new PSACustomPropertiesStruct(perpendicularAsymetricOffset,         7.72f,      false,  0f  ) },
            {"1x1 Accessible Parking",              new PSACustomPropertiesStruct(perpendicularAsymetricOffset,         9.07f,      false,  0f  ) },
            {"1x1 Parking Lot",                     new PSACustomPropertiesStruct(perpendicularAsymetricOffset,         7.72f,      false,  0f  ) },
            {"1x1 Single Space Parking",            new PSACustomPropertiesStruct(perpendicularAsymetricOffset,         2.58f,      false,  0f  ) },
            {"3x2 Parking Lot",                     new PSACustomPropertiesStruct(0f,                                   23.14f,     false,  0f ) },
            {"4x1 Parking Row",                     new PSACustomPropertiesStruct(perpendicularAsymetricOffset,         33.46f,     false,  0f ) },
            {"4x2 Parking Lot",                     new PSACustomPropertiesStruct(0f,                                   33.46f,     false,  0f ) },
            {"8x2 Parking Lot",                     new PSACustomPropertiesStruct(0f,                                   59.14f,     false,  0f ) },
            {"1x1 Parking Lot - LHD",               new PSACustomPropertiesStruct(angledAsymetricOffset,                5.72f,      false,  0f ) },
            {"1x1 End Side Left - LHD",             new PSACustomPropertiesStruct(angledAsymetricOffset,                3.25f,      false,  0f  ) },
            {"1x1 Parking Right End - LHD",         new PSACustomPropertiesStruct(angledAsymetricOffset,                3.25f,      false,  0f  ) },
            {"1x1 Accessible Parking - LHD",        new PSACustomPropertiesStruct(angledAsymetricOffset,                7.28f,      false,  0f  ) },
            {"1x1 Single Parking Space - LHD",      new PSACustomPropertiesStruct(angledAsymetricOffset,                2.89f,      false,  0f  ) },
            {"3x2 Parking Lot 60 deg - LHD",        new PSACustomPropertiesStruct(0f,                                   20.05f,     false,  0f ) },
            {"4x1 Parking Lot 60 Degree - LHD",     new PSACustomPropertiesStruct(angledAsymetricOffset,                34.39f,     false,  0f ) },
            {"1x1 Parking Lot - RHD",               new PSACustomPropertiesStruct(angledAsymetricOffset,                5.72f,      false,  0f  ) },
            {"1x1 End Side Left - RHD",             new PSACustomPropertiesStruct(angledAsymetricOffset,                3.25f,      false,  0f  ) },
            {"1x1 Parking Right End - RHD",         new PSACustomPropertiesStruct(angledAsymetricOffset,                3.25f,      false,  0f  ) },
            {"1x1 Accessible Parking - RHD",        new PSACustomPropertiesStruct(angledAsymetricOffset,                7.28f,      false,  0f  ) },
            {"1x1 Single Parking Space - RHD",      new PSACustomPropertiesStruct(angledAsymetricOffset,                2.89f,      false,  0f  ) },
            {"3x2 Parking Lot 60 degree - RHD",     new PSACustomPropertiesStruct(0f,                                   20.05f,     false,  0f ) },
            {"4x1 Parking Lot 60 Degree - RHD",     new PSACustomPropertiesStruct(angledAsymetricOffset,                34.39f,     false,  0f ) },
            {"2 Spots with Planters (Mirrored)",    new PSACustomPropertiesStruct(parralelAsymetricOffset,              24.43f,     true,   90f) },
            {"4 Spots with Planters (Mirrored)",    new PSACustomPropertiesStruct(parralelAsymetricOffset,              36.80f,     true,   90f) },
            {"6 Spots with Planters (Mirrored)",    new PSACustomPropertiesStruct(parralelAsymetricOffset,              50.22f,     true,   90f) },
            {"8 Spots with Planters (Mirrored)",    new PSACustomPropertiesStruct(parralelAsymetricOffset,              62.66f,     true,   90f) },
            {"10 Spts wth Plntrs EC (Mirrored)",    new PSACustomPropertiesStruct(parralelAsymetricOffset,              79.85f,     true,   90f) },
            {"10 Spts wth Planters (Mirrored)",     new PSACustomPropertiesStruct(parralelAsymetricOffset,              75.03f,     true,   90f) },
            {"PLR II - 2 Spots",                    new PSACustomPropertiesStruct(parralelAsymetricOffset,              13.43f,     true,   90f) },
            {"PLR II - 2 Spots (Mirrored)",         new PSACustomPropertiesStruct(parralelAsymetricOffset,              13.43f,     true,   90f) },
            {"PLR II - 2 Spots with Planters",      new PSACustomPropertiesStruct(parralelAsymetricOffset,              24.43f,     true,   90f) },
            {"PLR II - 4 Spots ",                   new PSACustomPropertiesStruct(parralelAsymetricOffset,              25.80f,     true,   90f) },
            {"PLR II - 4 Spots (Mirrored)",         new PSACustomPropertiesStruct(parralelAsymetricOffset,              25.80f,     true,   90f) },
            {"PLR II - 4 Spots with Planters",      new PSACustomPropertiesStruct(parralelAsymetricOffset,              36.80f,     true,   90f) },
            {"PLR II - 6 Spots with Planters",      new PSACustomPropertiesStruct(parralelAsymetricOffset,              50.22f,     true,   90f) },
            {"PLR II - 8 Spots",                    new PSACustomPropertiesStruct(parralelAsymetricOffset,              50.65f,     true,   90f) },
            {"PLR II - 8 Spots (mirrored)",         new PSACustomPropertiesStruct(parralelAsymetricOffset,              50.65f,     true,   90f) },
            {"PLR II - 8 Spots with Planters",      new PSACustomPropertiesStruct(parralelAsymetricOffset,              62.66f,     true,   90f) },
            {"PLR II - 10 Spots with Planters",     new PSACustomPropertiesStruct(parralelAsymetricOffset,              79.85f,     true,   90f) },
            {"PLR II - 10 Spts wth Plntrs NEC",     new PSACustomPropertiesStruct(parralelAsymetricOffset,              75.06f,     true,   90f) },
            {"PLR II - 60° Planters #03",           new PSACustomPropertiesStruct(angledAsymetricOffsetPLR2,            12.05f,     false,  270f) },
            {"PLR II - 60° Planters #04",           new PSACustomPropertiesStruct(angledAsymetricOffsetPLR2,            24.03f,     false,  270f) },
            {"PLR II - 60° Planters #02",           new PSACustomPropertiesStruct(angledAsymetricOffsetPLR2,            33.92f,     false,  270f) },
            {"PLR II - 60° Plain #01",              new PSACustomPropertiesStruct(angledAsymetricOffsetPLR2,            27.50f,     false,  270f) },
            {"PLR II - 90° Planters #01",           new PSACustomPropertiesStruct(perpendicularAsymetricOffsetPLR2,     33.25f,     false,  270f) },
            {"PLR II - 90° - Planters #02",         new PSACustomPropertiesStruct(perpendicularAsymetricOffsetPLR2,     61.28f,     false,  270f) },
            {"PLR II - 90° Planters #03",           new PSACustomPropertiesStruct(perpendicularAsymetricOffsetPLR2,     11.22f,     false,  270f) },
            {"PLR II - 90° Planters #04",           new PSACustomPropertiesStruct(perpendicularAsymetricOffsetPLR2,     22.44f,     false,  270f) },

        };

        public static Dictionary<string, PLRCustomPropertiesClass> PLRCustomProperties = new Dictionary<string, PLRCustomPropertiesClass>()
        {
            {"1285230481.16m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 3f,-3f },         true,   true,   false) },
            {"1303766506.16m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 3f,-3f },         true,   true,   false) },
            {"1285201733.22m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false) },
            {"1303772884.22m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false) },
            {"1285201733.40m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {9f,-9f},            new List<float> { 9f, -9f },        false,  false,  false) },
            {"1303772884.40m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {9f,-9f},            new List<float> { 9f, -9f },        false,  false,  false) },
            {"1285201733.58m Parking Lot_Data",                 new PLRCustomPropertiesClass(new List<float>() {0f, 18f,-18f},      new List<float> { 0f,18f,-18f},     false,  false,  false) },
            {"1303772884.58m Poorly Maintained Parking_Data",   new PLRCustomPropertiesClass(new List<float>() {0f, 18f,-18f},      new List<float> { 0f,18f,-18f},     false,  false,  false) },
            {"1578348250.Tram Road #01_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 4.5f, -4.5f },    true,   false,  false) },
            {"1581742834.Tram Road #02_Data",                   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float>{ 0f },              false,  false,  false) },
            {"1423812793.60°/90° Parking Road 2L Urban_Data",   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 1.65f,-1.65f },   true,   false,  false) },
            {"1426090117.60°/90° Parking Road 2L Suburban_Data",new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 1.65f, -1.65f },  true,   false,  false) },
            {"1608297735.Tram Road#02 - 90° Parking Lots_Data", new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float>{ 0f },              false,  false,  false) },
            {"1608293777.Tram Road#01 - 90° Parking Lots_Data", new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 4.5f, -4.5f },    true,   false,  false) },
            {"2409968332.US 2L 2W Parking Asym Concr Tree_Data",new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409968332.US 2L 2W Parking Asym Concrete_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409968332.US 2L 2W Parking Asym Red_Data",       new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409968332.US 2L 2W Parking Asym Red Tree_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409968332.US 2L 2W Parking Sym Concrete_Data",   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false) },
            {"2409968332.US 2L 2W Parking Sym Red_Data",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false) },
            {"Parking Lot 01",                                  new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false) },
            {"1969112396.PL - Road #01_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0f },             false,   false,  true) },
            {"1969113204.PL - Road #02_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0f },             false,   false,  true) },
            {"1971236400.PL - Road #03_Data",                   new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0f },             false,   false,  true) },
            {"2409941439.US 2L 2W Parking Asym Concr Tree_Data",new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409946015.US 2L 2W Parking Asym Concrete_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409955382.US 2L 2W Parking Asym Red_Data",       new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409958009.US 2L 2W Parking Asym Red Tree_Data",  new PLRCustomPropertiesClass(new List<float>() {},                  new List<float> { 0.15f, -0.15f},   true,   true,   false) },
            {"2409960665.US 2L 2W Parking Sym Concrete_Data",   new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false) },
            {"2409962948.US 2L 2W Parking Sym Red_Data",        new PLRCustomPropertiesClass(new List<float>() {0f},                new List<float> { 0f },             false,  false,  false) },
        };


        public class PLRCustomPropertiesClass
        {
            private List<float> m_symetricAisleOffsets;
            private List<float> m_asymetricAisleOffsets;
            private bool m_onesided;
            private bool m_invertOffset;
            private bool m_parralel;
            public PLRCustomPropertiesClass() { } // Need in order to make de-serializer work
            public PLRCustomPropertiesClass(List<float> symetricAisleOffsets, List<float> asymetricAisleOffsets, bool onesided, bool invertOffset, bool parralel)
            {
                this.m_symetricAisleOffsets = symetricAisleOffsets;
                this.m_asymetricAisleOffsets = asymetricAisleOffsets;
                this.m_onesided = onesided;
                this.m_invertOffset = invertOffset;
                this.m_parralel = parralel;
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

            public bool InvertOffset
            {
                get { return this.m_invertOffset; }
                set { this.m_invertOffset = value; }
            }

            public bool Parralel
            {
                get { return this.m_parralel; }
                set { this.m_parralel = value; }
            }
        }

        public static bool assetCreatorMode = false;
        public static List<string> unacceptableInfoNames = new List<string>() { "Highway", "Ship Path", "Airplane Path", "Airplane Connection Path", "Castle Wall 3", "HighwayRamp" , "HighwayRampElevated" };
        public static string lastAssetCreatorModeInfo = "";

        public static void SerializePLRProperties(PLRCustomPropertiesClass target) // Only for testing output format! Will save reference XML to desktop
        {
            XmlSerializer serTool = new XmlSerializer(target.GetType()); // Create serializer
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/TestOutput.xml"; // Find path
            System.IO.FileStream file = System.IO.File.Create(path); // Create file
            serTool.Serialize(file, target); // Serialize whole properties
            file.Close(); // Close file
        }
        public static PLRCustomPropertiesClass DeserializePLRProperties(string path)
        {
            XmlSerializer serTool = new XmlSerializer(typeof(PLRCustomPropertiesClass)); // Create serializer
            System.IO.FileStream readStream = new System.IO.FileStream(path, System.IO.FileMode.Open); // Open file
            var result = (PLRCustomPropertiesClass)serTool.Deserialize(readStream); // Des-serialize to new Properties
            return result;
        }
    }
}
