using WUIEngine.Population;
using System;

namespace WUIEngine.Runtime
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
            mOptions.locationOptions.latitudeLongitude = "" + Engine.INPUT.Simulation.LowerLeftLatLong.x + "," + Engine.INPUT.Simulation.LowerLeftLatLong.y;
            mOptions.locationOptions.zoom = Engine.INPUT.Map.zoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double tiles = Math.Pow(4.0, mOptions.locationOptions.zoom);
            double degreesPerTile = 360.0 / (Math.Pow(2.0, mOptions.locationOptions.zoom));
            Vector2d mapDegrees = LocalGPWData.SizeToDegrees(Engine.INPUT.Simulation.LowerLeftLatLong, Engine.INPUT.Simulation.Size);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Math.Cos((Math.PI / 180.0) * Engine.INPUT.Simulation.LowerLeftLatLong.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!WUInity.WUInityEngine.MAP.IsAccessTokenValid)
            {
                Engine.LOG(Engine.LogType.Error, "Mapbox token not valid.");
                return false;
            }
            Engine.LOG(Engine.LogType.Log, "Starting to load Mapbox map.");
            WUInity.WUInityEngine.MAP.Initialize(new Mapbox.Utils.Vector2d(Engine.INPUT.Simulation.LowerLeftLatLong.x, Engine.INPUT.Simulation.LowerLeftLatLong.y), Engine.INPUT.Map.zoomLevel);
            Engine.LOG(Engine.LogType.Log, "Map loaded succesfully.");

            return true;
        }
    }
}

