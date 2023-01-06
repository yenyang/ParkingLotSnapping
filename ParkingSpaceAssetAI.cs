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
            if (!ModSettings.PSACustomProperties.ContainsKey(this.m_info.name))
            {
                Debug.Log("[PLS].PSAai.CreateBuilding PSA Custom Properties does not contain " + this.m_info.name);
                return;
            }
            
            data.Info.m_autoRemove = true;
            
            if (ModSettings.PSACustomProperties[this.m_info.name].Raisable == false)
            {
                data.Info.m_placementMode = BuildingInfo.PlacementMode.OnTerrain;
            } else
            {
                data.Info.m_placementMode = BuildingInfo.PlacementMode.OnGround;
                if (PositionLocking.GetRaisableLockedY() != 0)
                {
                    //Debug.Log("[RF]PSAai.createBuilding data.m_position.y = " + data.m_position.y.ToString() + " PositionLocking.GetRaisableLockedY() = " + PositionLocking.GetRaisableLockedY().ToString());
                    Vector3 newPosition = new Vector3(data.m_position.x, PositionLocking.GetRaisableLockedY(), data.m_position.z);
                    data.m_flags = data.m_flags | Building.Flags.FixedHeight;

                    ElevateBuilding(buildingID, ref data, newPosition, data.m_angle);
                }
            }
            if (ParkingSpaceGrid.areYouAwake() == false)
            {
                ParkingSpaceGrid.Awake();
            }
            ushort overlappedPSA = ParkingSpaceGrid.CheckGrid(data.m_position);
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
                        PositionLocking.InitiateLock(ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth, data.Info.name);
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
                PositionLocking.AddParkingSpaceAsset(ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth);
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
            if (ModSettings.AllowSnapping && ModSettings.PSACustomProperties.ContainsKey(this.m_info.name))
            {
                
                if (ModSettings.PSACustomProperties[this.m_info.name].Offset == 0f)
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
                        if (ModSettings.PSACustomProperties[this.m_info.name].Raisable)
                        {
                            position.y = finalPosition.y;
                            PositionLocking.SetRaisableLockedY(finalPosition.y);
                        }

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
                        if (ModSettings.PSACustomProperties[this.m_info.name].Raisable)
                        {
                            position.y = finalPosition.y;
                            PositionLocking.SetRaisableLockedY(finalPosition.y);
                        }
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
            if (!ModSettings.PSACustomProperties.ContainsKey(this.m_info.name))
            {
                Debug.Log("[PLS].PSAai.SnapToRoad PSA Custom Properties does not contain " + this.m_info.name);
                return false;
            }
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
            List<ushort> potentialInfoIds = new List<ushort>();
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
                                        if (ModSettings.PSACustomProperties[this.name].Raisable == true || ModSettings.PSACustomProperties[this.name].Raisable == false && potentialInfo.m_netAI is RoadAI)
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
                    float parkingOffset = ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth;
                    if (PositionLocking.GetLastParkingWidth() > 0 && PositionLocking.GetLastParkingWidth() != ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth)
                    {
                        parkingOffset = ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth / 2 + PositionLocking.GetLastParkingWidth() / 2;
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
                List<float> PLRsymmetricAisleOffsets = ModSettings.PLRCustomProperties[info.name].SymmetricAisleOffsets;
                float distance2 = maxDistance + info.m_halfWidth;
                foreach (float offset in PLRsymmetricAisleOffsets)
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
                        PositionLocking.InitiateLock(ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth, currentBuilding.Info.name);
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
            if (!ModSettings.PSACustomProperties.ContainsKey(this.m_info.name))
            {
                Debug.Log("[PLS].PSAai.SnapToOneSide PSA Custom Properties does not contain " + this.m_info.name);
                return false;
            }
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
            List<ushort> potentialInfoIds = new List<ushort>();
            float potentialMaxDistance = maxDistance;
            //float potentialMinHeight = 0f;
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
                            
                            if (ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name) || ModSettings.AssetCreatorMode == true && ModSettings.unacceptableInfoNames.Contains(potentialInfo.name) == false /*&& potentialInfo.m_netAI is RoadAI*/)
                            {
                                
                                Vector3 min = segments.m_buffer[(int)segmentGridZX].m_bounds.min;
                                Vector3 max = segments.m_buffer[(int)segmentGridZX].m_bounds.max;
                                if (min.x < maxX && min.z < maxZ && max.x > minX && max.z > minZ)
                                {
                                    Vector3 potentialCenterPos;
                                    Vector3 potentialCenterDirection;
                                    segments.m_buffer[(int)segmentGridZX].GetClosestPositionAndDirection(refPos, out potentialCenterPos, out potentialCenterDirection);

                                    float distanceToRoad = Vector3.Distance(potentialCenterPos, refPos) - potentialInfo.m_halfWidth;
                                    bool parallelRoadCheck = true;
                                    if (ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name))
                                    {
                                        if (ModSettings.PLRCustomProperties[potentialInfo.name].Parallel && !ModSettings.PSACustomProperties[this.m_info.name].Parallel)
                                        {
                                            parallelRoadCheck = false;
                                        }
                                    }
                                    if (distanceToRoad < potentialMaxDistance && ModSettings.PLRCustomProperties.ContainsKey(potentialInfo.name) && parallelRoadCheck || distanceToRoad < potentialMaxDistance && ModSettings.AssetCreatorMode == true && ModSettings.unacceptableInfoNames.Contains(potentialInfo.name) == false)
                                    {
                                        if (ModSettings.PSACustomProperties[this.name].Raisable == true || ModSettings.PSACustomProperties[this.name].Raisable == false && potentialInfo.m_netAI is RoadAI || ModSettings.AssetCreatorMode == true)
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
            if (result == true && ModSettings.AssetCreatorMode == true && ModSettings.unacceptableInfoNames.Contains(info.name) == false /*&& info.m_netAI is RoadAI*/ && info.name != ModSettings.lastAssetCreatorModeInfo && !ModSettings.PLRCustomProperties.ContainsKey(info.name))
            {
                Debug.Log("[PLS].SnapToOneSide refpos.y = " + refPos.y.ToString());
                Debug.Log("[PLS].SnapToOneSide centerPos.y = " + centerPos.y.ToString());
                Debug.Log("[PLS].Parking Lot Snapping Asset Creator Mode Log : " + info.name);
                List<float> laneOffsets = new List<float>();
                List<NetInfo.Lane> carLanes = new List<NetInfo.Lane>();
                Dictionary<float, NetInfo.Direction> offsetAndDirection = new Dictionary<float, NetInfo.Direction>();
                foreach (NetInfo.Lane lane in info.m_lanes)
                {
                    
                    if ((lane.m_vehicleType & VehicleInfo.VehicleType.Car) != 0)
                    {
                        Debug.Log("[PLS].Parking Lot Snapping Asset Creator Mode Log : lane offset = " + lane.m_position.ToString() + " lane type = " + lane.m_vehicleType.ToString() + " lane direction = " + lane.m_direction.ToString());
                        if (!laneOffsets.Contains(lane.m_position)) laneOffsets.Add(lane.m_position);
                        if (!carLanes.Contains(lane)) carLanes.Add(lane);
                        if (!offsetAndDirection.ContainsKey(lane.m_position))    offsetAndDirection.Add(lane.m_position, lane.m_direction);
                    }
                    
                }
                if (laneOffsets.Count >= 2)
                {
                    List<float> symmetricAisleOffsets = new List<float>();
                    List<float> asymmetricAisleOffsets = new List<float>();
                    bool onesided = false;
                    bool invertOffset = false;
                    float roadwayMaxSeperation = 4f;
                    float asymmetricOffsetAmount = 1.5f;
                    if (laneOffsets.Min() >= 0 || laneOffsets.Max() <= 0)
                    {
                        onesided = true;
                        invertOffset = true;
                        if (laneOffsets.Count == 2)
                        {
                            if (!asymmetricAisleOffsets.Contains(laneOffsets.Average()))     asymmetricAisleOffsets.Add(laneOffsets.Average());
                            if(!asymmetricAisleOffsets.Contains(-1 * laneOffsets.Average())) asymmetricAisleOffsets.Add(-1*laneOffsets.Average());
                        }
                    }
                    else
                    {
                        laneOffsets.Sort();
                        for (int i = 1; i < laneOffsets.Count(); i++)
                        {
                            if (Mathf.Abs(laneOffsets[i] - laneOffsets[i - 1]) < roadwayMaxSeperation && offsetAndDirection[laneOffsets[i]] != offsetAndDirection[laneOffsets[i - 1]])
                            {
                                if (!symmetricAisleOffsets.Contains((laneOffsets[i] + laneOffsets[i - 1]) / 2f))
                                {
                                    symmetricAisleOffsets.Add((laneOffsets[i] + laneOffsets[i - 1]) / 2f);
                                }
                                if (!asymmetricAisleOffsets.Contains((laneOffsets[i] + laneOffsets[i - 1]) / 2f))
                                {
                                    asymmetricAisleOffsets.Add((laneOffsets[i] + laneOffsets[i - 1]) / 2f);
                                }
                            }
                        }
                        if (symmetricAisleOffsets.Count == 0 && asymmetricAisleOffsets.Count == 0)
                        {
                            onesided = true;
                            if (!asymmetricAisleOffsets.Contains(laneOffsets.Min() + asymmetricOffsetAmount)) asymmetricAisleOffsets.Add(laneOffsets.Min() + asymmetricOffsetAmount);
                            if (!asymmetricAisleOffsets.Contains(laneOffsets.Max() - asymmetricOffsetAmount)) asymmetricAisleOffsets.Add(laneOffsets.Max() - asymmetricOffsetAmount);
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append(info.name);
                    sb.Append(", {");
                    foreach(float offset in symmetricAisleOffsets)
                    {
                        sb.Append(offset.ToString());
                        sb.Append(", ");
                    }
                    sb.Append("}, {");
                    foreach (float offset in asymmetricAisleOffsets)
                    {
                        sb.Append(offset.ToString());
                        sb.Append(", ");
                    }
                    sb.Append("}, ");
                    sb.Append(onesided.ToString());
                    sb.Append(", ");
                    sb.Append(invertOffset.ToString());
                    sb.Append(", False, ");
                    sb.Append(info.name);
                    sb.Append(", ");
                    string filename = info.name;
                    filename = filename.Replace(".", "-");
                    filename = filename.Replace("/", " and ");
                    filename = filename.Replace("°", " deg");
                    sb.Append(filename);
                    sb.Append("}");
                    sb.Append(",");
                    Debug.Log(sb.ToString());
                    if (!ModSettings.PLRCustomProperties.ContainsKey(info.name))
                    {
                        ModSettings.PLRCustomPropertiesClass assetCreatorPLRCustomProperties = new ModSettings.PLRCustomPropertiesClass(symmetricAisleOffsets, asymmetricAisleOffsets, onesided, invertOffset, false, info.name, filename, 0f);
                        Debug.Log("[PLS].PSAai.SnapToOneSide.AssetCreatorMode: Export PLR properties = " + ModSettings.SerializeOnePLR(assetCreatorPLRCustomProperties).ToString());
                        Debug.Log("[PLS].PSAai.SnapToOneSide.AssetCreatorMode: Import PLR properties = " + ModSettings.DeserializeOnePLR(assetCreatorPLRCustomProperties).ToString());
                    }

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
                    
                    float parkingOffset = ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth;
                    if (PositionLocking.GetLastParkingWidth() > 0 && PositionLocking.GetLastParkingWidth() != ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth)
                    {
                        parkingOffset = ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth / 2 + PositionLocking.GetLastParkingWidth() / 2;
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

                    if (ModSettings.PSACustomProperties[this.m_info.name].Rotation == 0f)
                    {
                        Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                        dir = vector2.normalized;
                        offsetDir = dir;
                    }
                    else if (ModSettings.PSACustomProperties[this.m_info.name].Rotation == 90f)
                    {
                        if (logging) Debug.Log("[PLS].PSAai.SnapToOneSide this.m_rotation = " + ModSettings.PSACustomProperties[this.m_info.name].Rotation.ToString());
                        Vector3 vector2 = new Vector3(centerDirection.x, 0f, centerDirection.z);
                        dir = vector2.normalized;
                        Vector3 vector3 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                        offsetDir = vector3.normalized;
                    } else if (ModSettings.PSACustomProperties[this.m_info.name].Rotation == 270f)
                    {
                        if (logging) Debug.Log("[PLS].PSAai.SnapToOneSide this.m_rotation = " + ModSettings.PSACustomProperties[this.m_info.name].Rotation.ToString());
                        Vector3 vector2 = new Vector3(-centerDirection.x, 0f, -centerDirection.z);
                        dir = vector2.normalized;
                        Vector3 vector3 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                        offsetDir = vector3.normalized;
                    }
                    //now there are two rotations you can't rely on dir being the same direction that you offsite your isle positions.
                   
                    
                    List<float> PLRasymmetricAisleOffsets = ModSettings.PLRCustomProperties[info.name].AsymmetricAisleOffsets;
                    Dictionary<Vector3, Vector3> alternativePositions = new Dictionary<Vector3, Vector3>();
                    foreach (float aisleOffset in PLRasymmetricAisleOffsets)
                    {
                        Vector3 alternativeAislePosition = new Vector3();
                       
                        alternativeAislePosition = centerPos + offsetDir * aisleOffset; 
                        
                       
                        float alternativeAisleDistance = Vector3.Distance(alternativeAislePosition, refPos);
                        if (ModSettings.PLRCustomProperties[info.name].Onesided == false)
                        {
                            if (!alternativePositions.ContainsKey(alternativeAislePosition + offsetDir * ModSettings.PSACustomProperties[this.m_info.name].Offset)) alternativePositions.Add(alternativeAislePosition + offsetDir * ModSettings.PSACustomProperties[this.m_info.name].Offset, alternativeAislePosition);
                            if (!alternativePositions.ContainsKey(alternativeAislePosition - offsetDir * ModSettings.PSACustomProperties[this.m_info.name].Offset)) alternativePositions.Add(alternativeAislePosition - offsetDir * ModSettings.PSACustomProperties[this.m_info.name].Offset, alternativeAislePosition);
                        }
                        else
                        {
                            Vector3 tempDir = new Vector3(alternativeAislePosition.x - centerPos.x, 0, alternativeAislePosition.z - centerPos.z);
                            tempDir.Normalize();
                            
                            if (ModSettings.PLRCustomProperties[info.name].InvertOffset)
                            {
                                tempDir *= -1;
                            }
                            if (!alternativePositions.ContainsKey(alternativeAislePosition + tempDir * ModSettings.PSACustomProperties[this.m_info.name].Offset)) alternativePositions.Add(alternativeAislePosition + tempDir * ModSettings.PSACustomProperties[this.m_info.name].Offset, alternativeAislePosition);
                           
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
                            if (ModSettings.PSACustomProperties[this.m_info.name].Rotation == 90f)
                            {
                                Vector3 vector4 = new Vector3(tempDir.z, tempDir.y, -tempDir.x);
                                dir = vector4.normalized;
                            } else if (ModSettings.PSACustomProperties[this.m_info.name].Rotation == 270f)
                            {
                                Vector3 vector4 = new Vector3(-tempDir.z, tempDir.y, tempDir.x);
                                dir = vector4.normalized;
                            }
                        }
                    }
                    if (ModSettings.PSACustomProperties[this.m_info.name].Raisable == true)
                    {
                        Vector3 centerPos3;
                        Vector3 centerDirection3;
                        segments.m_buffer[snappedSegmentID].GetClosestPositionAndDirection(pos, out centerPos3, out centerDirection3);
                        pos.y = centerPos3.y;
                    }
                    pos.y += ModSettings.PLRCustomProperties[info.name].VerticalOffset;
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
                        PositionLocking.InitiateLock(ModSettings.PSACustomProperties[this.m_info.name].ParkingWidth, parkingSpaceAsset.Info.name);
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
            if (!ModSettings.PSACustomProperties.ContainsKey(this.m_info.name))
            {
                Debug.Log("[PLS].PSAai.GetConstructionInfo PSA Custom Properties does not contain " + this.m_info.name);
                return "";
            }
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
            StringBuilder sb = new StringBuilder();
            if (ModSettings.DisableTooltips)
            {
                return "";
            }
            
            if (PositionLocking.IsLocked() && PositionLocking.GetOverlapping() == false)
            {   
                sb.AppendLine("Locked!");
                if (ModSettings.KeyboardShortcutHints == false)
                {
                    return sb.ToString();
                }
               
                
                // KEYBIND: To obtain current keybind's setting in "string form", just use SavedInputKey.ToLocalizedString("KEYNAME")
                sb.Append("Press " + ModSettings.UnlockKeybind.ToLocalizedString("KEYNAME") + " to Unlock.");
                return sb.ToString();
            } else if (PositionLocking.IsLocked())
            {
                sb.AppendLine("Locked but Overlapping!");
                if (ModSettings.KeyboardShortcutHints == false)
                {
                    return sb.ToString();
                }
                
                
                sb.AppendLine("Placing a new Parking Space Asset ");
                if (ModSettings.OverrideOverlapping)
                {
                    sb.Append("here will override existing.");
                } else
                {
                    sb.Append("here may causes problems later!");
                }
                return sb.ToString();
            } else if (PositionLocking.canReturnToPreviousLockPosition() && ModSettings.KeyboardShortcutHints == true)
            {
                // KEYBIND: To obtain current keybind's setting in "string form", just use SavedInputKey.ToLocalizedString("KEYNAME")
                return "Press " + ModSettings.ReturnLockKeybind.ToLocalizedString("KEYNAME") + " to Re-lock to last locked position.";
            }
            return sb.ToString();
        }
        private void ElevateBuilding(ushort building, ref Building data, Vector3 position, float angle)
        {
            BuildingInfo info = data.Info;
            /*
            if (info.m_hasParkingSpaces != VehicleInfo.VehicleType.None)
            {
                BuildingManager.instance.UpdateParkingSpaces(building, ref data);
            }*/
            data.m_position = position;
            data.m_angle = angle;

           
            data.CalculateBuilding(building);
            BuildingManager.instance.UpdateBuildingRenderer(building, true);
        }
    }
}