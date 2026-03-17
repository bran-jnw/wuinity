//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using PREACT.Input;
using PREACT.Math;
using System;

namespace PREACT.Population
{
    [Serializable]
    public class LocalGPWData
    {
        public Vector2d ActualOriginLatLon;
        public Vector2d OriginOffset;
        public Vector2d RealWorldSize;
        public Vector2int CellCount;
        public int TotalPopulation;
        private double[] _density;     


        public LocalGPWData(Vector2d actualOriginDegrees, Vector2d originOffset, Vector2d realWorldSize, Vector2int cellCount, int totalPopulation, double[] density)
        {
            ActualOriginLatLon = actualOriginDegrees;
            OriginOffset = originOffset;
            RealWorldSize = realWorldSize;
            CellCount = cellCount;
            TotalPopulation = totalPopulation;
            _density = density;
        }

        //http://www.land-navigation.com/latitude-and-longitude.html
        public static Vector2d SizeToDegrees(Vector2d latLon, Vector2d desiredSize)
        {
            double earth = 40075000.0 * 0.5;
            double yDegrees = 180.0 * desiredSize.y / earth;
            double xDegrees = 180.0 * desiredSize.x / Mathd.Abs(Mathd.Cos((Mathd.PI / 180.0) * latLon.x) * earth);

            return new Vector2d(xDegrees, yDegrees);
        }

        public static Vector2d DegreesToSize(Vector2d latLon, Vector2d degrees)
        {
            double earth = 40075000.0 * 0.5;
            double ySize = degrees.y * earth / 180.0;
            double xSize = degrees.x * Mathd.Abs(Mathd.Cos((Mathd.PI / 180.0) * latLon.x) * earth) / 180.0;

            return new Vector2d(xSize, ySize);
        }

        public void SaveToDisk(string filePath)
        {            
            string[] data = new string[6];
                        
            data[0] = ActualOriginLatLon.x + " " + ActualOriginLatLon.y;
            data[1] = OriginOffset.x + " " + OriginOffset.y;
            data[2] = RealWorldSize.x + " " + RealWorldSize.y;
            data[3] = CellCount.x + " " + CellCount.y;
            data[4] = TotalPopulation.ToString();

            string densityData = "";
            for (int i = 0; i < _density.Length; ++i)
            {
                densityData += _density[i] + " ";
            }
            data[5] = densityData;

            File.WriteAllLines(filePath, data);
        }

        public static LocalGPWData LoadFromFile(string localGpwFilePath, out bool success)
        {
            success = false;
            LocalGPWData localGPWData = null;

            if (File.Exists(localGpwFilePath))
            {
                string[] d = File.ReadAllLines(localGpwFilePath);

                Vector2d actualOriginDegrees;
                Vector2d originOffset;
                Vector2d realWorldSize;
                Vector2int _cellCount;  
                int totalPopulation;
                double[] density;

                string[] dummy = d[0].Split(' ');
                double xD;
                double yD;
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                actualOriginDegrees = new Vector2d(xD, yD);

                dummy = d[1].Split(' ');
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                originOffset = new Vector2d(xD, yD);

                dummy = d[2].Split(' ');
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                realWorldSize = new Vector2d(xD, yD);

                dummy = d[3].Split(' ');
                int xI;
                int yI;
                int.TryParse(dummy[0], out xI);
                int.TryParse(dummy[1], out yI);
                _cellCount = new Vector2int(xI, yI);

                int.TryParse(d[4], out totalPopulation);

                dummy = d[5].Split(' ');
                density = new double[_cellCount.x * _cellCount.y];
                for (int i = 0; i < density.Length; ++i)
                {
                    double.TryParse(dummy[i], out density[i]);
                }

                localGPWData = new LocalGPWData(actualOriginDegrees, originOffset, realWorldSize, _cellCount, totalPopulation, density);
                success = true;
                Engine.Message(null, Engine.LogType.Log, " Loaded local GPW data from " + localGpwFilePath);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, " No local GPW data was found, build from global GPW or create custom population.");                
            }

            return localGPWData;
        }

