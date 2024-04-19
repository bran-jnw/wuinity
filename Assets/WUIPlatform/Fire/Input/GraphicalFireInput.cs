//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;

namespace WUIPlatform
{
    public static class GraphicalFireInput
    {
        public static void SaveGraphicalFireInput()
        {
            string path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Simulation.SimulationID + ".gfi");
            WUIEngine.INPUT.Fire.graphicalFireInputFile = WUIEngine.INPUT.Simulation.SimulationID + ".gfi";

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    //always assume 30 meters for now
                    int xCount = (int)(0.5 + WUIEngine.INPUT.Simulation.Size.x / 30);
                    int yCount = (int)(0.5 + WUIEngine.INPUT.Simulation.Size.x / 30);
                    bw.Write(xCount);
                    bw.Write(yCount);
                    bw.Write(GetBytes(WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices));
                    bw.Write(GetBytes(WUIEngine.RUNTIME_DATA.Fire.RandomIgnitionIndices));
                    bw.Write(GetBytes(WUIEngine.RUNTIME_DATA.Fire.InitialIgnitionIndices));
                    bw.Write(GetBytes(WUIEngine.RUNTIME_DATA.Fire.TriggerBufferIndices));
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
            string path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.graphicalFireInputFile); //graphical fire input

            if(File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int ncols = br.ReadInt32();
                        int nrows = br.ReadInt32();

                        //always assume 30 meters for now
                        int xCount = (int)(0.5 + WUIEngine.INPUT.Simulation.Size.x / 30);
                        int yCount = (int)(0.5 + WUIEngine.INPUT.Simulation.Size.x / 30);

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

                            WUIEngine.RUNTIME_DATA.Fire.UpdateWUIArea(wuiAreaIndices, xCount, yCount);
                            WUIEngine.RUNTIME_DATA.Fire.UpdateRandomIgnitionIndices(randomIgnitionArea, xCount, yCount);
                            WUIEngine.RUNTIME_DATA.Fire.UpdateInitialIgnitionIndices(initialIgnitionIndices, xCount, yCount);
                            WUIEngine.RUNTIME_DATA.Fire.UpdateTriggerBufferIndices(triggerBufferIndices, xCount, yCount);
                            success = true;
                        }
                        else
                        {
                            WUIEngine.LOG(WUIEngine.LogType.Warning, "Graphical fire input file does not match current mesh, using default.");
                            CreateDefaultInputs();                           
                        }
                    }
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "could not read GFI data, creating empty default.");
                CreateDefaultInputs();                
            }
        }

        private static void CreateDefaultInputs()
        {
            //always assume 30 meters for now
            int xCount = (int)(0.5 + WUIEngine.INPUT.Simulation.Size.x / 30);
            int yCount = (int)(0.5 + WUIEngine.INPUT.Simulation.Size.x / 30);

            WUIEngine.RUNTIME_DATA.Fire.UpdateWUIArea(null, xCount, yCount);
            WUIEngine.RUNTIME_DATA.Fire.UpdateRandomIgnitionIndices(null, xCount, yCount);
            WUIEngine.RUNTIME_DATA.Fire.UpdateInitialIgnitionIndices(null, xCount, yCount);
            WUIEngine.RUNTIME_DATA.Fire.UpdateTriggerBufferIndices(null, xCount, yCount);
            SaveGraphicalFireInput();
        }
    }
}

