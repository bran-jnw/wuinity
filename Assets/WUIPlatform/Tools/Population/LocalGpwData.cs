﻿//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System;
using WUIPlatform.IO;
using WUIPlatform.Runtime;

namespace WUIPlatform.Population
{
    [Serializable]
    public class LocalGPWData
    {     
        public int ncols;
        public int nrows;
        public double xllcorner;
        public double yllcorner;
        public double cellsize; //size in degrees
        public int NODATA_value;
        public double[] density;
        public Vector2int _cells;
        public Vector2d actualOriginDegrees;
        public Vector2d originOffset;
        public Vector2d realWorldSize;
        public int totalPopulation;
                        
        private bool _haveData;
        public bool HavedData { get => _haveData; }
        private PopulationData _populationData;

        public LocalGPWData(PopulationData populationData)
        {
            _haveData = false;
            _populationData = populationData;
        }

        //http://www.land-navigation.com/latitude-and-longitude.html
        public static Vector2d SizeToDegrees(Vector2d latLon, Vector2d desiredSize)
        {
            double earth = 40075000.0 * 0.5;
            double yDegrees = 180.0 * desiredSize.y / earth;
            double xDegrees = 180.0 * desiredSize.x / Math.Abs(Math.Cos((Math.PI / 180.0) * latLon.x) * earth);

            return new Vector2d(xDegrees, yDegrees);
        }

        public static Vector2d DegreesToSize(Vector2d latLon, Vector2d degrees)
        {
            double earth = 40075000.0 * 0.5;
            double ySize = degrees.y * earth / 180.0;
            double xSize = degrees.x * Math.Abs(Math.Cos((Math.PI / 180.0) * latLon.x) * earth) / 180.0;

            return new Vector2d(xSize, ySize);
        }

        private void SaveLocalGPWData()
        {            
            string[] data = new string[13];

            //save data stamp to make sure data fits input
            WUIEngineInput input = WUIEngine.INPUT;
            string dataStamp = input.Simulation.LowerLeftLatLon.x.ToString() + " " + input.Simulation.LowerLeftLatLon.y.ToString()
                    + " " + input.Simulation.DomainSize.y.ToString() + " " + input.Simulation.DomainSize.y.ToString();
            data[0] = dataStamp;

            data[1] = ncols.ToString();
            data[2] = nrows.ToString();
            data[3] = xllcorner.ToString();
            data[4] = yllcorner.ToString();
            data[5] = cellsize.ToString();
            data[6] = NODATA_value.ToString();

            string densData = "";
            for (int i = 0; i < density.Length; ++i)
            {
                densData += density[i] + " ";
            }
            data[7] = densData;
            data[8] = _cells.x + " " + _cells.y;
            data[9] = actualOriginDegrees.x + " " + actualOriginDegrees.y;
            data[10] = originOffset.x + " " + originOffset.y;
            data[11] = realWorldSize.x + " " + realWorldSize.y;
            data[12] = totalPopulation.ToString();

            string path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Simulation.Id + ".gpw");
            File.WriteAllLines(path, data);
        }

        public bool LoadFromFile(string localGpwFile)
        {
            bool success = false;
            if (File.Exists(localGpwFile))
            {
                string[] d = File.ReadAllLines(localGpwFile);

                int.TryParse(d[1], out ncols);
                int.TryParse(d[2], out nrows);
                double.TryParse(d[3], out xllcorner);
                double.TryParse(d[4], out yllcorner);
                double.TryParse(d[5], out cellsize);
                int.TryParse(d[6], out NODATA_value);

                //switch order due to needing data size first
                string[] dummy = d[8].Split(' ');
                int xI;
                int yI;
                int.TryParse(dummy[0], out xI);
                int.TryParse(dummy[1], out yI);
                _cells = new Vector2int(xI, yI);

                dummy = d[7].Split(' ');
                density = new double[_cells.x * _cells.y];
                for (int i = 0; i < density.Length; ++i)
                {
                    double.TryParse(dummy[i], out density[i]);
                }

                dummy = d[9].Split(' ');
                double xD;
                double yD;
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                actualOriginDegrees = new Vector2d(xD, yD);

                dummy = d[10].Split(' ');
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                originOffset = new Vector2d(xD, yD);

                dummy = d[11].Split(' ');
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                realWorldSize = new Vector2d(xD, yD);

                //some older files might not have the total population
                if (d.Length > 12)
                {
                    int.TryParse(d[12], out totalPopulation);
                }
                else
                {
                    CalculateTotalPopulation();
                }
                _haveData = true;
                success = true;
                WUIEngine.LOG(WUIEngine.LogType.Log, " Loaded local GPW data from " + localGpwFile);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, " No local GPW data was found, build from global GPW or create custom population.");                
            }

