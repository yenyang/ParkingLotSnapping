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
            if (this.m_symmetrical == true && this.m_offset == 0f)
            {
                if (this.SnapToRoad(position, out finalPosition, out finalAngle, 20f))
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
        private bool SnapToRoad(Vector3 refPos, out Vector3 pos, out Vector3 dir, float maxDistance)
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
                                        
                                        Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                                        dir = vector2.normalized;
                                        if (Vector3.Dot(centerPos - refPos, dir) < 0f)
                                        {
                                            dir = -dir;
                                        }
                                        pos = centerPos;
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
                                        if (this.m_rotation == 0 || this.m_rotation == 180)
                                        {
                                            Vector3 vector2 = new Vector3(centerDirection.z, 0f, -centerDirection.x);
                                            dir = vector2.normalized;
                                            if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                            {
                                                if (this.m_rotation == 0)
                                                    dir = -dir;
                                            }
                                        } else if (this.m_rotation == 90 || this.m_rotation == 270)
                                        {
                                            Vector3 vector2 = new Vector3(centerDirection.x, 0f, centerDirection.z);
                                            dir = vector2.normalized;
                                            if (Vector3.Dot(centerPos - refPos, dir) > 0f)
                                            {
                                                if (this.m_rotation == 90)
                                                    dir = -dir;
                                            }
                                        }
                                        Vector3 centerDirectionNormalized = centerDirection.normalized;
                                        if (Vector3.Dot(centerPos - refPos, dir) < 0f)
                                        {
                                            pos = centerPos + dir*this.m_offset;
                                        } else
                                        {
                                            pos = centerPos - dir * this.m_offset;
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
    }
}
