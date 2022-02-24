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
                    bw.Write(WUInity.SIM.GetFireMesh().cellCount.x);
                    bw.Write(WUInity.SIM.GetFireMesh().cellCount.y);
                    bw.Write(GetBytes(WUInity.INPUT.fire.wuiAreaIndices));
                    bw.Write(GetBytes(WUInity.INPUT.fire.randomIgnitionIndices));
                    bw.Write(GetBytes(WUInity.INPUT.fire.initialIgnitionIndices));
                    bw.Write(GetBytes(WUInity.INPUT.fire.triggerBufferIndices));
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

                        if (ncols == WUInity.SIM.GetFireMesh().cellCount.x && nrows == WUInity.SIM.GetFireMesh().cellCount.y)
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

                            WUInity.SIM.UpdateWUIArea(wuiAreaIndices);
                            WUInity.SIM.UpdateRandomIgnitionIndices(randomIgnitionArea);
                            WUInity.SIM.UpdateInitialIgnitionIndices(initialIgnitionIndices);
                            WUInity.SIM.UpdateTriggerBufferIndices(triggerBufferIndices);
                        }
                        else
                        {
                            CreateDefaultInputs();
                            WUInity.LogMessage("WARNING: Graphical fire input file does not match current mesh, using default.");
                        }
                    }
                }
            }
            else
            {
                CreateDefaultInputs();
                WUInity.LogMessage("WARNING: could not read GFI data, creating empty default.");
            }
        }

        private static void CreateDefaultInputs()
        {
            WUInity.SIM.UpdateWUIArea(null);
            WUInity.SIM.UpdateRandomIgnitionIndices(null);
            WUInity.SIM.UpdateInitialIgnitionIndices(null);
            WUInity.SIM.UpdateTriggerBufferIndices(null);
            SaveGraphicalFireInput();
        }
    }
}

