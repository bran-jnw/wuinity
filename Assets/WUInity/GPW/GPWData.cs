using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace WUInity.GPW
{
    [System.Serializable]
    public class GPWData
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

        public GPWData()
        {

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

        private void SaveGPWDataToFile()
        {
            string[] data = new string[11];
            data[0] = ncols.ToString();
            data[1] = nrows.ToString();
            data[2] = xllcorner.ToString();
            data[3] = yllcorner.ToString();
            data[4] = cellsize.ToString();
            data[5] = NODATA_value.ToString();

            string densData = "";
            for (int i = 0; i < density.Length; ++i)
            {
                densData += density[i] + " ";
            }
            data[6] = densData;
            data[7] = dataSize.x + " " + dataSize.y;
            data[8] = actualOriginDegrees.x + " " + actualOriginDegrees.y;
            data[9] = unityOriginOffset.x + " " + unityOriginOffset.y;
            data[10] = realWorldSize.x + " " + realWorldSize.y;

            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.WUINITY_IN.simName + ".gpw");
            System.IO.File.WriteAllLines(path, data);
        }

        public void LoadGPWData(Vector2D latLong, Vector2D size)
        {
            //first try if local filtered file exists
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.WUINITY_IN.simName + ".gpw");
            if (File.Exists(path))
            {
                LoadGPWDataFromFile();
            }
            else
            {
                LoadRelevantGPWData(latLong, size);
            }
        }

        public void LoadGPWDataFromFile()
        {
            string[] d = new string[11];
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.WUINITY_IN.simName + ".gpw");
            StreamReader sr = new StreamReader(path);
            for (int i = 0; i < 11; ++i)
            {
                d[i] = sr.ReadLine();
            }

            int.TryParse(d[0], out ncols);
            int.TryParse(d[1], out nrows);
            double.TryParse(d[2], out xllcorner);
            double.TryParse(d[3], out yllcorner);
            double.TryParse(d[4], out cellsize);
            int.TryParse(d[5], out NODATA_value);

            //switch order due to needing data size first
            string[] dummy = d[7].Split(' ');
            int xI;
            int yI;
            int.TryParse(dummy[0], out xI);
            int.TryParse(dummy[1], out yI);
            dataSize = new Vector2Int(xI, yI);

            dummy = d[6].Split(' ');
            density = new double[dataSize.x * dataSize.y];
            for (int i = 0; i < density.Length; ++i)
            {
                double.TryParse(dummy[i], out density[i]);
            }

            dummy = d[8].Split(' ');
            double xD;
            double yD;
            double.TryParse(dummy[0], out xD);
            double.TryParse(dummy[1], out yD);
            actualOriginDegrees = new Vector2D(xD, yD);

            dummy = d[9].Split(' ');
            double.TryParse(dummy[0], out xD);
            double.TryParse(dummy[1], out yD);
            unityOriginOffset = new Vector2D(xD, yD);

            dummy = d[10].Split(' ');
            double.TryParse(dummy[0], out xD);
            double.TryParse(dummy[1], out yD);
            realWorldSize = new Vector2D(xD, yD);
        }

        /// <summary>
        /// Reads specified dataset from GPW.
        /// </summary>
        private void ReadGPW(string file, Vector2D latLong, Vector2D size)
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
                WUInity.WUINITY_SIM.LogMessage("GPW data file not found.");
                return;
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
            //start point to read data
            int xSI = (int)((latLong.y - xllcorner) / cellsize);
            int ySI = (int)((latLong.x - yllcorner) / cellsize);
            //end point to read data
            int xEI = xSI + (int)(degreesToRead.x / cellsize);
            int yEI = ySI + (int)(degreesToRead.y / cellsize);            

            //handle negative stuff when recalculating origin and consequently offset
            actualOriginDegrees = new Vector2D(ySI * cellsize - Math.Abs(yllcorner), xSI * cellsize - Math.Abs(xllcorner));
            //actualOrigin = new Vector2D(xSI * cellsize - xllcorner, ySI * cellsize - yllcorner);

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

            WUInity.WUINITY_SIM.LogMessage("xSI: " + xSI + ", ySI: " + ySI + ", xEI: " + xEI + "yEI: " + yEI);

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

            SaveGPWDataToFile();
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

            x = Mathf.Clamp(x, 0, dataSize.x - 1);
            y = Mathf.Clamp(y, 0, dataSize.y - 1);
            return density[x + y * dataSize.x];
        }

        //since data is read to compensdate for the lower corner being the last row we do not have to read data mirrored on y-axis, so we use the function above instead
        /*/// <summary>
        /// Returns the density data at a gridpoint
        /// </summary>
        public double GetDensity(int x, int y)
        {
            double d = (double)NODATA_value;
            if (x >= 0 && x < dataSize.x && y >= 0 && y < dataSize.y)
            {
                //data starts with the top left corner as first value, last value is lower right corner, 
                //which means you have to flip the y-axis when getting the values.
                d = density[x + (dataSize.y - y - 1) * dataSize.x];
            }
            return d;
        }*/

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

        /// <summary>
        /// Returns the density data at a gridpoint based on polar coordinates.
        /// </summary>
        public void LoadRelevantGPWData(Vector2D latLong, Vector2D size)
        {
            string path = Path.Combine(WUInity.DATA_FOLDER, WUInity.WUINITY_IN.gpw.gpwDataFolder);
            if (latLong.x >= -3.4106051316485e-012)
            {
                if (latLong.y < -90.000000000005)
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_1.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 1");
                }
                else if (latLong.y < -1.0231815394945e-011)
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_2.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 2");
                }
                else if (latLong.y < 89.999999999985)
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_3.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 3");
                }
                else
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_4.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 4");
                }
            }
            else
            {
                if (latLong.y < -90.000000000005)
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_5.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 5");
                }
                else if (latLong.y < -1.0231815394945e-011)
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_6.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 6");
                }
                else if (latLong.x < 89.999999999985)
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_7.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 7");
                }
                else
                {
                    path = Path.Combine(path, "gpw_v4_population_density_rev10_2015_30_sec_8.asc");
                    WUInity.WUINITY_SIM.LogMessage("Loading GPW from sector 8");
                }
            }

            ReadGPW(path, latLong, size);
        }

        //not done, array too big, out of memory
        public static void CreateBinaryDatabase()
        {
            int totalNCols = 0;
            int totalNrows = 0;
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            for (int i = 0; i < 8; i++)
            {
                string file = Application.dataPath + "/Resources/_input/gpw-v4-population-density-rev10_2015_30_sec_asc/";
                file += "gpw_v4_population_density_rev10_2015_30_sec_" + (i + 1) + ".asc";

                string[] d = new string[6];
                StreamReader sr = new StreamReader(file);
                if (File.Exists(file))
                {
                    for (int j = 0; j < 6; ++j)
                    {
                        d[j] = sr.ReadLine();
                    }
                }
                else
                {
                    WUInity.WUINITY_SIM.LogMessage("GPW data file not found.");
                    return;
                }

                int ncols, nrows;
                double xllcorner, yllcorner, cellsize;
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

                totalNCols += ncols;
                totalNrows += nrows;
                minX = MathD.Min(minX, xllcorner);
                minY = MathD.Min(minY, yllcorner);
                maxX = MathD.Max(maxX, xllcorner + ncols * cellsize);
                maxY = MathD.Max(maxY, yllcorner + nrows * cellsize);
            }

            double[] gpwDatabase = new double[totalNCols * totalNrows];

            for (int i = 0; i < 8; i++)
            {
                string file = Path.Combine(WUInity.DATA_FOLDER, WUInity.WUINITY_IN.gpw.gpwDataFolder);
                file = Path.Combine(file, "gpw_v4_population_density_rev10_2015_30_sec_" + (i + 1) + ".asc");

                string[] d = new string[6];
                StreamReader sr = new StreamReader(file);
                if (File.Exists(file))
                {
                    for (int j = 0; j < 6; ++j)
                    {
                        d[i] = sr.ReadLine();
                    }
                }
                else
                {
                    WUInity.WUINITY_SIM.LogMessage("GPW data file not found.");
                    return;
                }

                int ncols, nrows, NODATA_value;
                double xllcorner, yllcorner, cellsize;
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

                for (int y = 0; y < nrows; ++y)
                {
                    int yOffset = MathD.RoundToInt((yllcorner - minY) / cellsize);
                    //since read begin from upper corner and not lower
                    int realYindex = nrows - 1 - i + yOffset;
                    string[] e = sr.ReadLine().Split(' ');
                    for (int x = 0; x < ncols; ++x)
                    {
                        int xOffset = MathD.RoundToInt((xllcorner - minX) / cellsize);
                        int realXindex = x + xOffset;
                        double.TryParse(e[x], out gpwDatabase[realXindex + realYindex * totalNCols]);
                    }
                }
                
            }
        }
    }
}