            if(success)
            {
                if(_populationData != null)
                {
                    _populationData.Visualizer.CreateGPWTexture(this);
                }                
                _haveData = true;                
            }

            return success;
        }

        public bool CreateLocalGPWData(string globalGpwFolder)
        {
            bool success = LoadRelevantGPWData(globalGpwFolder);

            if (success)
            {
                if(_populationData != null)
                {
                    _populationData.Visualizer.CreateGPWTexture(this);
                }                
                _haveData = true;
            }

            return success;
        }

        bool AreSame(double a, double b)    // for comparing double values, added 14/08/2023
        {
            double tolerance = 1.0E-8;
            double absDiff = Math.Abs(a - b);

            if (absDiff <= tolerance) return true;
            if (absDiff < Math.Max(Math.Abs(a), Math.Abs(b)) * tolerance) return true;

            return false;
        }

        private void CalculateTotalPopulation()
        {
            totalPopulation = 0;

            double cellSizeX = realWorldSize.x / _cells.x;
            double cellSizeY = realWorldSize.y / _cells.y;
            double cellArea = cellSizeX * cellSizeY / (1000000d); // people/square km
            totalPopulation = 0;
            for (int y = 0; y < _cells.y; ++y)
            {
                for (int x = 0; x < _cells.x; ++x)
                {
                    double density = GetDensity(x, y);
                    int pop = Mathf.CeilToInt((float)(cellArea * density));
                    pop = Mathf.Clamp(pop, 0, pop);
                    totalPopulation += pop;
                }
            }
        }

        /// <summary>
        /// Returns the density data at a gridpoint
        /// </summary>
        public double GetDensity(int x, int y)
        {
            if(density == null || density.Length == 0)
            {
                return -1.0;
            }

            x = Mathf.Clamp(x, 0, _cells.x - 1);
            y = Mathf.Clamp(y, 0, _cells.y - 1);
            return density[x + y * _cells.x];
        }

        public double GetDensitySimulationSpace(Vector2d pos)
        {
            Vector2d positiveSize = realWorldSize + originOffset; //since offset is always negative we add it here
            int xInt = (int)((pos.x / positiveSize.x) * _cells.x);
            int yInt = (int)((pos.y / positiveSize.y) * _cells.y);
            double dens = GetDensity(xInt, yInt);
            return dens;
        }

        public  double GetDensitySimulationSpaceBilinear(Vector2d pos)
        {
            Vector2d positiveSize = realWorldSize + originOffset; //since offset is always negative we add it here

            double x = (pos.x / positiveSize.x) * _cells.x;
            int xLow = (int)x;
            int xHigh = xLow + 1;
            double xWeight = x - xLow;

            double y = (pos.y / positiveSize.y) * _cells.y;
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

        /// <summary>
        /// Returns the density data at a gridpoint based on polar coordinates.
        /// </summary>
        private bool LoadRelevantGPWData(string globalGpwFolder)
        {
            Vector2d latLong = WUIEngine.INPUT.Simulation.LowerLeftLatLon;
            Vector2d size = WUIEngine.INPUT.Simulation.DomainSize;

            bool success = false;
            if (IsGPWAvailable(globalGpwFolder))
            {
                // New code to accept GPW data-sets from any version and any year
                string[] AscFiles = Directory.GetFiles(globalGpwFolder, "*.asc");  // Get all ASCII files of the GPW data set
                Array.Sort(AscFiles);   // Sort the array in case it is not already sorted.
                string relevantAscFile;

                if (latLong.x >= -3.4106051316485e-012)
                {
                    if (latLong.y < -90.000000000005)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_1.asc");
                        relevantAscFile = AscFiles[0];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 1");
                    }
                    else if (latLong.y < -1.0231815394945e-011)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_2.asc");
                        relevantAscFile = AscFiles[1];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 2");
                    }
                    else if (latLong.y < 89.999999999985)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_3.asc");
                        relevantAscFile = AscFiles[2];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 3");
                    }
                    else
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_4.asc");
                        relevantAscFile = AscFiles[3];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 4");
                    }
                }
                else
                {
                    if (latLong.y < -90.000000000005)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_5.asc");
                        relevantAscFile = AscFiles[4];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 5");
                    }
                    else if (latLong.y < -1.0231815394945e-011)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_6.asc");
                        relevantAscFile = AscFiles[5];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 6");
                    }
                    else if (latLong.y < 89.999999999985)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_7.asc");
                        relevantAscFile = AscFiles[6];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 7");
                    }
                    else
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_8.asc");
                        relevantAscFile = AscFiles[7];
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Loading GPW from sector 8");
                    }
                }

                success = ReadGlobalGPWSector(relevantAscFile, latLong, size);
            }

            return success;
        }

        public static bool IsGPWAvailable(string path)
        {
            bool isAvailable = false;

            if (Directory.Exists(path))
            {
                string[] AscFiles = Directory.GetFiles(path, "*.asc");
                WUIEngine.LOG(WUIEngine.LogType.Log, AscFiles.Length.ToString() + " GPW files found.");

                if (AscFiles.Length == 8)
                {
                    isAvailable = true;
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.SimError, "Not all GPW files found.");
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "GPW path does NOT exist.");
            }

            return isAvailable;
        }

        /// <summary>
        /// Reads specified dataset from global GPW.
        /// </summary>
        private bool ReadGlobalGPWSector(string file, Vector2d latLong, Vector2d size)
        {
            string[] d = new string[6];
            StreamReader sr = new StreamReader(file);
            if (File.Exists(file))
            {
                for (int i = 0; i < 6; ++i)
                {
                    d[i] = sr.ReadLine();
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.SimError, " Global GPW data files not found. Please make sure the folder structure is correct.");
                return false;
            }

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

            Vector2d degreesToRead = SizeToDegrees(latLong, size);
            //number of columns and rows
            _cells = new Vector2int(Mathd.CeilToInt(degreesToRead.x / cellsize), Mathd.CeilToInt(degreesToRead.y / cellsize));
            //start index to read data
            int xSI = (int)((latLong.y - xllcorner) / cellsize);
            int ySI = (int)((latLong.x - yllcorner) / cellsize);
            //end index to read data
            int xEI = xSI + (int)(degreesToRead.x / cellsize);
            int yEI = ySI + (int)(degreesToRead.y / cellsize);

            //how far are we into the data set? 
            actualOriginDegrees = new Vector2d(ySI * cellsize + yllcorner, xSI * cellsize + xllcorner);

            //calculate how many units we have to move the data in Unity when drawing quad
            Vector2d dOffset = actualOriginDegrees - latLong;
            //flip these as lat/long has reversed order to x/y 
            dOffset = new Vector2d(dOffset.y, dOffset.x);
            originOffset = DegreesToSize(latLong, dOffset);
            realWorldSize = DegreesToSize(latLong, new Vector2d(_cells.x * cellsize, _cells.y * cellsize));
            //check if we need to add another cell after shifting origin
            Vector2d actualPositiveSize = realWorldSize + originOffset; //since offset is always negative we add it here
            bool updateSize = false;
            if (actualPositiveSize.x < size.x)
            {
                ++xEI;
                ++_cells.x;
                updateSize = true;
            }
            if (actualPositiveSize.y < size.y)
            {
                ++yEI;
                ++_cells.y;
                updateSize = true;
            }
            if (updateSize)
            {
                realWorldSize = DegreesToSize(latLong, new Vector2d(_cells.x * cellsize, _cells.y * cellsize));
            }

            //create needed array
            density = new double[_cells.x * _cells.y];

            for (int i = 0; i < nrows; ++i) //density.Length
            {
                //since read begin from upper corner and not lower
                int realYIndex = nrows - 1 - i;
                string[] e = sr.ReadLine().Split(' ');
                for (int j = 0; j < ncols; ++j)
                {
                    if (j >= xSI && j <= xEI && realYIndex >= ySI && realYIndex <= yEI)
                    {
                        int index = (j - xSI) + (realYIndex - ySI) * _cells.x;
                        double.TryParse(e[j], out density[index]);
                    }
                }
            }
            sr.Close();

            CalculateTotalPopulation();
            SaveLocalGPWData();

            return true;
        }

    }
}