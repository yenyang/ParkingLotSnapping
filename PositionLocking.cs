using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using UnityEngine;
using ICities;


namespace ParkingLotSnapping
{
    public class PositionLocking
    {
        private Vector3 lockPosition;
        private int parkingAislesFilled;
        private int parkingAislesLocked;
        private int lockRange;
        private bool locked;
        public PositionLocking()
        {
            lockRange = 10;
            locked = false;
            
        }
        public bool initiateLock(Vector3 position, NetInfo info, int parkingAislesPerAsset)
        {
            if (locked == false && position != null && info != null)
            {
                
                if (info.m_lanes.Length == 9 || info.m_halfWidth == 20)
                {
                    parkingAislesLocked = 4;
                } else if (info.m_lanes.Length == 13 || info.m_halfWidth == 29)
                {
                    parkingAislesLocked = 6;
                } else if (info.m_lanes.Length != 4 && info.m_halfWidth != 8)
                {
                    parkingAislesLocked = 2;
                }
                if (parkingAislesPerAsset >= parkingAislesLocked)
                {
                    parkingAislesLocked = 0;
                    locked = false;
                    return locked;
                }
                lockPosition = position;
                parkingAislesFilled = parkingAislesPerAsset;
                locked = true;
                return locked;

            }
            locked = false;
            return locked;
        }

        
    }
}
