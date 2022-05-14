using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using System.Text;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ParkingLotSnapping
{
    class ParkingSpaceAssetAI : DummyBuildingAI
    {
        [CustomizableProperty("Offset", "Parking Space Asset")]
        public float m_offset = 0f;


        [CustomizableProperty("ParkingWidth", "Parking Space Asset")]
        public float m_parkingWidth = 0;

        [CustomizableProperty("Rotation", "Parking Space Asset")]
        public float m_rotation = 0;

        [CustomizableProperty("Parralel", "Parking Space Asset")]
        public bool m_parralel = false;

        public override bool IgnoreBuildingCollision()
        {
            return true;
        }
        protected override bool CanSufferFromFlood(out bool onlyCollapse)
        {
            onlyCollapse = false;
            return false;
        }
        protected override bool CanEvacuate()
        {
            return base.CanEvacuate();
        }
        public override float ElectricityGridRadius()
        {

            if (ModSettings.PassElectricity == true)
                return base.ElectricityGridRadius();
            else
                return 0f;
        }
        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            Building currentBuilding = BuildingManager.instance.m_buildings.m_buffer[buildingID];
            currentBuilding.Info.m_autoRemove = true;
            currentBuilding.Info.m_placementMode = BuildingInfo.PlacementMode.OnTerrain;
            if (ParkingSpaceGrid.areYouAwake() == false)
            {
                ParkingSpaceGrid.Awake();
            }
            ushort overlappedPSA = ParkingSpaceGrid.CheckGrid(currentBuilding.m_position);
            if (overlappedPSA != 0 && ModSettings.OverrideOverlapping) {
                try
                {
                    BuildingManager.instance.ReleaseBuilding(overlappedPSA);
                } catch(Exception e)
                {
                    Debug.Log("[PLS].ParkingSpaceAssetAI.CreateBuilding Tried to remove building " + overlappedPSA + " but encountered exception " + e.ToString());
                }
            }
            ParkingSpaceGrid.AddToGrid(buildingID);
            if (!PositionLocking.IsLocked() && ModSettings.AllowLocking)
            {
                try
                {
                    NetSegment lastSnappedSegment = NetManager.instance.m_segments.m_buffer[PositionLocking.GetLastSnappedSegmentID()];
                    if (lastSnappedSegment.IsStraight())
                    {
                        PositionLocking.InitiateLock(this.m_parkingWidth, currentBuilding.Info.name);
                    }/* else
                    {
                        bool curvedLock = PositionLocking.InitiateLock(this.m_parkingWidth, true);
                        Debug.Log("[PLS].ParkingSpaceAssetAI.CreateBuilding Trying to initiate Lock on Curve. Locked = " + curvedLock.ToString());
                    }*/
                }catch (Exception e)
                {
                    Debug.Log("[PLS].ParkingSpaceAssetAI.CreateBuilding Couldn't lock encoutnred exception " + e.ToString());
                }
                //Debug.Log("[PLS]PSAai CreateBuilding Initiating lock for asset named " + this.name);
            }
            else
            {
                PositionLocking.AddParkingSpaceAsset(this.m_parkingWidth);
                //Debug.Log("[PLS]PSAai CreateBuilding adding link in PSA chain!");
            }

            if (!Undo.areYouAwake()) Undo.Awake();
            Undo.addLotToStack(buildingID);

            base.CreateBuilding(buildingID, ref data);

        }
        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            if (ParkingSpaceGrid.areYouAwake() == false)
            {
                ParkingSpaceGrid.Awake();
            }
            try
            {
                ParkingSpaceGrid.RemoveFromGrid(buildingID);
            } catch (Exception e)
            {
                Debug.Log("[PLS].ParkingSpaceAssetAi.ReleaseBuilding Tried to remove Building from grid " + buildingID + " but encountered exception " + e.ToString());
            }

            if (!Undo.areYouAwake()) Undo.Awake();
            Undo.removeLotFromStack(buildingID); // This function will be called either by manual removal (bulldoze) or called by the Undo.releaseLastBuilding

            base.ReleaseBuilding(buildingID, ref data);
        }
        public override ToolBase.ToolErrors CheckBuildPosition(ushort relocateID, ref Vector3 position, ref float angle, float waterHeight, float elevation, ref Segment3 connectionSegment, out int productionRate, out int constructionCost)
        {
            Vector3 finalPosition;
            Vector3 finalAngle;
            finalPosition = Vector3.forward;
            Building currentBuilding = BuildingManager.instance.m_buildings.m_buffer[relocateID];
            currentBuilding.Info.m_placementMode = BuildingInfo.PlacementMode.OnTerrain;
            BuildingInfo info = currentBuilding.Info;
            if (ParkingSpaceGrid.areYouAwake() == false)
            {
                ParkingSpaceGrid.Awake();
            }
            // KEYBIND: Check if unlock keybind (whichever it is currently) is pressed
            if (ModSettings.AllowLocking != true && PositionLocking.IsLocked() || ModSettings.AllowSnapping != true && PositionLocking.IsLocked() || ModSettings.UnlockKeybind.IsPressed() && PositionLocking.IsLocked())
            {
                PositionLocking.DisengageLock();
            }
            // KEYBIND: Check if return keybind (whichever it is currently) is pressed
            if (PositionLocking.canReturnToPreviousLockPosition() == true && ModSettings.ReturnLockKeybind.IsPressed())
            {
                PositionLocking.returnToPreviousLockPosition();
            }
            if (ModSettings.AllowSnapping)
            {
                if (this.m_offset == 0f)
                {
                    float snapDistance = (float)ModSettings.SnappingDistance;
                    if (PositionLocking.IsLocked())
                    {
                        snapDistance = (float)ModSettings.UnlockingDistance;
                    }
                    if (this.SnapToRoad(position, out finalPosition, out finalAngle, snapDistance, currentBuilding))
                    {

                        angle = Mathf.Atan2(finalAngle.x, -finalAngle.z);
                        position.x = finalPosition.x;
                        position.z = finalPosition.z;


                    }
                    else if (PositionLocking.IsLocked())
                    {
                        PositionLocking.DisengageLock();
                    }
                }
                else
                {
                    float snapDistance = (float)ModSettings.SnappingDistance;
                    if (PositionLocking.IsLocked())
                    {
                        snapDistance = (float)ModSettings.UnlockingDistance;
                    }
                    if (this.SnapToOneSide(position, out finalPosition, out finalAngle, snapDistance))
                    {
                        angle = Mathf.Atan2(finalAngle.x, -finalAngle.z);
                        position.x = finalPosition.x;
                        position.z = finalPosition.z;
                    }
                    else if (PositionLocking.IsLocked())
                    {
                        PositionLocking.DisengageLock();
                    }
                }
            }
            bool flag;
            this.GetConstructionCost(relocateID != 0, out constructionCost, out flag);
            productionRate = 0;
            if (PositionLocking.IsLocked() == true && ParkingSpaceGrid.CheckGrid(finalPosition) != 0)
            {
                PositionLocking.SetOverlapping(true);
            } else
            {
                PositionLocking.SetOverlapping(false);
            }
            return ToolBase.ToolErrors.None;
        }
        private bool SnapToRoad(Vector3 refPos, out Vector3 pos, out Vector3 dir, float maxDistance, Building currentBuilding)
        {
            bool result = false;
            pos = refPos;
            dir = Vector3.forward;
            float additionalDistance = 100f;
            float minX = refPos.x - maxDistance - additionalDistance;
            float minZ = refPos.z - maxDistance - additionalDistance;
            float maxX = refPos.x + maxDistance + additionalDistance;
            float maxZ = refPos.z + maxDistance + additionalDistance;
            int minXint = Mathf.Max((int)(minX / 64f + 135f), 0);
            int minZint = Mathf.Max((int)(minZ / 64f + 135f), 0);
            int maxXint = Mathf.Min((int)(maxX / 64f + 135f), 269);
            int maxZint = Mathf.Min((int)(maxZ / 64f + 135f), 269);
            Vector3 centerPos = new Vector3();
            Vector3 centerDirection = Vector3.forward;
            int snappedSegmentID = 0;
            NetInfo info = new NetInfo();
            Array16<NetSegment> segments = Singleton<NetManager>.instance.m_segments;
            ushort[] segmentGrid = Singleton<NetManager>.instance.m_segmentGrid;
            float potentialMaxDistance = maxDistance;
            for (int i = minZint; i <= maxZint; i++)
            {
                for (int j = minXint; j <= maxXint; j++)
                {
                    ushort segmentGridZX = segmentGrid[i * 270 + j];
                    int iterator = 0;
                    while (segmentGridZX != 0)
                    {
                        NetSegment.Flags flags = segments.m_buffer[(int)segmentGridZX].m_flags;
                        if ((flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) == NetSegment.Flags.Created)
                        {
                            NetInfo potentialInfo = segments.m_buffer[(int)segmentGridZX].Info;
                            if (ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name))
                            {
                                Vector3 min = segments.m_buffer[(int)segmentGridZX].m_bounds.min;
                                Vector3 max = segments.m_buffer[(int)segmentGridZX].m_bounds.max;
                                if (min.x < maxX && min.z < maxZ && max.x > minX && max.z > minZ)
                                {
                                    Vector3 potentialCenterPos;
                                    Vector3 potentialCenterDirection;
                                    segments.m_buffer[(int)segmentGridZX].GetClosestPositionAndDirection(refPos, out potentialCenterPos, out potentialCenterDirection);
                                    float distanceToRoad = Vector3.Distance(potentialCenterPos, refPos) - potentialInfo.m_halfWidth;
                                    //Debug.Log("[APL].PSAAI.Snap to Road info.lanes = " + info.m_lanes.Length.ToString());
                                    //Debug.Log("[APL].PSAAI.Snap to Road info.m_halfwidth = " + info.m_halfWidth.ToString());
                                    if (distanceToRoad < potentialMaxDistance && ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name))
                                    {
                                        potentialMaxDistance = distanceToRoad;
                                        snappedSegmentID = segmentGridZX;
                                        centerPos = potentialCenterPos;
                                        centerDirection = potentialCenterDirection;
                                        info = potentialInfo;
                                        result = true;
                                    }


                                }
                            }
                        }
                        segmentGridZX = segments.m_buffer[(int)segmentGridZX].m_nextGridSegment;
                        if (++iterator >= 32768)
                        {
                            Debug.Log("[APL].PSAAI.SnapToRoad Invalid List Detected!!!");
                            break;
                        }
                    }
                }
            }
            if (result == true && snappedSegmentID != 0 && ModSettings.PLRCustomProperties.ContainsKey(info.name))
            {
                if (PositionLocking.IsLocked() && segments.m_buffer[snappedSegmentID].IsStraight() && Vector3.Distance(centerPos, PositionLocking.GetCenterPosition()) < maxDistance + info.m_halfWidth)
                {
                    centerPos = PositionLocking.GetCenterPosition();
                    centerDirection = PositionLocking.GetCenterDirection();
                    centerDirection.y = 0; //edit for steep slope problem
                    Vector3 centerDir = centerDirection.normalized;
                    float parkingOffset = this.m_parkingWidth;
                    if (PositionLocking.GetLastParkingWidth() > 0 && PositionLocking.GetLastParkingWidth() != this.m_parkingWidth)
                    {
                        parkingOffset = this.m_parkingWidth / 2 + PositionLocking.GetLastParkingWidth() / 2;
                        //Debug.Log("[PLS].PSAai Snap to Road Now Parking Offset = " + parkingOffset);
                    }


                    List<Vector3> alternativeCenterPositions = new List<Vector3> { centerPos + centerDir * parkingOffset, centerPos - centerDir * parkingOffset };

                    float distance = Vector3.Distance(centerPos, refPos);
                    //Debug.Log("[PLS].PSAai Snap to Road CenterPos = " + centerPos.ToString());
                    foreach (Vector3 alternativePosition in alternativeCenterPositions)
                    {
                        
                        float alternativeDistance = Vector3.Distance(alternativePosition, refPos);
                        //Debug.Log("[PLS].PSAai Snap to Road Checking AlternativeCenterPos = " + alternativePosition.ToString());
                        if (alternativeDistance < distance)
                        {
                            distance = alternativeDistance;
                            centerPos = alternativePosition;
                            //Debug.Log("[PLS].PSAai Snap to Road found AlternativeCenterPos = " + centerPos.ToString());
                        }
                    }
                    if (PositionLocking.IsLocked())
                    {
                        info = PositionLocking.GetLockedNetInfo();
                        PositionLocking.SetNextCenterPosition(centerPos);
                    }

                } else if (PositionLocking.IsLocked())
                {
                    PositionLocking.DisengageLock();
                }
                
                Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                dir = vector2.normalized;
                List<float> PLRsymetricAisleOffsets = ModSettings.PLRCustomProperties[info.name].SymetricAisleOffsets;
                float distance2 = maxDistance + info.m_halfWidth;
                foreach (float offset in PLRsymetricAisleOffsets)
                {
                    Vector3 alternativePosition = centerPos + dir * offset;
                    float alternativeDistance = Vector3.Distance(alternativePosition, refPos);
                    if (alternativeDistance < distance2)
                    {
                        distance2 = alternativeDistance;
                        pos = alternativePosition;
                    }
                }

                if (!PositionLocking.IsLocked())
                {
                    PositionLocking.SetSnappedNetInfo(info);
                    PositionLocking.SetSnappedPosition(pos);
                    PositionLocking.SetCenterPosition(centerPos);
                    PositionLocking.SetCenterDirection(centerDirection);
                    PositionLocking.SetSnappedSegmentId(snappedSegmentID);
                }
                else if (ParkingSpaceGrid.CheckGrid(pos) != 0)
                {
                    PositionLocking.SetSnappedNetInfo(info);
                    PositionLocking.SetSnappedPosition(pos);
                    PositionLocking.SetCenterPosition(centerPos);
                    PositionLocking.SetCenterDirection(centerDirection);
                    PositionLocking.SetSnappedSegmentId(snappedSegmentID);
                    //Debug.Log("[PLS]SnapToRoad Snap Rolling!");

                }
                
            }
            
            if (result == true && !PositionLocking.IsLocked() && ModSettings.AllowLocking && ModSettings.LockOnToPSAKeybind.IsPressed())
            {
                bool flag = false;
                Vector3 position = Vector3.forward;
                if (ParkingSpaceGrid.CheckGrid(pos) != 0)
                {
                    
                    position = pos;
                    flag = true;
                }
                else if (ParkingSpaceGrid.CheckGrid(refPos) != 0)
                {
                    position = refPos;
                    flag = true;
                }
                if (flag)
                {
                    Building parkingSpaceAsset = Singleton<BuildingManager>.instance.m_buildings.m_buffer[ParkingSpaceGrid.CheckGrid(position)];
                    ParkingSpaceAssetAI parkingSpaceAssetAI = parkingSpaceAsset.Info.m_buildingAI as ParkingSpaceAssetAI;
                    Vector3 centerPos2;
                    Vector3 centerDirection2;
                    segments.m_buffer[PositionLocking.GetLastSnappedSegmentID()].GetClosestPositionAndDirection(parkingSpaceAsset.m_position, out centerPos2, out centerDirection2);
                    if (segments.m_buffer[PositionLocking.GetLastSnappedSegmentID()].IsStraight())
                    {
                        pos = parkingSpaceAsset.m_position;
                        PositionLocking.SetSnappedPosition(pos);
                        PositionLocking.SetCenterPosition(centerPos2);
                        PositionLocking.SetCenterDirection(centerDirection2);
                        PositionLocking.InitiateLock(parkingSpaceAssetAI.m_parkingWidth, currentBuilding.Info.name);
                        //Debug.Log("[PLS]SnapToRoad Snaping to existing asset!");
                    }
                }
            }
            return result;
        }
        private bool SnapToOneSide(Vector3 refPos, out Vector3 pos, out Vector3 dir, float maxDistance)
        {
            bool result = false;
            pos = refPos;
            dir = Vector3.forward;
            float minX = refPos.x - maxDistance - 100f;
            float minZ = refPos.z - maxDistance - 100f;
            float maxX = refPos.x + maxDistance + 100f;
            float maxZ = refPos.z + maxDistance + 100f;
            int minXint = Mathf.Max((int)(minX / 64f + 135f), 0);
            int minZint = Mathf.Max((int)(minZ / 64f + 135f), 0);
            int maxXint = Mathf.Min((int)(maxX / 64f + 135f), 269);
            int maxZint = Mathf.Min((int)(maxZ / 64f + 135f), 269);
            Array16<NetSegment> segments = Singleton<NetManager>.instance.m_segments;
            ushort[] segmentGrid = Singleton<NetManager>.instance.m_segmentGrid;
            Vector3 centerPos = new Vector3();
            Vector3 centerDirection = Vector3.forward;
            int snappedSegmentID = 0;
            float potentialMaxDistance = maxDistance;
            NetInfo info = new NetInfo();
            for (int i = minZint; i <= maxZint; i++)
            {
                for (int j = minXint; j <= maxXint; j++)
                {
                    ushort segmentGridZX = segmentGrid[i * 270 + j];
                    int iterator = 0;
                    while (segmentGridZX != 0)
                    {
                        NetSegment.Flags flags = segments.m_buffer[(int)segmentGridZX].m_flags;
                        if ((flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) == NetSegment.Flags.Created)
                        {
                            NetInfo potentialInfo = segments.m_buffer[(int)segmentGridZX].Info;
                                
                            if (ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name) || ModSettings.assetCreatorMode == true && ModSettings.unacceptableInfoNames.Contains(potentialInfo.name) == false && potentialInfo.m_netAI is RoadAI)
                            {
                                Vector3 min = segments.m_buffer[(int)segmentGridZX].m_bounds.min;
                                Vector3 max = segments.m_buffer[(int)segmentGridZX].m_bounds.max;
                                if (min.x < maxX && min.z < maxZ && max.x > minX && max.z > minZ)
                                {
                                    Vector3 potentialCenterPos;
                                    Vector3 potentialCenterDirection;
                                    segments.m_buffer[(int)segmentGridZX].GetClosestPositionAndDirection(refPos, out potentialCenterPos, out potentialCenterDirection);
                                
                                    float distanceToRoad = Vector3.Distance(potentialCenterPos, refPos) - potentialInfo.m_halfWidth;
                                    bool parralelRoadCheck = true;
                                    if (ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name))
                                    {
                                        if (ModSettings.PLRCustomProperties[potentialInfo.name].Parralel && !this.m_parralel)
                                        {
                                            parralelRoadCheck = false;
                                        }
                                    }
                                    if (distanceToRoad < potentialMaxDistance && ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name) && parralelRoadCheck || distanceToRoad < potentialMaxDistance && ModSettings.assetCreatorMode == true && ModSettings.unacceptableInfoNames.Contains(potentialInfo.name) == false && potentialInfo.m_netAI is RoadAI)
                                    {
                                        potentialMaxDistance = distanceToRoad;
                                        snappedSegmentID = segmentGridZX;
                                        centerPos = potentialCenterPos;
                                        centerDirection = potentialCenterDirection;
                                        info = potentialInfo;
                                        result = true;
                                    }

                                }
                            } 
                            
                        }
                        segmentGridZX = segments.m_buffer[(int)segmentGridZX].m_nextGridSegment;
                        if (++iterator >= 32768)
                        {
                            Debug.Log("[APL].PSAAI.SnapToRoad Invalid List Detected!!!");
                            break;
                        }
                    }
                   
                }
            }
            /*asset Creator mod*/
            if (result == true && ModSettings.assetCreatorMode == true && ModSettings.unacceptableInfoNames.Contains(info.name) == false && info.m_netAI is RoadAI && info.name != ModSettings.lastAssetCreatorModeInfo)
            {

                Debug.Log("[PLS].Parking Lot Snapping Asset Creator Mode Log : " + info.name);
                List<float> laneOffsets = new List<float>();
                List<NetInfo.Lane> carLanes = new List<NetInfo.Lane>();
                Dictionary<float, NetInfo.Direction> offsetAndDirection = new Dictionary<float, NetInfo.Direction>();
                foreach (NetInfo.Lane lane in info.m_lanes)
                {
                    
                    if ((lane.m_vehicleType & VehicleInfo.VehicleType.Car) != 0)
                    {
                        Debug.Log("[PLS].Parking Lot Snapping Asset Creator Mode Log : lane offset = " + lane.m_position.ToString() + " lane type = " + lane.m_vehicleType.ToString() + " lane direction = " + lane.m_direction.ToString());

                        laneOffsets.Add(lane.m_position);
                        carLanes.Add(lane);
                        offsetAndDirection.Add(lane.m_position, lane.m_direction);
                    }
                    
                }
                if (laneOffsets.Count >= 2)
                {
                    List<float> symetricAisleOffsets = new List<float>();
                    List<float> asymetricAisleOffsets = new List<float>();
                    bool onesided = false;
                    bool invertOffset = false;
                    float roadwayMaxSeperation = 4f;
                    float asymetricOffsetAmount = 1.5f;
                    if (laneOffsets.Min() >= 0 || laneOffsets.Max() <= 0)
                    {
                        onesided = true;
                        invertOffset = true;
                        if (laneOffsets.Count == 2)
                        {
                            asymetricAisleOffsets.Add(laneOffsets.Average());
                            asymetricAisleOffsets.Add(-1*laneOffsets.Average());
                        }
                    }
                    else
                    {
                        laneOffsets.Sort();
                        for (int i = 1; i < laneOffsets.Count(); i++)
                        {
                            if (Mathf.Abs(laneOffsets[i] - laneOffsets[i - 1]) < roadwayMaxSeperation && offsetAndDirection[laneOffsets[i]] != offsetAndDirection[laneOffsets[i - 1]])
                            {
                                symetricAisleOffsets.Add((laneOffsets[i] + laneOffsets[i - 1]) / 2f);
                                asymetricAisleOffsets.Add((laneOffsets[i] + laneOffsets[i - 1]) / 2f);
                            }
                        }
                        if (symetricAisleOffsets.Count == 0 && asymetricAisleOffsets.Count == 0)
                        {
                            onesided = true;
                            asymetricAisleOffsets.Add(laneOffsets.Min() + asymetricOffsetAmount);
                            asymetricAisleOffsets.Add(laneOffsets.Max() - asymetricOffsetAmount);
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append(info.name);
                    sb.Append(", {");
                    foreach(float offset in symetricAisleOffsets)
                    {
                        sb.Append(offset.ToString());
                        sb.Append(", ");
                    }
                    sb.Append("}, {");
                    foreach (float offset in asymetricAisleOffsets)
                    {
                        sb.Append(offset.ToString());
                        sb.Append(", ");
                    }
                    sb.Append("}, ");
                    sb.Append(onesided.ToString());
                    sb.Append(", ");
                    sb.Append(invertOffset.ToString());
                    sb.Append(";");
                    Debug.Log(sb.ToString());
                    ModSettings.PLRCustomPropertiesClass assetCreatorPLRCustomProperties = new ModSettings.PLRCustomPropertiesClass(symetricAisleOffsets, asymetricAisleOffsets, onesided, invertOffset, false);
                }
                ModSettings.lastAssetCreatorModeInfo = info.name;
            }
            /*end asset crator mode*/
             
            if (result == true && snappedSegmentID != 0 && ModSettings.PLRCustomProperties.ContainsKey(info.name))
            {
                if (PositionLocking.IsLocked() && segments.m_buffer[snappedSegmentID].IsStraight() && Vector3.Distance(centerPos, PositionLocking.GetCenterPosition()) < maxDistance + info.m_halfWidth)
                {
                    centerPos = PositionLocking.GetCenterPosition();
                    centerDirection = PositionLocking.GetCenterDirection();
                    centerDirection.y = 0; //edit for steep slope problem.
                    Vector3 centerDir = centerDirection.normalized;
                    
                    float parkingOffset = this.m_parkingWidth;
                    if (PositionLocking.GetLastParkingWidth() > 0 && PositionLocking.GetLastParkingWidth() != this.m_parkingWidth)
                    {
                        parkingOffset = this.m_parkingWidth / 2 + PositionLocking.GetLastParkingWidth() / 2;
                        //Debug.Log("[PLS].PSAai Snap to Road Now Parking Offset = " + parkingOffset);
                    }


                    List<Vector3> alternativeCenterPositions = new List<Vector3> { centerPos + centerDir * parkingOffset, centerPos - centerDir * parkingOffset };

                    float distance = Vector3.Distance(centerPos, refPos);
                    //Debug.Log("[PLS].PSAai Snap to Road CenterPos = " + centerPos.ToString());
                    foreach (Vector3 alternativePosition in alternativeCenterPositions)
                    {
                        float alternativeDistance = Vector3.Distance(alternativePosition, refPos);
                        //Debug.Log("[PLS].PSAai Snap to Road Checking AlternativeCenterPos = " + alternativePosition.ToString());
                        if (alternativeDistance < distance)
                        {
                            distance = alternativeDistance;
                            centerPos = alternativePosition;
                            //Debug.Log("[PLS].PSAai Snap to Road found AlternativeCenterPos = " + centerPos.ToString());
                        }
                    }
                    if (PositionLocking.IsLocked())
                    {
                        info = PositionLocking.GetLockedNetInfo();
                        PositionLocking.SetNextCenterPosition(centerPos);

                    }
                }
                else if (PositionLocking.IsLocked() /*&& PositionLocking.IsLockedOntoACurve() == false*/)
                {
                    PositionLocking.DisengageLock();
                }
                /*
                //Locked onto a curved doesn't actually work yet
                else if (PositionLocking.IsLocked() == true && PositionLocking.IsLockedOntoACurve() == true && segments.m_buffer[snappedSegmentID].IsStraight() == false && Vector3.Distance(centerPos, PositionLocking.GetCenterPosition()) < maxDistance + info.m_halfWidth)
                {
                    centerPos = PositionLocking.GetCenterPosition();
                    centerDirection = PositionLocking.GetCenterDirection();
                    Vector3 centerDir = centerDirection.normalized;
                    if (PositionLocking.GetLastParkingWidth() > 0 && PositionLocking.GetLastParkingWidth() != this.m_parkingWidth)
                    {
                        PositionLocking.DisengageLock();
                        //Debug.Log("[PLS].PSAai DisengagingLock Cannot change parking width on curves.");
                    }
                    
                    Vector3 lockedCircularCenterPosition = PositionLocking.GetCurvedLockCircularCenterPosition();
                    float lockedCircularRadius = PositionLocking.GetCurvedLockCircularRadius();
                    Vector3 lockedDir = PositionLocking.GetCurvedLockDir();
                    float lockedDirRadian = Mathf.Atan(lockedDir.z / lockedDir.x);
                    float radianDifference = PositionLocking.GetCurvedLockRadianDifference();
                    
                    Vector3 dirPos = new Vector3(Mathf.Cos(lockedDirRadian+radianDifference), 0, Mathf.Sin(lockedDirRadian + radianDifference));
                    Vector3 dirNeg = new Vector3(Mathf.Cos(lockedDirRadian - radianDifference), 0, Mathf.Sin(lockedDirRadian - radianDifference));
                    Debug.Log("[PLS].PSAai Snap to Curved Road dirPos.X = " + dirPos.x.ToString()+ " dirPos.z = " + dirPos.z.ToString());
                    Debug.Log("[PLS].PSAai Snap to Curved Road dirNeg.X = " + dirNeg.x.ToString() + " dirNeg.z = " + dirNeg.z.ToString());
                    Vector3 newPosPosition = lockedCircularCenterPosition - dirPos * lockedCircularRadius;
                    Vector3 newNegPosition = lockedCircularCenterPosition - dirNeg * lockedCircularRadius;
                    Debug.Log("[PLS].PSAai Snap to Curved Road newPosPosition.X = " + newPosPosition.x.ToString() + " newPosPosition.z = " + newPosPosition.z.ToString());
                    newPosPosition.y = centerPos.y;
                    Debug.Log("[PLS].PSAai Snap to Curved Road newNegPosition.X = " + newNegPosition.x.ToString() + " newNegPosition.z = " + newNegPosition.z.ToString());
                    newNegPosition.y = centerPos.y;

                    Dictionary<Vector3, Vector3> dirDictionary = new Dictionary<Vector3, Vector3>{ { centerPos, lockedDir } };
                    if (!dirDictionary.ContainsKey(newPosPosition)) dirDictionary.Add(newPosPosition, dirPos);
                    if (!dirDictionary.ContainsKey(newNegPosition)) dirDictionary.Add(newNegPosition, dirNeg);
                 
                    List<Vector3> alternativeCenterPositions = new List<Vector3> {centerPos, newPosPosition, newNegPosition };
                    float distance = Vector3.Distance(centerPos, refPos);
                    Debug.Log("[PLS].PSAai Snap to Curved Road CenterPos = " + centerPos.ToString());
                    foreach (Vector3 alternativePosition in alternativeCenterPositions)
                    {
                        float alternativeDistance = Vector3.Distance(alternativePosition, refPos);
                        Debug.Log("[PLS].PSAai Snap to Curved Road Checking AlternativeCenterPos = " + alternativePosition.ToString());
                        
                        if (alternativeDistance < distance)
                        {
                            distance = alternativeDistance;
                            centerPos = alternativePosition;
                           
                            if (dirDictionary.ContainsKey(alternativePosition))
                            {
                                PositionLocking.SetNextCurvedLockDir(dirDictionary[alternativePosition]);
                                Debug.Log("[PLS].PSAai dirDictionary[alternativePosition].x = " + dirDictionary[alternativePosition].x.ToString() + "dirDictionary[alternativePosition].z = " + dirDictionary[alternativePosition].z.ToString());
                                
                            }
                            Debug.Log("[PLS].PSAai Snap to Curved Road found AlternativeCenterPos = " + centerPos.ToString());
                        }
                    }
                    if (PositionLocking.IsLocked())
                    {
                        info = PositionLocking.GetLockedNetInfo();
                        PositionLocking.SetNextCenterPosition(centerPos);

                    }
                }*/
                bool logging = false;
                float distanceToRoad = Vector3.Distance(centerPos, refPos) - info.m_halfWidth;
                Vector3 offsetDir = new Vector3();
                if (distanceToRoad < maxDistance && ModSettings.PLRCustomProperties.ContainsKey(info.name))
                {
                    //Debug.Log(info.name);

                    if (this.m_rotation == 0f)
                    {
                        Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                        dir = vector2.normalized;
                        offsetDir = dir;
                    }
                    else if (this.m_rotation == 90f)
                    {
                        if (logging) Debug.Log("[PLS].PSAai.SnapToOneSide this.m_rotation = " + this.m_rotation.ToString());
                        Vector3 vector2 = new Vector3(centerDirection.x, 0f, centerDirection.z);
                        dir = vector2.normalized;
                        Vector3 vector3 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                        offsetDir = vector3.normalized;
                    } else if (this.m_rotation == 270f)
                    {
                        if (logging) Debug.Log("[PLS].PSAai.SnapToOneSide this.m_rotation = " + this.m_rotation.ToString());
                        Vector3 vector2 = new Vector3(-centerDirection.x, 0f, -centerDirection.z);
                        dir = vector2.normalized;
                        Vector3 vector3 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                        offsetDir = vector3.normalized;
                    }
                    //now there are two rotations you can't rely on dir being the same direction that you offsite your isle positions.
                   
                    
                    List<float> PLRasymetricAisleOffsets = ModSettings.PLRCustomProperties[info.name].AsymetricAisleOffsets;
                    Dictionary<Vector3, Vector3> alternativePositions = new Dictionary<Vector3, Vector3>();
                    foreach (float aisleOffset in PLRasymetricAisleOffsets)
                    {
                        Vector3 alternativeAislePosition = new Vector3();
                       
                        alternativeAislePosition = centerPos + offsetDir * aisleOffset; 
                        
                       
                        float alternativeAisleDistance = Vector3.Distance(alternativeAislePosition, refPos);
                        if (ModSettings.PLRCustomProperties[info.name].Onesided == false)
                        {
                            alternativePositions.Add(alternativeAislePosition + offsetDir * this.m_offset, alternativeAislePosition);
                            alternativePositions.Add(alternativeAislePosition - offsetDir * this.m_offset, alternativeAislePosition);
                        }
                        else
                        {
                            Vector3 tempDir = new Vector3(alternativeAislePosition.x - centerPos.x, 0, alternativeAislePosition.z - centerPos.z);
                            tempDir.Normalize();
                            
                            if (ModSettings.PLRCustomProperties[info.name].InvertOffset)
                            {
                                tempDir *= -1;
                            }
                            alternativePositions.Add(alternativeAislePosition + tempDir * this.m_offset, alternativeAislePosition);
                           
                        }
                    }
                    float distance2 = maxDistance + info.m_halfWidth;
                    
                    foreach (KeyValuePair<Vector3, Vector3> alternativePosition in alternativePositions)
                    {
                        float alternativeDistance = Vector3.Distance(alternativePosition.Key, refPos);
                       
                        if (alternativeDistance < distance2)
                        {
                            distance2 = alternativeDistance;
                            pos = alternativePosition.Key;
                           
                            Vector3 tempDir = alternativePosition.Key - alternativePosition.Value;
                            dir = tempDir.normalized; //may need to be adjusted for asset rotation
                            if (this.m_rotation == 90f)
                            {
                                Vector3 vector4 = new Vector3(tempDir.z, tempDir.y, -tempDir.x);
                                dir = vector4.normalized;
                            } else if (this.m_rotation == 270f)
                            {
                                Vector3 vector4 = new Vector3(-tempDir.z, tempDir.y, tempDir.x);
                                dir = vector4.normalized;
                            }
                        }
                    }
                    
                    result = true;
                    if (!PositionLocking.IsLocked())
                    {
                        PositionLocking.SetSnappedNetInfo(info);
                        PositionLocking.SetSnappedPosition(pos);
                        PositionLocking.SetCenterPosition(centerPos);
                        PositionLocking.SetCenterDirection(centerDirection);
                        PositionLocking.SetSnappedSegmentId(snappedSegmentID);
                    }
                    else if (ParkingSpaceGrid.CheckGrid(pos) != 0)
                    {
                        PositionLocking.SetSnappedNetInfo(info);
                        PositionLocking.SetSnappedPosition(pos);
                        PositionLocking.SetCenterPosition(centerPos);
                        PositionLocking.SetCenterDirection(centerDirection);
                        PositionLocking.SetSnappedSegmentId(snappedSegmentID);
                        //Debug.Log("[PLS]SnapToRoad Snap Rolling!");

                    }
                }
            }
            if (result == true && !PositionLocking.IsLocked() && ModSettings.AllowLocking && ModSettings.LockOnToPSAKeybind.IsPressed())
            {
                bool flag = false;
                Vector3 position = Vector3.forward;
                if (ParkingSpaceGrid.CheckGrid(pos) != 0)
                {
                    position = pos;
                    flag = true;
                }
                else if (ParkingSpaceGrid.CheckGrid(refPos) != 0)
                {
                    position = refPos;
                    flag = true;
                }
                if (flag)
                {
                    Building parkingSpaceAsset = Singleton<BuildingManager>.instance.m_buildings.m_buffer[ParkingSpaceGrid.CheckGrid(position)];
                    ParkingSpaceAssetAI parkingSpaceAssetAI = parkingSpaceAsset.Info.m_buildingAI as ParkingSpaceAssetAI;
                    Vector3 centerPos2;
                    Vector3 centerDirection2;
                    segments.m_buffer[PositionLocking.GetLastSnappedSegmentID()].GetClosestPositionAndDirection(parkingSpaceAsset.m_position, out centerPos2, out centerDirection2);
                    if (segments.m_buffer[PositionLocking.GetLastSnappedSegmentID()].IsStraight())
                    {
                        pos = parkingSpaceAsset.m_position;
                        PositionLocking.SetSnappedPosition(pos);
                        PositionLocking.SetCenterPosition(centerPos2);
                        PositionLocking.SetCenterDirection(centerDirection2);
                        PositionLocking.InitiateLock(parkingSpaceAssetAI.m_parkingWidth, parkingSpaceAsset.Info.name);
                        //Debug.Log("[PLS]SnapToRoad Snaping to existing asset!");
                    }
                }
            }
            return result;
        }
        

        // Undo method properties
        private static bool UndoEnabled = true;
        private static uint UndoCount = 0; // Number of undo actions performed in a row
        private static double CurrentUndoTime = SlowUndoTime; // Current Undo speed (fast or slow)
        private static DateTime LastUndoTime; // Time of last Undo action
        private const double SlowUndoTime = 0.8; // Time between undo actions that will be performed (slow)
        private const double FastUndoTime = 0.15; // (fast)
        private const uint SlowToFastUndoThreshold = 5; // How many undos to switch from slow to fast undo

        public override string GetConstructionInfo(int productionRate)
        {
            // KEYBIND: Check if undo keybind (whichever it is currently) is pressed
            if (!ModSettings.UndoKeybind.IsPressed()) // When key ot pressed, Undo is reset
            {
                UndoEnabled = true; // Enable future undo
                UndoCount = 0;
                CurrentUndoTime = SlowUndoTime;
            }
            else // When undo key pressed
            {
                if (UndoEnabled) // If undo is enabled
                {
                    UndoEnabled = false; // Disable undo temporarily
                    Undo.releaseLastBuilding(); // Perform Undo
                    LastUndoTime = DateTime.Now;
                    UndoCount++;
                    if (ModSettings.UndoDisengagesLock && PositionLocking.IsLocked()) PositionLocking.DisengageLock();
                }
                else
                {
                    if ((DateTime.Now - LastUndoTime).TotalSeconds >= CurrentUndoTime) UndoEnabled = true; // Enables undo after enough time passed
                    if ((CurrentUndoTime >= SlowUndoTime) && (UndoCount >= SlowToFastUndoThreshold)) CurrentUndoTime = FastUndoTime; // If undo count surpassed threshold and we're in slow speed, switch to fast speed
                }
            }

            if (ModSettings.DisableTooltips)
            {
                return "";
            }
            if (PositionLocking.IsLocked() && PositionLocking.GetOverlapping() == false)
            {
                if (ModSettings.KeyboardShortcutHints == false)
                {
                    return "Locked!";
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Locked!");
                // KEYBIND: To obtain current keybind's setting in "string form", just use SavedInputKey.ToLocalizedString("KEYNAME")
                sb.Append("Press " + ModSettings.UnlockKeybind.ToLocalizedString("KEYNAME") + " to Unlock.");
                return sb.ToString();
            } else if (PositionLocking.IsLocked())
            {
                if (ModSettings.KeyboardShortcutHints == false)
                {
                    return "Locked but Overlapping!";
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Locked but Overlapping!");
                sb.AppendLine("Placing a new Parking Space Asset ");
                if (ModSettings.OverrideOverlapping)
                {
                    sb.Append("here will override existing.");
                } else
                {
                    sb.Append("here may causes problems latter!");
                }
                return sb.ToString();
            } else if (PositionLocking.canReturnToPreviousLockPosition() && ModSettings.KeyboardShortcutHints == true)
            {
                // KEYBIND: To obtain current keybind's setting in "string form", just use SavedInputKey.ToLocalizedString("KEYNAME")
                return "Press " + ModSettings.ReturnLockKeybind.ToLocalizedString("KEYNAME") + " to Re-lock to last locked position.";
            }
            return "";
        }
    }
}