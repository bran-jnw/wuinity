//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;

namespace PREACT.Wildfire
{
	public struct LandscapeCellData
	{
		public short elevation;
		public short slope;
		public short aspect;
		public short fuel_model;
		public short canopy_cover;
		public double crown_canopy_height;
		public double crown_base;  //bran-jnw: added crown_ as base is taken by C#
		public double crown_bulk_density;
		public double ground_duff_model;
		public long ground_coarse_woody_model;
	}

	struct celldata
	{		
		public short e;                 // elevation
		public short s;                 // slope
		public short a;                 // aspect
		public short f;                 // fuel models
		public short c;					// canopy cover
	}
		
	struct crowndata
	{

		public short h;					// canopy height
		public short b;					// crown base
		public short p;					// bulk density
	}

	struct grounddata
	{
		public short d;                // duff model
		public short w;				// coarse woody model
	}

	public class LandscapeData
	{
		// header for landscape file
		private class LCpHeader
		{
			public int CrownFuels;         // 20 if no crown fuels, 21 if crown fuels exist
			public int GroundFuels;      // 20 if no ground fuels, 21 if ground fuels exist
			public int latitude;
			public double loeast;
			public double hieast;
			public double lonorth;
			public double hinorth;
			public int loelev;
			public int hielev;
			public int numelev;          //-1 if more than 100 categories
			public int[] elevs = new int[100];
			public int loslope;
			public int hislope;
			public int numslope;         //-1 if more than 100 categories
			public int[] slopes = new int[100];
			public int loaspect;
			public int hiaspect;
			public int numaspect;        //-1 if more than 100 categories
			public int[] aspects = new int[100];
			public int lofuel;
			public int hifuel;
			public int numfuel;          //-1 if more than 100 categories
			public int[] fuels = new int[100];
			public int locover;
			public int hicover;
			public int numcover;         //-1 if more than 100 categories
			public int[] covers = new int[100];
			public int loheight;
			public int hiheight;
			public int numheight;        //-1 if more than 100 categories
			public int[] heights = new int[100];
			public int lobase;
			public int hibase;
			public int numbase;          //-1 if more than 100 categories
			public int[] bases = new int[100];
			public int lodensity;
			public int hidensity;
			public int numdensity;       //-1 if more than 100 categories
			public int[] densities = new int[100];
			public int loduff;
			public int hiduff;
			public int numduff;          //-1 if more than 100 categories
			public int[] duffs = new int[100];
			public int lowoody;
			public int hiwoody;
			public int numwoody;         //-1 if more than 100 categories
			public int[] woodies = new int[100];
			public int numeast;
			public int numnorth;
			public double EastUtm;
			public double WestUtm;
			public double NorthUtm;
			public double SouthUtm;
			public int GridUnits;        // 0 for metric, 1 for English
			public double XResol;
			public double YResol;
			public short EUnits;
			public short SUnits;
			public short AUnits;
			public short FOptions;
			public short CUnits;
			public short HUnits;
			public short BUnits;
			public short PUnits;
			public short DUnits;
			public short WOptions;
			public char[] ElevFile = new char[256];
			public char[] SlopeFile = new char[256];
			public char[] AspectFile = new char[256];
			public char[] FuelFile = new char[256];
			public char[] CoverFile = new char[256];
			public char[] HeightFile = new char[256];
			public char[] BaseFile = new char[256];
			public char[] DensityFile = new char[256];
			public char[] DuffFile = new char[256];
			public char[] WoodyFile = new char[256];
			public char[] Description = new char[512];
		}

		private LCpHeader Header = new LCpHeader();
		public bool NEED_CUST_MODELS;// = false;	// custom fuel models
		public bool HAVE_CUST_MODELS;// = false;
		public bool NEED_CONV_MODELS;// = false;     // fuel model conversions
		public bool HAVE_CONV_MODELS;// = false;
		public double RasterCellResolutionX;
		public double RasterCellResolutionY;
		public long NumVals;
		public long OldFilePosition;
		public bool CantAllocLCP;
		public short[] landscape;
		static readonly int headsize = 7316; //header size taken from farsite source code

		Vector2d _origin; //offset from common origin (map lower left)
		public Vector2d OriginOffset { get => _origin; }
		Vector2int _originCellOffset; //cells offset from common origin
        //public Vector2int OriginCellOffset { get => originCellOffset; }


		public LandscapeData(string filePath, Vector2d simulationUtmOrigin)					
		{
			bool readGeoTIFF = false;
            if (filePath.ToLower().EndsWith("tif") || filePath.EndsWith("tiff"))
			{
				readGeoTIFF = true;
			}

			if(readGeoTIFF)
			{
				ReadGeoTIFF(filePath);
            }
			else
			{
                ReadLCP(filePath);
            }
			CalculateOrigin(simulationUtmOrigin);
		}

