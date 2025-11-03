//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;

namespace PREACT
{
    public static class GraphicalFireInput
    {
        public static void SaveGraphicalFireInput()
        {
            string path = Path.Combine(Engine.WorkingFolder, Engine.Input.Simulation.Id + ".gfi");
            //WUIEngine.INPUT.Fire.GraphicalFireInputFile = WUIEngine.INPUT.Simulation.Id + ".gfi";

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    int xCount = Engine.ScenarioData.Fire.LCPData.GetCellCountX();
                    int yCount = Engine.ScenarioData.Fire.LCPData.GetCellCountY();
                    bw.Write(xCount);
                    bw.Write(yCount);
                    bw.Write(GetBytes(Engine.ScenarioData.Fire.WuiArea));
                    bw.Write(GetBytes(Engine.ScenarioData.Fire.RandomIgnition));
                    bw.Write(GetBytes(Engine.ScenarioData.Fire.InitialIgnition));
                    bw.Write(GetBytes(Engine.ScenarioData.Fire.ManualTriggerBuffer));
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
            string path = Path.Combine(Engine.WorkingFolder, Engine.Input.Fire.GraphicalFireInputFile); //graphical fire input

            if(Engine.ScenarioData.Fire.LCPData == null)
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No LCP data has been loaded, can't try and look for GFI data.");
                return;
            }

            if(File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int ncols = br.ReadInt32();
                        int nrows = br.ReadInt32();
                        if(ncols == Engine.ScenarioData.Fire.LCPData.GetCellCountX() && nrows == Engine.ScenarioData.Fire.LCPData.GetCellCountY())
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

                            Engine.ScenarioData.Fire.UpdateWUIArea(wuiAreaIndices, ncols, nrows);
                            Engine.ScenarioData.Fire.UpdateRandomIgnitionIndices(randomIgnitionArea, ncols, nrows);
                            Engine.ScenarioData.Fire.UpdateInitialIgnitionIndices(initialIgnitionIndices, ncols, nrows);
                            Engine.ScenarioData.Fire.UpdateTriggerBufferIndices(triggerBufferIndices, ncols, nrows);
                            success = true;
                        }
                        else
                        {
                            Engine.MESSAGE(null, Engine.LogType.Warning, "Could read GFI data but there was a mismatch with the LCP file colums/rows, creating empty default.");
                            br.Close();
                            CreateDefaultInputs();
                        }
                    }
                }
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Could not find GFI data, creating empty default.");
                CreateDefaultInputs();                
            }
        }

        private static void CreateDefaultInputs()
        {
            //LCP file has already been read, use that for dimensions
            int xCount = Engine.ScenarioData.Fire.LCPData.GetCellCountX();
            int yCount = Engine.ScenarioData.Fire.LCPData.GetCellCountY();

            Engine.ScenarioData.Fire.UpdateWUIArea(null, xCount, yCount);
            Engine.ScenarioData.Fire.UpdateRandomIgnitionIndices(null, xCount, yCount);
            Engine.ScenarioData.Fire.UpdateInitialIgnitionIndices(null, xCount, yCount);
            Engine.ScenarioData.Fire.UpdateTriggerBufferIndices(null, xCount, yCount);
            SaveGraphicalFireInput();
        }
    }
}

