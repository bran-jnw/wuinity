using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.Runtime
{
    public class PopulationData
    {
        public void LoadAll()
        {
            LoadPopulation(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Population.populationFile), false);
            LoadLocalGPW(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Population.localGPWFile), false);
            LoadGlobalGPWFolder(WUIEngine.INPUT.Population.gpwDataFolder, false);
        }

        public bool LoadPopulation(string path, bool updateInputFile)
        {
            bool success = WUIEngine.POPULATION.LoadPopulationFromFile(path, updateInputFile);
            WUIEngine.DATA_STATUS.PopulationLoaded = success;
            if (success)
            {
                WUIEngine.DATA_STATUS.PopulationCorrectedForRoutes = WUIEngine.POPULATION.IsPopulationCorrectedForRoutes();

                //this is now done in actual call
                /*if (updateInputFile)
                {
                    Input.Population.populationFile = Path.GetFileName(path);
                    WUInityInput.SaveInput();
                }*/
            }            

            return success;
        }

        public bool LoadLocalGPW(string path, bool updateInputFile)
        {
            bool success = WUIEngine.POPULATION.LoadLocalGPWFromFile(path);
            WUIEngine.DATA_STATUS.LocalGPWLoaded = success;
            if(success && updateInputFile)
            {
                WUIEngine.INPUT.Population.localGPWFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadGlobalGPWFolder(string path, bool updateInputFile)
        {            
            bool success = Population.LocalGPWData.IsGPWAvailable(path);
            WUIEngine.DATA_STATUS.GlobalGPWAvailable = success;
            if(success && updateInputFile)
            {
                WUIEngine.INPUT.Population.gpwDataFolder = path;
                WUIEngineInput.SaveInput();
            }

            return success;
        }
    }
}
