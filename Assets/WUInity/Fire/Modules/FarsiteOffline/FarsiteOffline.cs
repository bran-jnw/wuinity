using System.IO;
using System.Collections.Generic;

namespace WUInity.Fire
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
    /// Supports only Flammap version of Farsite.
    /// </summary>
    public class FarsiteOffline : FireModule
    {
        float maxTimeOfArrival = float.MinValue;
        int ncols, nrows, activeCells;
        double xllcorner, yllcorner, cellsize, NODATA_VALUE;
        FireRasterData[,] data;
        Vector2d offset;

        float[] firelineIntensityData;
        List<Vector2int> newlyIgnitedCells;

        public FarsiteOffline() 
        {
            string TOAFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.farsiteData.rootFolder, "output", "TOA.asc");
            string ROSFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.farsiteData.rootFolder, "output", "ROS.asc");
            string FIFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.farsiteData.rootFolder, "output", "FI.asc");
            string SDFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.farsiteData.rootFolder, "output", "SD.asc");
            ReadOutput(TOAFile, ROSFile, FIFile, SDFile);

            Vector2d farsiteUTM = new Vector2d(xllcorner, yllcorner);
            offset = farsiteUTM - WUInity.RUNTIME_DATA.Simulation.UTMOrigin;

            firelineIntensityData = new float[ncols * nrows];
            newlyIgnitedCells = new List<Vector2int>();

            //WUInity.LOG(WUInity.LogType.Log, "Farsite import offset by (x/y) meters: " + offset.x + ", " + offset.y);
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
                        if (!data[x, y].isActive && WUInity.SIM.CurrentTime > data[x, y].TOA)
                        {
                            data[x, y].isActive = true;
                            newlyIgnitedCells.Add(new Vector2int(x, y));
                            firelineIntensityData[index] = data[x, y].FI;
                            ++activeCells;
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
            xScale = (float)(cellsize * ncols / WUInity.INPUT.Simulation.Size.x);
            yScale = (float)(cellsize * nrows / WUInity.INPUT.Simulation.Size.y);
        }

        public override bool IsSimulationDone()
        {
            return WUInity.SIM.CurrentTime > maxTimeOfArrival ? true : false;
        }        

        public override int[,] GetMaxROS()
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
                WUInity.LOG(WUInity.LogType.Error, "Farsite time of arrival file not found.");
                return;
            }

            if (File.Exists(ROSFile))
            {
                ROSLines = File.ReadAllLines(ROSFile);
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Farsite rate of spread file not found.");
                return;
            }

            if (File.Exists(FIFile))
            {
                FILines = File.ReadAllLines(FIFile);
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Farsite fireline intensity file not found.");
                return;
            }

            if (File.Exists(SDFile))
            {
                SDLines = File.ReadAllLines(SDFile);
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Farsite fireline intensity file not found.");
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
            Vector2d pos = Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
            pos += offset;

            int x = (int)(pos.x / cellsize);
            int y = (int)(pos.y / cellsize);

            FireCellState result = FireCellState.Burning;

            if (!IsInside(x, y) || WUInity.SIM.CurrentTime < data[x, y].TOA)
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
            return WUInity.INPUT.Simulation.DeltaTime;
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
            throw new System.NotImplementedException();
        }

        public override int GetActiveCellCount()
        {
            return activeCells;
        }

        public override WindData GetCurrentWindData()
        {
            return new WindData();
        }
    }
}

