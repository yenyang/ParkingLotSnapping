using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using System.Text;
using System;
using UnityEngine;

namespace ParkingLotSnapping
{
    class ParkingSpaceAssetAI : DummyBuildingAI
    {
        [CustomizableProperty("Symmetrical", "Parking Space Asset")]
        public bool m_symmetrical = true;

        [CustomizableProperty("Spaces", "Parking Space Asset")]
        public int m_spaces = 1;

        [CustomizableProperty("Rows", "Parking Space Asset")]
        public int m_rows = 2;

        [CustomizableProperty("Aisles", "Parking Space Asset")]
        public int m_aisles = 1;

        [CustomizableProperty("Offset", "Parking Space Asset")]
        public float m_offset = 0f;

        [CustomizableProperty("Rotation", "Parking Space Asset")]
        public int m_rotation = 0;

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
            base.CreateBuilding(buildingID, ref data);

        }
        public override ToolBase.ToolErrors CheckBuildPosition(ushort relocateID, ref Vector3 position, ref float angle, float waterHeight, float elevation, ref Segment3 connectionSegment, out int productionRate, out int constructionCost)
        {
            Vector3 finalPosition;
            Vector3 finalAngle;
            Building currentBuilding = BuildingManager.instance.m_buildings.m_buffer[relocateID];
            currentBuilding.Info.m_placementMode = BuildingInfo.PlacementMode.OnTerrain;
            BuildingInfo info = currentBuilding.Info;
           
            if (this.m_symmetrical == true || this.m_offset == 0f)
            {
                if (this.SnapToRoad(position, out finalPosition, out finalAngle, 20f, currentBuilding))
                {
                    
                    angle = Mathf.Atan2(finalAngle.x, -finalAngle.z);
                    position.x = finalPosition.x;
                    position.z = finalPosition.z;
                    
                    
                }
            } else if (this.m_symmetrical == false && this.m_offset != 0f)
            {
                if (this.SnapToOneSide(position, out finalPosition, out finalAngle, 20f))
                {
                    angle = Mathf.Atan2(finalAngle.x, -finalAngle.z);
                    position.x = finalPosition.x;
                    position.z = finalPosition.z;
                }
            }
            
            bool flag;
            this.GetConstructionCost(relocateID != 0, out constructionCost, out flag);
            productionRate = 0;
            return ToolBase.ToolErrors.None;
        }
        private bool SnapToRoad(Vector3 refPos, out Vector3 pos, out Vector3 dir, float maxDistance, Building currentBuilding)
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
            int maxXint = Mathf.Max((int)(maxX / 64f + 135f), 269);
            int maxZint = Mathf.Max((int)(maxZ / 64f + 135f), 269);
            
