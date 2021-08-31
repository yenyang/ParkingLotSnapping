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
    public static class Undo
    {
        private static List<ushort> parkingLotsList;
        private static bool awake = false;
        private static readonly BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

        public static void Awake()
        {
            if (!awake)
            {
                parkingLotsList = new List<ushort>();
                awake = true;
            }
        }
        public static bool areYouAwake() => awake; // Short notation for return awake

        public static void addLotToStack(ushort buildingID)
        {
            // Could verify building is created but, aren't the flags completed later in base.CreateBuilding?
            parkingLotsList.Add(buildingID);
        }
        public static void releaseLastBuilding()
        {
            if (parkingLotsList.Count < 1) return;
            ushort buildingID = parkingLotsList[parkingLotsList.Count - 1];
            if (ModSettings.BulldozeEffect)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                Building bldg = instance.m_buildings.m_buffer[buildingID];
                bool isBldgCollapsed = (bldg.m_flags & Building.Flags.Collapsed) != 0;
                BuildingTool.DispatchPlacementEffect(bldg.Info, buildingID, bldg.m_position, bldg.m_angle, bldg.m_width, bldg.m_length, bulldozing: true, isBldgCollapsed);
            }
            _buildingManager.ReleaseBuilding(buildingID); // Actually the building manager already performs flag checking for us!
            removeLotFromStack(buildingID); // Eliminate from list (sometimes redundant since releasebuilding also calls it, but we need to make sure it's not in the list anymore)
        }

        public static void removeLotFromStack(ushort buildingID)
        {
            parkingLotsList.Remove(buildingID); // No problems if the building is not in the list
            // Remove(T) would return false in this case
        }
    }
}
