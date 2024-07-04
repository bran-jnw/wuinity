//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace WUIPlatform.Traffic
{
    public class OpticalDensityRamp
    {
        LinearSpline1D rampData;

        public bool LoadOpticalDensityRampFile(string path)
        {
            string[] rampLines;
            if (File.Exists(path))
            {
                rampLines = File.ReadAllLines(path);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Optical density ramp file " + path + " not found.");
                return false;
            }

            List<Vector2> validRampLines = new List<Vector2>();
            //skip first line as that is just the header
            for (int i = 1; i < rampLines.Length; i++)
            {
                string[] rampLine = rampLines[i].Split(',');
                //make sure there is some data and not just empty line
                if(rampLine.Length == 2)
                {
                    float time, value;
                    bool validTime = float.TryParse(rampLine[0], out time);
                    bool validValue = float.TryParse(rampLine[1], out value);
                    if(validTime && validValue)
                    {
                        validRampLines.Add(new Vector2(time, value));
                    }                    
                }
            }

            if(validRampLines.Count >= 2)
            {
                rampData = new LinearSpline1D(validRampLines);
                return true;
            }
            else
            {
                return false;
            }            
        }

        public float GetOpticalDensity(float time)
        {
            return rampData.GetYValue(time);
        }
    }
}