		private void ReadGeoTIFF(string filePath)
		{
            //OSGeo.GDAL.Gdal.AllRegister(); //should be done by engine
            using (OSGeo.GDAL.Dataset tif = OSGeo.GDAL.Gdal.Open(filePath, OSGeo.GDAL.Access.GA_ReadOnly))
            {
                Header.numeast = tif.RasterXSize;
                Header.numnorth = tif.RasterYSize;
                NumVals = tif.RasterCount;

				if(NumVals != 8)
				{
					Engine.Message(null, Engine.LogType.InputError, "The landscape trying to be read from GeoTIFF does not contain the expected 8 raster sets, aborting.");
					CantAllocLCP = true;

                    return;
				}

                //https://gdal.org/en/stable/tutorials/geotransforms_tut.html
				//assume these contain UTM coordinates
                double[] transform = new double[6];
				tif.GetGeoTransform(transform);
                Header.WestUtm = transform[0];
                Header.EastUtm = transform[0] + transform[1] * tif.RasterXSize;
                Header.NorthUtm = transform[3];
                Header.SouthUtm = transform[3] + transform[5] * tif.RasterYSize; //negative cell size when north up
                Header.XResol = transform[1];
                Header.YResol = -transform[5];
				RasterCellResolutionX = Mathd.Max(Header.XResol);
				RasterCellResolutionY = Mathd.Max(Header.YResol);

				//https://gdal.org/en/stable/drivers/raster/lcp.html
				Header.CrownFuels = 20; //20 if no crown fuels, 21 if crown fuels exist(crown fuels = canopy height, canopy base height, canopy bulk density)
                Header.GroundFuels = 20; //20 if no ground fuels, 21 if ground fuels exist (ground fuels = duff loading, coarse woody)
                switch (NumVals)
                {
                    case 8:
						// 5 basic, crown fuels
						Header.CrownFuels = 21;
                        break;
                    case 10:
                        // 5 basic, crown fuels, and duff and woody
                        Header.CrownFuels = 21;
						Header.GroundFuels = 21;
                        break;
                }                

				//TODO: fix, probably used to determine UTM zone and when doing pre-runs for fuel moisture content
                //Header.latitude = reader.ReadInt32();

                //offset to preserve coordinate precision (legacy from 16-bit OS days), ignore
                Header.loeast = 0;
                Header.hieast = 0;
                Header.lonorth = 0;
                Header.hinorth = 0;

				//gets updated in loop when reading data
                Header.loelev = int.MaxValue;
                Header.hielev = int.MinValue;

                Header.loslope = int.MaxValue;
                Header.hislope = int.MinValue;

                Header.loaspect = int.MaxValue;
                Header.hiaspect = int.MinValue;

                Header.lofuel = int.MaxValue;
                Header.hifuel = int.MinValue;

                Header.locover = int.MaxValue;
                Header.hicover = int.MinValue;

                Header.loheight = int.MaxValue;
                Header.hiheight = int.MinValue;

                Header.lobase = int.MaxValue;
                Header.hibase = int.MinValue;

                Header.lodensity = int.MaxValue;
                Header.hidensity = int.MinValue;

				//does not seem to be included in GeoTIFF from Landfire, ignore for now
                Header.loduff = 0;
                Header.hiduff = 0;
                Header.lowoody = 0;
                Header.hiwoody = 0;

                //https://gdal.org/en/stable/drivers/raster/lcp.html
                Header.GridUnits = 0; //linear unit: 0 = meters, 1 = feet, 2 = kilometers
                //SI defaults
                Header.EUnits = 0; //meters
                Header.SUnits = 0; //degrees
                Header.AUnits = 2;
                Header.FOptions = 0;
                Header.CUnits = 1;
                Header.HUnits = 1;
                Header.BUnits = 1;
                Header.PUnits = 1;
                Header.DUnits = 1;
                Header.WOptions = 0; //coarse woody options(1 if coarse woody band is present)

				//dummy stuff
				char[] filePaths = new char[256];// "Not set...".PadRight(256).ToCharArray();
				char[] description = new char[512];// "This data was read from a GeoTiff.".PadRight(512).ToCharArray();
                Header.ElevFile = filePaths;
                Header.SlopeFile = filePaths;
                Header.AspectFile = filePaths;
                Header.FuelFile = filePaths;
                Header.CoverFile = filePaths;
                Header.HeightFile = filePaths;
                Header.BaseFile = filePaths;
                Header.DensityFile = filePaths;
                Header.DuffFile = filePaths;
                Header.WoodyFile = filePaths;
                Header.Description = description;

                //from: https://landfire.gov/fuel/landscape
                //Eight bands are included in a landscape file: elevation, slope, aspect, fire behavior fuel model, tree canopy cover, canopy height, canopy base height, and canopy bulk density.
                //So should be the same order as classic LCP it seems
                landscape = new short[Header.numeast * Header.numnorth * NumVals];
                for (int rasterIndex = 0; rasterIndex < NumVals; rasterIndex++)
                {
                    OSGeo.GDAL.Band band = tif.GetRasterBand(rasterIndex + 1);
                    short[] bandData = new short[Header.numeast * Header.numnorth];
                    band.ReadRaster(0, 0, Header.numeast, Header.numnorth, bandData, Header.numeast, Header.numnorth, 0, 0);

                    for (int j = 0; j < Header.numnorth; j++)
                    {
                        for (int i = 0; i < Header.numeast; i++)
                        {
							long index = rasterIndex + i * NumVals + j * Header.numeast * NumVals;
                            landscape[index] = bandData[i + j * Header.numeast];

							if(landscape[index] == -9999)
							{
								continue;
							}

                            //update min/max for header
                            //elevation
                            if (rasterIndex == 0)
							{
								Header.loelev = Mathf.Min(landscape[index], Header.loelev);
                                Header.hielev = Mathf.Max(landscape[index], Header.hielev);
                            }

                            //slope
                            if (rasterIndex == 1)
                            {
                                Header.loslope = Mathf.Min(landscape[index], Header.loslope);
                                Header.hislope = Mathf.Max(landscape[index], Header.hislope);
                            }

                            //aspect
                            if (rasterIndex == 2)
                            {
                                Header.loaspect = Mathf.Min(landscape[index], Header.loaspect);
                                Header.hiaspect = Mathf.Max(landscape[index], Header.hiaspect);
                            }

                            //fuel model
                            if (rasterIndex == 3)
                            {
                                Header.lofuel = Mathf.Min(landscape[index], Header.lofuel);
                                Header.hifuel = Mathf.Max(landscape[index], Header.hifuel);
                            }

                            //canopy cover
                            if (rasterIndex == 4)
                            {
                                Header.locover = Mathf.Min(landscape[index], Header.locover);
                                Header.hicover = Mathf.Max(landscape[index], Header.hicover);
                            }

                            //canopy height
                            if (rasterIndex == 5)
                            {
                                Header.loheight = Mathf.Min(landscape[index], Header.loheight);
                                Header.hiheight = Mathf.Max(landscape[index], Header.hiheight);
                            }

                            //canopy base height
                            if (rasterIndex == 6)
                            {
                                Header.lobase = Mathf.Min(landscape[index], Header.lobase);
                                Header.hibase = Mathf.Max(landscape[index], Header.hibase);
                            }

                            //canopy bulk density
                            if (rasterIndex == 7)
                            {
                                Header.lodensity = Mathf.Min(landscape[index], Header.lodensity);
                                Header.hidensity = Mathf.Max(landscape[index], Header.hidensity);
                            }
                        }
                    }
                }

                //category counts
                Header.numelev = (Header.hielev - Header.loelev) / 200;
				for(int i = 0; i < Header.numelev; ++i)
				{
					Header.elevs[i] = Header.loelev + i * 200;
                }
                Header.numslope = 100;
                Header.numaspect = 100;
                Header.numfuel = 100;
                Header.numcover = 100;
                Header.numheight = 100;
                Header.numbase = 100;
                Header.numdensity = 100;
                Header.numduff = 100;
                Header.numwoody = 100;

                CantAllocLCP = false;
            }
        }

