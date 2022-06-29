using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace WUInity
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

        public static ResponseCurve[] LoadResponseCurves()
        {
            List<ResponseCurve> responseCurves = new List<ResponseCurve>();
            for (int i = 0; i < WUInity.INPUT.evac.responseCurveFiles.Length; i++)
            {
                string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.evac.responseCurveFiles[i] + ".rsp");
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
                        responseCurves.Add(new ResponseCurve(dataPoints, WUInity.INPUT.evac.responseCurveFiles[i]));
                        WUInity.LOG("LOG: Loaded response curve from " + path + " named " + responseCurves[i].name);
                    }                    
                }
                else
                {
                    WUInity.LOG("WARNING: Response curve file not found in " + path + " and could not be loaded, might be issues with evacuation (will not run).");
                }
            }

            if(responseCurves.Count > 0)
            {
                ResponseCurve[] rCurves = responseCurves.ToArray();
                return rCurves;
            }
            else
            {
                WUInity.LOG("ERROR: No response curves could be loaded, simulation will stall.");
                return null;
            }   
        }
    }
}

