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
        public float TimeOfAArrival; //seconds
        public float RateOfSpread; //m/min
        public float FirelineIntensity; //kW/m
        public float SpreadDirection; // Degrees, 0 in North, then clockwise
        public bool isActive;
    }

    /// <summary>
    /// Supports/tested using only Flammap version of Farsite.
    /// </summary>
    public class AscFireImport : FireModule
    {
        private float _maxTimeOfArrival = float.MinValue;
        private int ncols, nrows, _activeCells;
        private double _xllcorner, _yllcorner, _cellsize, _NODATA_VALUE;
        private FireRasterData[,] _data;

        //TODO: clean this up, this is duplicate data but is needed for shaders, come up with some way of better data storage
        float[] firelineIntensityData;

        List<Vector2int> newlyIgnitedCells;
        float[] _sootInjection;

        public AscFireImport() 
        {
            string TOAFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "TOA.asc");
            string ROSFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "ROS.asc");
            string FIFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "FI.asc");
            string SDFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ascData.rootFolder, "output", "SD.asc");
            ReadOutput(TOAFile, ROSFile, FIFile, SDFile);

            Vector2d farsiteUTM = new Vector2d(_xllcorner, _yllcorner);
            _originOffset = farsiteUTM - WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin;

            firelineIntensityData = new float[ncols * nrows];
            newlyIgnitedCells = new List<Vector2int>();
            _sootInjection = new float[ncols * nrows];

            WUIEngine.LOG(WUIEngine.LogType.Log, "Wildfire ASCII data offset by (x/y) meters: " + _originOffset.x + ", " + _originOffset.y);
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
                        if (!_data[x, y].isActive && WUIEngine.SIM.CurrentTime > _data[x, y].TimeOfAArrival)
                        {
                            _data[x, y].isActive = true;
                            newlyIgnitedCells.Add(new Vector2int(x, y));
                            firelineIntensityData[index] = _data[x, y].FirelineIntensity;
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
            offset = this._originOffset;
            xScale = (float)(_cellsize * ncols / WUIEngine.INPUT.Simulation.Size.x);
            yScale = (float)(_cellsize * nrows / WUIEngine.INPUT.Simulation.Size.y);
        }

        public override bool IsSimulationDone()
        {
            return WUIEngine.SIM.CurrentTime > _maxTimeOfArrival ? true : false;
        }

        float[,] maxROS;
        /// <summary>
        /// Returns rate of spread (m/min), data is "flipped" on y-axis (so lower left is a (0, nrows - 1))
        /// </summary>
        /// <returns></returns>
        public override float[,] GetMaxROS()
        {
            if(maxROS == null)
            {
                maxROS = new float[ncols, nrows];
                for(int y = 0; y < nrows; ++y)
                {
                    int yFlipped = nrows - 1 - y;
                    for (int x = 0; x < nrows; ++x)
                    {
                        maxROS[x, y] = _data[x, yFlipped].RateOfSpread;
                    }
                }
            }
            
            return maxROS;
        }

        float[,] maxROSAzimuth;
        public override float[,] GetMaxROSAzimuth()
        {
            if (maxROSAzimuth == null)
            {
                maxROSAzimuth = new float[ncols, nrows];
                for (int y = 0; y < nrows; ++y)
                {
                    int yFlipped = nrows - 1 - y;
                    for (int x = 0; x < nrows; ++x)
                    {
                        maxROSAzimuth[x, y] = _data[x, yFlipped].SpreadDirection;
                    }
                }
            }

            return maxROSAzimuth;
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
            return (float)_cellsize;
        }

        public override float GetCellSizeY()
        {
            return (float)_cellsize;
        }

        /// <summary>
        /// Imports as files from external wildfire simulation.
        /// </summary>
        /// <param name="TOAFile">Time of arrival, minutes.</param>
        /// <param name="ROSFile">Rate of spread, m/min.</param>
        /// <param name="FIFile">Fireline intensity, kW/m.</param>
        /// <param name="SDFile">Spread direction, degrees from North = 0 and then clockwise.</param>
        private void ReadOutput(string TOAFile, string ROSFile, string FIFile, string SDFile)
        {
            string[] TOALines, ROSLines, FILines, SDLines;

            if (File.Exists(TOAFile))
            {
                TOALines = File.ReadAllLines(TOAFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Time of arrival file not found.");
                return;
            }

            if (File.Exists(ROSFile))
            {
                ROSLines = File.ReadAllLines(ROSFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Rate of spread file not found.");
                return;
            }

            if (File.Exists(FIFile))
            {
                FILines = File.ReadAllLines(FIFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Fireline intensity file not found.");
                return;
            }

            if (File.Exists(SDFile))
            {
                SDLines = File.ReadAllLines(SDFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Fireline intensity file not found.");
                return;
            }

            int.TryParse(TOALines[0].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out ncols);
            int.TryParse(TOALines[1].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out nrows);
            double.TryParse(TOALines[2].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out _xllcorner);
            double.TryParse(TOALines[3].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out _yllcorner);
            double.TryParse(TOALines[4].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out _cellsize);
            double.TryParse(TOALines[5].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out _NODATA_VALUE);

            _data = new FireRasterData[ncols, nrows];

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
                    if (TOAValue > _maxTimeOfArrival)
                    {
                        _maxTimeOfArrival = TOAValue;
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
                    _data[x, yIndex].TimeOfAArrival = TOAValue;
                    _data[x, yIndex].RateOfSpread = ROSValue;
                    _data[x, yIndex].FirelineIntensity = FIValue;
                    _data[x, yIndex].SpreadDirection = SDValue;
                    _data[x, yIndex].isActive = false; ;
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
            pos += _originOffset;

            int x = (int)(pos.x / _cellsize);
            int y = (int)(pos.y / _cellsize);

            FireCellState result = FireCellState.Burning;

            if (!IsInside(x, y) || WUIEngine.SIM.CurrentTime < _data[x, y].TimeOfAArrival)
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
            return new WindData(0f, 150f, 20f, 0f);
        }

        public override void Stop()
        {
            //throw new System.NotImplementedException();
        }        
    }
}

