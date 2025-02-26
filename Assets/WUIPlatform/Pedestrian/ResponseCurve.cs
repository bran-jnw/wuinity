//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;

namespace WUIPlatform
{
    [System.Serializable]
    public struct ResponseDataPoint
    {
        public float probability;
        public float time;

        public ResponseDataPoint(float time, float probability)
        {
            this.time = time;
            this.probability = probability;
        }
    }

    /// <summary>
    /// Response curve used for when people will start evacuating.
    /// Time relative to evacuation order being announced.
    /// </summary>
    [System.Serializable]
    public struct ResponseCurve
    {
        public string name;
        public ResponseDataPoint[] dataPoints;


        public ResponseCurve(ResponseDataPoint[] dataPoints, string name)
        {
            this.name = name;
            this.dataPoints = dataPoints;
        }

        public ResponseCurve(List<ResponseDataPoint> dataPoints, string name)
        {
            this.name = name;
            this.dataPoints = dataPoints.ToArray();
        }

        public static ResponseCurve[] LoadResponseCurves(out bool success)
        {
            success = false;
            List<ResponseCurve> responseCurves = new List<ResponseCurve>();
            for (int i = 0; i < WUIEngine.INPUT.Evacuation.ResponseCurves.Length; i++)
            {
                string path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Evacuation.ResponseCurves[i] + ".rsp");
                if (File.Exists(path))
                {
                    string[] dataLines = File.ReadAllLines(path);
                    List<ResponseDataPoint> dataPoints = new List<ResponseDataPoint>();
                    //skip first line (header)
                    for (int j = 1; j < dataLines.Length; j++)
                    {
                        string[] data = dataLines[j].Split(',');

                        if(data.Length >= 2)
                        {
                            float time, probability;

                            bool timeRead = float.TryParse(data[0], out time);
                            bool probabilityRead = float.TryParse(data[1], out probability);
                            if (timeRead && probabilityRead)
                            {
                                ResponseDataPoint dataPoint = new ResponseDataPoint(time, probability);
                                dataPoints.Add(dataPoint);
                            }
                        }                                            
                    }

                    //need at least two to make a curve
                    if(dataPoints.Count >= 2)
                    {
                        responseCurves.Add(new ResponseCurve(dataPoints, WUIEngine.INPUT.Evacuation.ResponseCurves[i]));
                        WUIEngine.LOG(WUIEngine.LogType.Log, " Loaded response curve from " + path + " named " + responseCurves[i].name);
                    }                    
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, "Response curve file not found in " + path + " and could not be loaded, might be issues with evacuation (will not run).");
                }
            }

            if(responseCurves.Count > 0)
            {
                ResponseCurve[] rCurves = responseCurves.ToArray();
                success = true;
                return rCurves;
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, " No response curves could be loaded, simulation will stall.");
                return null;
            }   
        }
    }
}

