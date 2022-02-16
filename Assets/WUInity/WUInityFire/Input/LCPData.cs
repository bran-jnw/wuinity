using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WUInity.Fire
{
	public struct LandScapeStruct
	{
		public short elevation;
		public short slope;
		public short aspect;
		public short fuel_model;
		public short canopy_cover;               // READ OR DERIVED FROM LANDSCAPE DATA
		public double aspectf;
		public double canopy_height;
		public double crown_base;  //bran-jnw: added _crown as base is taken by C#
		public double bulk_density;
		public double duff_model;
		public long coarse_woody_model;
	}

	// structure for holding basic cell information (is there a reason to have short variable names here?)
	struct celldata
	{		
		public short e;                 // elevation
		public short s;                 // slope
		public short a;                 // aspect
		public short f;                 // fuel models
		public short c;					// canopy cover
	}

	// structure for holding optional crown fuel information
	struct crowndata
	{

		public short h;					// canopy height
		public short b;					// crown base
		public short p;					// bulk density
	}

	// structure for holding duff and woody fuel information
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
		public class LCpHeader
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

		public LCpHeader Header = new LCpHeader();
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

		public LCPData()								//blank constructor does nothing
		{
			
		}

		public LCPData(string path)					//constructor with file string reads file
		{
			ReadLCP(path);
		}

		public void ReadLCP(string path)
		{
			ReadData(path);
			//SetCustFuelModelID(HaveCustomFuelModels());
			//SetConvFuelModelID(HaveFuelConversions());
		}

		public LandScapeStruct GetCellData(int x, int y)
        {
			//flip y since dataset is north down
			long posit = (x + (Header.numnorth - y - 1) * Header.numeast);
			celldata cell = new celldata();
			crowndata cfuel = new crowndata();
			grounddata gfuel = new grounddata();
			GetCellDataFromMemory(posit, ref cell, ref cfuel, ref gfuel);

			LandScapeStruct l = new LandScapeStruct();

			l.elevation = cell.e;
			l.slope = cell.s;
			l.aspect = cell.a;
			l.fuel_model = cell.f;
			l.canopy_cover = cell.c;

			switch (NumVals)									//So the number of values can be used to find what kind of vegetation is used??
			{
				case 7:
					// 5 basic and duff and woody
					l.bulk_density = gfuel.d;
					l.coarse_woody_model = gfuel.w;
					break;
				case 8:
					// 5 basic and crown fuels
					l.canopy_height = cfuel.h;
					l.crown_base = cfuel.b;
					l.bulk_density = cfuel.p;
					break;
				case 10:
					// 5 basic, crown fuels, and duff and woody
					l.canopy_height = cfuel.h;
					l.crown_base = cfuel.b;
					l.bulk_density = cfuel.p;

					l.duff_model = gfuel.d;
					l.coarse_woody_model = gfuel.w;
					break;
			}

			return l;
		}

		celldata CellData(double east, double north, ref celldata cell, ref crowndata cfuel, ref grounddata gfuel)
		{
			long Position = GetCellPosition(east, north);
			GetCellDataFromMemory(Position, ref cell, ref cfuel, ref gfuel);

			return cell;
		}

		long GetCellPosition(double east, double north)								//Is there not another method to do this in another class?
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
			short[] ldata = new short[10], zero = null;

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

		void ReadData(string path)
		{
			if (!File.Exists(path))
			{
				CantAllocLCP = true;
				WUInity.SIM.LogMessage("WARNING: LCP file not found in " + path + ", using default.");
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

				WUInity.SIM.LogMessage("LOG: LCP found in " + path + ", read succesfully.");
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
						if (NumAlloc > 2147483647)	//if that number above is not above max (i think int.max), but why is it phrased like this?
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
							reader.ReadBytes(headsize);
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