            Array16<NetSegment> segments = Singleton<NetManager>.instance.m_segments;
            ushort[] segmentGrid = Singleton<NetManager>.instance.m_segmentGrid;
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
                            NetInfo info = segments.m_buffer[(int)segmentGridZX].Info;
                            if (info.m_class.m_service == ItemClass.Service.Road)
                            {
                                Vector3 min = segments.m_buffer[(int)segmentGridZX].m_bounds.min;
                                Vector3 max = segments.m_buffer[(int)segmentGridZX].m_bounds.max;
                                if (min.x < maxX && min.z < maxZ && max.x > minX && max.z > minZ)
                                {
                                    Vector3 centerPos;
                                    Vector3 centerDirection;
                                    segments.m_buffer[(int)segmentGridZX].GetClosestPositionAndDirection(refPos, out centerPos, out centerDirection);
                                    
                                    float distanceToRoad = Vector3.Distance(centerPos, refPos) - info.m_halfWidth;
                                    //Debug.Log("[APL].PSAAI.Snap to Road info.lanes = " + info.m_lanes.Length.ToString());
                                    //Debug.Log("[APL].PSAAI.Snap to Road info.m_halfwidth = " + info.m_halfWidth.ToString());
                                    if (distanceToRoad < maxDistance)
                                    {
                                        
                                        Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                                        dir = vector2.normalized;
                                        if (Vector3.Dot(centerPos - refPos, dir) < 0f)
                                        {
                                            dir = -dir;
                                        }
                                       
                                        if (info.m_halfWidth == 20 || info.m_lanes.Length == 9)
                                        {
                                            if (this.m_rotation == 0 || this.m_rotation == 180)
                                            {
                                                if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                                {
                                                    if (this.m_rotation == 0)
                                                        dir = -dir;
                                                }
                                            }
                                            else if (this.m_rotation == 90 || this.m_rotation == 270)
                                            {
                                                if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                                {
                                                    if (this.m_rotation == 90)
                                                        dir = -dir;
                                                }
                                            }
                                            Vector3 centerDirectionNormalized = centerDirection.normalized;
                                            if (Vector3.Dot(centerPos - refPos, dir) < 0f)
                                            {
                                                pos = centerPos + dir * (9f);
                                            }
                                            else
                                            {
                                                pos = centerPos - dir * (9f);
                                            }
                                        } else if (info.m_halfWidth == 29 || info.m_lanes.Length == 13)
                                        {
                                            pos = centerPos;
                                            if (this.m_rotation == 0 || this.m_rotation == 180)
                                            {
                                                if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                                {
                                                    if (this.m_rotation == 0)
                                                        dir = -dir;
                                                }
                                            }
                                            else if (this.m_rotation == 90 || this.m_rotation == 270)
                                            {
                                                if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                                {
                                                    if (this.m_rotation == 90)
                                                        dir = -dir;
                                                }
                                            }
                                            Vector3 centerDirectionNormalized = centerDirection.normalized;
                                            if (Vector3.Dot(centerPos - refPos, dir) < -11f)
                                            {
                                                pos = centerPos + dir * 18f;
                                            }
                                            else if ((Vector3.Dot(centerPos - refPos, dir) > 11f))
                                            {
                                                pos = centerPos - dir * 18f;
                                            }
                                        } else
                                        {
                                            float angle = Mathf.Atan2(dir.x, -dir.z);
                                            ushort adjacentParkingAssetID = CheckForParkingSpaces(centerPos, angle, currentBuilding.m_width, currentBuilding.m_length);
                                            bool snappedToParkingAsset = false;
                                            /*
                                            if (adjacentParkingAssetID != 0) {
                                                Building adjacentParkingAsset = BuildingManager.instance.m_buildings.m_buffer[adjacentParkingAssetID];
                                                if (adjacentParkingAsset.m_angle == angle)
                                                {
                                                    Debug.Log("[APL].PSAAi.SnapToRoad Snap to this asset id = " + adjacentParkingAssetID.ToString());
                                                    Vector3 centerDirectionNormalized = centerDirection.normalized;
                                                    
                                                    float dist1 = Vector3.Dot(centerPos - adjacentParkingAsset.m_position, centerDirectionNormalized);
                                                    Debug.Log("[APL].PSAAi.SnapToRoad Dot Product equals " + dist1.ToString());
                                                    pos = centerPos - (adjacentParkingAsset.Width*4f-dist1)*centerDirectionNormalized + currentBuilding.m_width*4f* centerDirectionNormalized;
                                                    snappedToParkingAsset = true;

                                                    //snappedToParkingAsset = true;
                                                }
                                            } 
                                            */
                                            if (snappedToParkingAsset == false) { 
                                                pos = centerPos;
                                            }
                                            
                                        }
                                        maxDistance = distanceToRoad;
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
            int maxXint = Mathf.Max((int)(maxX / 64f + 135f), 269);
            int maxZint = Mathf.Max((int)(maxZ / 64f + 135f), 269);

            Array16<NetSegment> segments = Singleton<NetManager>.instance.m_segments;
            ushort[] segmentGrid = Singleton<NetManager>.instance.m_segmentGrid;
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
                            NetInfo info = segments.m_buffer[(int)segmentGridZX].Info;
                            if (info.m_class.m_service == ItemClass.Service.Road)
                            {
                                Vector3 min = segments.m_buffer[(int)segmentGridZX].m_bounds.min;
                                Vector3 max = segments.m_buffer[(int)segmentGridZX].m_bounds.max;
                                if (min.x < maxX && min.z < maxZ && max.x > minX && max.z > minZ)
                                {
                                    Vector3 centerPos;
                                    Vector3 centerDirection;
                                    segments.m_buffer[(int)segmentGridZX].GetClosestPositionAndDirection(refPos, out centerPos, out centerDirection);

                                    float distanceToRoad = Vector3.Distance(centerPos, refPos) - info.m_halfWidth;
                                    if (distanceToRoad < maxDistance)
                                    {
                                        if (info.m_lanes.Length == 9 || info.m_halfWidth == 20)
                                        {
                                            if (this.m_rotation == 0 || this.m_rotation == 180)
                                            {
                                                Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                                                dir = vector2.normalized;
                                                float delta = Vector3.Dot(centerPos - refPos, dir);
                                                if (delta > info.m_halfWidth / 2f && delta > 0 || delta < 0 && delta > -info.m_halfWidth / 2f)
                                                {
                                                    if (this.m_rotation == 0)
                                                        dir = -dir;
                                                }
                                            }
                                            else if (this.m_rotation == 90 || this.m_rotation == 270)
                                            {
                                                Vector3 vector2 = new Vector3(centerDirection.x, 0f, centerDirection.z);
                                                dir = vector2.normalized;
                                                float delta = Vector3.Dot(centerPos - refPos, dir);
                                                if (delta > info.m_halfWidth / 2f && delta > 0 || delta < 0 && delta > -info.m_halfWidth / 2f)
                                                {
                                                    if (this.m_rotation == 90)
                                                        dir = -dir;
                                                }
                                            }
                                        }
                                        else if (info.m_lanes.Length == 13 || info.m_halfWidth == 29)
                                        {
                                            if (this.m_rotation == 0 || this.m_rotation == 180)
                                            {
                                                Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                                                dir = vector2.normalized;
                                                float delta = Vector3.Dot(centerPos - refPos, dir);
                                                if (delta > 2f * info.m_halfWidth / 3f && delta > 0 || delta > 0 && delta < info.m_halfWidth / 3f || delta < -info.m_halfWidth / 3f && delta > -2f * info.m_halfWidth / 3f)
                                                {
                                                    if (this.m_rotation == 0)
                                                        dir = -dir;
                                                }
                                            }
                                            else if (this.m_rotation == 90 || this.m_rotation == 270)
                                            {
                                                Vector3 vector2 = new Vector3(centerDirection.x, 0f, centerDirection.z);
                                                dir = vector2.normalized;
                                                float delta = Vector3.Dot(centerPos - refPos, dir);
                                                if (delta > 2f * info.m_halfWidth / 3f && delta > 0 || delta > 0 && delta < info.m_halfWidth / 3f || delta < -info.m_halfWidth / 3f && delta > 2f * info.m_halfWidth / 3f)
                                                {
                                                    if (this.m_rotation == 90)
                                                        dir = -dir;
                                                }
                                            }
                                        }
                                        else {
                                            if (this.m_rotation == 0 || this.m_rotation == 180)
                                            {
                                                Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                                                dir = vector2.normalized;
                                                if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                                {
                                                    if (this.m_rotation == 0)
                                                        dir = -dir;
                                                }
                                            }
                                            else if (this.m_rotation == 90 || this.m_rotation == 270)
                                            {
                                                Vector3 vector2 = new Vector3(centerDirection.x, 0f, centerDirection.z);
                                                dir = vector2.normalized;
                                                if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                                {
                                                    if (this.m_rotation == 90)
                                                        dir = -dir;
                                                }
                                            }
                                        }

                                        Vector3 centerDirectionNormalized = centerDirection.normalized;

                                        float distanceDivisor = 100f;
                                        if (info.m_lanes.Length == 9 || info.m_halfWidth == 20)
                                        {
                                            float delta2 = Vector3.Dot(centerPos - refPos, dir);
                                            if (delta2 < -info.m_halfWidth / 2f)
                                            {
                                                pos = centerPos + dir * (9f + this.m_offset - (float)ModSettings.DistanceFromCurb / distanceDivisor);
                                            } else if (delta2 > -info.m_halfWidth / 2f && delta2 < 0)
                                            {
                                                pos = centerPos + dir * (9f - this.m_offset - (float)ModSettings.DistanceBetweenParkingStalls/ distanceDivisor);
                                            } else if (delta2 < info.m_halfWidth)
                                            {
                                                pos = centerPos - dir * (9f - this.m_offset + (float)ModSettings.DistanceBetweenParkingStalls / distanceDivisor);
                                            } else
                                            {
                                                pos = centerPos - dir * (9f + this.m_offset - (float)ModSettings.DistanceFromCurb/ distanceDivisor);
                                            }
                                        } else if (info.m_lanes.Length == 13 || info.m_halfWidth == 29)
                                        {
                                            float delta2 = Vector3.Dot(centerPos - refPos, dir);
                                            if (delta2 < -2f * info.m_halfWidth / 3f)
                                            {
                                                pos = centerPos + dir * (18f + this.m_offset - (float)ModSettings.DistanceFromCurb / distanceDivisor);
                                            } else if (delta2 < -info.m_halfWidth / 3f)
                                            {
                                                pos = centerPos + dir * (18f - this.m_offset - (float)ModSettings.DistanceBetweenParkingStalls / distanceDivisor);
                                            } else if (delta2 < 0f)
                                            {
                                                pos = centerPos + dir * (this.m_offset - (float)ModSettings.DistanceBetweenParkingStalls / distanceDivisor);
                                            } else if (delta2 < info.m_halfWidth / 3f)
                                            {
                                                pos = centerPos - dir * (this.m_offset + (float)ModSettings.DistanceBetweenParkingStalls / distanceDivisor);
                                            } else if (delta2 < 2f * info.m_halfWidth / 3f)
                                            {
                                                pos = centerPos - dir * (18f - this.m_offset + (float)ModSettings.DistanceBetweenParkingStalls / distanceDivisor);
                                            } else
                                            {
                                                pos = centerPos - dir * (18f + this.m_offset - (float)ModSettings.DistanceFromCurb / distanceDivisor);
                                            }
                                        }
                                        else if (info.m_lanes.Length == 4 || info.m_halfWidth == 8)
                                        {
                                            if (Vector3.Dot(centerPos - refPos, dir) < 0f)
                                            {
                                                pos = centerPos + dir * (this.m_offset - 3f - (float)ModSettings.DistanceFromCurb / distanceDivisor);
                                            }
                                            else
                                            {
                                                pos = centerPos - dir * (this.m_offset-3f - (float)ModSettings.DistanceFromCurb / distanceDivisor);
                                            }
                                        } else { 
                                           
                                            if (Vector3.Dot(centerPos - refPos, dir) < 0f)
                                            {
                                                pos = centerPos + dir * (this.m_offset - (float)ModSettings.DistanceFromCurb / distanceDivisor);
                                            }
                                            else
                                            {
                                                pos = centerPos - dir * (this.m_offset - (float)ModSettings.DistanceFromCurb / distanceDivisor);
                                            }
                                        }
                                        
                                        maxDistance = distanceToRoad;
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

            return result;
        }
        private ushort CheckForParkingSpaces(Vector3 pos, float angle, int width, int length)
        {
            Vector2 a = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 a2 = new Vector2(a.y, 0f - a.x);
            a *= (float)width * 4f;
            a2 *= (float)length * 4f;
            Vector2 vector = VectorUtils.XZ(pos);
            Quad2 quad = default(Quad2);
            quad.a = vector - a - a2;
            quad.b = vector + a - a2;
            quad.c = vector + a + a2;
            quad.d = vector - a + a2;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Vector2 vector3 = quad.Min();
            Vector2 vector2 = quad.Max();
            int num = Mathf.Max((int)((vector3.x - 72f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((vector3.y - 72f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((vector2.x + 72f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((vector2.y + 72f) / 64f + 135f), 269);
            //bool result = false;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = instance.m_buildingGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        BuildingInfo info = instance.m_buildings.m_buffer[num5].Info;
                        ItemClass.CollisionType collisionType = ItemClass.CollisionType.Zoned;
                        if ((object)info != null && instance.m_buildings.m_buffer[num5].OverlapQuad(num5, quad, pos.y-1000f, pos.y+1000f, collisionType))
                        {
                            if (info.m_buildingAI is ParkingSpaceAssetAI)
                                return num5;
                        }
                        num5 = instance.m_buildings.m_buffer[num5].m_nextGridBuilding;
                        if (++num6 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                        
                    }
                }
            }
            return (ushort)0;
        }
    }
}

