using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WUInity
{
    public static class GraphicalFireInput
    {
        public static void SaveGraphicalFireInput()
        {
            string filename = WUInity.WUINITY_IN.simName;

            string[] data = new string[5];
            //x cells
            data[0] = WUInity.WUINITY_SIM.GetFireMesh().cellCount.x.ToString();
            //y cells
            data[1] = WUInity.WUINITY_SIM.GetFireMesh().cellCount.y.ToString();
            //actual data, first wuiArea, then ignition map
            data[2] = "";
            data[3] = "";
            data[4] = "";
            for (int i = 0; i < WUInity.WUINITY_IN.fire.wuiAreaIndices.Length; ++i)
            {
                if(WUInity.WUINITY_IN.fire.wuiAreaIndices[i] == 1)
                {
                    data[2] += i + " ";
                }

                if (WUInity.WUINITY_IN.fire.randomIgnitionIndices[i] == 1)
                {
                    data[3] += i + " ";
                }

                if (WUInity.WUINITY_IN.fire.initialIgnitionIndices[i] == 1)
                {
                    data[4] += i + " ";
                }

                /*data[2] += WUInity.WUINITY_IN.fire.wuiAreaIndices[i] + " ";
                data[3] += WUInity.WUINITY_IN.fire.randomIgnitionIndices[i] + " ";
                data[4] += WUInity.WUINITY_IN.fire.initialIgnitionIndices[i] + " ";*/
            }

            string path = Path.Combine(WUInity.WORKING_FOLDER, filename + ".gfi");
            System.IO.File.WriteAllLines(path, data); //graphical fire input
        }

        public static void LoadGraphicalFireInput()
        {
            string filename = WUInity.WUINITY_IN.simName;
            string path = Path.Combine(WUInity.WORKING_FOLDER, filename + ".gfi"); //graphical fire input

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string[] header = new string[5];
                    for (int i = 0; i < 5; ++i)
                    {
                        header[i] = sr.ReadLine();
                    }

                    int ncols, nrows;
                    int.TryParse(header[0], out ncols);
                    int.TryParse(header[1], out nrows);
                    //make sure we have the correct size
                    if (ncols == WUInity.WUINITY_SIM.GetFireMesh().cellCount.x && nrows == WUInity.WUINITY_SIM.GetFireMesh().cellCount.y)
                    {
                        string[] data = header[2].Split(' ');
                        int[] wuiAreaIndices = new int[ncols * nrows];
                        for (int i = 0; i < data.Length; ++i)
                        {
                            
                            int pos;
                            int.TryParse(data[i], out pos);
                            wuiAreaIndices[pos] = 1;                       
                        }
                        WUInity.WUINITY_SIM.UpdateWUIArea(wuiAreaIndices);

                        data = header[3].Split(' ');
                        int[] randomIgnitionArea = new int[ncols * nrows];
                        for (int i = 0; i < data.Length; ++i)
                        {
                            int pos;
                            int.TryParse(data[i], out pos);
                            randomIgnitionArea[pos] = 1;                          
                        }
                        WUInity.WUINITY_SIM.UpdateRandomIgnitionIndices(randomIgnitionArea);

                        data = header[4].Split(' ');
                        int[] initialIgnitionIndices = new int[ncols * nrows];
                        for (int i = 0; i < data.Length; ++i)
                        {                            
                            int pos;
                            int.TryParse(data[i], out pos);
                            initialIgnitionIndices[pos] = 1;                    
                        }
                        WUInity.WUINITY_SIM.UpdateInitialIgnitionIndices(initialIgnitionIndices);

                        WUInity.WUINITY_SIM.LogMessage("LOG: Graphical fire input loaded, cells: " + ncols + ", " + nrows);

                        /*string[] data = header[2].Split(' ');
                        int[] wuiAreaIndices = new int[ncols * nrows];
                        for (int i = 0; i < wuiAreaIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out wuiAreaIndices[i]);
                        }
                        WUInity.WUINITY_SIM.UpdateWUIArea(wuiAreaIndices);

                        data = header[3].Split(' ');
                        int[] randomIgnitionArea = new int[ncols * nrows];
                        for (int i = 0; i < randomIgnitionArea.Length; ++i)
                        {
                            int.TryParse(data[i], out randomIgnitionArea[i]);
                        }
                        WUInity.WUINITY_SIM.UpdateRandomIgnitionIndices(randomIgnitionArea);

                        data = header[4].Split(' ');
                        int[] initialIgnitionIndices = new int[ncols * nrows];
                        for (int i = 0; i < initialIgnitionIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out initialIgnitionIndices[i]);
                        }
                        WUInity.WUINITY_SIM.UpdateInitialIgnitionIndices(initialIgnitionIndices);*/
                    }
                    else
                    {
                        CreateDefaultInputs();
                        WUInity.WUINITY_SIM.LogMessage("WARNING: Graphical fire input file does not match current mesh, using default.");
                    }
                }
            }
            catch (System.Exception e)
            {
                CreateDefaultInputs();
                WUInity.WUINITY_SIM.LogMessage("WARNING: could not read GFI due to: " + e.ToString() + ". Creating empty default.");
                //WUInity.WUINITY_SIM.LogMessage("WARNING: Graphical fire input file " + path + " not found, using default.");
            }

        }

        private static void CreateDefaultInputs()
        {
            WUInity.WUINITY_SIM.UpdateWUIArea(null);
            WUInity.WUINITY_SIM.UpdateRandomIgnitionIndices(null);
            WUInity.WUINITY_SIM.UpdateInitialIgnitionIndices(null);
        }
    }
}

