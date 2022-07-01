using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


namespace WUInity.Farsite
{
    [System.Serializable]
    public struct FarsiteBurnPeriod
    {
        public int MO, DY, StartHr, EndHr;
    }

    [System.Serializable]
    public struct FarsiteWind
    {
        public int MO, DY, HR, SPD, DIR, CL;
    }

    [System.Serializable]
    public struct FarsiteWeather
    {
        public int MO, DY, RN, AM, PM, TLo, THi, HHi, HLo, ELV;
    }
    /// <summary>
    /// Struct that contains all the data for a raster from Farsite.
    /// </summary>
    [System.Serializable]
    public struct FarsiteRasterData
    {
        //input
        public double slope, aspect, crownBulkDensity;
        public int elevation, fuelModel, canopyCover, standHeight, crownBaseHeight, duffLoading, coarseWoody;

        //output
        public double timeOfArrival, firelineIntensity, flameLength, rateOfSpread, heatPerArea, reactionIntensity, crownFireActivity, spreadDirection;
    }

    /// <summary>
    /// Struct to hold output vertex data.
    /// </summary>
    [System.Serializable]
    public class FarsiteOutputVertex
    {
        public Vector3D pos;
        public double timeOfArrival, firelineIntensity, flameLength, rateOfSpread, heatPerArea, reactionIntensity, crownFireActivity, spreadDirection;

        public FarsiteOutputVertex()
        {
            pos = Vector3D.zero;
            timeOfArrival = 0.0;
            firelineIntensity = 0.0;
            flameLength = 0.0;
            rateOfSpread = 0.0;
            heatPerArea = 0.0;
            crownFireActivity = 0.0;
            spreadDirection = 0.0;
        }

        public FarsiteOutputVertex(Vector3D pos)
        {
            this.pos = pos;
            timeOfArrival = 0.0;
            firelineIntensity = 0.0;
            flameLength = 0.0;
            rateOfSpread = 0.0;
            heatPerArea = 0.0;
            crownFireActivity = 0.0;
            spreadDirection = 0.0;
        }
    }

    /// <summary>
    /// Class that contains all the vector data in a fire perimeter polygon.
    /// </summary>
    [System.Serializable]
    public class FarsitePolygon
    {
        public double timeStamp;
        public int fireNumber;        
        public List<FarsiteOutputVertex> vertices;

        /// <summary>
        /// Creates FarsitePolygon.
        /// </summary>
        public FarsitePolygon()
        {
            vertices = new List<FarsiteOutputVertex>();
        }

        /// <summary>
        /// Creates FarsitePolygon and sets properties.
        /// </summary>
        public FarsitePolygon(double timeStamp, int fireNumber)
        {
            vertices = new List<FarsiteOutputVertex>();
            this.timeStamp = timeStamp;
            this.fireNumber = fireNumber;
        }

        ~FarsitePolygon()
        {
            vertices = null;
        }
    }

    /// <summary>
    /// Class that contains all the vector data.
    /// </summary>
    [System.Serializable]
    public class FarsiteVectorData
    {
        public List<FarsitePolygon> firePolygons;
        public bool positionsAreOffset;

        /// <summary>
        /// Creates FarsiteVectorData.
        /// </summary>
        public FarsiteVectorData()
        {
            firePolygons = new List<FarsitePolygon>();
            positionsAreOffset = false;
        }

        ~FarsiteVectorData()
        {
            firePolygons = null;
        }

        /// <summary>
        /// Offsets positons of vectors to have lower left corner as 0.0, 0.0 instead of using easting and northing coordinates.
        /// </summary>
        public void OffsetPositions(double xllcorner, double yllcorner)
        {
            positionsAreOffset = true;
            Vector3D offset = new Vector3D(xllcorner, 0.0, yllcorner);
            for(int i = 0; i < firePolygons.Count; ++i)
            {
                for(int j = 0; j < firePolygons[i].vertices.Count; ++j)
                {
                    firePolygons[i].vertices[j].pos -= offset;
                }
            }
        }
    }

