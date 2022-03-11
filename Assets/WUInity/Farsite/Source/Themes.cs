using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farsite
{
    /*
class ColorRamp
{
	long i, NumColors;
	double ColorIncR, ColorIncG, ColorIncB;
	COLORREF* Colors;
	void GetColorChanges(double NumColors, long R, long G, long B, long Var);
public:
	bool Reverse;
	long ColorType;
	double Min, Max;
	long Rval, Gval, Bval;
	char ColorFile[256];
	ColorRamp(long NumColors, long R, long G, long B, long Var, double Min,
		double Max, bool Reverse);
	~ColorRamp();
	COLORREF GetColor(long order);
	void SetRamp(long NumColors, long R, long G, long B, long Var, double Max);
	long GetNumColors();
	bool SetSpecificColor(long numcolor, long R, long G, long B);
	//void SetNumCats(long numcats);
	//long GetCat(long Num);
	//void CopyCats(long *cats);
	//long GetNumCats();
	//long CategoriesOK(long OK);
};
*/
/*
    public class GridTheme
    {
	    public char[] Name= new char[256];
        public long Continuous;
        public long RedVal, GreenVal, BlueVal, VarVal, NumColors, MaxBrite, ColorChange;
        public bool WantNewRamp, WantNewColor, LcpAscii, OnOff, OnOff3d, Changed3d, ConvertFuelColors;
        public long[] Cats = new long[100];
        public long  NumCats, CatsOK, Priority;
        public long LegendNum;
        public double MaxVal, MinVal;
        //	ColorRamp* ramp;

        public GridTheme();
        ~GridTheme();
        public void CreateRamp();
        public void DeleteRamp();
        //bool GetColor(double value, COLORREF* colr);
    }*/

    //------------------------------------------------------------------------------
    //------------------------------------------------------------------------------
    /*
    public class LandscapeTheme : GridTheme
    {
	    void FillCats();
        void SortCats();
        Farsite5 pFarsite;
    
	    public long[] NumAllCats = new long[10];
        public long[,] AllCats = new long[10,100];
	    public double[] maxval = new double[10], minval = new double[10];

        LandscapeTheme(bool Analyze, Farsite5 _pFarsite);
        void CopyStats(long layer);
        void ReadStats();
        void AnalyzeStats();
    }*/

//LandscapeTheme *GetLandscapeTheme()

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
/*
class RasterTheme : public GridTheme
{
	void FillCats();
	void SortCats();
public:
	double* map;
	double rW, rE, rN, rS, rCellSizeX, rCellSizeY, rMaxVal, rMinVal;
	long rRows, rCols;
	RasterTheme();
	~RasterTheme();
	bool SetTheme(const char *Name);
};
*/

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

    public class  VectorTheme
    {
        long VectorNum;
        char[] FileName = new char[256];
        long FileType;
        long Permanent;
        //COLORREF Color;
        int PenStyle;
        int PenWidth;
        bool OnOff;
        bool OnOff3d;
        bool Changed;
        long Priority;
    }
}

