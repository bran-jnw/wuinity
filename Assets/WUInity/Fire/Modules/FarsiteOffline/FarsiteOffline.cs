using System.IO;
using WUInity.Utility;
using System.Numerics;
using UnityEngine.UIElements;

namespace WUInity.Fire
{
    public struct FarsiteRasterData
    {
        public float TOA; //time of arrival
        public float ROS; //rate of spread
        public float FI; //fireline intensity

    }
    public class FarsiteOffline : FireModule
    {
        float maxTimeOfArrival = float.MinValue;
        int ncols, nrows, activeCells;
        double xllcorner, yllcorner, cellsize, NODATA_VALUE;
        FarsiteRasterData[,] data;
        Vector2d offset;
        bool _hasUpdatedRenderer = false;

        public FarsiteOffline() 
        {
            string TOAFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.FarsiteData.rootFolder, "output", "TOA.asc");
            string ROSFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.FarsiteData.rootFolder, "output", "ROS.asc");
            string FIFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.FarsiteData.rootFolder, "output", "FI.asc");
            ReadOutput(TOAFile, ROSFile, FIFile);

            LatLngUTMConverter converter = new LatLngUTMConverter(null); //default WGS84
            LatLngUTMConverter.UTMResult simUTM = converter.convertLatLngToUtm(WUInity.INPUT.Simulation.LowerLeftLatLong.x, WUInity.INPUT.Simulation.LowerLeftLatLong.y);
            Vector2d farsiteUTM = new Vector2d(xllcorner, yllcorner);
            offset = farsiteUTM - new Vector2d(simUTM.Easting, simUTM.Northing);

            //WUInity.LOG(WUInity.LogType.Log, "Farsite import offset by (x/y) meters: " + offset.x + ", " + offset.y);
        }

        public override void Update(float currentTime, float deltaTime)
        {
            activeCells = 0; //TODO: calculate this
        }

        public void GetOffsetAndScale(out Vector2d offset, out float xScale, out float yScale)
        {
            offset = this.offset;
            xScale = (float)(cellsize * ncols / WUInity.INPUT.Simulation.Size.x);
            yScale = (float)(cellsize * nrows / WUInity.INPUT.Simulation.Size.y);
        }

        public override bool SimulationDone()
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
        private void ReadOutput(string TOAFile, string ROSFile, string FIFile)
        {
            string[] TOALines, ROSLines, FILines;

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

            int.TryParse(TOALines[0].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out ncols);
            int.TryParse(TOALines[1].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out nrows);
            double.TryParse(TOALines[2].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out xllcorner);
            double.TryParse(TOALines[3].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out yllcorner);
            double.TryParse(TOALines[4].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out cellsize);
            double.TryParse(TOALines[5].Split(" ", System.StringSplitOptions.RemoveEmptyEntries)[1], out NODATA_VALUE);

            data = new FarsiteRasterData[ncols, nrows];

            for (int y = 0; y < nrows; y++)
            {
                string[] TOALine = TOALines[y + 6].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                string[] ROSLine = ROSLines[y + 6].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
                string[] FILine = FILines[y + 6].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

                for (int x = 0; x < ncols; x++)
                {
                    float TOAValue, ROSValue, FIValue;

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

                    //flip y-axis
                    int yIndex = nrows - 1 - y;
                    data[x, yIndex].TOA = TOAValue;
                    data[x, yIndex].ROS = ROSValue;
                    data[x, yIndex].FI = FIValue;
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
            float[] result = new float[ncols * nrows];
            int index = 0;
            for (int y = 0; y < nrows; y++)
            {
                for (int x = 0; x < ncols; x++)
                {
                    result[index] = 0f;
                    if (WUInity.SIM.CurrentTime > data[x, y].TOA)
                    {
                        result[index] = data[x, y].FI;
                    }                    
                    ++index;
                }
            }
            return result;
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