    /// <summary>
    /// Class that contains general data from Farsite.
    /// </summary>
    [System.Serializable]
    public class FarsiteData
    {
        public int ncols, nrows;
        public double xllcorner, yllcorner, cellsize;
        public FarsiteRasterData[] rasterData;
        public FarsiteVectorData vectorData;
        public int minElevation = int.MaxValue;
        public int maxElevation = int.MinValue;
        public List<int> fuelModelsInFile;
        public double maxTimeOfArrivalRaster = double.MinValue;
        public double maxFireLineIntensity = double.MinValue, minFireLineIntensity = double.MaxValue;
        public List<FarsiteWeather> weather;
        public bool metricWeather;
        public List<FarsiteWind> wind;
        public bool metricWind;
        public List<FarsiteBurnPeriod> burnPeriod;
        public double llLongitude, llLatitude;

        /// <summary>
        /// Imports input and output from Farsite.
        /// </summary>
        /// <param name="mainPath">Main folder containing farsite input data as well as sub folder with output (../output).</param>
        public FarsiteData(string mainPath)
        {
            //just make sure the path format is always the same
            if (!mainPath.EndsWith("/"))
            {
                mainPath +=  "/";
            }
            string input = mainPath;// + inputPrefix;
            string output = mainPath + "output/" + WUInity.INPUT.Fire.FarsiteData.outputPrefix;// + outputPrefix;

            //first read input
            ReadElevation(input);
            //guess that elevation data is always present? maybe some other check would be better but works for now. 
            if(rasterData == null)
            {
                //Debug.Log("Missing elevation data, can't continue with import.");
                return;
            }
            ReadSlope(input);
            ReadAspect(input);
            ReadFuelModel(input);
            ReadCanopyCover(input);
            //ReadStandHeight(input);
            //ReadCrownBaseHeight(input);
            ReadCrownBulkDensity(input);
            ReadDuffLoading(input);
            ReadCoarseWoody(input);
            //ReadWeather(input);
            ReadWind(input);
            ReadBurnPeriod(input);
            //then output, first raster
            ReadTimeOfArrival(output);
            ReadFirelineIntensity(output);
            ReadFlameLength(output);
            ReadRateOfSpread(output);
            ReadHeatPerArea(output);
            ReadReactionIntensity(output);
            ReadCrownFireActivity(output);
            ReadSpreadDirection(output);
            //then the vector data
            //ReadVector(output);
            //vectorData.OffsetPositions(xllcorner, yllcorner);
        }

        ~FarsiteData()
        {
            rasterData = null;
            vectorData = null;
        }  

        /// <summary>
        /// Reads and save general info from Farsite
        /// </summary>
        private void ReadGeneralData(string[] data)
        {
            //read and save general stuff
            string[] dummy = data[0].Split(' ');
            int.TryParse(dummy[dummy.Length - 1], out ncols);
            dummy = data[1].Split(' ');
            int.TryParse(dummy[dummy.Length - 1], out nrows);
            dummy = data[2].Split(' ');
            double.TryParse(dummy[dummy.Length - 1], out xllcorner);
            dummy = data[3].Split(' ');
            double.TryParse(dummy[dummy.Length - 1], out yllcorner);
            dummy = data[4].Split(' ');
            double.TryParse(dummy[dummy.Length - 1], out cellsize);
            //we are skipping NODATA value for now since they are specific per file
            //create needed array
            rasterData = new FarsiteRasterData[ncols * nrows];
            //convert to lat and long for Mapbox
            //CalcLatLong(xllcorner, yllcorner, );
        }

