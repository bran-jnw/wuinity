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

            string[] data = new string[4];
            //x cells
            data[0] = WUInity.WUINITY_SIM.GetFireMesh().cellCount.x.ToString();
            //y cells
            data[1] = WUInity.WUINITY_SIM.GetFireMesh().cellCount.y.ToString();
            //actual data, first wuiArea, then ignition map
            data[2] = "";
            data[3] = "";
            for (int i = 0; i < WUInity.WUINITY_IN.fire.wuiAreaIndices.Length; ++i)
            {
                data[2] += WUInity.WUINITY_IN.fire.wuiAreaIndices[i] + " ";
                data[3] += WUInity.WUINITY_IN.fire.ignitionIndices[i] + " ";
            }

            System.IO.File.WriteAllLines(Application.dataPath + "/Resources/_input/" + filename + ".gfi", data); //graphical fire input
        }

        public static void LoadGraphicalFireInput()
        {
            string filename = WUInity.WUINITY_IN.simName;
            string path = Application.dataPath + "/Resources/_input/" + filename + ".gfi"; //graphical fire input

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string[] header = new string[4];
                    for (int i = 0; i < 4; ++i)
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
                        for (int i = 0; i < wuiAreaIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out wuiAreaIndices[i]);
                        }
                        WUInity.WUINITY_SIM.UpdateWUIArea(wuiAreaIndices);

                        data = header[3].Split(' ');
                        int[] ignitionIndices = new int[ncols * nrows];
                        for (int i = 0; i < ignitionIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out ignitionIndices[i]);
                        }
                        WUInity.WUINITY_SIM.UpdateIgnitionIndices(ignitionIndices);
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
                WUInity.WUINITY_SIM.LogMessage("WARNING: Graphical fire input file " + path + " not found, using default.");
                //WUInity.WUINITY_SIM.LogMessage(e.Message);
            }

        }

        private static void CreateDefaultInputs()
        {
            WUInity.WUINITY_SIM.UpdateWUIArea(null);
            WUInity.WUINITY_SIM.UpdateIgnitionIndices(null);
        }
    }
}

