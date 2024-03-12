﻿namespace WUInity.Population
{
    public class PopulationManager
    {
        protected LocalGPWData localGPWData;
        protected PopulationData populationData;

        protected PopulationVisualizer _visualizer;
        public PopulationVisualizer Visualizer { get { return _visualizer; } }


        public PopulationManager()
        {
            localGPWData = new LocalGPWData(this);
            populationData = new PopulationData(this);

            #if USING_UNITY
            _visualizer = new PopulationVisualizerUnity(this);
            #else

            #endif
        }

        public void CreateTexture()
        {
            _visualizer.CreateTexture();
        }

        public void CreateGPWTexture()
        {
            _visualizer.CreateGPWTexture();
        }

        public bool IsPopulationLoaded()
        {
            return populationData.isLoaded;
        }

        public bool IsPopulationCorrectedForRoutes()
        {
            return populationData.correctedForRoutes;
        }

        public PopulationData GetPopulationData()
        {
            return populationData;
        }

        public LocalGPWData GetLocalGPWData()
        {
            return localGPWData;
        }

        public bool[] GetPopulationMask()
        {
            return populationData.populationMask;
        }

        public void PlaceUniformPopulation(int newTotalPopulation)
        {
            populationData.PlaceUniformPopulation(newTotalPopulation);
        }

        public bool IsLocalGPWLoaded()
        {
            return localGPWData.isLoaded;
        }

        /*public PopulationData GetPopulationData()
        {
            return populationData;
        }*/

        /*public LocalGPWData GetLocalGPWData()
        {
            return localGPWData;
        }*/
        

        public int GetTotalPopulation()
        {
            return populationData.totalPopulation;
        }

        public int GetTotalActiveCells()
        {
            return populationData.totalActiveCells;
        }

        public int GetPopulation(int x, int y)
        {
            return populationData.cellPopulation[x + y * populationData.cells.x];
        }

        /// <summary>
        /// Get number of people in cell based on "world space" coordinates. Clamps to dimensions of defined area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPopulationUnitySpace(double x, double y)
        {
            int xInt = (int)((x / WUInity.INPUT.Simulation.Size.x) * populationData.cells.x);
            int yInt = (int)((y / WUInity.INPUT.Simulation.Size.y) * populationData.cells.y);
            return GetPopulation(xInt, yInt);
        }

        public int GetLocalGPWTotalPopulation()
        {
            return localGPWData.totalPopulation;
        }        

        public bool LoadPopulationFromFile(string file, bool updateInput)
        {
            return populationData.LoadPopulationFromFile(file, updateInput);
        }

        public bool LoadLocalGPWFromFile(string file)
        {
            return localGPWData.LoadLocalGPWDataFromFile(file);
        }

        public bool CreatePopulationFromLocalGPW(string file)
        {            
            bool success = localGPWData.LoadLocalGPWDataFromFile(file);

            if(success)
            {
                _visualizer.SetDataPlane(false);
                populationData.CreatePopulationFromLocalGPW(localGPWData);
            }            

            return success;
        }

        public bool CreateLocalGPW()
        {
            return localGPWData.CreateLocalGPWData();
        }

        public void UpdatePopulationBasedOnRoutes(RouteCollection[] cellRoutes)
        {
            populationData.UpdatePopulationBasedOnRoutes(cellRoutes);
        }

        public void ScaleTotalPopulation(int newTotal)
        {
            populationData.ScaleTotalPopulation(newTotal);
        }        

        //colors from GPW website
        static WUInityColor c0 = new WUInityColor(190f / 255f, 232f / 255f, 255f / 255f);
        static WUInityColor c1 = new WUInityColor(1.0f, 241f / 255f, 208f / 255f);
        static WUInityColor c2 = new WUInityColor(1.0f, 218f / 255f, 165f / 255f);
        static WUInityColor c3 = new WUInityColor(252f / 255f, 183f / 255f, 82f / 255f);
        static WUInityColor c4 = new WUInityColor(1.0f, 137f / 255f, 63f / 255f);
        static WUInityColor c5 = new WUInityColor(238f / 255f, 60f / 255f, 30f / 255f);
        static WUInityColor c6 = new WUInityColor(191f / 255f, 1f / 255f, 39f / 255f);

        public static WUInityColor GetGPWColor(float density)
        {
            WUInityColor color;
            if (density < 0.0f)
            {
                color = c0;
            }
            else if (density < 1.0f)
            {
                color = c1;
            }
            else if (density <= 5.0f)
            {
                color = c2;
            }
            else if (density <= 25.0f)
            {
                color = c3;
            }
            else if (density <= 250.0f)
            {
                color = c4;
            }
            else if (density <= 1000.0f)
            {
                color = c5;
            }
            else
            {
                color = c6;
            }
            color.a = 0.7f;
            return color;
        }
    }
}