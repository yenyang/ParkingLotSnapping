using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingLotSnapping
{
    internal static class ModSettings
    {
       
        private static bool _PassElectricity;
        private static int? _PassElectricityInt;
        public static bool PassElectricity
        {
            get
            {
                if (_PassElectricityInt == null)
                {
                    _PassElectricityInt = PlayerPrefs.GetInt("RF_PassElectricity", 0);
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

        private static int? _distanceFromCurb;
        public const int defaultDistanceFromCurb = -10;
        public const int minDistanceFromCurb = -35;
        public const int maxDistanceFromCurb = 65;
        public const float rangeDistanceFromCurb = 100f;
        public const float stepDistanceFromCurb = 1f;
        public static int DistanceFromCurb
        {

            get
            {
                if (!_distanceFromCurb.HasValue)
                {
                    _distanceFromCurb = PlayerPrefs.GetInt("PLS_DistanceFromCurb", defaultDistanceFromCurb);
                }
                return _distanceFromCurb.Value;
            }
            set
            {
                if (value > maxDistanceFromCurb || value < minDistanceFromCurb)
                    throw new ArgumentOutOfRangeException();
                if (value == _distanceFromCurb)
                {
                    return;
                }
                PlayerPrefs.SetInt("PLS_DistanceFromCurb", value);
                _distanceFromCurb = value;
            }
        }
        
        private static int? _distanceBetweenParkingStalls;
        public const int minDistanceBetweenParkingStalls = -10;
        public const int defaultDistanceBetweenParkingStalls = -10;
        public const int maxDistanceBetweenParkingStalls = 90;
        public const float rangeDistanceBetweenParkingStalls = 100f;
        public const float stepDistanceBetweenParkingStalls = 1f;
        public static int DistanceBetweenParkingStalls
        {

            get
            {
                if (!_distanceBetweenParkingStalls.HasValue)
                {
                    _distanceBetweenParkingStalls = PlayerPrefs.GetInt("PLS_DistanceBetweenParkingStalls", defaultDistanceBetweenParkingStalls);
                }
                return _distanceBetweenParkingStalls.Value;
            }
            set
            {
                if (value > maxDistanceBetweenParkingStalls || value < minDistanceBetweenParkingStalls)
                    throw new ArgumentOutOfRangeException();
                if (value == _distanceBetweenParkingStalls)
                {
                    return;
                }
                PlayerPrefs.SetInt("PLS_DistanceBetweenParkingStalls", value);
                _distanceBetweenParkingStalls = value;
            }
        }

        public static void resetModSettings()
        {
         
            ModSettings.PassElectricity = false;
            ModSettings.DistanceFromCurb = defaultDistanceFromCurb;
            ModSettings.DistanceBetweenParkingStalls = defaultDistanceBetweenParkingStalls;
            

        }

    }
}
