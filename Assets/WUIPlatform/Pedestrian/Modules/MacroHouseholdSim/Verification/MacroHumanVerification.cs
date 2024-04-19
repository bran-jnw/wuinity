//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.IO;

namespace WUIPlatform.Pedestrian
{
    public static class MacroHumanVerification
    {
        public static void RunVerification()
        {
            WUIEngineInput.LoadInput("evac_verification");
            EvacuationInput evacOptions = WUIEngine.INPUT.Evacuation;
            WUIEngineInput wuinityOptions = WUIEngine.INPUT;
            WUIEngine.INPUT.Simulation.RunPedestrianModule = true;
            WUIEngine.INPUT.Simulation.RunTrafficModule = false;
            WUIEngine.INPUT.Simulation.RunFireModule = false;
            WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations = true;

            //WUInity.INSTANCE.LoadGPW(); //TODO: fix this input 
            //WUIEngine.SIM_DATA.LoadRouterDatabase();

            //P1
            wuinityOptions.Simulation.SimulationID = "P1";
            //evacOptions.overrideTotalPopulation = true;
            //evacOptions.totalPopulation = 1000;
            //WUIEngine.SIM.Start();

            //P2
            for (int i = 1; i < 6; i++)
            {
                //evacOptions.totalPopulation = 1000 * i;
                evacOptions.allowMoreThanOneCar = true;
                evacOptions.minHouseholdSize = i;
                evacOptions.maxHouseholdSize = i;
                evacOptions.maxCars = i;
                evacOptions.maxCarsChance = 1f;
                wuinityOptions.Simulation.SimulationID = "P2_" + i;
                //WUIEngine.SIM.Start();
            }

            //P3, need more curves
            WUIEngineInput.LoadInput("evac_verification");
            wuinityOptions = WUIEngine.INPUT;
            evacOptions = WUIEngine.INPUT.Evacuation;

            wuinityOptions.Simulation.SimulationID = "P3_1";
            //evacOptions.overrideTotalPopulation = true;
            //evacOptions.totalPopulation = 1000;
            //WUIEngine.SIM.Start();

            //TODO_broken after chnage of response curve input
            /*wuinityOptions.simName = "P3_2";
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 500f;
            WUIEngine.SIM.StartSimulation();

            wuinityOptions.simName = "P3_3";
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 1000f;
            WUIEngine.SIM.StartSimulation();

            //P4 - can't do it

            //PT1            
            //evacOptions.overrideTotalPopulation = true;
            //evacOptions.totalPopulation = 1000;
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 1f;
            for (int i = 0; i < 3; i++)
            {
                wuinityOptions.simName = "PT1_mod" + (i + 1);
                evacOptions.walkingDistanceModifier = (i + 1);
                WUIEngine.SIM.StartSimulation();
            }        */    
        }
    }
}

