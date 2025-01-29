//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Collections.Generic;

namespace WUIPlatform.Fire
{
    public struct FireRasterData
    {
        public float TOA; //time of arrival
        public float ROS; //rate of spread
        public float FI; //fireline intensity
        public float SD; // spread direction
        public bool isActive;
    }

    /// <summary>
    /// Supports/tested using only Flammap version of Farsite.
    /// </summary>
    public class AsciiFireImport : FireModule
    {
        float maxTimeOfArrival = float.MinValue;
        int ncols, nrows, _activeCells;
        double xllcorner, yllcorner, cellsize, NODATA_VALUE;
        FireRasterData[,] data;
        Vector2d offset;

        float[] firelineIntensityData;
        List<Vector2int> newlyIgnitedCells;
        float[] _sootInjection;

        public AsciiFireImport() 
        {
            string TOAFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "TOA.asc");
            string ROSFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "ROS.asc");
            string FIFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "FI.asc");
            string SDFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "SD.asc");
            ReadOutput(TOAFile, ROSFile, FIFile, SDFile);

            Vector2d farsiteUTM = new Vector2d(xllcorner, yllcorner);
            offset = farsiteUTM - WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin;

            firelineIntensityData = new float[ncols * nrows];
            newlyIgnitedCells = new List<Vector2int>();
            _sootInjection = new float[ncols * nrows];

            WUIEngine.LOG(WUIEngine.LogType.Log, "Wildfire ASCII data offset by (x/y) meters: " + offset.x + ", " + offset.y);
        }

        public override void Step(float currentTime, float deltaTime)
        {
            bool updateVisuals = (int)currentTime % 60 == 0 ? true : false;

            if (updateVisuals)
            {
                int index = 0;
                for (int y = 0; y < nrows; y++)
                {
                    for (int x = 0; x < ncols; x++)
                    {
                        if (!data[x, y].isActive && WUIEngine.SIM.CurrentTime > data[x, y].TOA)
                        {
                            data[x, y].isActive = true;
                            newlyIgnitedCells.Add(new Vector2int(x, y));
                            firelineIntensityData[index] = data[x, y].FI;
                            _sootInjection[index] = 1f;
                            ++_activeCells;
                        }
                        ++index;
                    }
                }
            }            
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            return newlyIgnitedCells;
        }

        public override void ConsumeIgnitedFireCells()
        {
            newlyIgnitedCells.Clear();
        }

        public void GetOffsetAndScale(out Vector2d offset, out float xScale, out float yScale)
        {
            offset = this.offset;
            xScale = (float)(cellsize * ncols / WUIEngine.INPUT.Simulation.Size.x);
            yScale = (float)(cellsize * nrows / WUIEngine.INPUT.Simulation.Size.y);
        }

        public override bool IsSimulationDone()
        {
            return WUIEngine.SIM.CurrentTime > maxTimeOfArrival ? true : false;
        }        

        public override float[,] GetMaxROS()
        {
            throw new System.NotImplementedException();
        }

        public override float[,] GetMaxROSAzimuth()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCellCountX()
        {
            return ncols;
        }

        public override int GetCellCountY()
        {
            return nrows;
        }

        public override float GetCellSizeX()
        {
            return (float)cellsize;
        }

        public override float GetCellSizeY()
        {
            return (float)cellsize;
        }

        /// <summary>
        /// Imports time of arrival.
        /// </summary>
        private void ReadOutput(string TOAFile, string ROSFile, string FIFile, string SDFile)
        {
            string[] TOALines, ROSLines, FILines, SDLines;

            if (File.Exists(TOAFile))
            {
                TOALines = File.ReadAllLines(TOAFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Farsite time of arrival file not found.");
                return;
            }

            if (File.Exists(ROSFile))
            {
                ROSLines = File.ReadAllLines(ROSFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Farsite rate of spread file not found.");
                return;
            }

            if (File.Exists(FIFile))
            {
                FILines = File.ReadAllLines(FIFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Farsite fireline intensity file not found.");
                return;
            }

            if (File.Exists(SDFile))
            {
                SDLines = File.ReadAllLines(SDFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Farsite fireline intensity file not found.");
                return;
            }

            int.TryParse(TOALines[0].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out ncols);
            int.TryParse(TOALines[1].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out nrows);
            double.TryParse(TOALines[2].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out xllcorner);
            double.TryParse(TOALines[3].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out yllcorner);
            double.TryParse(TOALines[4].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out cellsize);
            double.TryParse(TOALines[5].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out NODATA_VALUE);

            data = new FireRasterData[ncols, nrows];

            for (int y = 0; y < nrows; y++)
            {
                string[] TOALine = TOALines[y + 6].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                string[] ROSLine = ROSLines[y + 6].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                string[] FILine = FILines[y + 6].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                string[] SDLine = SDLines[y + 6].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

                for (int x = 0; x < ncols; x++)
                {
                    float TOAValue, ROSValue, FIValue, SDValue;

                    float.TryParse(TOALine[x], out TOAValue);
                    TOAValue *= 60f; //received in minutes, want seconds
                    if (TOAValue > maxTimeOfArrival)
                    {
                        maxTimeOfArrival = TOAValue;
                    }
                    if(TOAValue < 0)
                    {
                        TOAValue = float.MaxValue;
                    }

                    float.TryParse(ROSLine[x], out ROSValue);

                    float.TryParse(FILine[x], out FIValue);

                    float.TryParse(SDLine[x], out SDValue);

                    //flip y-axis
                    int yIndex = nrows - 1 - y;
                    data[x, yIndex].TOA = TOAValue;
                    data[x, yIndex].ROS = ROSValue;
                    data[x, yIndex].FI = FIValue;
                    data[x, yIndex].SD = SDValue;
                    data[x, yIndex].isActive = false; ;
                }
            }
        }

        /// <summary>
        /// Returns state of cell on mesh based on lat/long. Returns dead if outside of mesh.
        /// </summary>
        /// <param name="latLong"></param>
        /// <returns></returns>
        public override FireCellState GetFireCellState(Vector2d latLong)
        {
            Vector2d pos = GeoConversions.GeoToWorldPosition(latLong.x, latLong.y,  WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale);
            pos += offset;

            int x = (int)(pos.x / cellsize);
            int y = (int)(pos.y / cellsize);

            FireCellState result = FireCellState.Burning;

            if (!IsInside(x, y) || WUIEngine.SIM.CurrentTime < data[x, y].TOA)
            {
                return FireCellState.Dead;
            }

            return result;
        }

        private bool IsInside(int x, int y)
        {
            bool result = false;
            if(x >= 0 && x < ncols && y >= 0 && y < nrows)
            {
                result = true;
            }

            return result;
        }

        public override float GetInternalDeltaTime()
        {
            return WUIEngine.INPUT.Simulation.DeltaTime;
        }

        public override float[] GetFireLineIntensityData()
        {
            return firelineIntensityData;
        }

        public override float[] GetFuelModelNumberData()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetSootProduction()
        {
            return _sootInjection;
        }

        public override int GetActiveCellCount()
        {
            return _activeCells;
        }

        public override WindData GetCurrentWindData()
        {
            return new WindData(0f, 270f, 10f, 0f);
        }

        public override void Stop()
        {
            //throw new System.NotImplementedException();
        }
    }
}

