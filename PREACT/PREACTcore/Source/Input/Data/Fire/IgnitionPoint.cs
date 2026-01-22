//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;

namespace PREACT.Wildfire
{
    [System.Serializable]                           
    public struct IgnitionPointInput
    {
        public Vector2d LatLon;                    
        public float IgnitionTime;           

        public IgnitionPointInput(Vector2d latLong, float ignitionTime)    
        {
            this.LatLon = latLong;
            this.IgnitionTime = ignitionTime;
        }

        public IgnitionPointInput(double lat, double lon, float ignitionTime)
        {
            this.LatLon = new Vector2d(lat, lon);
            this.IgnitionTime = ignitionTime;
        }

        /// <summary>
        /// Tries to load ignition points froma file defined in the general input file.
        /// Returns an array with anu loaded ignition points, othwerwise returns null.
        /// Sends message to the WUI_LOG to inform the user.
        /// </summary>
        /// <returns></returns>
        public static IgnitionPointInput[] LoadIgnitionPointsFile(string path, out bool success)
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
                        float ignitionTime;

                        bool b1 = double.TryParse(data[0], out lat);
                        bool b2 = double.TryParse(data[1], out lon);
                        bool b3 = float.TryParse(data[2], out ignitionTime);

                        if (b1 && b2 && b3)
                        {
                            IgnitionPointInput iP = new IgnitionPointInput(lat, lon, ignitionTime);
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
