using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using UnityEngine;
using ICities;
using System.Runtime.CompilerServices;
using System.Xml.Schema;

namespace ParkingLotSnapping
{
    public static class ParkingSpaceGrid
    {
        private static Dictionary<int, ushort> parkingSpaceGrid;
        private static bool awake = false;
        
        private static readonly BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
        private static readonly int _capacity = _buildingManager.m_buildings.m_buffer.Length;
        private static readonly int gridCoefficient = 8640;
        private static readonly float gridQuotient = 2f;
        private static readonly float gridAddition = gridCoefficient/2f;
        //private static readonly int gridDimension = gridCoefficient^2;
        public static void Awake()
        {
            parkingSpaceGrid = new Dictionary<int, ushort>();
            for (ushort id = 0; id < _capacity; id++)
            {
                try
                {
                    Building currentBuilding = _buildingManager.m_buildings.m_buffer[id];

                    bool flag = (currentBuilding.m_flags & Building.Flags.Completed) != Building.Flags.None;

                    ParkingSpaceAssetAI currentParkingSpaceAi;

                    currentParkingSpaceAi = currentBuilding.Info.m_buildingAI as ParkingSpaceAssetAI;
                   
                    if (currentParkingSpaceAi != null && flag == true)
                    {
                        AddToGrid(id);
                        currentBuilding.Info.m_autoRemove = true;
                    }
                } catch (Exception e )
                {
                    Debug.Log("[PLS]ParkingSpaceGrid.Awake Encountered Exception " + e.ToString() + " trying to awaken.");
                }
             }
             awake = true;
        }
        public static bool AddToGrid(ushort id)
        {
            Building currentAsset = _buildingManager.m_buildings.m_buffer[id];
            if (currentAsset.Info.m_buildingAI.GetType() != typeof(ParkingSpaceAssetAI))
            {
                return false;
            }
            Vector3 buildingPosition = currentAsset.m_position;
            int gridX = Mathf.Clamp((int)(buildingPosition.x / gridQuotient + gridAddition),0,gridCoefficient-1);
            int gridZ = Mathf.Clamp((int)(buildingPosition.z / gridQuotient + gridAddition),0,gridCoefficient-1);
            int gridLocation = gridZ * gridCoefficient + gridX;
            try
            {
                //Debug.Log("[PLS]ParkingSpaceGrid Added id= " + id.ToString() + " at location =" + gridLocation.ToString() + " position = " + buildingPosition.ToString());
                parkingSpaceGrid[gridLocation] = id;
                return true;
            } catch (Exception e)
            {
                Debug.Log("[PLS]ParkingSpaceGrid checkGrid Encountered exception " + e.ToString());
                return false;
            }
            
        }
        public static bool RemoveFromGrid(ushort id )
        {
            Building currentAsset = _buildingManager.m_buildings.m_buffer[id];
            if (currentAsset.Info.m_buildingAI.GetType() != typeof(ParkingSpaceAssetAI))
            {
                return false;
            }
            Vector3 buildingPosition = currentAsset.m_position;
            int gridX = Mathf.Clamp((int)(buildingPosition.x / gridQuotient + gridAddition), 0, gridCoefficient - 1);
            int gridZ = Mathf.Clamp((int)(buildingPosition.z / gridQuotient + gridAddition), 0, gridCoefficient - 1);
            int gridLocation = gridZ * gridCoefficient + gridX;
            if (parkingSpaceGrid.ContainsKey(gridLocation))
            {
                try
                {
                    //Debug.Log("[PLS]ParkingSpaceGrid Removed id= " + id.ToString() + " at location =" + gridLocation.ToString());

                    parkingSpaceGrid[gridLocation] = 0;
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log("[PLS]ParkingSpaceGrid checkGrid Encountered exception " + e.ToString());
                    return false;
                }
            }
            return false;
        }
        public static bool areYouAwake()
        {
            return awake;
        }
        public static ushort CheckGrid(Vector3 position)
        {
            int gridX = Mathf.Clamp((int)(position.x / gridQuotient + gridAddition), 0, gridCoefficient - 1);
            int gridZ = Mathf.Clamp((int)(position.z / gridQuotient + gridAddition), 0, gridCoefficient - 1);
            int gridLocation = gridZ * gridCoefficient + gridX;
            if (parkingSpaceGrid.ContainsKey(gridLocation))
            {
                try
                {
                    if (parkingSpaceGrid[gridLocation] != 0)
                        return parkingSpaceGrid[gridLocation];
                }
                catch (Exception e)
                {
                    Debug.Log("[PLS]ParkingSpaceGrid checkGrid Encountered exception " + e.ToString());
                }
            }
                
            return 0;
        }
    }
}
