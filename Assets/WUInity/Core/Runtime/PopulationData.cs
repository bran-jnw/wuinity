using System.IO;
using static WUInity.WUInity;

namespace WUInity.Runtime
{
    public class PopulationData
    {
        public void LoadAll()
        {
            LoadPopulation(Path.Combine(WORKING_FOLDER, INPUT.Population.populationFile), false);
            LoadLocalGPW(Path.Combine(WORKING_FOLDER, INPUT.Population.localGPWFile), false);
            LoadGlobalGPWFolder(INPUT.Population.gpwDataFolder, false);
        }

        public bool LoadPopulation(string path, bool updateInputFile)
        {
            bool success = POPULATION.LoadPopulationFromFile(path, updateInputFile);
            DATA_STATUS.PopulationLoaded = success;
            if (success)
            {
                DATA_STATUS.PopulationCorrectedForRoutes = POPULATION.IsPopulationCorrectedForRoutes();

                //this is now done in actual call
                /*if (updateInputFile)
                {
                    INPUT.Population.populationFile = Path.GetFileName(path);
                    WUInityInput.SaveInput();
                }*/
            }            

            return success;
        }

        public bool LoadLocalGPW(string path, bool updateInputFile)
        {
            bool success = POPULATION.LoadLocalGPWFromFile(path);
            DATA_STATUS.LocalGPWLoaded = success;
            if(success && updateInputFile)
            {
                INPUT.Population.localGPWFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadGlobalGPWFolder(string path, bool updateInputFile)
        {            
            bool success = Population.LocalGPWData.IsGPWAvailable(path);
            DATA_STATUS.GlobalGPWAvailable = success;
            if(success && updateInputFile)
            {
                INPUT.Population.gpwDataFolder = path;
                WUInityInput.SaveInput();
            }

            return success;
        }
    }
}
