//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;
using PREACT.IO;

namespace PREACT.Evacuation
{
    public class EvacuationGroup
    {
        public string Name;
        public PREACTColor Color;

        public List<string> DestinationNames;
        public List<double> DestinationProbabilities;
        public List<ResponseCurve> ResponseCurves;
        public List<double> ResponseCurveProbabilities;
        public string ShapeFilePath;
        public bool Default;
        List<Vector2d> shapePolygonLocal;
        Vector2d boundingBoxMin;
        Vector2d boundingBoxMax;

        public EvacuationGroup()
        {
            DestinationNames = new List<string>();
            DestinationProbabilities = new List<double>();
            ResponseCurves = new List<ResponseCurve>();
            ResponseCurveProbabilities = new List<double>();
            shapePolygonLocal = new List<Vector2d>();
        }

        private void CreateShapeFilePolygon(SimulationInput simulationInput)
        {
            boundingBoxMin = new Vector2d(double.MaxValue, double.MaxValue);
            boundingBoxMax = new Vector2d(double.MinValue, double.MinValue);

            using (OSGeo.OGR.Driver driver = OSGeo.OGR.Ogr.GetDriverByName("ESRI Shapefile"))
            {
                OSGeo.OGR.DataSource dataSource = driver.Open(ShapeFilePath, 0);
                OSGeo.OGR.Layer layer = dataSource.GetLayerByIndex(0);
                OSGeo.OGR.Feature feature = layer.GetFeature(0);
                OSGeo.OGR.Geometry geometry = feature.GetGeometryRef();
                for (int i = 0; i < geometry.GetPointCount(); ++i)
                {
                    double[] geopoint = { 0, 0, 0 };
                    geometry.GetPoint(i, geopoint);
                    Vector2d wgs84LatLon = new Vector2d(geopoint[0], geopoint[1]);
                    Vector2d localPos = simulationInput.Data.GetSimulationPosition(wgs84LatLon);
                    shapePolygonLocal.Add(localPos);

                    //update bounding box
                    boundingBoxMin.x = Mathd.Min(localPos.x, boundingBoxMin.x);
                    boundingBoxMax.x = Mathd.Max(localPos.x, boundingBoxMax.x);
                    boundingBoxMin.y = Mathd.Min(localPos.y, boundingBoxMin.y);
                    boundingBoxMax.y = Mathd.Max(localPos.y, boundingBoxMax.y);
                }

                //clean up
                geometry.Dispose();
                feature.Dispose();
                layer.Dispose();
                dataSource.FlushCache();
                dataSource.Dispose();
            }
        }

        //https://en.wikipedia.org/wiki/Point_in_polygon
        //https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        public bool LatLonBelongsToGroup(Vector2d latLon, Simulation simulation)
        {            
            bool result = false;
            Vector2d testedPoint = simulation.Input.Simulation.Data.GetSimulationPosition(latLon);

            //first check bounding box for potential early exit
            if(testedPoint.x < boundingBoxMin.x || testedPoint.x > boundingBoxMax.x || testedPoint.y < boundingBoxMin.y || testedPoint.y > boundingBoxMax.y)
            {
                return false;
            }

            Vector2d a = shapePolygonLocal[shapePolygonLocal.Count - 1];
            foreach (Vector2d polygonPoint in shapePolygonLocal)
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

        public EvacuationDestination GetWeightedRandomDestination(Dictionary<string, EvacuationDestination> destinations)
        {
            float randomChoice = Random.valueF;
            EvacuationDestination eD;            

            for (int i = 0; i < DestinationProbabilities.Count; i++)
            {
                if (randomChoice <= DestinationProbabilities[i])
                {                    
                    if(destinations.TryGetValue(DestinationNames[i], out eD))
                    {
                        return eD;
                    }                    
                }
            }

            //this should not happen, but keep as backup as we do not want to return null
            Engine.Message(null, Engine.LogType.Warning, "The evacuation destinations specified have cumulative probability under 1.0 and a higher probability was drawn, using last user destination specified as fallback.");
            destinations.TryGetValue(DestinationNames[0], out eD);
            return eD;
        }

        public EvacuationDestination GetClosestEuclideanDestination(Dictionary<string, EvacuationDestination> destinations, Vector2d startLatLon, Simulation simulation)
        {
            Vector2d householdPos = simulation.Input.Simulation.Data.GetSimulationPosition(startLatLon);
            double closestDistance = double.MaxValue;
            EvacuationDestination closestDestination = null;

            for (int i = 0; i < DestinationNames.Count; ++i)
            {
                EvacuationDestination eD;
                if(destinations.TryGetValue(DestinationNames[i], out eD))
                {
                    Vector2d destPos = simulation.Input.Simulation.Data.GetSimulationPosition(eD.LatLon);
                    double distance = Vector2d.SqrMagnitude(destPos - householdPos);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestDestination = eD;
                    }
                }                               
            }
            
            return closestDestination;
        }

        public float GetWeightedRandomResponseTime(float evacuationOrderStart)
        {
            float responseTime = float.MaxValue;
            float r = Random.valueF;
            //get curve index from evac group
            ResponseCurve pickedCurve = ResponseCurves[0];
            for (int i = 0; i < ResponseCurves.Count; i++)
            {
                if (r <= ResponseCurveProbabilities[i])
                {
                    pickedCurve = ResponseCurves[i];
                    break;
                }
            }

            //need new random
            r = Random.valueF;            
            for (int i = 1; i < pickedCurve.DataPoints.Length; i++) //skip first as that is always zero probability
            {
                if (r <= pickedCurve.DataPoints[i].probability)
                {
                    //offset with evacuation order time
                    responseTime = Random.Range(pickedCurve.DataPoints[i - 1].time, pickedCurve.DataPoints[i].time) + evacuationOrderStart;
                    break;
                }
            }

            return responseTime;
        }

