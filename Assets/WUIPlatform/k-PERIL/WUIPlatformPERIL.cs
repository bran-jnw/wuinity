using System.IO;

namespace WUIPlatform
{
    public static class WUIPlatformPERIL
    {
        private static kPERIL_DLL.kPERIL PERIL;
        private static int[,] compoundBoundary;
        private static int heatmapMax;
        private static Vector2int cellCount;

                
        /// <summary>
        /// Runs k-PERIL, should be called after a completed simulation
        /// </summary>
        /// <param name="midFlameWindspeed">User have to pick a representative mid flame wind speed as k-PERIL does not take changing weather into account</param>
        public static void RunPERIL(float midFlameWindspeed)
        {            
            if(PERIL == null)
            {
                PERIL = new kPERIL_DLL.kPERIL();
            }

            int xDim = WUIEngine.SIM.FireModule.GetCellCountX();
            int yDim = WUIEngine.SIM.FireModule.GetCellCountY();
            //assume cell/raster is square
            int cellSize = Mathf.RoundToInt(WUIEngine.SIM.FireModule.GetCellSizeX());
            compoundBoundary = new int[cellCount.x, cellCount.y];
            //get wuiarea from a user defined map painted in wuinity
            int[,] WUIarea = GetWUIArea();
            heatmapMax = 0;            

            int triggerBuffer = (int)WUIEngine.OUTPUT.totalEvacTime; // tbuffer - actual evac time, user specify desired extra buffer time in input file
            //collect ROS, how to send ROS data is ROStheta[X*Y,8], y first, x second, start in north and then clockwise
            float[,] maxROS = WUIEngine.SIM.FireModule.GetMaxROS();
            float[,] rosAzimuth = WUIEngine.SIM.FireModule.GetMaxROSAzimuth();
            int[,] wuiArea = GetWUIArea();

            using (StringWriter output = new StringWriter())
            {
                PERIL.CalculateBoundary(cellSize, triggerBuffer, midFlameWindspeed, wuiArea, maxROS, rosAzimuth, output);
                WUIEngine.LOG(WUIEngine.LogType.Log, "k-PERIL output:\n" + output.ToString());
            }        
        }

        private static int[,] GetWUIArea()
        {
            //first count how many cells we have to add to array
            int count = 0;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices.Length; i++)
            {
                int xIndex = i % WUIEngine.SIM.FireModule.GetCellCountX();
                int yIndex = i / WUIEngine.SIM.FireModule.GetCellCountY();
                if(WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices[i] == true)
                {
                    ++count;
                }
            }

            //then create array of correct size and fill it
            int[,] wuiArea = new int[2, count];
            int position = 0;
            for (int i = 0; i < count; i++)
            {
                int xIndex = i % WUIEngine.SIM.FireModule.GetCellCountX();
                int yIndex = i / WUIEngine.SIM.FireModule.GetCellCountY();
                if (WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices[i] == true)
                {
                    wuiArea[0, position] = xIndex;
                    wuiArea[1, position] = yIndex; //need to flip Y?
                    ++position;    
                }
            }
            return wuiArea;
        }
    }    
}