//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.IO;

namespace PREACT.Evacuation
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
        public string Name;
        public ResponseDataPoint[] DataPoints;


        public ResponseCurve(ResponseDataPoint[] dataPoints, string name)
        {
            this.Name = name;
            this.DataPoints = dataPoints;
        }

        public ResponseCurve(List<ResponseDataPoint> dataPoints, string name)
        {
            Name = name;
            DataPoints = dataPoints.ToArray();
        }

        public void SetDataPoints(List<ResponseDataPoint> dataPoints)
        {
            DataPoints = dataPoints.ToArray();
        }

        public static Dictionary<string, ResponseCurve> Parse(string[] inputLines, List<int> responseCurveLineIndices, out bool success)
        {
            Dictionary<string, ResponseCurve> newInputs = new Dictionary<string, ResponseCurve>();
            success = false;

            for (int i = 0; i < responseCurveLineIndices.Count; ++i)
            {
                ResponseCurve newInput = new ResponseCurve();
                success = true;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, responseCurveLineIndices[i], true);
                string nameOfInput, userInput;

                //critical
                nameOfInput = nameof(Name);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.Name = userInput;
                    success = true;
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

                //this one is a bit special as each line does not have a name
                int startIndex = responseCurveLineIndices[i] + 2; //skip header and name line
                int endIndex = startIndex + inputToParse.Count - 1; //remove name line from count
                List<ResponseDataPoint> points = new List<ResponseDataPoint>(endIndex - startIndex);
                for (int j = startIndex; j < endIndex; ++j)
                {
                    issues = 0;
                    string[] data = inputLines[j].Split(',');
                    if(data.Length == 2)
                    {
                        ResponseDataPoint dataPoint = new ResponseDataPoint();

                        issues += float.TryParse(data[0], out dataPoint.time) ? 0 : 1;
                        issues += float.TryParse(data[1], out dataPoint.probability) ? 0 : 1;

                        if (issues > 0)
                        {
                            PREACTInput.CouldNotInterpretInputMessage("Response curve data point on line " + j, userInput);
                            break;
                        }
                        else
                        {
                            points.Add(new ResponseDataPoint());
                        }
                    }
                    else
                    {
                        issues++;
                        PREACTInput.CouldNotInterpretInputMessage("Response curve data point on line " + j, userInput);
                        break;
                    }    
                }
                if (points.Count > 1 && issues == 0)
                {
                    newInput.SetDataPoints(points);
                }
                else
                {
                    success = false;
                    PREACTInput.CouldNotInterpretInputMessage("Response curve could not be constructed, too few points.", userInput);
                }

                if (success)
                {
                    newInputs.Add(newInput.Name, newInput);
                }                
            }

            if (newInputs.Count == responseCurveLineIndices.Count)
            {
                success = true;
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "Could not read all specified ResponseCurves.");
            }
            return newInputs;
        }
    }
}