        /// <summary>
        /// Calculates latitude/longitude from easting and northing.
        /// </summary>
        private void CalcLatLong(double utmX, double utmY, string utmZone)
        {
            bool isNorthHemisphere = utmZone[utmZone.Length - 1] >= 'N';

            double diflat = -0.00066286966871111111111111111111111111;
            double diflon = -0.0003868060578;

            int zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
            double c_sa = 6378137.000000;
            double c_sb = 6356752.314245;
            double e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            double e2cuadrada = Math.Pow(e2, 2);
            double c = Math.Pow(c_sa, 2) / c_sb;
            double x = utmX - 500000;
            double y = isNorthHemisphere ? utmY : utmY - 10000000;

            double s = ((zone * 6.0) - 183.0);
            double lat = y / (c_sa * 0.9996);
            double v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            double a = x / v;
            double a1 = Math.Sin(2 * lat);
            double a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            double j2 = lat + (a1 / 2.0);
            double j4 = ((3 * j2) + a2) / 4.0;
            double j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            double alfa = (3.0 / 4.0) * e2cuadrada;
            double beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            double gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            double bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            double b = (y - bm) / v;
            double epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            double eps = a * (1 - (epsi / 3.0));
            double nab = (b * (1 - epsi)) + lat;
            double senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            double delt = Math.Atan(senoheps / (Math.Cos(nab)));
            double tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            llLongitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            llLatitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        /// <summary>
        /// Imports elevation data, also collects general data.
        /// </summary>
        private void ReadElevation(string path)
        {
            string file = path + "elevation.asc";
            string[] data = new string[0];
            if (File.Exists(file))
            {
                data = File.ReadAllLines(file);
            }
            else
            {
                //Debug.Log("Elevation file not found.");
                return;
            }

            ReadGeneralData(data);     

            //read actual elevation data
            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    int.TryParse(e[j], out rasterData[index].elevation);
                    //save min/max elevation for later when creating terrain, make sure we do not use the NODATA value
                    if(rasterData[index].elevation != -9999)
                    {
                        if (rasterData[index].elevation < minElevation)
                        {
                            minElevation = rasterData[index].elevation;
                        }
                        if (rasterData[index].elevation > maxElevation)
                        {
                            maxElevation = rasterData[index].elevation;
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Returns elevation data from Farsite.
        /// </summary>
        public double GetElevation(int x, int y)
        {
            double h = -9999.0;
            if (x > -1 && x < ncols && y > -1 && y < nrows)
            {
                //Farsite for some reason starts with the top left corner as first value, last value is lower right corner, 
                //which means you have to flip the y-axis when getting the values.
                h = rasterData[x + (nrows - y - 1) * ncols].elevation;
            }            
            return h;
        }

        /// <summary>
        /// Returns elevation data from Farsite based on world position using bilinear interpolation.
        /// </summary>
        public double GetElevationWorldSpace(double posX, double posY)
        {
            posX = posX / cellsize;
            double xWeight = posX - (int)posX;
            int xLow = (int)posX;
            int xHigh = xLow + 1;

            posY = posY / cellsize;
            double yWeight = posY - (int)posY;
            int yLow = (int)posY;
            int yHigh = yLow + 1;

            double h = -9999.0;
            if (xLow > -1 && xHigh < ncols && yLow > -1 && yHigh < nrows)
            {
                //do bilinear interpoaltion
                double h1 = rasterData[xLow + (nrows - yLow - 1) * ncols].elevation;
                double h2 = rasterData[xHigh + (nrows - yLow - 1) * ncols].elevation;
                double h3 = rasterData[xLow + (nrows - yHigh - 1) * ncols].elevation;                
                double h4 = rasterData[xHigh + (nrows - yHigh - 1) * ncols].elevation;
                double hYLow = (1.0 - xWeight) * h1 + xWeight * h2;
                double hYHigh = (1.0 - xWeight) * h3 + xWeight * h4;
                h = (1.0 - yWeight) * hYLow + yWeight * hYHigh;
            }
            return h;
        }

        /// <summary>
        /// Takes data from Farsite input and converts to heigth data that can be used in the Unity terrain engine.
        /// </summary>
        public float[,] GetElevationUnityTerrain()
        {
            //for some reason Unity wants the array indexed [y, x]. Also wants it normalized between 0-1
            float[,] hD = new float[nrows, ncols];
            float heightDiff = (float)(maxElevation - minElevation);
            for (int y = 0; y < nrows; ++y)
            {
                for (int x = 0; x < ncols; ++x)
                {
                    //Farsite for some reason starts with the top left corner as first value, last value is lower right corner, 
                    //which means you have to flip the y-axis when getting the values.
                    hD[y, x] = (float)(rasterData[x + (nrows - y - 1) * ncols].elevation - minElevation) / heightDiff;
                }
            }
            return hD;
        }

        /// <summary>
        /// Imports slope model
        /// </summary>
        private void ReadSlope(string path)
        {
            string fFile = path + "slope.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Slope file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    double.TryParse(e[j], out rasterData[index].slope);
                }
            }
        }

        /// <summary>
        /// Imports aspect
        /// </summary>
        private void ReadAspect(string path)
        {
            string fFile = path + "aspect.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Aspect file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    double.TryParse(e[j], out rasterData[index].aspect);
                }
            }
        }