		private void ReadLCP(string path)
		{
			ReadData(path);
			//SetCustFuelModelID(HaveCustomFuelModels());
			//SetConvFuelModelID(HaveFuelConversions());
		}

        public Vector2int GetCellCount()
        {
            return new Vector2int(Header.numeast, Header.numnorth);
        }

        public int GetCellCountX()
        {
			return Header.numeast;
        }

        public int GetCellCountY()
        {
            return Header.numnorth;
        }

		public Vector2d GetLowerLeftUTM()
		{
			return new Vector2d(Header.WestUtm, Header.SouthUtm);
		}

		/// <summary>
		/// Entire landscape dimensions in meters.
		/// </summary>
		/// <returns></returns>
        public Vector2d GetSize()
		{
			double x = Header.EastUtm - Header.WestUtm;
            double y = Header.NorthUtm - Header.SouthUtm;

			return new Vector2d(x, y);
        }

        /// <summary>
        /// Entire landscape dimensions in meters.
        /// </summary>
        /// <returns></returns>
        public double GetLandscapeSizeX()
        {
			return Header.EastUtm - Header.WestUtm;
;        }

        /// <summary>
        /// Entire landscape dimensions in meters.
        /// </summary>
        /// <returns></returns>
        public double GetLandscapeSizeY()
        {
			return Header.NorthUtm - Header.SouthUtm;
        }

