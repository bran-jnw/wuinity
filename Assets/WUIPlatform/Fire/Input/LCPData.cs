//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;

namespace WUIPlatform.Fire
{
	public struct LandscapeStruct
	{
		public short elevation;
		public short slope;
		public short aspect;
		public short fuel_model;
		public short canopy_cover;               // READ OR DERIVED FROM LANDSCAPE DATA
		public double aspectf;
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

	[System.Serializable]
	public class LCPData
	{
		// header for landscape file
		[System.Serializable]
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

		Vector2d originOffset; //offset from common origin (map lower left)
		public Vector2d OriginOffset { get => originOffset; }
		Vector2int originCellOffset; //cells offset from common origin
        //public Vector2int OriginCellOffset { get => originCellOffset; }

		public LCPData(Vector2d cellSize, Vector2int cellCount)								
		{
            RasterCellResolutionX = cellSize.x;
            RasterCellResolutionY = cellSize.y;
            Header.numnorth = cellCount.y;
            Header.numeast = cellCount.x;
            NumVals = 10;
            Header.loelev = 0;
        }

		public LCPData(string path)					
		{
			ReadLCP(path);
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
		/// In meters.
		/// </summary>
		/// <returns></returns>
        public Vector2d GetLCPSize()
		{
			double x = Header.EastUtm - Header.WestUtm;
            double y = Header.NorthUtm - Header.SouthUtm;

			return new Vector2d(x, y);
        }

		/// <summary>
		/// In meters.
		/// </summary>
		/// <returns></returns>
        public double GetLCPSizeX()
        {
			return Header.EastUtm - Header.WestUtm;
;        }

		/// <summary>
		/// In meters.
		/// </summary>
		/// <returns></returns>
        public double GetLCPSizeY()
        {
			return Header.NorthUtm - Header.SouthUtm;
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
        public LandscapeStruct GetCellData(int xIndex, int yIndex)
        {
			return GetCellDataSimulationIndex(xIndex, yIndex, false);
        }

		public LandscapeStruct GetCellData(int linearIndex)
		{
            celldata cell = new celldata();
            crowndata cfuel = new crowndata();
            grounddata gfuel = new grounddata();
            GetCellDataFromMemory(linearIndex, ref cell, ref cfuel, ref gfuel);

            LandscapeStruct l = new LandscapeStruct();

            l.elevation = cell.e;
            l.slope = cell.s;
            l.aspect = cell.a;
            l.fuel_model = cell.f;
            l.canopy_cover = cell.c;

            switch (NumVals)
            {
                case 7:
                    // 5 basic and duff and woody
                    l.ground_duff_model = gfuel.d;
                    l.ground_coarse_woody_model = gfuel.w;
                    break;
                case 8:
                    // 5 basic and crown fuels
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
        public LandscapeStruct GetCellDataSimulationIndex(int xIndex, int yIndex, bool correctForOrigin)
        {
			if(correctForOrigin)
			{
                //WUIEngine.LOG(WUIEngine.LogType.Log, "X/Y offset cells: " + originCellOffset.x + ", " + originCellOffset.y);
                //correct for any difference in origin
                xIndex += originCellOffset.x;
                yIndex += originCellOffset.y;
            }

            //flip y since dataset is north down
            long posit = (xIndex + (Header.numnorth - yIndex - 1) * Header.numeast);

			return GetCellData((int)posit);
		}

		public int[] GetExisitingFuelModelNumbers()
        {
			int[] indexMap = new int[256];
			int cells = landscape.Length / (int)NumVals;
            List<int> invalidFuelModelNumbers = new List<int>();
			int invalidCount = 0;
            for (int i = 0; i < cells; i++)
            {
				int fuelModelNumber = landscape[i * NumVals + 3]; //fuel number is offset by 3
				if (fuelModelNumber > 0 && fuelModelNumber <= 256)
                {
					indexMap[fuelModelNumber - 1] += 1;
                }
				else
                {
                    if(!invalidFuelModelNumbers.Contains(fuelModelNumber))
					{
						invalidFuelModelNumbers.Add(fuelModelNumber);
						++invalidCount;
                    }
                }
            }

			if(invalidFuelModelNumbers.Count > 0)
            {
				string error = "Landscape data contains " + invalidCount + " cells with fuel model numbers outside of the valid range 1-256, numbers are: ";
				for (int i = 0; i < invalidFuelModelNumbers.Count; i++)
				{
					error += invalidFuelModelNumbers[i];
					if(i < invalidFuelModelNumbers.Count - 1)
					{
						error += ", ";
					}
				}
                WUIEngine.LOG(WUIEngine.LogType.Error, error);
            }

			List<int> presentFuelModelNumbers= new List<int>();
			for (int i = 0; i < indexMap.Length; i++)
			{
				if(indexMap[i] > 0)
                {
					presentFuelModelNumbers.Add(i + 1);
                }
			}

			return presentFuelModelNumbers.ToArray();
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
				return Header.XResol * 1000.0;   // to kilometers
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
				WUIEngine.LOG(WUIEngine.LogType.Warning, " LCP file not found in " + path + ".");
				return false;
			}

			return true;
		}

		void ReadData(string path)
		{
			if(!DoesLCPExist(path))
            {
				CantAllocLCP = true;
				return;
			}			

			using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
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
				NumViewEast++;
		*/
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

					using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
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

            Vector2d lcpUTM = new Vector2d(Header.WestUtm, Header.SouthUtm);
            originOffset = lcpUTM - WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin;
			originCellOffset = new Vector2int(-(int)(originOffset.x / GetCellResolutionX()), -(int)(originOffset.y / GetCellResolutionY()));

            if (CantAllocLCP)
            {
				WUIEngine.LOG(WUIEngine.LogType.Log, " LCP found in " + path + " but could not properly read it.");
			}
			else
            {
				WUIEngine.LOG(WUIEngine.LogType.Log, " LCP found in " + path + ", read succesfully.");
			}

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
	}
}