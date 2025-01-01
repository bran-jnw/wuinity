using UnityEngine;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using WUInity.Population;
using WUInity.Fire;
using static WUInity.WUInity;
using System;
using Mapbox.Map;

namespace WUInity.Runtime
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
            Mapbox.Unity.Map.MapOptions mOptions = MAP.Options; // new Mapbox.Unity.Map.MapOptions();
            mOptions.locationOptions.latitudeLongitude = "" + INPUT.Simulation.LowerLeftLatLong.x + "," + INPUT.Simulation.LowerLeftLatLong.y;
            mOptions.locationOptions.zoom = INPUT.Map.ZoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double tiles = Math.Pow(4.0, mOptions.locationOptions.zoom);
            double degreesPerTile = 360.0 / (Math.Pow(2.0, mOptions.locationOptions.zoom));
            Vector2D mapDegrees = LocalGPWData.SizeToDegrees(INPUT.Simulation.LowerLeftLatLong, INPUT.Simulation.Size);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Math.Cos((Math.PI / 180.0) * INPUT.Simulation.LowerLeftLatLong.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!MAP.IsAccessTokenValid)
            {
                LOG(WUInity.LogType.Error, "Mapbox token not valid.");
                return false;
            }
            else
            {
                LOG(WUInity.LogType.Log, "Mapbox token: " + MAP.myAccessToken);
            }

            LOG(WUInity.LogType.Log, "Starting to load Mapbox map.");
            MAP.Initialize(new Mapbox.Utils.Vector2d(INPUT.Simulation.LowerLeftLatLong.x, INPUT.Simulation.LowerLeftLatLong.y), INPUT.Map.ZoomLevel);
            LOG(WUInity.LogType.Log, "Map loaded succesfully.");

            return true;
        }
    }
}