		/// <summary>
		/// Returns the elevation on local space bilinearly interpolated, meaning that lower left is 0,0 meters. Clamps elevation outside domain.
		/// </summary>
		/// <returns></returns>
		public double GetElevationLocalPos(double x, double y)
		{
            int xIndexLow = (int)(x / GetCellResolutionX());
            int yIndexLow = (int)(y / GetCellResolutionY());
            int xIndexHigh = xIndexLow + 1;
            int yIndexHigh = yIndexLow + 1;
			//we assume that mid-point in cell is holding actual value
			double xFraction = (x - (xIndexLow + 0.5) * GetCellResolutionX()) / GetCellResolutionX();
            double yFraction = (y - (yIndexLow + 0.5) * GetCellResolutionY()) / GetCellResolutionY();

			double lowerLeft = GetCellData(xIndexLow, yIndexLow).elevation;
            double lowerRight = GetCellData(xIndexHigh, yIndexLow).elevation;
            double upperLeft = GetCellData(xIndexLow, yIndexHigh).elevation;
            double upperRight = GetCellData(xIndexHigh, yIndexHigh).elevation;

            return Interpolation.BilinearInterpolation(lowerLeft, lowerRight, xFraction, upperLeft, upperRight, yFraction);
		}

		/// <summary>
		/// Return a 1D array with offset elevation (meaning lowest point is zero and not actual elevation), leading x-dimension.
		/// </summary>
		/// <returns></returns>
		public float[] Get1DElevation()
		{
			float[] elevation = new float[Header.numeast * Header.numnorth];

			for(int i = 0; i < elevation.Length; ++i)
			{
				LandscapeCellData c = GetCellData(i);
                elevation[i] = c.elevation - Header.loelev;
			}

			return elevation;
		}

		public Vector2d GetElevationMinMax()
		{
			return new Vector2d(Header.loelev, Header.hielev);
        }

        public double GetElevationMin()
        {
			return Header.loelev;
        }

        public double GetElevationMax()
        {
            return Header.hielev;
        }

        public Vector2d GetSlopeMinMax()
        {
            return new Vector2d(Header.loslope, Header.hislope);
        }

        public double GetSlopeMin()
        {
            return Header.loslope;
        }

        public double GetSlopeMax()
        {
            return Header.hislope;
        }

        public Vector2d GetAspectMinMax()
        {
            return new Vector2d(Header.loaspect, Header.hiaspect);
        }

        public double GetAspectMin()
        {
            return Header.loaspect;
        }

        public double GetAspectMax()
        {
            return Header.hiaspect;
        }

        /// <summary>
        /// Returns cell data of requested index in LCP file, no correction for offset.
        /// </summary>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        /// <returns></returns>
        public LandscapeCellData GetCellData(int xIndex, int yIndex)
        {
			return GetCellDataSimulationIndex(xIndex, yIndex, false);
        }

		public LandscapeCellData GetCellData(int linearIndex)
		{
            celldata cell = new celldata();
            crowndata cfuel = new crowndata();
            grounddata gfuel = new grounddata();
            GetCellDataFromMemory(linearIndex, ref cell, ref cfuel, ref gfuel);

            LandscapeCellData l = new LandscapeCellData();

            l.elevation = cell.e;
            l.slope = cell.s;
            l.aspect = cell.a;
            l.fuel_model = cell.f;
            l.canopy_cover = cell.c;

            switch (NumVals)
            {
                case 7:
                    // 5 basic, duff and woody
                    l.ground_duff_model = gfuel.d;
                    l.ground_coarse_woody_model = gfuel.w;
                    break;
                case 8:
                    // 5 basic, crown fuels
                    l.crown_canopy_height = cfuel.h;
                    l.crown_base = cfuel.b;
                    l.crown_bulk_density = cfuel.p;
                    break;
                case 10:
                    // 5 basic, crown fuels, and duff and woody
                    l.crown_canopy_height = cfuel.h;
                    l.crown_base = cfuel.b;
                    l.crown_bulk_density = cfuel.p;

                    l.ground_duff_model = gfuel.d;
                    l.ground_coarse_woody_model = gfuel.w;
                    break;
            }

            return l;
        }

