//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.Population;
using System;

namespace WUIPlatform.Runtime
{
    /// <summary>
    /// Contains all data that gets created during runtime before simulation starts.
    /// </summary>
    public class RuntimeData
    {
        public SimulationData Simulation;
        public PopulationData Population;
        public RoutingData Routing;
        public EvacuationData Evacuation;
        public TrafficData Traffic;
        public FireData Fire;       

        public RuntimeData()
        {
            Simulation = new SimulationData();
            //Map = new MapData();
            //Visualization = new VisualizationData();
            Population = new PopulationData();
            Routing = new RoutingData();
            Evacuation = new EvacuationData();
            Traffic = new TrafficData();
            Fire = new FireData();
            //Smoke = new SmokeData();
        }

        public bool LoadMapbox()
        {
            //Mapbox: calculate the amount of grids needed based on zoom level, coord and size
            Mapbox.Unity.Map.MapOptions mOptions = WUInity.WUInityEngine.MAP.Options; // new Mapbox.Unity.Map.MapOptions();

            mOptions.locationOptions.latitudeLongitude = "" + WUIEngine.INPUT.Simulation.LowerLeftLatLong.x + "," + WUIEngine.INPUT.Simulation.LowerLeftLatLong.y;
            mOptions.locationOptions.zoom = WUIEngine.INPUT.Map.zoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double tiles = Math.Pow(4.0, mOptions.locationOptions.zoom);
            double degreesPerTile = 360.0 / (Math.Pow(2.0, mOptions.locationOptions.zoom));
            Vector2d mapDegrees = LocalGPWData.SizeToDegrees(WUIEngine.INPUT.Simulation.LowerLeftLatLong, WUIEngine.INPUT.Simulation.Size);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Math.Cos((Math.PI / 180.0) * WUIEngine.INPUT.Simulation.LowerLeftLatLong.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!WUInity.WUInityEngine.MAP.IsAccessTokenValid)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Mapbox token not valid.");
                return false;
            }
            
            WUIEngine.LOG(WUIEngine.LogType.Log, "Starting to load Mapbox map.");
            WUInity.WUInityEngine.MAP.Initialize(new Mapbox.Utils.Vector2d(WUIEngine.INPUT.Simulation.LowerLeftLatLong.x, WUIEngine.INPUT.Simulation.LowerLeftLatLong.y), WUIEngine.INPUT.Map.zoomLevel);
            WUIEngine.LOG(WUIEngine.LogType.Log, "Map loaded succesfully.");

            return true;
        }
    }
}

