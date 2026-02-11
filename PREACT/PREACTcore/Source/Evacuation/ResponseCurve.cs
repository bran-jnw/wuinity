//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.IO;
using System;

namespace PREACT.Evacuation
{
    public enum TimeInputs { Absolute, Relative }

    public struct ResponseDataPoint
    {
        public float Probability;
        public float Time;

        public ResponseDataPoint(float time, float probability)
        {
            Time = time;
            Probability = probability;
        }
    }

    /// <summary>
    /// Response curve used for when people will start evacuating.
    /// Time relative to evacuation order being announced.
    /// </summary>
    public struct ResponseCurve
    {
        public string Name;
        public TimeInputs TimeInput;
        public ResponseDataPoint[] DataPoints;


        public ResponseCurve(ResponseDataPoint[] dataPoints, string name)
        {
            Name = name;
            DataPoints = dataPoints;
            TimeInput = TimeInputs.Relative;
        }

        public ResponseCurve(List<ResponseDataPoint> dataPoints, string name)
        {
            Name = name;
            DataPoints = dataPoints.ToArray();
            TimeInput = TimeInputs.Relative;
        }

        public void SetDataPoints(List<ResponseDataPoint> dataPoints)
        {
            DataPoints = dataPoints.ToArray();
        }

        public static void Parse(Dictionary<string, ResponseCurve> newInputs, string[] inputLines, List<int> responseCurveLineIndices, SimulationInput simulationInput, out bool success)
        {
            success = false;
            newInputs.Clear();

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

                //critical
                nameOfInput = nameof(TimeInput);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    switch (userInput)
                    {
                        case nameof(TimeInputs.Absolute):
                            newInput.TimeInput = TimeInputs.Absolute;
                            break;
                        case nameof(TimeInputs.Relative):
                            newInput.TimeInput = TimeInputs.Relative;
                            break;
                        default:
                            ++issues;
                            PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                            break;
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

                //this one is a bit special as each line does not have a name
                int startIndex = responseCurveLineIndices[i] + 3; //skip header, name line and time relation
                int endIndex = startIndex + inputToParse.Count - 2; //remove name and timeinput line from count
                List<ResponseDataPoint> points = new List<ResponseDataPoint>(endIndex - startIndex);
                for (int j = startIndex; j < endIndex; ++j)
                {
                    issues = 0;
                    string[] data = inputLines[j].Split(',');
                    if(data.Length == 2)
                    {
                        ResponseDataPoint dataPoint = new ResponseDataPoint();

                        if(newInput.TimeInput == TimeInputs.Relative)
                        {
                            issues += float.TryParse(data[0], out dataPoint.Time) ? 0 : 1;
                        }
                        else
                        {
                            DateTime dateTime;
                            if(DateTime.TryParse(data[0], out dateTime))
                            {
                                dataPoint.Time = (float)(dateTime - simulationInput.StartDateTime).TotalSeconds;
                            }
                            else
                            {
                                ++issues;
                            }
                        }                        
                        issues += float.TryParse(data[1], out dataPoint.Probability) ? 0 : 1;

                        if (issues > 0)
                        {
                            PREACTInput.CouldNotInterpretInputMessage("Response curve data point on line " + j, userInput);
                            break;
                        }
                        else
                        {
                            points.Add(dataPoint);
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
        }
    }
}

