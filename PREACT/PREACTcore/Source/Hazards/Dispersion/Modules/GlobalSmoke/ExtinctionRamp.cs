//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace PREACT.Smoke
{
    public class ExtinctionRamp
    {
        LinearSpline1D _rampData;

        public ExtinctionRamp(LinearSpline1D rampData)
        {
            _rampData = rampData;
        }

        public float GetOpticalDensity(float time)
        {
            return _rampData.GetYValue(time);
        }
        public static ExtinctionRamp LoadExtinctionRampFile(string filePath, out bool success)
        {
            success = false;
            ExtinctionRamp extinctionRamp = null;

            string[] rampLines;
            if (File.Exists(filePath))
            {
                rampLines = File.ReadAllLines(filePath);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Extinction coefficient ramp file " + filePath + " not found.");
                return extinctionRamp;
            }

            List<Vector2> validRampLines = new List<Vector2>();
            //skip first line as that is just the header
            for (int i = 1; i < rampLines.Length; i++)
            {
                string[] rampLine = rampLines[i].Split(',');
                //make sure there is some data and not just empty line
                if (rampLine.Length == 2)
                {
                    float time, value;
                    bool validTime = float.TryParse(rampLine[0], out time);
                    bool validValue = float.TryParse(rampLine[1], out value);
                    if (validTime && validValue)
                    {
                        validRampLines.Add(new Vector2(time, value));
                    }
                }
            }

            if (validRampLines.Count >= 2)
            {
                success = true;
                extinctionRamp = new ExtinctionRamp(new LinearSpline1D(validRampLines));
            }

            return extinctionRamp;
        }
    }
}

