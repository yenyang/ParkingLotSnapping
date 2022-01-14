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
        //private static bool previousLockWasOnACurve = false;
        private static float lastParkingWidth;
        private static bool overlapping = false;
        private static NetInfo lockedNetInfo;
        private static int lockedSegmentID;
        private static int previousLockSegmentID;
        //private static Vector3 curvedLockFirstCenterPosition;
        //private static Vector3 curvedLockSecondCenterPosition;
        //private static Vector3 curvedLockFirstCenterDirection;
        //private static Vector3 curvedLockSecondCenterDirection;
        //private static bool lockedOntoACurve = false;
        //private static bool curvedFirstPositionStored = false;
        //private static float curvedLockFirstOffset;
        //private static float curvedLockSecondOffset;
        //private static Vector3 curvedLockCircularCenterPosition;
        //private static float curvedLockCircularRadius;
        //private static float curvedLockRadianDifference;
        //private static Vector3 curvedLockDir;
        //private static Vector3 nextCurvedLockDir;
        //private static Vector3 previousCurvedLockCircularCenterPosition;
        //private static float previousCurvedLockCircularRadius;
        //private static float previousCurvedLockRadianDifference;
        //private static Vector3 previousCurvedLockDir;

        public static bool InitiateLock(float parkingWidth, bool curved)
        {
            //if (curved) return false; //should temporaily stop any curved locking.

            if (locked == false && lastSnappedPosition != null && lastSnappedNetInfo != null && curved == false)
            {
                
                if (previousLockPosition != null)
                {
                    //Debug.Log("[PLS]PositionLocking Distance between Locks = " + Vector3.Distance(previousLockPosition, lastSnappedPosition));
                }
                lastParkingWidth = parkingWidth;
                lockPosition = lastSnappedPosition;
                lockedSegmentID = lastSnappedSegmentID;
                lockedNetInfo = lastSnappedNetInfo;
                //curvedFirstPositionStored = false;
                //curvedLockFirstCenterPosition = new Vector3();
                //curvedLockSecondCenterPosition =  new Vector3();
                //lockedOntoACurve = false;
                //Debug.Log("[PLS]PositionLocking lastSnapped Position = " + lastSnappedPosition.ToString() +  " info = " + lastSnappedNetInfo.name );
                
                locked = true;
                return locked;

            }
            /* 
             else if (locked == false && centerPosition != null && lastSnappedNetInfo != null && curved == true && curvedFirstPositionStored == false)
            {
                curvedLockFirstCenterPosition = centerPosition;
                Debug.Log("[PLS]PositionLocking.InitiateLock1 curvedLockFirstPosition = " + curvedLockFirstCenterPosition.ToString());
                curvedLockFirstOffset = Vector3.Distance(curvedLockFirstCenterPosition, lastSnappedPosition);
                Debug.Log("[PLS]PositionLocking.InitiateLock1 curvedLockFirstOffset = " + curvedLockFirstOffset.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock1 centerPosition = " + centerPosition.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock1 centerDirection.x = " + centerDirection.x.ToString() + " centerDirection.y = " + centerDirection.y.ToString() + " centerDirection.z = " + centerDirection.z.ToString());
                curvedLockFirstCenterDirection = centerDirection;
                curvedFirstPositionStored = true;
                lockedOntoACurve = false;
                locked = false;
                return locked;
            } else if (locked == false && lastSnappedPosition != null && lastSnappedNetInfo != null && curvedLockFirstCenterPosition != null && curvedLockFirstCenterPosition != Vector3.zero)
            {
                lockPosition = lastSnappedPosition;
                lockedSegmentID = lastSnappedSegmentID;
                lockedNetInfo = lastSnappedNetInfo;
                curvedLockSecondCenterPosition = centerPosition;
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockSecondPosition = " + curvedLockSecondCenterPosition.ToString());
                Vector3 curvedLockInitialDisplacement = curvedLockSecondCenterPosition - curvedLockFirstCenterPosition;
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockDisplacement = " + curvedLockInitialDisplacement.ToString());
                curvedLockSecondOffset = Vector3.Distance(curvedLockSecondCenterPosition, centerPosition);
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockSecondOffset = " + curvedLockSecondOffset.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 centerPosition = " + centerPosition.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 centerDirection.x = " + centerDirection.x.ToString() + " centerDirection.y = " + centerDirection.y.ToString()+ " centerDirection.z = " + centerDirection.z.ToString());
                curvedLockSecondCenterDirection = centerDirection;
                float dx = curvedLockInitialDisplacement.x;
                float dz = curvedLockInitialDisplacement.z;
                float displacementAngle = Mathf.Atan(dz / dx);
                float firstPositionAngle = Mathf.Atan(curvedLockFirstCenterDirection.z / curvedLockFirstCenterDirection.x);
                float alpha = displacementAngle - firstPositionAngle;
                float c = Mathf.Sqrt(dx*dx + dz*dz);
                curvedLockCircularRadius = c / (2 * Mathf.Sin(alpha));
                Vector3 dir1 = new Vector3(curvedLockFirstCenterDirection.z, 0, -curvedLockFirstCenterDirection.x);
                Vector3 dir2 = new Vector3(curvedLockSecondCenterDirection.z, 0, -curvedLockSecondCenterDirection.x);
                curvedLockDir = dir2;

                Debug.Log("[PLS]PositionLocking.InitiateLock2 alpha = " + alpha.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 c = " + c.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockCircularRadius = " + curvedLockCircularRadius.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 dir1x = " + dir1.x.ToString()+ " dir1z = " + dir1.z.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 dir2x = " + dir2.x.ToString() + " dir2z = " + dir2.z.ToString());

                Vector3 curvedLockCenterPosition1 = new Vector3(curvedLockFirstCenterPosition.x + dir1.x * curvedLockCircularRadius, 0, curvedLockFirstCenterPosition.z + dir1.z * curvedLockCircularRadius);
                Vector3 curvedLockCenterPosition2 = new Vector3(curvedLockSecondCenterPosition.x + dir2.x * curvedLockCircularRadius, 0, curvedLockSecondCenterPosition.z + dir2.z * curvedLockCircularRadius);
                curvedLockCircularCenterPosition = new Vector3(0, 0, 0);
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockCenterPosition1x = " + curvedLockCenterPosition1.x.ToString() + " curvedLockCenterPosition1z = " + curvedLockCenterPosition1.z.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockCenterPosition2x = " + curvedLockCenterPosition2.x.ToString() + " curvedLockCenterPosition2z = " + curvedLockCenterPosition2.z.ToString());
                if ((curvedLockCenterPosition1 - curvedLockCenterPosition2).magnitude <= 0.5)
                {
                    curvedLockCircularCenterPosition = (curvedLockCenterPosition1 + curvedLockCenterPosition2) / 2f;
                }
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockCircularCenterPosition.X = " + curvedLockCircularCenterPosition.x.ToString() + " curvedLockCircularCenterPosition.Z = " + curvedLockCircularCenterPosition.z.ToString());
                float dir1radian = Mathf.Atan(dir1.z / dir1.x);
                float dir2radian = Mathf.Atan(dir2.z / dir2.x);
                curvedLockRadianDifference = dir2radian - dir1radian;
                float dir3radian = dir2radian + curvedLockRadianDifference;
                Vector3 dir3 = new Vector3(Mathf.Cos(dir3radian),0, Mathf.Sin(dir3radian));
                Vector3 curvedLockThirdPosition = curvedLockCircularCenterPosition - dir3 * curvedLockCircularRadius;

                Debug.Log("[PLS]PositionLocking.InitiateLock2 dir1radian = " + dir1radian.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 dir2radian = " + dir2radian.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 radianDifference = " + curvedLockRadianDifference.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 dir3radian = " + dir3radian.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 dir3.X = " + dir3.x.ToString() + " dir3.Z = " + dir3.z.ToString());
                Debug.Log("[PLS]PositionLocking.InitiateLock2 curvedLockThirdPosition.X = " + curvedLockThirdPosition.x.ToString() + " curvedLockThirdPosition.Z = " + curvedLockThirdPosition.z.ToString());

               
                
                //curvedLockCalculatedDeltaAngle 
                lockedOntoACurve = true;
                locked = true;
                return locked;
            }
            */
            locked = false;
            return locked;
        }
        public static bool IsLocked()
        {
            return locked;
        }
        /*
        public static bool IsLockedOntoACurve()
        {
            return lockedOntoACurve;
        }*/
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
        /*
        public static void SetNextCurvedLockDir(Vector3 dir)
        {
            nextCurvedLockDir = dir;
        }*/
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
        
       /*
        public static Vector3 GetCurvedLockCircularCenterPosition()
        {
            return curvedLockCircularCenterPosition;
        }
        public static float GetCurvedLockCircularRadius()
        {
            return curvedLockCircularRadius;
        }
        public static float GetCurvedLockRadianDifference()
        {
            return curvedLockRadianDifference;
        }
        public static Vector3 GetCurvedLockDir()
        {
            return curvedLockDir;
        }
        public static void SetCurvedLockDir(Vector3 dir)
        {
            curvedLockDir = dir;
        }
       */
        public static void DisengageLock()
        {
            previousLockPosition = lockPosition;
            previousLockCenterDirection = centerDirection;
            previousLockCenterPosition = centerPosition;
            previousLockSnappedNetInfo = lockedNetInfo;
            previousLockParkingWidth = lastParkingWidth;
            previousLockSegmentID = lockedSegmentID;
            //previousLockWasOnACurve = lockedOntoACurve;
            //previousCurvedLockCircularCenterPosition = curvedLockCircularCenterPosition;
            //previousCurvedLockCircularRadius = curvedLockCircularRadius;
            //previousCurvedLockDir = curvedLockDir;
            //previousCurvedLockRadianDifference = curvedLockRadianDifference;
            lockPosition = new Vector3();
            centerPosition = new Vector3();
            centerDirection = new Vector3();
            lockedNetInfo = new NetInfo();
            //PSAPositionChain.Clear();
            lastParkingWidth = 0f;
            //Debug.Log("[PLS]PositionLocking disengageLock");
            //curvedFirstPositionStored = false;
            //curvedLockFirstCenterPosition = new Vector3();
            //curvedLockSecondCenterPosition = new Vector3();
            //curvedLockCircularCenterPosition = new Vector3();
            //curvedLockCircularRadius = 0f;
            //curvedLockRadianDifference = 0f;
            //curvedLockDir = new Vector3();
            //lockedOntoACurve = false;

            locked = false;

        }
       
        public static void AddParkingSpaceAsset(float parkingWidth)
        {
            
            lastParkingWidth = parkingWidth;
            lockPosition = lastSnappedPosition;
            lockedNetInfo = lastSnappedNetInfo;
            
            centerPosition = nextCenterPosition;
            /*if (lockedOntoACurve)
            {
                curvedLockDir = nextCurvedLockDir;
            }*/
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
                /*lockedOntoACurve = previousLockWasOnACurve;
                if (lockedOntoACurve)
                {
                    curvedLockRadianDifference = previousCurvedLockRadianDifference;
                    curvedLockDir = previousCurvedLockDir;
                    curvedLockCircularRadius = previousCurvedLockCircularRadius;
                    curvedLockCircularCenterPosition = previousCurvedLockCircularCenterPosition;
                }*/
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
