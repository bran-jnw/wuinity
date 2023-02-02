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
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Simulation.SimulationID + ".gfi");
            WUInity.INPUT.Fire.graphicalFireInputFile = WUInity.INPUT.Simulation.SimulationID + ".gfi";

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(WUInity.SIM.FireMesh().cellCount.x);
                    bw.Write(WUInity.SIM.FireMesh().cellCount.y);
                    bw.Write(GetBytes(WUInity.RUNTIME_DATA.Fire.WuiAreaIndices));
                    bw.Write(GetBytes(WUInity.RUNTIME_DATA.Fire.RandomIgnitionIndices));
                    bw.Write(GetBytes(WUInity.RUNTIME_DATA.Fire.InitialIgnitionIndices));
                    bw.Write(GetBytes(WUInity.RUNTIME_DATA.Fire.TriggerBufferIndices));
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

        public static void LoadGraphicalFireInput(out bool success)
        {
            success = false;
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.graphicalFireInputFile); //graphical fire input

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

                            WUInity.RUNTIME_DATA.Fire.UpdateWUIArea(wuiAreaIndices);
                            WUInity.RUNTIME_DATA.Fire.UpdateRandomIgnitionIndices(randomIgnitionArea);
                            WUInity.RUNTIME_DATA.Fire.UpdateInitialIgnitionIndices(initialIgnitionIndices);
                            WUInity.RUNTIME_DATA.Fire.UpdateTriggerBufferIndices(triggerBufferIndices);
                            success = true;
                        }
                        else
                        {
                            CreateDefaultInputs();
                            WUInity.LOG(WUInity.LogType.Warning, "Graphical fire input file does not match current mesh, using default.");
                        }
                    }
                }
            }
            else
            {
                CreateDefaultInputs();
                WUInity.LOG(WUInity.LogType.Warning, "could not read GFI data, creating empty default.");
            }
        }

        private static void CreateDefaultInputs()
        {
            WUInity.RUNTIME_DATA.Fire.UpdateWUIArea(null);
            WUInity.RUNTIME_DATA.Fire.UpdateRandomIgnitionIndices(null);
            WUInity.RUNTIME_DATA.Fire.UpdateInitialIgnitionIndices(null);
            WUInity.RUNTIME_DATA.Fire.UpdateTriggerBufferIndices(null);
            SaveGraphicalFireInput();
        }
    }
}