        public static Dictionary<string, EvacuationGroup> Parse(string[] inputLines, SimulationInput simulationInput, List<int> evacGroupLineIndices, Dictionary<string, EvacuationDestinationInput> destinationInputs, Dictionary<string, ResponseCurve> responseCurves, string rootFolder, out bool success)
        {
            Dictionary<string, EvacuationGroup> newInputs = new Dictionary<string, EvacuationGroup>();
            success = false;

            for (int i = 0; i < evacGroupLineIndices.Count; ++i)
            {
                EvacuationGroup newInput = new EvacuationGroup();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, evacGroupLineIndices[i]);
                string nameOfInput, userInput;

                //critical
                nameOfInput = nameof(Name);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.Name = userInput;
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    break;
                }

                //critical
                nameOfInput = nameof(DestinationNames);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    for(int j = 0; j < data.Length; ++i)
                    {
                        if (destinationInputs.ContainsKey(data[j]))
                        {
                            newInput.DestinationNames.Add(data[j]);
                        }
                        else
                        {
                            success = false;
                            PREACTInput.MissingReferenceToOtherInput(nameOfInput, data[j]);
                        }
                    }                    
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    break;
                }

                //maybe critical
                if(newInput.DestinationNames.Count == 1)
                {
                    newInput.DestinationProbabilities.Add(1.0);
                }
                else
                {
                    nameOfInput = nameof(DestinationProbabilities);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        string[] data = userInput.Split(',');
                        for (int j = 0; j < data.Length; ++i)
                        {
                            double cumulativeProbability;
                            success = double.TryParse(data[j], out cumulativeProbability);
                            if (success)
                            {
                                newInput.DestinationProbabilities.Add(cumulativeProbability);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput, true);
                    }
                    if (newInput.DestinationNames.Count != newInput.DestinationProbabilities.Count)
                    {
                        success = false;
                        PREACTInput.IncorrectInputCount(nameOfInput);
                    }
                    if (!success)
                    {
                        break;
                    }
                }                    

                //critical
                nameOfInput = nameof(ResponseCurves);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    for (int j = 0; j < data.Length; ++i)
                    {
                        ResponseCurve rC;
                        if (responseCurves.TryGetValue(data[j], out rC))
                        {
                            newInput.ResponseCurves.Add(rC);
                        }
                        else
                        {
                            success = false;
                            PREACTInput.MissingReferenceToOtherInput(nameOfInput, data[j]);
                        }
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    break;
                }

                //maybe critical
                if(newInput.ResponseCurves.Count == 1)
                {
                    newInput.DestinationProbabilities.Add(1.0);
                }
                else
                {
                    nameOfInput = nameof(ResponseCurveProbabilities);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        string[] data = userInput.Split(',');
                        for (int j = 0; j < data.Length; ++i)
                        {
                            double cumulativeProbability;
                            success = double.TryParse(data[j], out cumulativeProbability);
                            if (success)
                            {
                                newInput.DestinationProbabilities.Add(cumulativeProbability);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput, true);
                    }
                    if (newInput.ResponseCurves.Count != newInput.ResponseCurveProbabilities.Count)
                    {
                        success = false;
                        PREACTInput.IncorrectInputCount(nameOfInput);
                    }
                    if (!success)
                    {
                        break;
                    }
                }                    

                //maybe critical
                nameOfInput = nameof(ShapeFilePath);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.ShapeFilePath = userInput;
                    PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
                    if(success)
                    {
                        newInput.CreateShapeFilePolygon(simulationInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success && evacGroupLineIndices.Count > 1) //if only one then it is not critical
                {
                    break;
                }

                //not critical
                if (newInputs.Count == 0)
                {
                    newInput.Default = true;
                }
                else
                {
                    nameOfInput = nameof(Default);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        bool.TryParse(userInput, out newInput.Default);
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput);
                    }
                    if (!success)
                    {
                        newInput.Default = false;
                    }
                    else if (newInput.Default)
                    {
                        foreach (KeyValuePair<string, EvacuationGroup> prevInput in newInputs)
                        {
                            EvacuationGroup eG = prevInput.Value;
                            eG.Default = false;
                        }
                    }
                }

                //not critical
                nameOfInput = nameof(Color);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    if (data.Length == 3)
                    {
                        issues += float.TryParse(data[0], out newInput.Color.r) ? 0 : 1;
                        issues += float.TryParse(data[1], out newInput.Color.g) ? 0 : 1;
                        issues += float.TryParse(data[2], out newInput.Color.b) ? 0 : 1;
                    }
                    else
                    {
                        issues++;
                    }
                    if (issues > 0)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if (!success)
                {
                    newInput.Color = PREACTColor.Random();
                }
            }

            return newInputs;
        }

        public static void SaveEvacGroupIndices(string filePath, Vector2int cells, int groupCount, int[] EvacGroupIndices)
        {

            string[] data = new string[4];
            //nrows
            data[0] = cells.x.ToString();
            //ncols
            data[1] = cells.y.ToString();
            //how many evac groups
            data[2] = groupCount.ToString();
            //actual data
            data[3] = "";
            for (int i = 0; i < EvacGroupIndices.Length; ++i)
            {
                data[3] += EvacGroupIndices[i] + " ";
            }

            File.WriteAllLines(filePath, data);
        }
    }
}
