using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using UnityEngine;
using ICities;
using System.Runtime.CompilerServices;

namespace ParkingLotSnapping
{
    public static class PositionLocking
    {
        private static Vector3 lockPosition;
        //private static int parkingRowsFilled;
        //private static int parkingRowsLocked;
        private static bool locked = false;
        private static NetInfo lastSnappedNetInfo;
        private static int lastSnappedSegmentID;
        private static Vector3 lastSnappedPosition;
        private static Vector3 centerPosition;
        private static Vector3 centerDirection;
        private static Vector3 nextCenterPosition;
        private static Vector3 previousLockPosition;
        private static Vector3 previousLockCenterPosition;
        private static Vector3 previousLockCenterDirection;
        private static NetInfo previousLockSnappedNetInfo;
        private static float previousLockParkingWidth;
        private static float lastParkingWidth;
        private static bool overlapping = false;
        private static NetInfo lockedNetInfo;
        private static int lockedSegmentID;
        private static int previousLockSegmentID;
        //private static HashSet<Vector3> PSAPositionChain = new HashSet<Vector3>();

        public static bool InitiateLock(float parkingWidth)
        {

            if (locked == false && lastSnappedPosition != null && lastSnappedNetInfo != null)
            {
                /*
                if (lastSnappedNetInfo.m_lanes.Length == 9 || lastSnappedNetInfo.m_halfWidth == 20)
                {
                    parkingRowsLocked = 4;
                } else if (lastSnappedNetInfo.m_lanes.Length == 13 || lastSnappedNetInfo.m_halfWidth == 29)
                {
                    parkingRowsLocked = 6;
                } else if (lastSnappedNetInfo.m_lanes.Length != 4 && lastSnappedNetInfo.m_halfWidth != 8)
                {
                    parkingRowsLocked = 2;
                }
                if (parkingRowsPerAsset >= parkingRowsLocked)
                {
                    parkingRowsLocked = 0;
                    locked = false;
                    return locked;
                }
                */
                if (previousLockPosition != null)
                {
                    //Debug.Log("[PLS]PositionLocking Distance between Locks = " + Vector3.Distance(previousLockPosition, lastSnappedPosition));
                }
                lastParkingWidth = parkingWidth;
                lockPosition = lastSnappedPosition;
                lockedSegmentID = lastSnappedSegmentID;
                lockedNetInfo = lastSnappedNetInfo;
                //PSAPositionChain.Add(lockPosition);
                //parkingRowsFilled = parkingRowsPerAsset;
                //Debug.Log("[PLS]PositionLocking lastSnapped Position = " + lastSnappedPosition.ToString() +  " info = " + lastSnappedNetInfo.name );
                
                locked = true;
                return locked;

            }
            locked = false;
            return locked;
        }
        public static bool IsLocked()
        {
            return locked;
        }
        public static void SetSnappedPosition(Vector3 position)
        {
            lastSnappedPosition = position;
        }
        public static void SetCenterDirection(Vector3 direction)
        {
            centerDirection = direction;
        }
        public static void SetNextCenterPosition(Vector3 position)
        {
            nextCenterPosition = position;
        }
        public static void SetCenterPosition(Vector3 position)
        {
            centerPosition = position;
        }
        public static void SetSnappedNetInfo(NetInfo info)
        {
            lastSnappedNetInfo = info;
        }
        public static void SetSnappedSegmentId(int id)
        {
            lastSnappedSegmentID = id;
        }
        
        public static Vector3 GetCenterPosition()
        {
            return centerPosition;
        }
        public static Vector3 GetCenterDirection()
        {
            return centerDirection;
        }
        public static float GetLastParkingWidth()
        {           
            return lastParkingWidth;
        }
        public static int GetLastSnappedSegmentID ()
        {
            return lastSnappedSegmentID;
        }
       
        public static NetInfo GetLockedNetInfo()
        {
            return lockedNetInfo;
        }

        public static void SetOverlapping(bool flag)
        {
            overlapping = flag;
        }
        public static bool GetOverlapping()
        {
            return overlapping;
        }
        public static void DisengageLock()
        {
            previousLockPosition = lockPosition;
            previousLockCenterDirection = centerDirection;
            previousLockCenterPosition = centerPosition;
            previousLockSnappedNetInfo = lockedNetInfo;
            previousLockParkingWidth = lastParkingWidth;
            previousLockSegmentID = lockedSegmentID;
            lockPosition = new Vector3();
            centerPosition = new Vector3();
            centerDirection = new Vector3();
            lockedNetInfo = new NetInfo();
            //PSAPositionChain.Clear();
            lastParkingWidth = 0f;
            //Debug.Log("[PLS]PositionLocking disengageLock");

            locked = false;

        }
        /*public static bool CheckForPreviousPSAPosition(Vector3 position)
        {
            return PSAPositionChain.Contains(position);
            
        }
        */
        public static void AddParkingSpaceAsset(float parkingWidth)
        {
            //parkingRowsFilled += parkingRowsPerAsset;
            /*if (parkingRowsFilled >= parkingRowsLocked)
            {
                disengageLock();
                Debug.Log("[PLS]PositionLocking AddLinkInPSAChaing all rows filled!");
                return false;
            } else
            {*/
            lastParkingWidth = parkingWidth;
            lockPosition = lastSnappedPosition;
            lockedNetInfo = lastSnappedNetInfo;
            //PSAPositionChain.Add(lockPosition);
            centerPosition = nextCenterPosition;
            //Debug.Log("[PLS]PositionLocking AddLinkInPSAChain PSA Chain Count = " + PSAPositionChain.Count.ToString());
            //}
        }

        public static bool returnToPreviousLockPosition()
        {
            if (previousLockPosition != null && previousLockCenterPosition != null && previousLockCenterDirection != null && previousLockParkingWidth > 0f && previousLockSnappedNetInfo != null && locked == false)
            {
                lockPosition = previousLockPosition;
                centerPosition = previousLockCenterPosition;
                centerDirection = previousLockCenterDirection;
                lockedNetInfo = previousLockSnappedNetInfo;
                lastParkingWidth = previousLockParkingWidth;
                locked = true;
                //Debug.Log("[PLS]PositionLocking Return to previous lock position.");
                return true;
                
            }
            return false;
        }

        public static bool canReturnToPreviousLockPosition()
        {
            if (previousLockPosition != null && previousLockCenterPosition != null && previousLockCenterDirection != null && previousLockParkingWidth > 0f && previousLockSnappedNetInfo != null && locked == false)
            {
                if (Vector3.Distance(previousLockPosition, lastSnappedPosition) > ModSettings.UnlockingDistance)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
