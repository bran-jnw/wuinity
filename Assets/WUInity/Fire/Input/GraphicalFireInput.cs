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
                    //always assume 30 meters for now
                    int xCount = (int)(0.5 + WUInity.INPUT.Simulation.Size.x / 30);
                    int yCount = (int)(0.5 + WUInity.INPUT.Simulation.Size.x / 30);
                    bw.Write(xCount);
                    bw.Write(yCount);
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

                        //always assume 30 meters for now
                        int xCount = (int)(0.5 + WUInity.INPUT.Simulation.Size.x / 30);
                        int yCount = (int)(0.5 + WUInity.INPUT.Simulation.Size.x / 30);

                        if (ncols == xCount && nrows == yCount)
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

                            WUInity.RUNTIME_DATA.Fire.UpdateWUIArea(wuiAreaIndices, xCount, yCount);
                            WUInity.RUNTIME_DATA.Fire.UpdateRandomIgnitionIndices(randomIgnitionArea, xCount, yCount);
                            WUInity.RUNTIME_DATA.Fire.UpdateInitialIgnitionIndices(initialIgnitionIndices, xCount, yCount);
                            WUInity.RUNTIME_DATA.Fire.UpdateTriggerBufferIndices(triggerBufferIndices, xCount, yCount);
                            success = true;
                        }
                        else
                        {
                            WUInity.LOG(WUInity.LogType.Warning, "Graphical fire input file does not match current mesh, using default.");
                            CreateDefaultInputs();                           
                        }
                    }
                }
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Warning, "could not read GFI data, creating empty default.");
                CreateDefaultInputs();                
            }
        }

        private static void CreateDefaultInputs()
        {
            //always assume 30 meters for now
            int xCount = (int)(0.5 + WUInity.INPUT.Simulation.Size.x / 30);
            int yCount = (int)(0.5 + WUInity.INPUT.Simulation.Size.x / 30);

            WUInity.RUNTIME_DATA.Fire.UpdateWUIArea(null, xCount, yCount);
            WUInity.RUNTIME_DATA.Fire.UpdateRandomIgnitionIndices(null, xCount, yCount);
            WUInity.RUNTIME_DATA.Fire.UpdateInitialIgnitionIndices(null, xCount, yCount);
            WUInity.RUNTIME_DATA.Fire.UpdateTriggerBufferIndices(null, xCount, yCount);
            SaveGraphicalFireInput();
        }
    }
}

