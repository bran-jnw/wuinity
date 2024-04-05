using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace WUInity.Traffic
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
                WUInity.LOG(WUInity.LogType.Warning, "Optical density ramp file " + path + " not found.");
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