        public static LocalGPWData CreateLocalGPWData(Vector2d lowerLeftLatLon, Vector2d domainSize, string globalGpwFolder, out bool success)
        {
            success = false;
            LocalGPWData localGPWData = null;

            if (IsGPWAvailable(globalGpwFolder))
            {
                // New code to accept GPW data-sets from any version and any year
                string[] AscFiles = Directory.GetFiles(globalGpwFolder, "*.asc");  // Get all ASCII files of the GPW data set
                Array.Sort(AscFiles);   // Sort the array in case it is not already sorted.
                string relevantAscFile;

                if (lowerLeftLatLon.x >= -3.4106051316485e-012)
                {
                    if (lowerLeftLatLon.y < -90.000000000005)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_1.asc");
                        relevantAscFile = AscFiles[0];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 1");
                    }
                    else if (lowerLeftLatLon.y < -1.0231815394945e-011)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_2.asc");
                        relevantAscFile = AscFiles[1];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 2");
                    }
                    else if (lowerLeftLatLon.y < 89.999999999985)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_3.asc");
                        relevantAscFile = AscFiles[2];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 3");
                    }
                    else
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_4.asc");
                        relevantAscFile = AscFiles[3];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 4");
                    }
                }
                else
                {
                    if (lowerLeftLatLon.y < -90.000000000005)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_5.asc");
                        relevantAscFile = AscFiles[4];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 5");
                    }
                    else if (lowerLeftLatLon.y < -1.0231815394945e-011)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_6.asc");
                        relevantAscFile = AscFiles[5];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 6");
                    }
                    else if (lowerLeftLatLon.y < 89.999999999985)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_7.asc");
                        relevantAscFile = AscFiles[6];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 7");
                    }
                    else
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_8.asc");
                        relevantAscFile = AscFiles[7];
                        Engine.Message(null, Engine.LogType.Log, "Loading GPW from sector 8");
                    }
                }

                localGPWData = CreateFromGlobalGPWSector(relevantAscFile, lowerLeftLatLon, domainSize, out success);
                localGPWData.CalculateTotalPopulation();
            }

            return localGPWData;
        }

        bool AreSame(double a, double b)    // for comparing double values, added 14/08/2023
        {
            double tolerance = 1.0E-8;
            double absDiff = Mathd.Abs(a - b);

            if (absDiff <= tolerance) return true;
            if (absDiff < Mathd.Max(Mathd.Abs(a), Mathd.Abs(b)) * tolerance) return true;

            return false;
        }

        private void CalculateTotalPopulation()
        {
            TotalPopulation = 0;

            double cellSizeX = RealWorldSize.x / CellCount.x;
            double cellSizeY = RealWorldSize.y / CellCount.y;
            double cellArea = cellSizeX * cellSizeY / (1000000d); // people/square km
            TotalPopulation = 0;
            for (int y = 0; y < CellCount.y; ++y)
            {
                for (int x = 0; x < CellCount.x; ++x)
                {
                    double density = GetDensity(x, y);
                    int pop = Mathf.CeilToInt((float)(cellArea * density));
                    pop = Mathf.Clamp(pop, 0, pop);
                    TotalPopulation += pop;
                }
            }
        }

        /// <summary>
        /// Returns the density data at a gridpoint
        /// </summary>
        public double GetDensity(int x, int y)
        {
            if(_density == null || _density.Length == 0)
            {
                return -1.0;
            }

            x = Mathf.Clamp(x, 0, CellCount.x - 1);
            y = Mathf.Clamp(y, 0, CellCount.y - 1);
            return _density[x + y * CellCount.x];
        }

        public double GetDensitySimulationSpace(Vector2d pos)
        {
            Vector2d positiveSize = RealWorldSize + OriginOffset; //since offset is always negative we add it here
            int xInt = (int)((pos.x / positiveSize.x) * CellCount.x);
            int yInt = (int)((pos.y / positiveSize.y) * CellCount.y);
            double dens = GetDensity(xInt, yInt);
            return dens;
        }

        public  double GetDensitySimulationSpaceBilinear(Vector2d pos)
        {
            Vector2d positiveSize = RealWorldSize + OriginOffset; //since offset is always negative we add it here

            double x = (pos.x / positiveSize.x) * CellCount.x;
            int xLow = (int)x;
            int xHigh = xLow + 1;
            double xWeight = x - xLow;

            double y = (pos.y / positiveSize.y) * CellCount.y;
            int yLow = (int)y;
            int yHigh = yLow + 1;
            double yWeight = y - yLow;

            //do bilinear interpoaltion
            double h1 = GetDensity(xLow, yLow);
            double h2 = GetDensity(xHigh, yLow);
            double h3 = GetDensity(xLow, yHigh);
            double h4 = GetDensity(xHigh, yHigh);
            double hYLow = (1.0 - xWeight) * h1 + xWeight * h2;
            double hYHigh = (1.0 - xWeight) * h3 + xWeight * h4;
            double h = (1.0 - yWeight) * hYLow + yWeight * hYHigh;

            return h;
        }       

        private static bool IsGPWAvailable(string path)
        {
            bool isAvailable = false;

            if (Directory.Exists(path))
            {
                string[] AscFiles = Directory.GetFiles(path, "*.asc");
                Engine.Message(null, Engine.LogType.Log, AscFiles.Length.ToString() + " GPW files found.");

                if (AscFiles.Length == 8)
                {
                    isAvailable = true;
                }
                else
                {
                    Engine.Message(null, Engine.LogType.InputError, "Not all GPW files found.");
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "GPW path does NOT exist.");
            }

            return isAvailable;
        }

        /// <summary>
        /// Reads specified dataset from global GPW.
        /// </summary>
        private static LocalGPWData CreateFromGlobalGPWSector(string filePath, Vector2d latLon, Vector2d size, out bool success)
        {
            success = false;
            LocalGPWData localGPWData = null;
            
            StreamReader sr = new StreamReader(filePath);
            if (File.Exists(filePath))
            {
                string[] d = new string[6];
                for (int i = 0; i < 6; ++i)
                {
                    d[i] = sr.ReadLine();
                }

                int ncols;
                int nrows;
                double xllcorner;
                double yllcorner;
                double cellsize; //size in degrees
                int NODATA_value;

                //read and save general stuff
                string[] dummy = d[0].Split(' ');
                int.TryParse(dummy[dummy.Length - 1], out ncols);
                dummy = d[1].Split(' ');
                int.TryParse(dummy[dummy.Length - 1], out nrows);
                dummy = d[2].Split(' ');
                double.TryParse(dummy[dummy.Length - 1], out xllcorner);
                dummy = d[3].Split(' ');
                double.TryParse(dummy[dummy.Length - 1], out yllcorner);
                dummy = d[4].Split(' ');
                double.TryParse(dummy[dummy.Length - 1], out cellsize);
                dummy = d[5].Split(' ');
                int.TryParse(dummy[dummy.Length - 1], out NODATA_value);

                Vector2d degreesToRead = SizeToDegrees(latLon, size);
                //number of columns and rows
                Vector2int cells = new Vector2int(Mathd.CeilToInt(degreesToRead.x / cellsize), Mathd.CeilToInt(degreesToRead.y / cellsize));
                //start index to read data
                int xSI = (int)((latLon.y - xllcorner) / cellsize);
                int ySI = (int)((latLon.x - yllcorner) / cellsize);
                //end index to read data
                int xEI = xSI + (int)(degreesToRead.x / cellsize);
                int yEI = ySI + (int)(degreesToRead.y / cellsize);

                //how far are we into the data set? 
                Vector2d actualOriginDegrees = new Vector2d(ySI * cellsize + yllcorner, xSI * cellsize + xllcorner);

                //calculate how many units we have to move the data when drawing quad
                Vector2d dOffset = actualOriginDegrees - latLon;
                //flip these as lat/long has reversed order to x/y 
                dOffset = new Vector2d(dOffset.y, dOffset.x);
                Vector2d originOffset = DegreesToSize(latLon, dOffset);
                Vector2d realWorldSize = DegreesToSize(latLon, new Vector2d(cells.x * cellsize, cells.y * cellsize));
                //check if we need to add another cell after shifting origin
                Vector2d actualPositiveSize = realWorldSize + originOffset; //since offset is always negative we add it here
                bool updateSize = false;
                if (actualPositiveSize.x < size.x)
                {
                    ++xEI;
                    ++cells.x;
                    updateSize = true;
                }
                if (actualPositiveSize.y < size.y)
                {
                    ++yEI;
                    ++cells.y;
                    updateSize = true;
                }
                if (updateSize)
                {
                    realWorldSize = DegreesToSize(latLon, new Vector2d(cells.x * cellsize, cells.y * cellsize));
                }

                //create needed array
                double[] density = new double[cells.x * cells.y];

                for (int i = 0; i < nrows; ++i) //density.Length
                {
                    //since read begin from upper corner and not lower
                    int realYIndex = nrows - 1 - i;
                    string[] e = sr.ReadLine().Split(' ');
                    for (int j = 0; j < ncols; ++j)
                    {
                        if (j >= xSI && j <= xEI && realYIndex >= ySI && realYIndex <= yEI)
                        {
                            int index = (j - xSI) + (realYIndex - ySI) * cells.x;
                            double.TryParse(e[j], out density[index]);
                        }
                    }
                }
                sr.Close();

                success = true;
                localGPWData = new LocalGPWData(actualOriginDegrees, originOffset, realWorldSize, cells, 0, density);
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, " Global GPW data files not found. Please make sure the folder structure is correct.");
            }            

            return localGPWData;
        }

    }
}