using System.IO;
using WUIEngine.IO;

namespace WUIEngine.Runtime
{
    public class PopulationData
    {
        public void LoadAll()
        {
            LoadPopulation(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Population.populationFile), false);
            LoadLocalGPW(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Population.localGPWFile), false);
            LoadGlobalGPWFolder(Engine.INPUT.Population.gpwDataFolder, false);
        }

        public bool LoadPopulation(string path, bool updateInputFile)
        {
            bool success = Engine.POPULATION.LoadPopulationFromFile(path, updateInputFile);
            Engine.DATA_STATUS.PopulationLoaded = success;
            if (success)
            {
                Engine.DATA_STATUS.PopulationCorrectedForRoutes = Engine.POPULATION.IsPopulationCorrectedForRoutes();

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
            bool success = Engine.POPULATION.LoadLocalGPWFromFile(path);
            Engine.DATA_STATUS.LocalGPWLoaded = success;
            if(success && updateInputFile)
            {
                Engine.INPUT.Population.localGPWFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadGlobalGPWFolder(string path, bool updateInputFile)
        {            
            bool success = Population.LocalGPWData.IsGPWAvailable(path);
            Engine.DATA_STATUS.GlobalGPWAvailable = success;
            if(success && updateInputFile)
            {
                Engine.INPUT.Population.gpwDataFolder = path;
                WUInityInput.SaveInput();
            }

            return success;
        }
    }
}
