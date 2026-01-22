//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
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
        public static void SaveGraphicalFireInput(string filePath, IO.WildfireData fireData)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    int xCount = fireData.LCPData.GetCellCountX();
                    int yCount = fireData.LCPData.GetCellCountY();
                    bw.Write(xCount);
                    bw.Write(yCount);
                    bw.Write(GetBytes(fireData.WuiArea));
                    bw.Write(GetBytes(fireData.RandomIgnition));
                    bw.Write(GetBytes(fireData.InitialIgnition));
                    bw.Write(GetBytes(fireData.ManualTriggerBuffer));
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

        public static void LoadGraphicalFireInput(string file, Wildfire.LandscapeData lcpData, out bool[] wuiArea, out bool[] randomIgnitionArea, out bool[] initialIgnitionIndices, out bool[] triggerBufferIndices, out bool success)
        {
            success = false;

            if(File.Exists(file))
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int ncols = br.ReadInt32();
                        int nrows = br.ReadInt32();
                        if(ncols == lcpData.GetCellCountX() && nrows == lcpData.GetCellCountY())
                        {
                            int dataSize = ncols * nrows;

                            byte[] b = br.ReadBytes(dataSize * sizeof(bool));
                            wuiArea = GetBools(b, dataSize);

                            b = br.ReadBytes(dataSize * sizeof(bool));
                            randomIgnitionArea = GetBools(b, dataSize);

                            b = br.ReadBytes(dataSize * sizeof(bool));
                            initialIgnitionIndices = GetBools(b, dataSize);

                            b = br.ReadBytes(dataSize * sizeof(bool));
                            triggerBufferIndices = GetBools(b, dataSize);

                            success = true;
                        }
                        else
                        {
                            Engine.Message(null, Engine.LogType.Warning, "Could read GFI data but there was a mismatch with the LCP file colums/rows, creating empty default.");
                            CreateDefault(lcpData, out wuiArea, out randomIgnitionArea, out initialIgnitionIndices, out triggerBufferIndices);
                        }
                    }
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Could not find GFI data, creating empty default.");
                CreateDefault(lcpData, out wuiArea, out randomIgnitionArea, out initialIgnitionIndices, out triggerBufferIndices);
            }
        }

        private static void CreateDefault(Wildfire.LandscapeData lcpData, out bool[] wuiArea, out bool[] randomIgnitionArea, out bool[] initialIgnitionIndices, out bool[] triggerBufferIndices)
        {
            //LCP file has already been read, use that for dimensions
            int xDim = lcpData.GetCellCountX();
            int yDim = lcpData.GetCellCountY();
            wuiArea = new bool[xDim * yDim];
            randomIgnitionArea = new bool[xDim * yDim];
            initialIgnitionIndices = new bool[xDim * yDim];
            triggerBufferIndices = new bool[xDim * yDim];
        }
    }
}

