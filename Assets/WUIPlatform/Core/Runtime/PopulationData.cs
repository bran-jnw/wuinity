//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
