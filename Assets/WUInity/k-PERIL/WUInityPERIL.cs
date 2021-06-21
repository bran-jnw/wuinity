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

    public struct PerilOutput
    {
        public int[,] peril;
        public int[,] epi;
    }

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
    public void RunAllCases(int numberOfCases, int cell, int tBuffer, int yDim, int xDim, int[,] WUIarea, float midFlameWindspeed, string ROSpathInput, string AzimuthPathInput, string PerilOutput, string EPIoutput)
    {
        for (int i = 0; i < numberOfCases; i++)
        {
            RunPeril(i, cell, tBuffer, yDim, xDim, WUIarea, midFlameWindspeed, ROSpathInput, AzimuthPathInput, PerilOutput, EPIoutput);
        }        
    }

    void RunPeril(int caseNumber, int cell, int tBuffer, int yDim, int xDim, int[,] WUIarea, float midFlameWindspeed, string ROSpathInput, string AzimuthPathInput, string PerilOutput, string EPIoutput)
    {
        peril.Run(caseNumber, cell, tBuffer, yDim, xDim, WUIarea, midFlameWindspeed, ROSpathInput, AzimuthPathInput, PerilOutput, EPIoutput);
    }

    public static int[,] GetDefaultWUIArea()
    {
        int[,] WUIarea = new int[,]                                                        
            {
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