        /// <summary>
        /// Returns data in requested cell, index is based on boundary of simulation (lcp data must be bigger or equal to this size)
        /// </summary>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        /// <param name="correctForOrigin"></param>
        /// <returns></returns>
        public LandscapeCellData GetCellDataSimulationIndex(int xIndex, int yIndex, bool correctForOrigin)
        {
			if(correctForOrigin)
			{
                //WUIEngine.LOG(WUIEngine.LogType.Log, "X/Y offset cells: " + originCellOffset.x + ", " + originCellOffset.y);
                //correct for any difference in origin
                xIndex += _originCellOffset.x;
                yIndex += _originCellOffset.y;
            }

			xIndex = Mathd.Clamp(xIndex, 0, GetCellCountX() - 1);
            yIndex = Mathd.Clamp(yIndex, 0, GetCellCountY() - 1);

            //flip y since dataset is north down
            long posit = (xIndex + (Header.numnorth - yIndex - 1) * Header.numeast);

			return GetCellData((int)posit);
		}

		public HashSet<int> GetExisitingFuelModelNumbers()
        {
			HashSet<int> uniqueFuels = new HashSet<int>();
			int cells = landscape.Length / (int)NumVals;
            HashSet<int> invalidFuelModelNumbers = new HashSet<int>();
			int invalidCount = 0;
            for (int i = 0; i < cells; i++)
            {
				int fuelModelNumber = landscape[i * NumVals + 3]; //fuel number is offset by 3
				if (fuelModelNumber > 0 && fuelModelNumber <= short.MaxValue) //&& fuelModelNumber <= 256 this needed to increase for canadian data
                {
					uniqueFuels.Add(fuelModelNumber);
                }
				else
                {
					invalidFuelModelNumbers.Add(fuelModelNumber);
					++invalidCount;
                }
            }

			if(invalidFuelModelNumbers.Count > 0)
            {
				string error = "Landscape data contains " + invalidCount + " cells with fuel model numbers outside of the valid range, numbers are: ";
				int index = 0;
				foreach (int i in invalidFuelModelNumbers)
				{
					error += i;
					if(index < invalidFuelModelNumbers.Count - 1)
					{
						error += ", ";
					}
					index++;
				}
                Engine.Message(null, Engine.LogType.Warning, error);
            }

			return uniqueFuels;
		}

		celldata CellData(double east, double north, ref celldata cell, ref crowndata cfuel, ref grounddata gfuel)
		{
			long Position = GetCellPosition(east, north);
			GetCellDataFromMemory(Position, ref cell, ref cfuel, ref gfuel);

			return cell;
		}

		long GetCellPosition(double east, double north)								
		{
			double xpt = (east - Header.loeast) / GetCellResolutionX();
			double ypt = (north - Header.lonorth) / GetCellResolutionY();
			long easti = (long)xpt;
			long northi = (long)ypt;
			northi = Header.numnorth - northi - 1;
			if (northi < 0)
            {
				northi = 0;
			}				
			long posit = (northi * Header.numeast + easti);

			return posit;
		}

		double GetCellResolutionX()
		{
			if (Header.GridUnits == 2)
            {
				return Header.XResol * 1000.0;   // from kilometers
			}				

			return Header.XResol;
		}

		double GetCellResolutionY()
		{
			if (Header.GridUnits == 2)
			{
				return Header.YResol * 1000.0;
			}

			return Header.YResol;
		}

		void GetCellDataFromMemory(long posit, ref celldata cell, ref crowndata cfuel, ref grounddata gfuel)
		{
			short[] ldata = new short[10];

			//read all data from position onwards
            for (int i = 0; i < NumVals; i++)
            {
				ldata[i] = landscape[posit * NumVals + i];
			}

			//always save this data
			cell.e = ldata[0];
			cell.s = ldata[1];
			cell.a = ldata[2];
			cell.f = ldata[3];
			cell.c = ldata[4];

			switch (NumVals)
			{
				case 7:
					// 5 basic and duff and woody
					gfuel.d = ldata[5];
					gfuel.w = ldata[6];
					break;
				case 8:
					// 5 basic and crown fuels
					cfuel.h = ldata[5];
					cfuel.b = ldata[6];
					cfuel.p = ldata[7];
					break;
				case 10:
					// 5 basic, crown fuels, and duff and woody
					cfuel.h = ldata[5];
					cfuel.b = ldata[6];
					cfuel.p = ldata[7];

					gfuel.d = ldata[8];
					gfuel.w = ldata[9];
					break;
			}
		}

		private static bool DoesLCPExist(string path)
        {
			if (!File.Exists(path))
			{
				Engine.Message(null, Engine.LogType.Warning, " LCP file not found in " + path + ".");
				return false;
			}

			return true;
		}

