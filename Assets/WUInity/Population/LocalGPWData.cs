using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace WUInity.Population
{
    [System.Serializable]
    public class LocalGPWData
    {     
        public int ncols;
        public int nrows;
        public double xllcorner;
        public double yllcorner;
        public double cellsize; //size in degrees
        public int NODATA_value;
        public double[] density;
        public Vector2Int dataSize;
        public Vector2D actualOriginDegrees;
        public Vector2D unityOriginOffset;
        public Vector2D realWorldSize;
        public int totalPopulation;

        //not saved
        public Texture2D densityTexture;
        public bool isLoaded;

        public LocalGPWData()
        {
            isLoaded = false;
        }

        //http://www.land-navigation.com/latitude-and-longitude.html
        public static Vector2D SizeToDegrees(Vector2D latLong, Vector2D desiredSize)
        {
            double earth = 40075000.0 * 0.5;
            double yDegrees = 180.0 * desiredSize.y / earth;
            double xDegrees = 180.0 * desiredSize.x / Math.Abs(Math.Cos((Math.PI / 180.0) * latLong.x) * earth);

            return new Vector2D(xDegrees, yDegrees);
        }

        public static Vector2D DegreesToSize(Vector2D latLong, Vector2D degrees)
        {
            double earth = 40075000.0 * 0.5;
            double ySize = degrees.y * earth / 180.0;
            double xSize = degrees.x * Math.Abs(Math.Cos((Math.PI / 180.0) * latLong.x) * earth) / 180.0;

            return new Vector2D(xSize, ySize);
        }

        private void SaveLocalGPWData()
        {            
            string[] data = new string[13];

            //save data stamp to make sure data fits input
            WUInityInput input = WUInity.INPUT;
            string dataStamp = input.Simulation.LowerLeftLatLong.x.ToString() + " " + input.Simulation.LowerLeftLatLong.y.ToString()
                    + " " + input.Simulation.Size.x.ToString() + " " + input.Simulation.Size.y.ToString();
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
            data[8] = dataSize.x + " " + dataSize.y;
            data[9] = actualOriginDegrees.x + " " + actualOriginDegrees.y;
            data[10] = unityOriginOffset.x + " " + unityOriginOffset.y;
            data[11] = realWorldSize.x + " " + realWorldSize.y;
            data[12] = totalPopulation.ToString();

            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Simulation.SimulationID + ".gpw");
            File.WriteAllLines(path, data);
        }

        public bool LoadLocalGPWDataFromFile(string file)
        {
            //first try if local filtered file exists
            bool success = false;
            if (File.Exists(file))
            {                
                success = LoadLocalGPWData(file);                
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, " No local GPW data was found, build from global GPW or create custom population.");                
            }

            if(success)
            {
                CreateTexture();
                isLoaded = true;                
            }

            WUInity.DATA_STATUS.LocalGPWLoaded = success;

            return success;
        }

        public bool CreateLocalGPWData()
        {
            bool success = LoadRelevantGPWData();

            if (success)
            {
                CreateTexture();
                isLoaded = true;
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

        bool IsDataValid(string dataStamp)
        {
            string[] dummy = dataStamp.Split(' ');

            bool success = false;
            if(dummy.Length == 4)
            {
                double lati, longi, xSize, ySize;

                double.TryParse(dummy[0], out lati);
                double.TryParse(dummy[1], out longi);
                double.TryParse(dummy[2], out xSize);
                double.TryParse(dummy[3], out ySize);

                WUInityInput input = WUInity.INPUT;

                // There is a problem in using "==" directly to compare two double values. Use AreSame() instead to avoid issues caused by rounding error
                //success =  lati == input.Simulation.LowerLeftLatLong.x && input.Simulation.LowerLeftLatLong.y == longi && xSize == input.Simulation.Size.x && ySize == input.Simulation.Size.y;
                success = AreSame(lati, input.Simulation.LowerLeftLatLong.x) && AreSame(longi, input.Simulation.LowerLeftLatLong.y) && AreSame(xSize, input.Simulation.Size.x) && AreSame(ySize, input.Simulation.Size.y);
            }
            else 
            {
                WUInity.LOG(WUInity.LogType.Error, " GPW data range is not valid. Delete the file and rebuild.");
            }
            return success;
        }

        private bool LoadLocalGPWData(string file)
        {
            string[] d = File.ReadAllLines(file);

            /*for (int i = 0; i < 12; ++i)
            {
                d[i] = sr.ReadLine();
            }*/

            bool success = IsDataValid(d[0]);
            if(success)
            {
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
                dataSize = new Vector2Int(xI, yI);

                dummy = d[7].Split(' ');
                density = new double[dataSize.x * dataSize.y];
                for (int i = 0; i < density.Length; ++i)
                {
                    double.TryParse(dummy[i], out density[i]);
                }

                dummy = d[9].Split(' ');
                double xD;
                double yD;
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                actualOriginDegrees = new Vector2D(xD, yD);

                dummy = d[10].Split(' ');
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                unityOriginOffset = new Vector2D(xD, yD);

                dummy = d[11].Split(' ');
                double.TryParse(dummy[0], out xD);
                double.TryParse(dummy[1], out yD);
                realWorldSize = new Vector2D(xD, yD);

                if(d.Length > 12)
                {
                    int.TryParse(d[12], out totalPopulation);
                }   
                else
                {
                    CalculateTotalPopulation();
                    SaveLocalGPWData();
                }

                WUInity.LOG(WUInity.LogType.Log, " Loaded local GPW data from pre-built file.");
            }
            else
            {
                success = false;
                WUInity.LOG(WUInity.LogType.Error, " Local GPW data not valid for current map.");
            }

            return success;
        }

        /// <summary>
        /// Reads specified dataset from GPW.
        /// </summary>
        private bool ReadGlobalGPW(string file, Vector2D latLong, Vector2D size)
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
                WUInity.LOG(WUInity.LogType.Error, " Global GPW data files not found. Please make sure the folder structure is correct.");
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

            Vector2D degreesToRead = SizeToDegrees(latLong, size);
            //number of columns and rows
            dataSize = new Vector2Int(MathD.CeilToInt(degreesToRead.x / cellsize), MathD.CeilToInt(degreesToRead.y / cellsize));
            //start index to read data
            int xSI = (int)((latLong.y - xllcorner) / cellsize);
            int ySI = (int)((latLong.x - yllcorner) / cellsize);
            //end index to read data
            int xEI = xSI + (int)(degreesToRead.x / cellsize);
            int yEI = ySI + (int)(degreesToRead.y / cellsize);            

            //how far are we into the data set? 
            actualOriginDegrees = new Vector2D(ySI * cellsize + yllcorner, xSI * cellsize + xllcorner);

            //calculate how many units we have to move the data in Unity when drawing quad
            Vector2D dOffset = actualOriginDegrees - latLong;
            //flip these as lat/long has reversed order to x/y 
            dOffset = new Vector2D(dOffset.y, dOffset.x);
            unityOriginOffset = DegreesToSize(latLong, dOffset);
            realWorldSize = DegreesToSize(latLong, new Vector2D(dataSize.x * cellsize, dataSize.y * cellsize));
            //check if we need to add another cell after shifting origin
            Vector2D actualPositiveSize = realWorldSize + unityOriginOffset; //since offset is always negative we add it here
            bool updateSize = false;
            if (actualPositiveSize.x < size.x)
            {
                ++xEI;
                ++dataSize.x;
                updateSize = true;
            }
            if (actualPositiveSize.y < size.y)
            {
                ++yEI;
                ++dataSize.y;
                updateSize = true;
            }
            if (updateSize)
            {
                realWorldSize = DegreesToSize(latLong, new Vector2D(dataSize.x * cellsize, dataSize.y * cellsize));
            }

            //WUInity.LogMessage("xSI: " + xSI + ", ySI: " + ySI + ", xEI: " + xEI + "yEI: " + yEI);

            //create needed array
            density = new double[dataSize.x * dataSize.y];

            for (int i = 0; i < nrows; ++i) //density.Length
            {
                //since read begin from upper corner and not lower
                int realYIndex = nrows - 1 - i;
                string[] e = sr.ReadLine().Split(' ');
                for (int j = 0; j < ncols; ++j)
                {
                    if (j >= xSI && j <= xEI && realYIndex >= ySI && realYIndex <= yEI)
                    {
                        int index = (j - xSI) + (realYIndex - ySI) * dataSize.x;                        
                        double.TryParse(e[j], out density[index]);
                    }
                }
            }
            sr.Close();

            CalculateTotalPopulation();
            SaveLocalGPWData();
            return true;
        }

        private void CalculateTotalPopulation()
        {
            totalPopulation = 0;

            double cellSizeX = realWorldSize.x / dataSize.x;
            double cellSizeY = realWorldSize.y / dataSize.y;
            double cellArea = cellSizeX * cellSizeY / (1000000d); // people/square km
            totalPopulation = 0;
            for (int y = 0; y < dataSize.y; ++y)
            {
                for (int x = 0; x < dataSize.x; ++x)
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
        private double GetDensity(int x, int y)
        {
            if(density == null || density.Length == 0)
            {
                return -1.0;
            }

            x = Mathf.Clamp(x, 0, dataSize.x - 1);
            y = Mathf.Clamp(y, 0, dataSize.y - 1);
            return density[x + y * dataSize.x];
        }

        public double GetDensityUnitySpace(Vector2D pos)
        {
            Vector2D positiveSize = realWorldSize + unityOriginOffset; //since offset is always negative we add it here
            int xInt = (int)((pos.x / positiveSize.x) * dataSize.x);
            int yInt = (int)((pos.y / positiveSize.y) * dataSize.y);
            double dens = GetDensity(xInt, yInt);
            return dens;
        }

        public double GetDensityUnitySpaceBilinear(Vector2D pos)
        {
            Vector2D positiveSize = realWorldSize + unityOriginOffset; //since offset is always negative we add it here

            double x = (pos.x / positiveSize.x) * dataSize.x;
            int xLow = (int)x;
            int xHigh = xLow + 1;
            double xWeight = x - xLow;

            double y = (pos.y / positiveSize.y) * dataSize.y;
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

        public static bool IsGPWAvailable(string path)
        {            
            bool isAvailable = false;

            if (Directory.Exists(path))
            {
                // This code only works for one version/year of the dataset.
                /*int fileCount = 0;

                for (int i = 0; i < 8; i++)
                {
                    string file = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_" + (i + 1) + ".asc");
                    if(!File.Exists(file))
                    {
                        break;
                    }
                    else
                    {
                        ++fileCount;
                    }
                }
                */

                // New code for any version and any year of the GPW data sets
                String[] AscFiles = Directory.GetFiles(path, "*.asc");
                WUInity.LOG(WUInity.LogType.Log, AscFiles.Length.ToString()+ " GPW files found.");

                //if (fileCount == 8)
                if(AscFiles.Length == 8)
                {
                    isAvailable = true;
                }
                else
                {
                    WUInity.LOG(WUInity.LogType.Error, "Not all GPW files found.");
                }
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Warning, "GPW path does NOT exist.");
            }

            return isAvailable;
        }

        /// <summary>
        /// Returns the density data at a gridpoint based on polar coordinates.
        /// </summary>
        public bool LoadRelevantGPWData()
        {
            Vector2D latLong = WUInity.INPUT.Simulation.LowerLeftLatLong;
            Vector2D size = WUInity.INPUT.Simulation.Size;

            bool success = false;
            string path = WUInity.INPUT.Population.gpwDataFolder;
            if (IsGPWAvailable(path))
            {
                // New code to accept GPW data-sets from any version and any year
                String[] AscFiles = Directory.GetFiles(path, "*.asc");  // Get all ASCII files of the GPW data set
                Array.Sort(AscFiles);   // Sort the array in case it is not already sorted.

                if (latLong.x >= -3.4106051316485e-012)
                {
                    if (latLong.y < -90.000000000005)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_1.asc");
                        path = AscFiles[0];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 1");
                    }
                    else if (latLong.y < -1.0231815394945e-011)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_2.asc");
                        path = AscFiles[1];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 2");
                    }
                    else if (latLong.y < 89.999999999985)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_3.asc");
                        path = AscFiles[2];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 3");
                    }
                    else
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_4.asc");
                        path = AscFiles[3];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 4");
                    }
                }
                else
                {
                    if (latLong.y < -90.000000000005)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_5.asc");
                        path = AscFiles[4];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 5");
                    }
                    else if (latLong.y < -1.0231815394945e-011)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_6.asc");
                        path = AscFiles[5];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 6");
                    }
                    else if (latLong.y < 89.999999999985)
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_7.asc");
                        path = AscFiles[6];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 7");
                    }
                    else
                    {
                        //path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_8.asc");
                        path = AscFiles[7];
                        WUInity.LOG(WUInity.LogType.Log, "Loading GPW from sector 8");
                    }
                }

                success = ReadGlobalGPW(path, latLong, size);
            }

            return success;
        }

        private void CreateTexture()
        {
            //first find the correct texture size
            int maxSide = Mathf.Max(dataSize.x, dataSize.y);
            Vector2Int res = new Vector2Int(2, 2);

            while (dataSize.x > res.x)
            {
                res.x *= 2;
            }
            while (dataSize.y > res.y)
            {
                res.y *= 2;
            }
            //Debug.Log("GPW texture resolution: " + res.x + ", " + res.y);

            //paint texture based time of arrival
            densityTexture = new Texture2D(res.x, res.y);
            densityTexture.filterMode = FilterMode.Point;
            for (int y = 0; y < dataSize.y; y++)
            {
                for (int x = 0; x < dataSize.x; x++)
                {
                    double density = GetDensity(x, y);
                    Color color = PopulationManager.GetGPWColor((float)density);

                    densityTexture.SetPixel(x, y, color);
                }
            }
            densityTexture.Apply();
        }
    }
}