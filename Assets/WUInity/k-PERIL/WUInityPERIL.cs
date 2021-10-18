using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WUInityPERIL
{
    /*
     k-PERIL has been upgraded with various new features: 
    It can deal with WUI points that were not affected by the simulated fire. Previous versions of PERIL considered such points safe as the fire never reached them, even though that is because the time frame of the simulation was not long enough.
    It can create a compound boundary for any number of fire simulation cases. Input, for example, five cases and k-PERIL will create the one boundary that applies to all cases. 
    It can input an area of points as the WUI area and will extrapolate the boundary of it for faster run times.
    General improvements and optimisations have been added to make the processing faster.
    It calculates the newly introduced Evacuation Probability Index (EPI, proper name pending), a value for each cell that notes how many simulations each point inside the trigger safety boundary is affected by. For example, if a point in the raster is inside the trigger boundary of three simulations, it gets a value of 3. A higher EPI value indicates that the specific cell is more likely to be affected by the fire after an evacuation has been called. Any efforts of slowing down the fire long-term should be focused on cells with a high EPI to allow for more evacuation time.
    Here is how it can be used:
 
    k-PERIL has been bundled as a DLL file. To call the function, include the DLL in the code as using k_PERIL_DLL, instantiate a "peril" object and call the command peril.Run(int caseNumber, int cell, int tBuffer, int yDim, int xDim, int[,] WUIarea, float midFlameWindspeed, string ROSpathInput, string AzimuthPathInput, string PerilOutput, string EPIoutput)
 
    The input variables are:
    caseNumber: number of simulated fire cases. The simulation files are expected to be named 1_ros.txt, 1_azimuth.txt, 2_ros.txt and so on.
    cell: cell size in meters
    tBuffer: trigger buffer evacuation time
    yDim , xDim: Dimensions of the raster (cell number in each axis)
    WUIarea: the [x,y] coordinates of the WUI area
    midFlameWindspeed: the wind speed 6m above the canopy
    ROSpathInput, AzimuthPathInput, PerilOutput, EPIoutput: Input and output paths for the input and output files (just the folder locations). Set them appropriately in your code.
    the command has no return value. It simply creates two sets of files: 
    PERIL_output contains the trigger boundary nodes in x y \n format
    EPIoutput contains a matrix of the raster with the EPI values.
    At this stage, k-PERIL is ready for testing. You should have received a separate email with a link to download it. Please feel free to test this code out and come back with any errors or suggestions for improvement. Feel free to forward this to other people in the team if they can test it. Any further questions feel free to reach out to me.
    */

    private k_PERIL_DLL.PERIL peril = new k_PERIL_DLL.PERIL();
    int[,] compoundBoundary;
    int heatmapMax;

    /// <summary>
    /// Runs a desired number of cases inside the folders specifed.
    /// </summary>
    /// <param name="numberOfCases"></param>
    /// <param name="cell"></param>
    /// <param name="tBuffer"></param>
    /// <param name="yDim"></param>
    /// <param name="xDim"></param>
    /// <param name="WUIarea"></param>
    /// <param name="midFlameWindspeed"></param>
    /// <param name="ROSpathInput"></param>
    /// <param name="AzimuthPathInput"></param>
    /// <param name="PerilOutput"></param>
    /// <param name="EPIoutput"></param>
    public void RunKPeril(int tBuffer, int[,] WUIarea, float midFlameWindspeed, string ROSpathInput, string AzimuthPathInput, string PerilOutput, string EPIoutput)
    {
        if(compoundBoundary != null)
        {
            int[,] compoundBoundary = new int[xDim, yDim];
            heatmapMax = 0;
        }
        //look for all of the files in subfolder which contains intermediate output files, gets number of cases
        // tbuffer - actual evac time, user specify desired extra buffer time in input file
        //get cell size from input file
        //int cell = WUInity.WUInity.WUINITY_IN.fire.
        //get yDim and xDim from input file
        //get wuiarea from a user defined map painted in wuinity

        /*for (int i = 0; i < numberOfCases; i++)
        {
            RunPeril(i, cell, tBuffer, yDim, xDim, WUIarea, midFlameWindspeed, ROSpathInput, AzimuthPathInput, PerilOutput, EPIoutput);
        }*/     
    }

    //how to send ROS data ROStheta[X*Y,8], y first, x second


    void RunPeril(int caseNumber, int cell, int tBuffer, int yDim, int xDim, int[,] WUIarea, float midFlameWindspeed, string ROSpathInput, string AzimuthPathInput, string PerilOutput, string EPIoutput)
    {
        //peril.Run(caseNumber, cell, tBuffer, yDim, xDim, WUIarea, midFlameWindspeed, ROSpathInput, AzimuthPathInput, PerilOutput, EPIoutput);
        int[,] boundary = new int[1, 1] ;// peril.getSingularBoundary(0, 0, 0f, WUIarea, );
        for (int y = 0; y < boundary.GetLength(1); y++)
        {
            for (int x = 0; x < boundary.GetLength(0); x++)
            {
                compoundBoundary[x, y] += boundary[x, y];
                if(compoundBoundary[x, y] > heatmapMax)
                {
                    heatmapMax = compoundBoundary[x, y];
                }
            }
        }
    }

    public static int[,] GetDefaultWUIArea()
    {
        int[,] WUIarea = new int[,]                                                        
            {
                { 38, 24},
                { 38, 25},
                { 39, 24},
                { 39, 25},
                { 39, 26},
                { 40, 23},
                { 40, 24},
                { 40, 25},
                { 40, 26},
                { 40, 27},
                { 41, 24},
                { 41, 25},
                { 41, 26},
                { 42, 25},
                { 43, 25},
                { 44, 25},
            };
        return WUIarea;
    }
}



/*

using System;
using System.IO;
using k_PERIL_DLL;

namespace EXPERIMENTAL_PERIL
{
    class perilTester
    {
        static void Main(string[] args)     //where PERIL is actually called
        {
            PERIL peril = new PERIL();
            int[,] WUIarea = new int[,]                                                         //WUI Area
            {
                { 7,1 },
            };

            int[,,] boundaries = new int[10, 6, 10];


            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("SIMULATION " + (i+1) + " NOW RUNNING");
                float[,] ROS = new float[,]
                {
                    {0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f },
                    {0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f },
                    {0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f },
                    {0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f },
                    {0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f },
                    {0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f,0.8f },
                };

                int[,] Azimuth = new int[,]
                {
                    {45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i },
                    {45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i },
                    {45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i },
                    {45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i },
                    {45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i },
                    {45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i,45+4*i },
                };
                int[,] currentBoundary = peril.getSingularBoundary(30, 140, (float)4.47, WUIarea, ROS, Azimuth);

                for (int j = 0; j < currentBoundary.GetLength(0); j++)
                {
                    for (int k = 0; k < currentBoundary.GetLength(1); k++)
                    {
                        boundaries[j, k, i] = currentBoundary[j, k];
                    }
                }
            }
            int[,] evax = peril.getEVAXmatrix(boundaries, WUIarea);
            int[,] overallboundary = peril.getCompoundBoundary(boundaries);
        }
    }
}


*/