		void ReadData(string filePath)
		{
			if(!DoesLCPExist(filePath))
            {
				CantAllocLCP = true;
				return;
			}			

			using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
			{
				Header.CrownFuels = reader.ReadInt32();
				Header.GroundFuels = reader.ReadInt32();
				Header.latitude = reader.ReadInt32();
				Header.loeast = reader.ReadDouble();
				Header.hieast = reader.ReadDouble();
				Header.lonorth = reader.ReadDouble();
				Header.hinorth = reader.ReadDouble();
				Header.loelev = reader.ReadInt32();
				Header.hielev = reader.ReadInt32();
				Header.numelev = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.elevs[i] = reader.ReadInt32();
				}
				Header.loslope = reader.ReadInt32();
				Header.hislope = reader.ReadInt32();
				Header.numslope = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.slopes[i] = reader.ReadInt32();
				}
				Header.loaspect = reader.ReadInt32();
				Header.hiaspect = reader.ReadInt32();
				Header.numaspect = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.aspects[i] = reader.ReadInt32();
				}
				Header.lofuel = reader.ReadInt32();
				Header.hifuel = reader.ReadInt32();
				Header.numfuel = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.fuels[i] = reader.ReadInt32();
				}
				Header.locover = reader.ReadInt32();
				Header.hicover = reader.ReadInt32();
				Header.numcover = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.covers[i] = reader.ReadInt32();
				}
				Header.loheight = reader.ReadInt32();
				Header.hiheight = reader.ReadInt32();
				Header.numheight = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.heights[i] = reader.ReadInt32();
				}
				Header.lobase = reader.ReadInt32();
				Header.hibase = reader.ReadInt32();
				Header.numbase = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.bases[i] = reader.ReadInt32();
				}
				Header.lodensity = reader.ReadInt32();
				Header.hidensity = reader.ReadInt32();
				Header.numdensity = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.densities[i] = reader.ReadInt32();
				}
				Header.loduff = reader.ReadInt32();
				Header.hiduff = reader.ReadInt32();
				Header.numduff = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.duffs[i] = reader.ReadInt32();
				}
				Header.lowoody = reader.ReadInt32();
				Header.hiwoody = reader.ReadInt32();
				Header.numwoody = reader.ReadInt32();
				for (int i = 0; i < 100; i++)
				{
					Header.woodies[i] = reader.ReadInt32();
				}
				Header.numeast = reader.ReadInt32();
				Header.numnorth = reader.ReadInt32();
				Header.EastUtm = reader.ReadDouble();
				Header.WestUtm = reader.ReadDouble();
				Header.NorthUtm = reader.ReadDouble();
				Header.SouthUtm = reader.ReadDouble();
				Header.GridUnits = reader.ReadInt32();
				Header.XResol = reader.ReadDouble();
				Header.YResol = reader.ReadDouble();
				Header.EUnits = reader.ReadInt16();
				Header.SUnits = reader.ReadInt16();
				Header.AUnits = reader.ReadInt16();
				Header.FOptions = reader.ReadInt16();
				Header.CUnits = reader.ReadInt16();
				Header.HUnits = reader.ReadInt16();
				Header.BUnits = reader.ReadInt16();
				Header.PUnits = reader.ReadInt16();
				Header.DUnits = reader.ReadInt16();
				Header.WOptions = reader.ReadInt16();
				Header.ElevFile = reader.ReadChars(256);
				Header.SlopeFile = reader.ReadChars(256);
				Header.AspectFile = reader.ReadChars(256);
				Header.FuelFile = reader.ReadChars(256);
				Header.CoverFile = reader.ReadChars(256);
				Header.HeightFile = reader.ReadChars(256);
				Header.BaseFile = reader.ReadChars(256);
				Header.DensityFile = reader.ReadChars(256);
				Header.DuffFile = reader.ReadChars(256);
				Header.WoodyFile = reader.ReadChars(256);
				Header.Description = reader.ReadChars(512);				
			}

			/*// do this in case a version 1.0 file has gotten through
			Header.loeast = ConvertUtmToEastingOffset(Header.WestUtm);
			Header.hieast = ConvertUtmToEastingOffset(Header.EastUtm);
			Header.lonorth = ConvertUtmToNorthingOffset(Header.SouthUtm);
			Header.hinorth = ConvertUtmToNorthingOffset(Header.NorthUtm);*/

			if (Header.FOptions == 1 || Header.FOptions == 3)
			{
				NEED_CUST_MODELS = true;
			}
			else
			{
				NEED_CUST_MODELS = false;
			}

			if (Header.FOptions == 2 || Header.FOptions == 3)
			{
				NEED_CONV_MODELS = true;
			}
			else
			{
				NEED_CONV_MODELS = false;
			}

			//HAVE_CUST_MODELS=false;
			//HAVE_CONV_MODELS=false;
			// set raster resolution
			RasterCellResolutionX = (Header.EastUtm - Header.WestUtm) / (double)Header.numeast;
			RasterCellResolutionY = (Header.NorthUtm - Header.SouthUtm) / (double)Header.numnorth;
			/*ViewPortNorth = RasterCellResolutionY * (double) Header.numnorth +
				Header.lonorth;
			ViewPortSouth = Header.lonorth;
			ViewPortEast = RasterCellResolutionX * (double) Header.numeast +
				Header.loeast;
			ViewPortWest = Header.loeast;
			//	NumViewNorth=(ViewPortNorth-ViewPortSouth)/Header.YResol;
			//	NumViewEast=(ViewPortEast-ViewPortWest)/Header.XResol;
			double rows, cols;
			rows = (ViewPortNorth - ViewPortSouth) / Header.YResol;
			NumViewNorth = (long)rows;
			if (modf(rows, &rows) > 0.5)
				NumViewNorth++;
			cols = (ViewPortEast - ViewPortWest) / Header.XResol;
			NumViewEast = (long)cols;
			if (modf(cols, &cols) > 0.5)
				NumViewEast++; */

			if (HaveCrownFuels() == 1)
			{
				if (HaveGroundFuels() == 1)
				{
					NumVals = 10;
				}
				else
				{
					NumVals = 8;
				}

			}
			else
			{
				if (HaveGroundFuels() == 1)
				{
					NumVals = 7;
				}
				else
				{
					NumVals = 5;
				}
			}
			CantAllocLCP = false;

			/*if (lcptheme)
			{
				delete lcptheme;
				lcptheme = 0;
			}
			lcptheme = new LandscapeTheme(false, this);*/

			if (landscape == null)
			{
				if (CantAllocLCP == false)
				{
					double NumAlloc;

					using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
					{
						//fseek(landfile, headsize, SEEK_SET);
						//if((landscape=(short *) calloc(Header.numnorth*Header.numeast, NumVals*sizeof(short)))!=NULL)
						NumAlloc = (double)(Header.numnorth * Header.numeast * NumVals * sizeof(short));
						if (NumAlloc > 2147483647)	
						{
							CantAllocLCP = true;
							return;
						}

						try
						{
							landscape = new short[Header.numnorth * Header.numeast * NumVals];
						}
						catch
						{
							landscape = null;
						}

						if (landscape != null)
						{
							//ZeroMemory(landscape,Header.numnorth * Header.numeast * NumVals * sizeof(short));
							//memset(landscape, 0x0, Header.numnorth * Header.numeast * NumVals * sizeof(short));

							//bran-jnw: original 
							/*for (i = 0; i < Header.numnorth; i++)
								fread(&landscape[i * NumVals * Header.numeast], sizeeof(short), NumVals * Header.numeast, landfile);*/
							reader.ReadBytes(headsize); //jump ahead to data
							for (int i = 0; i < Header.numnorth; i++)
							{
                                for (int j = 0; j < Header.numeast; j++)
                                {
                                    for (int k = 0; k < NumVals; k++)
                                    {
										landscape[i * Header.numeast * NumVals + j * NumVals + k] = reader.ReadInt16();
									}									
								}								
							}

							//fseek(landfile, headsize, SEEK_SET); //bran-jnw: resets position in reader, not really needed I think
							//OldFilePosition=0;     // thread local
							CantAllocLCP = false;
						}
						else
						{
							CantAllocLCP = true;
						}
					}
					//	long p;
					//   CellData(Header.loeast, Header.hinorth, &p);
				}
			}

            if (CantAllocLCP)
            {
                Engine.Message(null, Engine.LogType.Log, " LCP found in " + filePath + " but could not properly read it.");
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, " LCP found in " + filePath + ", read succesfully.");
            }
        }

		private void CalculateOrigin(Vector2d utmOriginReference)
		{
            Vector2d lcpUTM = new Vector2d(Header.WestUtm, Header.SouthUtm);
            _origin = lcpUTM - utmOriginReference;
            _originCellOffset = new Vector2int(-(int)(_origin.x / GetCellResolutionX()), -(int)(_origin.y / GetCellResolutionY()));            
        }

		long HaveCrownFuels()
		{
			return Header.CrownFuels - 20;      // subtract 10 to ID file as version 2.x
		}

		long HaveGroundFuels()
		{
			return Header.GroundFuels - 20;
		}

        /*double ConvertEastingOffsetToUtm(double input)
		{
			return input;
			double MetersToKm = 1.0;
			double ipart;

			if (Header.GridUnits == 2)
				MetersToKm = 0.001;

			modf(Header.WestUtm / 1000.0, &ipart);

			return (input + ipart * 1000.0) * MetersToKm;
		}

		double ConvertNorthingOffsetToUtm(double input)
		{
			return input;
			double MetersToKm = 1.0;
			double ipart;

			if (Header.GridUnits == 2)
				MetersToKm = 0.001;

			modf(Header.SouthUtm / 1000.0, &ipart);

			return (input + ipart * 1000.0) * MetersToKm;
		}

		double ConvertUtmToEastingOffset(double input)
		{
			return input;
			double KmToMeters = 1.0;
			double ipart;

			if (Header.GridUnits == 2)
				KmToMeters = 1000.0;

			modf(Header.WestUtm / 1000.0, &ipart);

			return input * KmToMeters - ipart * 1000.0;
		}

		double ConvertUtmToNorthingOffset(double input)
		{
			return input;
			double KmToMeters = 1.0;
			double ipart;

			if (Header.GridUnits == 2)
				KmToMeters = 1000.0;

			modf(Header.SouthUtm / 1000.0, &ipart);

			return input * KmToMeters - ipart * 1000.0;
		}*/

        public void SaveLCP(string filePath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            {
				writer.Write(Header.CrownFuels);
				writer.Write(Header.GroundFuels);
                writer.Write(Header.latitude);
                writer.Write(Header.loeast);
                writer.Write(Header.hieast);
                writer.Write(Header.lonorth);
                writer.Write(Header.hinorth);
                writer.Write(Header.loelev);
                writer.Write(Header.hielev);
                writer.Write(Header.numelev);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.elevs[i]);
                }
                writer.Write(Header.loslope);
                writer.Write(Header.hislope);
                writer.Write(Header.numslope);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.slopes[i]);
                }
                writer.Write(Header.loaspect);
                writer.Write(Header.hiaspect);
                writer.Write(Header.numaspect);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.aspects[i]);
                }
                writer.Write(Header.lofuel);
                writer.Write(Header.hifuel);
                writer.Write(Header.numfuel);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.fuels[i]);
                }
                writer.Write(Header.locover);
                writer.Write(Header.hicover);
                writer.Write(Header.numcover);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.covers[i]);
                }
                writer.Write(Header.loheight);
                writer.Write(Header.hiheight);
                writer.Write(Header.numheight);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.heights[i]);
                }
                writer.Write(Header.lobase);
                writer.Write(Header.hibase);
                writer.Write(Header.numbase);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.bases[i]);
                }
                writer.Write(Header.lodensity);
                writer.Write(Header.hidensity);
                writer.Write(Header.numdensity);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.densities[i]);
                }
                writer.Write(Header.loduff);
                writer.Write(Header.hiduff);
                writer.Write(Header.numduff);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.duffs[i]);
                }
                writer.Write(Header.lowoody);
                writer.Write(Header.hiwoody);
                writer.Write(Header.numwoody);
                for (int i = 0; i < 100; i++)
                {
                    writer.Write(Header.woodies[i]);
                }
                writer.Write(Header.numeast);
                writer.Write(Header.numnorth);
                writer.Write(Header.EastUtm);
                writer.Write(Header.WestUtm);
                writer.Write(Header.NorthUtm);
                writer.Write(Header.SouthUtm);
                writer.Write(Header.GridUnits);
                writer.Write(Header.XResol);
                writer.Write(Header.YResol);
                writer.Write(Header.EUnits);
                writer.Write(Header.SUnits);
                writer.Write(Header.AUnits);
                writer.Write(Header.FOptions);
                writer.Write(Header.CUnits);
                writer.Write(Header.HUnits);
                writer.Write(Header.BUnits);
                writer.Write(Header.PUnits);
                writer.Write(Header.DUnits);
                writer.Write(Header.WOptions);
				//arrays of chars
                writer.Write(Header.ElevFile);
                writer.Write(Header.SlopeFile);
                writer.Write(Header.AspectFile);
                writer.Write(Header.FuelFile);
                writer.Write(Header.CoverFile);
                writer.Write(Header.HeightFile);
                writer.Write(Header.BaseFile);
                writer.Write(Header.DensityFile);
                writer.Write(Header.DuffFile);
                writer.Write(Header.WoodyFile);
                writer.Write(Header.Description);

				//write actual data
                for (int i = 0; i < Header.numnorth; i++)
                {
                    for (int j = 0; j < Header.numeast; j++)
                    {
                        for (int k = 0; k < NumVals; k++)
                        {
                            writer.Write(landscape[i * Header.numeast * NumVals + j * NumVals + k] );
                        }
                    }
                }
            }          
        }
    }
}