        /// <summary>
        /// Imports fuel model
        /// </summary>
        private void ReadFuelModel(string path)
        {
            string fFile = path + "fuel.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Fuel model file not found.");
                return;
            }

            fuelModelsInFile = new List<int>();

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    int.TryParse(e[j], out rasterData[index].fuelModel);

                    if(!fuelModelsInFile.Contains(rasterData[index].fuelModel))
                    {
                        fuelModelsInFile.Add(rasterData[index].fuelModel);
                    }
                }
            }
        }

        /// <summary>
        /// Takes fuel data from Farsite input and returns a value normalized between 0-1, multiply by fuelModelsInFile.Count to get real value.
        /// </summary>
        public float GetFuelModel(int x, int y)
        {
            float c = -1.0f;

            if (x > -1 && x < ncols && y > -1 && y < nrows)
            {
                //fuel models are set between 0-256
                if (rasterData[x + (nrows - y - 1) * ncols].fuelModel != -9999)
                {
                    c = (float)rasterData[x + (nrows - y - 1) * ncols].fuelModel / (float)fuelModelsInFile.Count;
                }
            }

            return c;
        }

        /// <summary>
        /// Imports canopy cover
        /// </summary>
        private void ReadCanopyCover(string path)
        {
            string fFile = path + "canopy.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Canopy cover file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    int.TryParse(e[j], out rasterData[index].canopyCover);
                }
            }
        }

        /// <summary>
        /// Imports stand height
        /// </summary>
        private void ReadStandHeight(string path)
        {
            string fFile = path + "height.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Stand height file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    int.TryParse(e[j], out rasterData[index].standHeight);
                }
            }
        }

        /// <summary>
        /// Imports crown base height
        /// </summary>
        private void ReadCrownBaseHeight(string path)
        {
            string fFile = path + "cbh.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Crown base height file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    int.TryParse(e[j], out rasterData[index].crownBaseHeight);
                }
            }
        }

        /// <summary>
        /// Imports crown bulk density
        /// </summary>
        private void ReadCrownBulkDensity(string path)
        {
            string fFile = path + "cbd.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Crown bulk density file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    double.TryParse(e[j], out rasterData[index].crownBulkDensity);
                }
            }
        }

        /// <summary>
        /// Imports duff loading
        /// </summary>
        private void ReadDuffLoading(string path)
        {
            string fFile = path + "duff.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Duff loading file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    int.TryParse(e[j], out rasterData[index].duffLoading);
                }
            }
        }

        /// <summary>
        /// Imports coarse woody
        /// </summary>
        private void ReadCoarseWoody(string path)
        {
            string fFile = path + "cwd.asc";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Coarse woody file not found.");
                return;
            }

            for (int i = 6; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < e.Length - 1; ++j)
                {
                    int index = (i - 6) * (e.Length - 1) + j;
                    int.TryParse(e[j], out rasterData[index].coarseWoody);
                }
            }
        }

        /// <summary>
        /// Imports weather data
        /// </summary>
        private void ReadWeather(string path)
        {
            string fFile = path + "weather.wtr";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Weather file not found.");
                return;
            }

            weather = new List<FarsiteWeather>();
            metricWeather = data[0] == "METRIC";

            for (int i = 1; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                FarsiteWeather wD = new FarsiteWeather();
                int.TryParse(e[0], out wD.MO);
                int.TryParse(e[1], out wD.DY);
                int.TryParse(e[2], out wD.RN);
                int.TryParse(e[3], out wD.AM);
                int.TryParse(e[4], out wD.PM);
                int.TryParse(e[5], out wD.TLo);
                int.TryParse(e[6], out wD.THi);
                int.TryParse(e[7], out wD.HHi);
                int.TryParse(e[8], out wD.HLo);
                int.TryParse(e[9], out wD.ELV);
                weather.Add(wD);
            }
        }

        /// <summary>
        /// Imports wind data
        /// </summary>
        private void ReadWind(string path)
        {
            string fFile = path + "wind.wnd";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Wind file not found.");
                return;
            }

            wind = new List<FarsiteWind>();
            metricWind = data[0] == "METRIC";

            for (int i = 1; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                FarsiteWind w = new FarsiteWind();
                int.TryParse(e[0], out w.MO);
                int.TryParse(e[1], out w.DY);
                int.TryParse(e[2], out w.HR);
                int.TryParse(e[3], out w.SPD);
                int.TryParse(e[4], out w.DIR);
                int.TryParse(e[5], out w.CL);
                wind.Add(w);
            }
        }

        /// <summary>
        /// Imports burn period data
        /// </summary>
        private void ReadBurnPeriod(string path)
        {
            string fFile = path + "burn.bpd";
            string[] data = new string[0];
            if (File.Exists(fFile))
            {
                data = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Burn period file not found.");
                return;
            }

            burnPeriod = new List<FarsiteBurnPeriod>();

            for (int i = 0; i < data.Length; ++i)
            {
                string[] e = data[i].Split(' ');
                FarsiteBurnPeriod b = new FarsiteBurnPeriod();
                int.TryParse(e[0], out b.MO);
                int.TryParse(e[1], out b.DY);
                int.TryParse(e[2], out b.StartHr);
                int.TryParse(e[3], out b.EndHr);
                burnPeriod.Add(b);
            }
        }

        /// <summary>
        /// Imports time of arrival.
        /// </summary>
        private void ReadTimeOfArrival(string path)
        {
            string fFile = path + ".toa";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Time of arrival file not found.");
                return;
            }
            
            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].timeOfArrival);
                if(rasterData[i].timeOfArrival > maxTimeOfArrivalRaster)
                {
                    maxTimeOfArrivalRaster = rasterData[i].timeOfArrival;
                }
            }

            /*for (int i = 0; i < nrows; ++i)
            {
                string[] e = f[i].Split(' ');
                //since rows ends with a space you get one too many, deduct by one
                for (int j = 0; j < ncols; ++j)
                {
                    int index = j + i * ncols;
                    double.TryParse(e[j], out rasterData[index].timeOfArrival);
                    if (rasterData[index].timeOfArrival > maxTimeOfArrivalRaster)
                    {
                        maxTimeOfArrivalRaster = rasterData[i].timeOfArrival;
                    }
                }
            }*/
        }

        /// <summary>
        /// Returns time of arrival in hours.
        /// </summary>
        public float GetTimeOfArrival(int x, int y)
        {
            float t = -1.0f;
            if (x > -1 && x < ncols && y > -1 && y < nrows)
            {
                t = (float)rasterData[x + (nrows - y - 1) * ncols].timeOfArrival;
            }
            return t;
        }

        /// <summary>
        /// Imports fireline intensity
        /// </summary>
        private void ReadFirelineIntensity(string path)
        {
            string fFile = path + ".fli";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Fireline intensity file not found.");
                return;
            }

            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].firelineIntensity);
                if(rasterData[i].firelineIntensity >= 0)
                {
                    if (rasterData[i].firelineIntensity > maxFireLineIntensity)
                    {
                        maxFireLineIntensity = rasterData[i].firelineIntensity;
                    }
                    if (rasterData[i].firelineIntensity < minFireLineIntensity)
                    {
                        minFireLineIntensity = rasterData[i].firelineIntensity;
                    }
                }                
            }
        }

        /// <summary>
        /// Returns time of arrival in hours.
        /// </summary>
        public float GetFirelineIntensity(int x, int y)
        {
            float t = -1.0f;
            if (x > -1 && x < ncols && y > -1 && y < nrows)
            {
                t = (float)rasterData[x + (nrows - y - 1) * ncols].firelineIntensity;
            }
            return t;
        }

        /// <summary>
        /// Imports flame length
        /// </summary>
        private void ReadFlameLength(string path)
        {
            string fFile = path + ".fml";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Flame length file not found.");
                return;
            }

            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].flameLength);
            }
        }

        /// <summary>
        /// Imports rate of spread
        /// </summary>
        private void ReadRateOfSpread(string path)
        {
            string fFile = path + ".ros";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Rate of spread file not found.");
                return;
            }

            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].rateOfSpread);
            }
        }

        /// <summary>
        /// Imports heat per unit area
        /// </summary>
        private void ReadHeatPerArea(string path)
        {
            string fFile = path + ".hpa";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Heat per unit area file not found.");
                return;
            }

            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].heatPerArea);
            }
        }

        /// <summary>
        /// Imports reaction intensity
        /// </summary>
        private void ReadReactionIntensity(string path)
        {
            string fFile = path + ".rci";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Reaction intensity file not found.");
                return;
            }

            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].reactionIntensity);
            }
        }

        /// <summary>
        /// Imports crown fire activity
        /// </summary>
        private void ReadCrownFireActivity(string path)
        {
            string fFile = path + ".cfr";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Crown fire activity file not found.");
                return;
            }

            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].crownFireActivity);
            }
        }

        /// <summary>
        /// Imports spread direction
        /// </summary>
        private void ReadSpreadDirection(string path)
        {
            string fFile = path + ".sdr";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Spread direction file not found.");
                return;
            }

            for (int i = 0; i < rasterData.Length; ++i)
            {
                double.TryParse(f[i + 6], out rasterData[i].spreadDirection);
            }
        }

        /// <summary>
        /// Import vector file containing fire perimeters. See Farsite manual for format specification
        /// </summary>
        private void ReadVector(string path)
        {
            string fFile = path + ".vct";
            string[] f = new string[0];
            if (File.Exists(fFile))
            {
                f = File.ReadAllLines(fFile);
            }
            else
            {
                //Debug.Log("Vector file not found.");
                return;
            }

            vectorData = new FarsiteVectorData();
                        
            FarsitePolygon currentPolygon = null;
            bool optionalFormat = f[0].Length != 11;            
            if (optionalFormat)
            {
                currentPolygon = new FarsitePolygon(0.0, 0);
                vectorData.firePolygons.Add(currentPolygon);
                for (int i = 0; i < f.Length; ++i)
                {       
                    string[] v = f[i].Split(' ');
                    double x = 0.0;
                    double.TryParse(v[0], out x);
                    double y = 0.0;
                    double.TryParse(v[1], out y);
                    Vector3D pos = new Vector3D(x, 0.0, y);
                    FarsiteOutputVertex fOV = new FarsiteOutputVertex(pos);
                    double.TryParse(v[2], out fOV.timeOfArrival);
                    //read extra data if present
                    if (v.Length == 10)
                    {                        
                        double.TryParse(v[3], out fOV.firelineIntensity);
                        double.TryParse(v[4], out fOV.flameLength);
                        double.TryParse(v[5], out fOV.rateOfSpread);
                        double.TryParse(v[6], out fOV.heatPerArea);
                        double.TryParse(v[7], out fOV.reactionIntensity);
                        double.TryParse(v[8], out fOV.crownFireActivity);
                        double.TryParse(v[9], out fOV.spreadDirection);
                    }

                    if (fOV.timeOfArrival != currentPolygon.timeStamp)
                    {
                        currentPolygon = new FarsitePolygon(fOV.timeOfArrival, 0);
                        vectorData.firePolygons.Add(currentPolygon);
                    }

                    currentPolygon.vertices.Add(fOV);
                }
            }
            else
            {
                bool newPerimeter = true;
                for (int i = 0; i < f.Length; ++i)
                {
                    if (newPerimeter && f[i].Length == 11)
                    {
                        int time = -1;
                        int.TryParse(f[i].Substring(0, 6), out time);
                        int number = -1;
                        int.TryParse(f[i].Substring(6, 5), out number);
                        newPerimeter = false;
                        currentPolygon = new FarsitePolygon(time, number);
                        vectorData.firePolygons.Add(currentPolygon);
                    }
                    else if (f[i] == "END")
                    {
                        newPerimeter = true;
                    }
                    else
                    {
                        string[] v = f[i].Split(' ');
                        double x = 0.0;
                        double.TryParse(v[0], out x);
                        double y = 0.0;
                        double.TryParse(v[1], out y);
                        Vector3D pos = new Vector3D(x, 0.0, y);
                        FarsiteOutputVertex fOV = new FarsiteOutputVertex(pos);
                        currentPolygon.vertices.Add(fOV);
                    }
                }
            }

            
        }
    }
}

