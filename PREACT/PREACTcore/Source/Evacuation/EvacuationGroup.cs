//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Evacuation
{
    public class EvacuationGroup
    {
        private string _name;
        private System.DateTime _evacuationOrderDateTime;
        private DestinationChoices _destinationChoice;
        private PREACTColor _color;
        private bool _default;
        private List<EvacuationDestination> _destinations;
        private List<double> _destinationsCDF;
        private List<ResponseCurve> _responseCurves;
        private List<double> _responseCurvesCDF;
        private DemographicsInput _demographics;

        private List<Vector2d> _shapePolygonLocal;
        private Vector2d _boundingBoxMin;
        private Vector2d _boundingBoxMax;

        public string Name { get => _name; }
        public System.DateTime EvacuationOrderDateTime { get => _evacuationOrderDateTime; }
        public PREACTColor Color { get => _color; }
        public bool Default { get => _default; }
        public List<EvacuationDestination> Destinations { get => _destinations; }
        public DestinationChoices DestinationChoice { get => _destinationChoice; }
        public DemographicsInput Demographics { get => _demographics; }


        public EvacuationGroup(EvacuationGroupInput groupInput, Dictionary<string, EvacuationDestination> allDestinations, Dictionary<string, ResponseCurve> allResponseCurves, Dictionary<string, DemographicsInput> allDemographics, Simulation simulation)
        {
            _name = groupInput.Name;
            _evacuationOrderDateTime = groupInput.EvacuationOrderDateTime;
            _destinationChoice = groupInput.DestinationChoice;
            _color = groupInput.Color;
            _default = groupInput.Default;

            if(!allDemographics.TryGetValue(groupInput.Demographics, out _demographics))
            {
                _demographics = simulation.Evacuation.DefaultDemographics;
            }

            //this is where we need to "re-build" the information from input
            _destinations = new List<EvacuationDestination>(groupInput.Destinations.Count);
            for(int i = 0; i < groupInput.Destinations.Count; ++i)
            {
                EvacuationDestination eD;
                if (allDestinations.TryGetValue(groupInput.Destinations[i], out eD))
                {
                    Destinations.Add(eD);
                }
                else
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "When creating evacuation groups a referenced group name was not found.");
                }
            }
            _destinationsCDF = groupInput.DestinationsCDF;

            _responseCurves = new List<ResponseCurve>(groupInput.ResponseCurves.Count);
            for (int i = 0; i < groupInput.ResponseCurves.Count; ++i)
            {
                ResponseCurve rC;
                if (allResponseCurves.TryGetValue(groupInput.ResponseCurves[i], out rC))
                {
                    _responseCurves.Add(rC);
                }
                else
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "When creating evacuation groups a referenced response curve was not found.");
                }
            }

            _responseCurvesCDF = groupInput.ResponseCurvesCDF;

            string shapeFilePath = System.IO.Path.Combine(simulation.Input.RootFolder, groupInput.ShapeFile);
            CreateShapeFilePolygon(simulation, shapeFilePath);
        }

        public static EvacuationGroup[] CreateGroupsFromInput(Dictionary<string, EvacuationGroupInput> groupsInput, Dictionary<string, 
            EvacuationDestination> allDestinations, Dictionary<string, ResponseCurve> allResponseCurves, Dictionary<string, DemographicsInput> allDemographics, Simulation simulation)
        {
            EvacuationGroup[] groups = new EvacuationGroup[groupsInput.Count];
            int index = 0;
            foreach(EvacuationGroupInput eGI in groupsInput.Values)
            {
                groups[index] = new EvacuationGroup(eGI, allDestinations, allResponseCurves, allDemographics, simulation);
                ++index;
            }

            return groups;
        }

        private void CreateShapeFilePolygon(Simulation simulation, string shapeFilePath)
        {
            _shapePolygonLocal = new List<Vector2d>();
            _boundingBoxMin = new Vector2d(double.MaxValue, double.MaxValue);
            _boundingBoxMax = new Vector2d(double.MinValue, double.MinValue);

            List<Vector2d> rawPoints = new List<Vector2d>();

            using (OSGeo.OGR.Driver driver = OSGeo.OGR.Ogr.GetDriverByName("ESRI Shapefile"))
            {
                OSGeo.OGR.DataSource dataSource = driver.Open(shapeFilePath, 0);
                int layerCount = dataSource.GetLayerCount();
                for (int i = 0; i < layerCount; ++i)
                {
                    OSGeo.OGR.Layer layer = dataSource.GetLayerByIndex(i);
                    int featureCount = (int)layer.GetFeatureCount(0);
                    for (int j = 0; j < featureCount; ++j)
                    {
                        OSGeo.OGR.Feature feature = layer.GetFeature(j);
                        OSGeo.OGR.Geometry geometry = feature.GetGeometryRef();
                        WalkGeometry(geometry, rawPoints);
                        
                        geometry.Dispose();
                        feature.Dispose();
                    }
                    layer.Dispose();
                }                
                dataSource.FlushCache();
                dataSource.Dispose();
            }

            for(int i = 0; i < rawPoints.Count; ++i)
            {
                Vector2d localPos = simulation.Input.Simulation.Data.GetSimulationPosition(rawPoints[i]);
                _shapePolygonLocal.Add(localPos);
                //update bounding box
                _boundingBoxMin.x = Mathd.Min(localPos.x, _boundingBoxMin.x);
                _boundingBoxMax.x = Mathd.Max(localPos.x, _boundingBoxMax.x);
                _boundingBoxMin.y = Mathd.Min(localPos.y, _boundingBoxMin.y);
                _boundingBoxMax.y = Mathd.Max(localPos.y, _boundingBoxMax.y);
            }
        }

        private void WalkGeometry(OSGeo.OGR.Geometry geom, List<Vector2d> result)
        {
            OSGeo.OGR.wkbGeometryType type = geom.GetGeometryType();

            if (type == OSGeo.OGR.wkbGeometryType.wkbPoint)
            {
                double x = geom.GetX(0);
                double y = geom.GetY(0);
                result.Add(new Vector2d(y, x)); //LonLat in data, LatLon needed
            }
            else if (type == OSGeo.OGR.wkbGeometryType.wkbLineString || type == OSGeo.OGR.wkbGeometryType.wkbCircularString)
            {
                int n = geom.GetPointCount();
                for (int i = 0; i < n; i++)
                {
                    double x = geom.GetX(i);
                    double y = geom.GetY(i);
                    result.Add(new Vector2d(y, x)); //LonLat in data, LatLon needed
                }
            }
            else if (type == OSGeo.OGR.wkbGeometryType.wkbPolygon)
            {
                int rings = geom.GetGeometryCount();
                for (int r = 0; r < rings; r++)
                {
                    OSGeo.OGR.Geometry ring = geom.GetGeometryRef(r);
                    WalkGeometry(ring, result);
                }
            }
            else
            {
                /* MultiLineString, MultiPolygon, GeometryCollection, etc. */
                int parts = geom.GetGeometryCount();
                for (int i = 0; i < parts; i++)
                {
                    OSGeo.OGR.Geometry sub = geom.GetGeometryRef(i);
                    WalkGeometry(sub, result);
                }
            }
        }


        //https://en.wikipedia.org/wiki/Point_in_polygon
        //https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        public bool LatLonBelongsToGroup(Vector2d latLon, Simulation simulation)
        {            
            bool result = false;
            Vector2d testedPoint = simulation.Input.Simulation.Data.GetSimulationPosition(latLon);

            //first check bounding box for potential early exit
            if(testedPoint.x < _boundingBoxMin.x || testedPoint.x > _boundingBoxMax.x || testedPoint.y < _boundingBoxMin.y || testedPoint.y > _boundingBoxMax.y)
            {
                return false;
            }

            Vector2d a = _shapePolygonLocal[_shapePolygonLocal.Count - 1];
            foreach (Vector2d polygonPoint in _shapePolygonLocal)
            {
                //if we are the same point
                if ((polygonPoint.x == testedPoint.x) && (polygonPoint.y == testedPoint.y))
                {
                    return true;
                }                    

                //if we are along the same line fixed on y-axis
                if ((polygonPoint.y == a.y) && (testedPoint.y == a.y))
                {
                    if ((a.x <= testedPoint.x) && (testedPoint.x <= polygonPoint.x))
                    {
                        return true;
                    }                        

                    if ((polygonPoint.x <= testedPoint.x) && (testedPoint.x <= a.x))
                    {
                        return true;
                    }                        
                }

                //count intersections, even count means outside polygon, odd means inside
                if ((polygonPoint.y < testedPoint.y) && (a.y >= testedPoint.y) || (a.y < testedPoint.y) && (polygonPoint.y >= testedPoint.y))
                {
                    if (polygonPoint.x + (testedPoint.y - polygonPoint.y) / (a.y - polygonPoint.y) * (a.x - polygonPoint.x) <= testedPoint.x)
                    {
                        result = !result;
                    }                        
                }
                a = polygonPoint;
            }

            return result;
        }

        public EvacuationDestination GetWeightedRandomDestination()
        {
            float randomChoice = Random.valueF;
            EvacuationDestination eD = Destinations[0];            

            for (int i = 0; i < _destinationsCDF.Count; i++)
            {
                if (randomChoice <= _destinationsCDF[i])
                {
                    return Destinations[i];
                }
            }

            //this should not happen, but keep as backup as we do not want to return null
            Engine.Message(null, Engine.LogType.Warning, "The evacuation destinations specified have cumulative probability under 1.0 and a higher probability was drawn, using last user destination specified as fallback.");            
            return eD;
        }

        public EvacuationDestination GetClosestEuclideanDestination(Vector2d startLatLon, Simulation simulation)
        {
            Vector2d householdPos = simulation.Input.Simulation.Data.GetSimulationPosition(startLatLon);
            double closestDistance = double.MaxValue;
            EvacuationDestination closestDestination = null;

            for (int i = 0; i < Destinations.Count; ++i)
            {
                Vector2d destPos = simulation.Input.Simulation.Data.GetSimulationPosition(Destinations[i].LatLon);
                double distance = Vector2d.SqrMagnitude(destPos - householdPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDestination = Destinations[i];
                }                         
            }
            
            return closestDestination;
        }

        public float GetWeightedRandomResponseTime(float evacuationOrderStart)
        {
            float responseTime = float.MaxValue;
            float r = Random.valueF;
            //get curve index from evac group
            ResponseCurve pickedCurve = _responseCurves[0];
            for (int i = 0; i < _responseCurves.Count; i++)
            {
                if (r <= _responseCurvesCDF[i])
                {
                    pickedCurve = _responseCurves[i];
                    break;
                }
            }

            //need new random
            r = Random.valueF;            
            for (int i = 1; i < pickedCurve.DataPoints.Length; i++) //skip first as that is always zero probability
            {
                if (r <= pickedCurve.DataPoints[i].Probability)
                {
                    //offset with evacuation order time
                    responseTime = Random.Range(pickedCurve.DataPoints[i - 1].Time, pickedCurve.DataPoints[i].Time) + evacuationOrderStart;
                    break;
                }
            }

            return responseTime;
        }
    }
}
