//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;
using System;

namespace PREACT.Wildfire
{
    [System.Serializable]                           
    public struct IgnitionPointInput
    {
        public Vector2d LatLon;
        public bool AbsoluteTime;
        public float IgnitionTime;
        public DateTime IgnitionDateTime;

        public IgnitionPointInput(Vector2d latLong, bool absoluteTime, float ignitionTime, DateTime ignitionDateTime)    
        {
            LatLon = latLong;
            IgnitionTime = ignitionTime;
            AbsoluteTime = absoluteTime;
            IgnitionDateTime = ignitionDateTime;
        }

        public IgnitionPointInput(double lat, double lon, bool absoluteTime, float ignitionTime, DateTime ignitionDateTime)
        {
            LatLon = new Vector2d(lat, lon);
            IgnitionTime = ignitionTime;
            AbsoluteTime = absoluteTime;
            IgnitionDateTime = ignitionDateTime;
        }

        /// <summary>
        /// Tries to load ignition points froma file defined in the general input file.
        /// Returns an array with anu loaded ignition points, othwerwise returns null.
        /// Sends message to the WUI_LOG to inform the user.
        /// </summary>
        /// <returns></returns>
        public static IgnitionPointInput[] LoadIgnitionPointsFile(string path, IO.SimulationInput simulationInput, out bool success)
        {
            success = false;
            IgnitionPointInput[] result = null;
            List<IgnitionPointInput> ignitionPoints= new List<IgnitionPointInput>();
            
            bool fileExists = File.Exists(path);
            if (fileExists)
            {
                string[] dataLines = File.ReadAllLines(path);
                //skip first line (header)
                for (int j = 1; j < dataLines.Length; j++)
                {
                    string[] data = dataLines[j].Split(',');
                    if (data.Length >= 3)
                    {
                        double lat, lon;
                        float ignitionTime = 0;
                        DateTime dateTime = DateTime.Now;
                        bool absoluteTime = false;

                        bool b1 = double.TryParse(data[0], out lat);
                        bool b2 = double.TryParse(data[1], out lon);
                        bool b3 = bool.TryParse(data[2], out absoluteTime);

                        bool b4;
                        if(absoluteTime)
                        {
                            b4 = DateTime.TryParse(data[3], out dateTime);
                            if(b4)
                            {
                                ignitionTime = (float)(dateTime - simulationInput.StartDateTime).TotalSeconds;
                            }                           
                        }
                        else
                        {
                            b4 = float.TryParse(data[3], out ignitionTime);
                        }                           

                        if (b1 && b2 && b3 && b4)
                        {
                            IgnitionPointInput iP = new IgnitionPointInput(lat, lon, absoluteTime, ignitionTime, dateTime);
                            ignitionPoints.Add(iP);
                        }
                    }
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Ignition points data file " + path + " not found and could not be loaded, fire and smoke spread will have to rely on other ignition methods (painted map).");
            }

            if (ignitionPoints.Count > 0)
            {
                result = ignitionPoints.ToArray();
                Engine.Message(null, Engine.LogType.Log, " Ignition points data file " + path + " was found, " + ignitionPoints.Count + " valid data points were succesfully loaded.");
                success = true;
            }
            else if (fileExists)
            {
                Engine.Message(null, Engine.LogType.Warning, "Ignition points data file " + path + " was found but did not contain any valid data, fire and smoke spread will have to rely on other ignition methods (painted map).");
            }

            return result;
        }
    }
}
