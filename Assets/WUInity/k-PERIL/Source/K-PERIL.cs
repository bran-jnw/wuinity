/* 

<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
     k-PERIL (Population Evacuation Trigger Algorithm)
<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>

================================================================
CODE WRITTEN AND EDITED BY NIKOLAOS KALOGEROPOULOS
ORIGINALLY CREATED BY HARRY MITCHELL
================================================================

----------------------------------------------------------------
DEPARTMENT OF MECHANICAL ENGINEERING
IMPERIAL COLLEGE LONDON
----------------------------------------------------------------

MADE AS PART OF FYP PROJECT

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

~~~~~~~~~~~~~~~~ WHAT IT DOES ~~~~~~~~~~~~~~~~

THIS CODE AIMS TO WORK WITH FIRE SIMULATION SOFTWARE, SPECIFICALLY TARGETED TO WILDLIFE-URBAN INTERFACE (WUI) AREAS.

IT CREATES A TRIGGER BUFFER REGION AROUND POPULATED AREAS. IF A FIRE CROSSES THE BOUNDARY OF THAT AREA, AN EVACUATION ORDER SHOULD BE ISSUES. 
THESE AREAS HAVE BEEN CREATED TO ENSURE, THAT IF A FIRE REACHED ITS BOUNDARY, AND AN EVACUATION ORDER IS ISSUES, THE PEOPLE IN THE ARE WILL HAVE
ENOUGH TIME TO SAFELY EVACUATE. THIS TRIGGER BUFFER TIME DEPENDS ON MANY VARIABLES AND IS USUALLY AN OUTPUT OF OTHER EVACUATION MODELS.

THE ENTIRETY OF PERIL IS WRITTEN AS A SEPARATE CLASS "PREPARATION", A "PARSEINPUTFILES" CLASS AND A "PATHFINDER" CLASS FOR THE PATHFINDING PORTION, IN CASE IT NEEDS TO BE ADAPTED OR ADOPTED TO OTHER PROGRAMS.

~~~~~~~~~~~~~~~~ HOW IT DOES IT ~~~~~~~~~~~~~~~~

PERIL STARTS BY IMPORTING A SOLVED FIRE MODEL PROBLEM. THAT IS, A SIMULATION ON AN AREA HAS BEEN SOLVED, AND THE CORRESPONDING DATA HAS BEEN EXPORTED.
PERIL ASSUMES A HUYGENS METHOD MODEL HAS BEEN USED. IT IMPORTS THE FOLLOWING TABLES:

>RATEOFSPREAD: MAGNITUDE OF MAXIMUM RATE OF SPREAD
>AZIMUTH : MAJOR PROPAGATION BEARING (DEGREES) FROM VERTICAL

NAME THEM AS X_ros.txt AND x_azimuth.txt, WITH X BEING THE NUMBER OF THE SIMULATED CASE.

FROM THERE PERIL INTERPOLATES THE A,B,C VALUES AS PER THE HUYGENS METHOD, AND CALCULATES THE PROPAGATION "WEIGHTS" FROM THE CENTER OF EACH CELL AND INWARDS, ON THE 8 CARDINAL ORTHOGONAL DIRECTIONS.
THAT WAY A WEIGHTING NETWORK IS CREATED. DFS IS THEN USED TO FIND THE DISTANCE FROM THE DECLARED WUI-NODES TO ALL OTHER ACTIVE NODES 
(ACTIVE MEANING ON FIRE AT SOME POINT IN THE SIMULATION) UNTIL THE BUFFER TIME IS REACHED. THE NODES IMMEDIATELY ON THE SPECIFIED DISTANCE (HERE EVACUATION TIME, WRSET, TRIGGER 
BUFFER ARE ALL REFERRED TO AS DISTANCE) ARE SAVED AS THE BOUNDARY NODES. 

AS OF V1.5, PERIL REPEATS THESE STEPS FOR ANY NUMBER OF CASES FOR THE SAME WUI AREA AND RETURNS THE COMPOUND BOUNDARY FOR ALL THE CASES

~~~~~~~~~~~~~~~~ HOW TO USE IT ~~~~~~~~~~~~~~~~

FIRST INCLUDE THE DLL FILE IN YOUR CODE AS k_PERIL_DLL

THEN INSTANTIATE THE SOLUTION AS A "PERIL" OBJECT

THE COMMAND THAT CALLS THE PERIL SCRIPT IS CALLED RUN. IT WILL NEED THE FOLLOWING INPUT VARIABLES:

1.	CASENUMBER: NUMBER OF SIMULATED FIRE CASES. THE SIMULATION FILES ARE EXPECTED TO BE NAMED 1_ROS.TXT, 1_AZIMUTH.TXT, 2_ROS.TXT AND SO ON.
2.	CELL: CELL SIZE IN METERS
3.	TBUFFER: TRIGGER BUFFER EVACUATION TIME
4.	YDIM , XDIM: DIMENSIONS OF THE RASTER (CELL NUMBER IN EACH AXIS)
5.	WUIAREA: THE [X,Y] COORDINATES OF THE WUI AREA
6.	MIDFLAMEWINDSPEED: THE WIND SPEED 6M ABOVE THE CANOPY
7.	ROSPATHINPUT, AZIMUTHPATHINPUT, PERILOUTPUT, EPIOUTPUT: INPUT AND OUTPUT PATHS FOR THE INPUT AND OUTPUT FILES (JUST THE FOLDER LOCATIONS). SET THEM APPROPRIATELY IN YOUR CODE.


!!!NOTE: AFTER THE "WEIGHTS GENERATED" MESSAGE THE MOTS COMPUTATIONALLY EXPENSIVE PART OF THE CODE RUNS, DO NOT WORRY IF IT TAKES LONGER THAN THE REST OF THE PROGRAM

~~~~~~~~~~~~~~~~ VERSION HISTORY ~~~~~~~~~~~~~~~~

V0.0: PROGRAM CREATED (11/2020)
V0.1: ADDED ROSCARDINALDIRECTIONS (12/2020)
V0.2: ADDED SETWEIGHTNODES (12/2020)
V0.3: ADDED SETROSN (12/2020)
V0.4: ADDED SETROSLOC (12/2020)
V0.5: ADDED SETWEIGHT (12/2020)
V0.6: REMOVED THE WEIGHTNODES MATRIX AND METHOD (12/2020)
V0.7: SHAMELESSLY STOLE A DIJKSTRA ALGO CODE SNIPPET (12/2020)
V0.8: INTEGRATED CODE WITH DIJKSTRA ALGO(12/2020)

V1.0: INITIAL USABLE MODEL: CAN TAKE INPUT FILES AND OUTPUT THE RESULTS IN NEW FILES(12/2020)
V1.1: CHANGED THE PATHFINDING ALGORITHM TO CUSTOM DFS(01/2021)
V1.2: ADDED FUNCTIONALITY TO WORK WHEN WUI IS IN INACTIVE NODE AREA(01/2021)
V1.3: ADDED WUI NODE OUT OUF BOUNDS WARNING (01/2021)
V1.4: ADDED WUI AREA FUNCTIONALITY, SOLVING FOR WUI AREA ABOUNDARY INSTEAD OF NODES
V1.5: ADDED MULTIPLE SIMULATION, COMPOUND BOUNDARY OUTPUT. CHANGED THE INPUT FILE NAMING INPUT
V1.6: ADDED DLL EXTERNAL REFERENCES
V1.7: ADDED EVAX FUNCTIONALITY
V1.8: CHANGED FLOW OF DATA FROM TEXT FILE BASED TO MEMORY BASED
V1.9: SPLIT THE MAIN PERIL METHODS TO THREE (GET SAFE AREA, GET OVERALL BOUNDARY, GET EVAX INDEX)
~~~~~~~~~~~~~~~~ KNOWN BUGS ~~~~~~~~~~~~~~~~

THE DISTANCE ALGORITHM USED IS A DFS HEURISTIC ONE, MEANING IT DOES NOT PRIVIDE THE ABSOLUTELY SHORTEST PATH TO EVERY NODE. SOME DISCREPANCIES MAY BE 
DETECTED, SUCH AS BOUNDARIES FOR LARGER EVACUATION TIMES APPEARING SMALLER IN AREAS.

ENCLAVES ARE NOT EXCLUDED FROM THE FINAL BOUNDARY GENERATION.

~~~~~~~~~~~~~~~~ NOTES ~~~~~~~~~~~~~~~~
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace k_PERIL_DLL
{
    class Pathfinder        //get distances to target nodes
    {
        public float[] dfs(float[,] graph, int WUI, int[,] rosloc, int tBuffer, int[,] azimuth)//DFS algorithm. Recursive with stop condition
        {
            bool[] visited = new bool[graph.GetLength(0)];      //keep track of visited nodes
            HashSet<int> upNext = new HashSet<int>();           //keep track of nodes to be visited nest
            float[] distance = new float[graph.GetLength(0)];   //keep track of distance from node to target
            int currentNode = 0;                                //Current node being measured

            for (int i = 0; i < graph.GetLength(0); i++)       //set all points as unvisited, and set distance from all points to wui as infinite
            {
                visited[i] = false;
                distance[i] = int.MaxValue;
            }

            upNext.Add(WUI);                                    //add WUI node as first up next node
            distance[WUI] = 0;                                   //set distance from WUI to WUI as zero

            while (upNext.Count() != 0)                         //while there are still nodes to be visited
            {
                currentNode = upNext.ElementAt(0);              //set current node

                for (int i = 0; i < 8; i++)                     //for all 8 neighbors of each current node
                {
                    try                                         //Try the following (to avoid errors when the neighbor of the currrent node is in the edge of the analysis raster
                    {
                        if (!visited[rosloc[currentNode, i]] && rosloc[currentNode, i]!=0 && distance[currentNode] + graph[currentNode, i] < distance[rosloc[currentNode, i]]) //if the neighbor of the currrent node is not visited AND the distance from the neighbor node to the WUI is larger than the distance from current node to WUI plus distance from current node to neighbor node
                        {
                            distance[rosloc[currentNode, i]] = distance[currentNode] + graph[currentNode, i];    //update minimum distance
                            if (distance[rosloc[currentNode, i]] <= tBuffer && !(upNext.Contains(rosloc[currentNode, i])) && azimuth[rosloc[currentNode, i] % azimuth.GetLength(0), (int)(rosloc[currentNode, i] / azimuth.GetLength(0))] != -9999) //if tBuffer has not yet been reached AND neighbor node is not inactive
                            {
                                upNext.Add(rosloc[currentNode, i]);     //add neighbor node to up next list
                            }
                        }
                    }
                    catch (Exception e)                          //just to complete the try-catch statement
                    {

                    }

                }
                visited[currentNode] = true;                    //set current node as visited
                upNext.Remove(currentNode);                     //remove current node from up next list
            }
            return distance;                                    //return the distance array
        }
    }

    class Preparation       //peril setup methods
    {
        public float[,] ROScardinalDirections(int[,] azimuth, float[,] ROS, float U) //calculate ros, includes rate of spread to all 8 cardinal directions
        {
            double LB = 0.936 * Math.Exp(0.2566 * U) + 0.461 * Math.Exp(-0.1548 * U) - 0.397; //Calculate length to Breadth ratio of Huygens ellipse
            double HB = (LB + (float)Math.Sqrt(LB * LB - 1)) / (LB - (float)Math.Sqrt(LB * LB - 1));    //calculate head to back ratio

            int totalY = azimuth.GetLength(0);   //get maximum y dimension
            int totalX = azimuth.GetLength(1);   //get maximum x dimension

            float a = new float();              //create variable a for huygens ellipse
            float b = new float();              //create variable b for huygens ellipse
            float c = new float();              //create variable c for huygens ellipse
            double ROSX = new double();         //create variable ROSX for ROS calculation
            double ROSY = new double();         //create variable ROSY for ROS calculation
            float[,] ROStheta = new float[totalX * totalY, 8];  //create output matrix variable

            int linearIndex = 0; //create linearisation index variable

            //the code converts the raster from an X x Y raster to a X*Y x 1 linear array of elements. This makes further calculations easier as the resulting network of nodes is just a list of linear points.

            for (int x = 0; x < totalX; x++)    //for every element in the raster
            {
                for (int y = 0; y < totalY; y++)
                {
                    a = (ROS[y, x] / (2 * (float)LB)) * (1 + 1 / (float)HB);       //Calculate a
                    b = (ROS[y, x] / 2) * (1 + 1 / (float)HB);                     //Calculate b
                    c = (ROS[y, x] / 2) * (1 - 1 / (float)HB);                     //Calculate c

                    for (int cardinal = 0; cardinal < 8; cardinal++)                //for every cardinal direction (starting from north and going clockwise)
                    {
                        if (azimuth[y, x] != -9999)                                 //if the cell is active i.e. has been burned in the simulation
                        {
                            ROSX = a * Math.Sin((Math.PI * cardinal / 4) - azimuth[y, x] * 2 * Math.PI / 360);              //Calculate ROSX
                            ROSY = c + b * Math.Cos((Math.PI * cardinal / 4) - azimuth[y, x] * 2 * Math.PI / 360);          //Calculate ROSY
                            ROStheta[linearIndex, cardinal] = (float)Math.Sqrt(Math.Pow(ROSX, 2) + Math.Pow(ROSY, 2));      //Calculate ROS per cardinal direction
                        }
                        else { ROStheta[linearIndex, cardinal] = 0; }    //if the cell is inactive, set ros to zero
                    }
                    linearIndex++;                                                  //advance the linear index
                }
            }
            return ROStheta;                                                        //return completed matrix
        }

        public int[] SetRosN(int totalY, int totalX) //calculate rosn, a list of all the non-boundary nodes (boundary nodes do not have 8 neighbors and complicate the rest of the algorithm. As such only internal nodes are used, and boundary nodes are only used as "neighbors" for calculations
        {
            List<int> rosN = new List<int>();               //create a new list for rosN

            for (int i = 1; i < totalY-1; i++)              //for the X and Y dimensions
            {
                for (int j = 1; j < totalX-1; j++)
                {
                    rosN.Add(i * totalX + j);               //add it to the list                        
                }
            }
            return rosN.ToArray();                          //return rosN as an array
        }

        public int[,] SetRosLoc(int[] rosn, int totalY) //calculate rosloc, a catalog of the neighbors of each node. Orientation is same as ROS cardinal direction, starts from North and moves clockwise.
        {
            int[,] rosloc = new int[rosn.Max() + 1, 8];           //create output variable

            for (int i = 0; i < rosn.Length; i++)               //for every element is rosn calculate and catalog its linearised neighbor
            {
                rosloc[rosn[i], 0] = rosn[i] - 1;               //North
                rosloc[rosn[i], 1] = rosn[i] + totalY - 1;      //NE
                rosloc[rosn[i], 2] = rosn[i] + totalY;          //east
                rosloc[rosn[i], 3] = rosn[i] + totalY + 1;      //SE
                rosloc[rosn[i], 4] = rosn[i] + 1;               //South
                rosloc[rosn[i], 5] = rosn[i] - totalY + 1;      //SW
                rosloc[rosn[i], 6] = rosn[i] - totalY;          //west
                rosloc[rosn[i], 7] = rosn[i] - totalY - 1;      //NW          
            }
            return rosloc;
        }

        public float[,] SetWeight(int[] rosn, float[,] ROS, int cell, int[,] rosloc) //calculate the weight variable. contains the "Weight" between each node and its 8 neighbors
        {
            float[,] weight = new float[rosn.Max() + 1, 8];                     //create output variable
            int linearIndex = 0;                                                //create linear index

            for (int i = 0; i < rosn.Length; i++)                               //for each point in rosN
            {
                int point = rosn[i];                                            //save the current point linear index for use later
                for (int j = 0; j < 8; j++)                                     //for each neighbor 
                {
                    //weighting is the average of the inverses of the ROS of the neighboring points. If we are for example examining a point and its north neighbor, we are averaging the inverses of the ROS directions towards the north (not the south, since we are creating an inverse weight matrix technically)
                    if (ROS[point, j] != 0 && ROS[rosloc[point, j], j] != 0)    //if the point and its neighbor are active
                    {
                        if (j % 2 == 0)                                         //if the point is N S E or W
                        {
                            weight[point, j] = (cell / 2) * ((1 / ROS[point, (j + 4) % 8]) + (1 / ROS[rosloc[point, j], (j + 4) % 8]));                         //calculate weight
                        }
                        else                                                    //if the point is a corner node (have to account for a longer distance)
                        {
                            weight[point, j] = (float)Math.Sqrt(2) * (cell / 2) * ((1 / ROS[point, (j + 4) % 8]) + (1 / ROS[rosloc[point, j], (j + 4) % 8]));   //calculate weight
                        }
                        linearIndex++;                                          //advance linear index
                    }
                }
            }
            return weight;                                                      //return completed weight matrix
        }

        public void consoleMatrix(float[,] output) //DEBUG write a matrix on the console
        {
            for (int i = 0; i < output.GetLength(0); i++)       //for all elements in the matrix
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    Console.Write(output[i, j] + " ");          //Write the matrix element
                }
                Console.WriteLine(" ");                         //go to next line
            }
            Console.WriteLine(" ");                             //in the end go to next line
        }

        public int[] getBoundary(float[,] weightList, int tBuffer, int yy)      //create the boundary of the PERIL area
        {
            List<int> boundary = new List<int>();                               //create the new boundary list, includes all nodes within the PERIL boundary

            for (int i = 0; i < weightList.GetLength(1); i++)                   //for all the elements on the weight/distance list (output of pathfinder)
            {
                for (int j = 0; j < weightList.GetLength(0); j++)
                {
                    if (weightList[j, i] != int.MaxValue)                       //if the weight of the node is less than Tbuffer
                    {
                        boundary.Add(j);                                        //include it in the boundary list
                    }
                }
            }
            return boundary.Distinct().ToArray();                                          //return all nodes within the boundary
        }

        public int[] compoundBoundary(int[] boundary, int yy)
        {
            List<int> uniqueBoundary = new List<int>();                         //create a new list 
            uniqueBoundary.AddRange(boundary);                                  //Add all the nodes in the boundary in it 

            List<int> lineBoundary = new List<int>();                           //create a new list to include all nodes NOT on the edge of the boundary (made like this for debugging purposes)

            for (int i = 0; i < uniqueBoundary.Count; i++)
            {
                if (uniqueBoundary.Contains(uniqueBoundary.ElementAt(i) + 1) && uniqueBoundary.Contains(uniqueBoundary.ElementAt(i) - 1) && uniqueBoundary.Contains(uniqueBoundary.ElementAt(i) - yy) && uniqueBoundary.Contains(uniqueBoundary.ElementAt(i) + yy))
                {
                    //if all neighbors of the target node are in the boundary matrix
                    lineBoundary.Add(uniqueBoundary.ElementAt(i));  //Add the node to the new List
                }
            }
            //
            return uniqueBoundary.Except(lineBoundary).ToArray();   //return the difference between the old and new boundary list, should only include the edge nodes

        }

        public void outputFile(int[] boundary, int yy, string PerilOutput, int yDim)      //output variable to a new text file
        {
            int[,] output = delinearise(boundary, yy);

            using (var sw = new StreamWriter(PerilOutput))  //beyond here the code has been shamelessly stolen
            {
                for (int i = 0; i < boundary.Length; i++)   //for all elements in the output array
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sw.Write(output[i, j] + " ");       //write the element in the file
                    }
                    sw.Write("\n");                         //enter new line
                }
                sw.Flush();                                 //i dont really know
                sw.Close();                                 //close opened output text file
            }
        }

        public void DEBUGoutputFile(float[,] boundary, int yy, string PerilOutput)      //output variable to a new text file
        {
            float[,] output = new float[boundary.GetLength(0), 3];    //create new output variable
            for (int i = 0; i < boundary.GetLength(0); i++)       //for all the elements in the input variable
            {
                output[i, 0] = i / yy;       //convert the output from a linearised network back to the 2D raster
                output[i, 1] = i % yy;
                output[i, 2] = boundary[i, 0];  //include the value on that raster point
            }

            using (var sw = new StreamWriter(PerilOutput))  //beyond here the code has been shamelessly stolen
            {
                for (int i = 0; i < boundary.GetLength(0); i++)   //for all elements in the output array
                {
                    if (!(output[i, 2] == int.MaxValue))
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            sw.Write((int)output[i, j] + " ");       //write the element in the file
                        }
                        sw.Write("\n");                             //enter new line
                    }
                }
                sw.Flush();                                 //i dont really know
                sw.Close();                                 //close opened output text file
            }
        }

        public int[] checkFireOOB(int WUIx, int WUIy, int[,] azimuth)           //check if the WUI area is not on an active node 
        {
            int[] newWUI = { WUIx, WUIy };                              //create a vector for the WUI point

            if (azimuth[WUIy, WUIx] == -9999)                           //if the WUI is on an inactive note
            {
                Console.WriteLine("<><><><><><><><><><><><><><><><><><><><><><>");      //show error message
                Console.WriteLine("WARNING: WUI POINT OUT OF FIRE BOUNDS");
                Console.WriteLine("<><><><><><><><><><><><><><><><><><><><><><>");
                Console.WriteLine("SUBSTITUTING FOR CLOSEST ACTIVE NODE");
                Console.WriteLine("<><><><><><><><><><><><><><><><><><><><><><>");

                float minDistance = int.MaxValue;                       //create new minimum distance variable, set it to infinite. 
                float tryout = int.MaxValue;                            //create new calculated distance variable, set it to infinite. 

                for (int i = 0; i < azimuth.GetLength(0); i++)          //for all elements in the raster
                {
                    for (int j = 0; j < azimuth.GetLength(1); j++)
                    {
                        if (azimuth[i, j] != -9999)                    //if the current node is active
                        {
                            tryout = (float)Math.Sqrt(Math.Pow(Math.Abs(i - WUIy), 2) + Math.Pow(Math.Abs(j - WUIx), 2));       //calculate new distance
                            if (minDistance > tryout)       //if the min distance is larger than the new distance
                            {
                                minDistance = tryout;       //set distance as new distance
                                newWUI[0] = j;              //Set new WUI point X and Y
                                newWUI[1] = i;
                            }
                        }
                    }
                }
                Console.WriteLine("NEW NODE: X = " + newWUI[0] + " ,Y = " + newWUI[1]);    //output it on console
                Console.WriteLine("<><><><><><><><><><><><><><><><><><><><><><>");
            }

            return newWUI;  //return new WUI area. If WUI was originally on an active node then no change occurs and it returns the original WUI point
        }

        public void checkGeneralOOB(int WUIx, int WUIy, int[,] azimuth)         //check if the WUI point is outside of Bounds
        {
            if (WUIx > azimuth.GetLength(1) || WUIy > azimuth.GetLength(0) || WUIx < 0 || WUIy < 0)    //if the point is either outside the max raster size or negative
            {
                Console.WriteLine("MAX X: " + azimuth.GetLength(1) + " MAX Y: " + azimuth.GetLength(0));    //show a console error
                throw new Exception("ERROR: ONE OR MORE COORDINATES OUT OF BOUNDS");                        //throw new exception
            }
        }

        public int[] linearise(int[,] WUI, int yy)          //Method to turn the 2D Matrix points to their 1D linearised form
        {
            int[] output = new int[WUI.GetLength(0)];       //create new output variable
            for (int i = 0; i < WUI.GetLength(0); i++)      //for all the points in the input variable
            {
                output[i] = WUI[i, 0] * yy + WUI[i, 1];     //convert the output from a 2D raster to the 1D network
            }
            return output;
        }

        public int[,] delinearise(int[] WUI, int yy)        //Method to turn the raster from a 1D linear naming form to a 2D matrix form
        {
            int[,] output = new int[WUI.Length, 2];         //create new output variable
            for (int i = 0; i < WUI.Length; i++)            //for all the elements in the input variable
            {
                output[i, 0] = (int)WUI[i] / yy;            //convert the output from a linearised network back to the 2D raster
                output[i, 1] = WUI[i] % yy;
            }
            return output;
        }

        public void outputEPI(int[] EVAXarray, int yDim, string EVAXoutput, int[,] WUIarea)     //Method to create and output the EVAX matrix
        {
            int[,] output = new int[yDim, EVAXarray.Length / yDim];                             //Create the output matrix

            for (int i = 0; i < EVAXarray.Length; i++)                                          //Delinearise the input array
            {
                output[(int)i / yDim, i % yDim] = EVAXarray[i];
            }

            for (int i = 0; i < WUIarea.GetLength(0); i++)                                      //Set the WUI nodes as 9999 in the matrix
            {
                output[WUIarea[i, 0], WUIarea[i, 1]] = 9999;
            }

            using (var sw = new StreamWriter(EVAXoutput))  //beyond here the code has been shamelessly stolen
            {
                for (int i = 0; i < output.GetLength(0); i++)   //for all elements in the output array
                {
                    for (int j = 0; j < yDim; j++)
                    {
                        sw.Write(output[i, j] + " ");       //write the element in the file
                    }
                    sw.Write("\n");                         //enter new line
                }
                sw.Flush();                                 //i dont really know
                sw.Close();                                 //close opened output text file
            }
        }
    }

    class ParseInputFiles   //get and open the input files
    {
        public float[,] getFile(string Path)                                //save info in a text file as a variable (shamelessly stolen off the internet, read these comments with a hit of suspition)
        {
            var data = System.IO.File.ReadAllText(Path);                    //Save all data in var data
            var arrays = new List<float[]>();                               //create a new list of float arrays
            var lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);  //split the data var by line and save each line as an array

            foreach (var line in lines)                                     //for each line variable in the list(?) lines
            {
                var lineArray = new List<float>();                          //make a new list of floats
                foreach (var s in line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))   //for each variable s in the input array
                {
                    lineArray.Add(float.Parse(s, System.Globalization.NumberStyles.Float)); //add the element to the list of floats created before the loop
                }
                arrays.Add(lineArray.ToArray());                            //add the lineArray array to the general arrays list
            }
            var numberOfRows = lines.Count();                               //save the number of rows of the parsed and edited data in lines
            var numberOfValues = arrays.Sum(s => s.Length);                 //save the total number of values in arrays (i.e. all elements)

            int minorLength = arrays[0].Length;                             //create a variable to save the minor dimension of the arrays variable
            float[,] NumOut = new float[arrays.Count, minorLength];         //create output matrix
            for (int i = 0; i < arrays.Count; i++)                          //for all elements in arrays
            {
                var array = arrays[i];                                      //save the array in the arrays element
                for (int j = 0; j < minorLength; j++)                       //for the length of the minor array
                {
                    NumOut[i, j] = array[j];                                //place each element in the output matrix
                }
            }
            return NumOut;                                                  //return the output matrix
        }
    }

    public class PERIL           //the actual program
    {
        public int[,] getSingularBoundary(int cell, int tBuffer, float midFlameWindspeed, int[,] WUIarea, float[,] ROS, int[,] azimuth) //The main function
        {
            Preparation solve01 = new Preparation();                                        //instantiate peril preparation
            Pathfinder pathfinder = new Pathfinder();                                       //instantiate pathfinder

            int yDim = azimuth.GetLength(0);
            int xDim = azimuth.GetLength(1);

            int[,] WUI = solve01.delinearise(solve01.compoundBoundary(solve01.linearise(WUIarea, yDim), yDim), yDim);     //Linearise the WUIarea array, get its boundary, and delinearise

            int[,] safetyMatrix = new int[xDim,yDim];

            //GET WUI BOUNDARY----------------------------------
            Console.WriteLine("_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~");    //Output the WUI area boundary generated in the Console
            Console.Write("WUI Boundary Nodes: ");
            for (int i = 0; i < WUI.GetLength(0); i++)
            {
                Console.Write(WUI[i, 0] + "," + WUI[i, 1] + "   ");
            }
            Console.WriteLine();
            Console.WriteLine("_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~_~");
            //--------------------------------------------------

            Console.WriteLine("AZIMUTH FILE READ");                                         //console confirmation message

            int[] WUInput = new int[WUI.GetLength(0)];                                      //create the WUI variable (a new one to parse any kind of edit to it)
            int[] OOBfixer = new int[2];                                                    //create a temporary Out of bounds vector 

            for (int i = 0; i < WUI.GetLength(0); i++)                                      //for all WUI points 
            {
                solve01.checkGeneralOOB(WUI[i, 0], WUI[i, 1], azimuth);                     //check for out of bounds
                OOBfixer = solve01.checkFireOOB(WUI[i, 0], WUI[i, 1], azimuth);             //check whether WUI is on actice node
                WUInput[i] = (OOBfixer[0] - 1) * yDim + OOBfixer[1];                        //Linearise WUI accordingly
            }

            float[,] ROStheta = solve01.ROScardinalDirections(azimuth, ROS, midFlameWindspeed); //calculate Cardinal Direction ROS

            Console.WriteLine("ROS_THETA GENERATED");                                       //console confirmation message

            int[] rosN = solve01.SetRosN(xDim, yDim);                                       //Calculate ROSn

            Console.WriteLine("ROSn GENERATED");                                            //console confirmation message

            int[,] rosloc = solve01.SetRosLoc(rosN, yDim);                                  //Calculate Rosloc

            Console.WriteLine("ROSLOC GENERATED");                                          //console confirmation message

            float[,] plainweights = solve01.SetWeight(rosN, ROStheta, cell, rosloc);        //Calculate Weights Matrix

            Console.WriteLine("WEIGHTS GENERATED");                                         //console confirmation message

            float[,] allDistances = new float[plainweights.GetLength(0), WUInput.Length];   //create distance matrix

            for (int i = 0; i < WUInput.Length; i++)                                        //for all WUI nodes
            {
                float[] temp = pathfinder.dfs(plainweights, WUInput[i], rosloc, tBuffer, azimuth); //pathfind from the WUI node and save resulting array to temp 
                for (int j = 0; j < plainweights.GetLength(0); j++)                         //for all the elements in the temp array
                {
                    allDistances[j, i] = temp[j];                                           //parse the array elements to the big output matrix
                }
            }

            Console.WriteLine("TIMES TO WUI GENERATED");                                    //console confirmation message

            int[] currentDangerZone = solve01.getBoundary(allDistances, tBuffer, yDim);     //Get and save the boundary area

            for (int i = 0; i < currentDangerZone.Length; i++)                              //Add the boundary area results to the EVAX index matrix
            {
                safetyMatrix[currentDangerZone[i] / yDim, (currentDangerZone[i] % yDim)]++;
            }

            Console.WriteLine("BOUNDARY GENERATED");                                        //console confirmation message

            return safetyMatrix;
        }

        public int[,] getCompoundBoundary(int[,,] allBoundaries)
        {
            List<int> uniqueBoundary = new List<int>();

            for (int i = 0; i < allBoundaries.GetLength(2); i++)
            {
                for (int x = 0; x < allBoundaries.GetLength(0); x++)
                {
                    for (int y = 0; y < allBoundaries.GetLength(1); y++)
                    {
                        if (allBoundaries[x, y, i] == 1)
                        {
                            uniqueBoundary.Add(x * allBoundaries.GetLength(1) + y);
                        }
                    }
                }
            }

            List<int> lineBoundary = new List<int>();                           //create a new list to include all nodes NOT on the edge of the boundary (made like this for debugging purposes)
            List<int> noDupes = uniqueBoundary.Distinct().ToList();

            for (int i = 0; i < noDupes.Count; i++)
            {
                if (!(noDupes.Contains(noDupes.ElementAt(i) + 1) && noDupes.Contains(noDupes.ElementAt(i) - 1) && noDupes.Contains(noDupes.ElementAt(i) - allBoundaries.GetLength(1)) && noDupes.Contains(noDupes.ElementAt(i) + allBoundaries.GetLength(1))))
                {
                    //if all neighbors of the target node are in the boundary matrix
                    lineBoundary.Add(noDupes.ElementAt(i));  //Add the node to the new List
                }
            }

            int[,] output = new int[lineBoundary.Count, 2];
            int count = 0;

            foreach (int boundaryPoint in lineBoundary)
            {
                output[count, 0] = boundaryPoint / allBoundaries.GetLength(1);
                output[count, 1] = boundaryPoint % allBoundaries.GetLength(1);
                count++;
            }

            return output;   //return the difference between the old and new boundary list, should only include the edge nodes
        }

        public int[,] getEVAXmatrix(int[,,] allBoundaries, int[,] WUIarea)
        {
            int[,] EVAXmatrix = new int[allBoundaries.GetLength(0), allBoundaries.GetLength(1)];

            for (int y = 0; y < allBoundaries.GetLength(1); y++)
            {
                for (int x = 0; x < allBoundaries.GetLength(0); x++)
                {
                    for (int i = 0; i < allBoundaries.GetLength(2); i++)
                    {
                        EVAXmatrix[x,y] += allBoundaries[x, y, i];
                    }
                }
            }

            for (int i = 0; i < WUIarea.GetLength(0); i++)
            {
                EVAXmatrix[WUIarea[i, 0], WUIarea[i, 1]] = 255;
            }

            return EVAXmatrix;
        }

        public int[,] getUpdateBoundary(int cellSize, int tBuffer, int WUIarea, int maxROS)                 //placeholder for method with ready ROScardinal inputs
        {
            int[,] output = new int[,]
                {
                    {1,1,1,1,1,1,1,1,1,1 },
                    {1,1,1,1,1,1,1,1,1,1 },
                    {1,1,1,1,1,1,1,1,1,1 },
                    {1,1,1,1,1,1,1,1,1,1 },
                    {1,1,1,1,1,1,1,1,1,1 },
                    {1,1,1,1,1,1,1,1,1,1 },
                };
            return output;
        }

    }
}
