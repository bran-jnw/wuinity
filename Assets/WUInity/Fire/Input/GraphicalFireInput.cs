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
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simName + ".gfi");

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(WUInity.SIM.FireMesh().cellCount.x);
                    bw.Write(WUInity.SIM.FireMesh().cellCount.y);
                    bw.Write(GetBytes(WUInity.SIM_DATA.wuiAreaIndices));
                    bw.Write(GetBytes(WUInity.SIM_DATA.randomIgnitionIndices));
                    bw.Write(GetBytes(WUInity.SIM_DATA.initialIgnitionIndices));
                    bw.Write(GetBytes(WUInity.SIM_DATA.triggerBufferIndices));
                }
            }
        }

        static byte[] GetBytes(bool[] values)
        {
            byte[] result = new byte[values.Length * sizeof(bool)];
            System.Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        static bool[] GetBools(byte[] values, int length)
        {
            bool[] result = new bool[length];
            System.Buffer.BlockCopy(values, 0, result, 0, values.Length);
            return result;
        }

        public static void LoadGraphicalFireInput()
        {
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simName + ".gfi"); //graphical fire input

            if(File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int ncols = br.ReadInt32();
                        int nrows = br.ReadInt32();

                        if (ncols == WUInity.SIM.FireMesh().cellCount.x && nrows == WUInity.SIM.FireMesh().cellCount.y)
                        {
                            int dataSize = ncols * nrows;

                            byte[] b = br.ReadBytes(dataSize * sizeof(bool));
                            bool[] wuiAreaIndices = GetBools(b, dataSize);

                            b = br.ReadBytes(dataSize * sizeof(bool));
                            bool[] randomIgnitionArea = GetBools(b, dataSize);

                            b = br.ReadBytes(dataSize * sizeof(bool));
                            bool[] initialIgnitionIndices = GetBools(b, dataSize);

                            b = br.ReadBytes(dataSize * sizeof(bool));
                            bool[] triggerBufferIndices = GetBools(b, dataSize);

                            WUInity.SIM_DATA.UpdateWUIArea(wuiAreaIndices);
                            WUInity.SIM_DATA.UpdateRandomIgnitionIndices(randomIgnitionArea);
                            WUInity.SIM_DATA.UpdateInitialIgnitionIndices(initialIgnitionIndices);
                            WUInity.SIM_DATA.UpdateTriggerBufferIndices(triggerBufferIndices);
                        }
                        else
                        {
                            CreateDefaultInputs();
                            WUInity.WUI_LOG("WARNING: Graphical fire input file does not match current mesh, using default.");
                        }
                    }
                }
            }
            else
            {
                CreateDefaultInputs();
                WUInity.WUI_LOG("WARNING: could not read GFI data, creating empty default.");
            }
        }

        private static void CreateDefaultInputs()
        {
            WUInity.SIM_DATA.UpdateWUIArea(null);
            WUInity.SIM_DATA.UpdateRandomIgnitionIndices(null);
            WUInity.SIM_DATA.UpdateInitialIgnitionIndices(null);
            WUInity.SIM_DATA.UpdateTriggerBufferIndices(null);
            SaveGraphicalFireInput();
        }
    }
}

