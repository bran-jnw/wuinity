using WUIEngine.IO;

namespace WUIEngine.Pedestrian
{
    public static class MacroHumanVerification
    {
        public static void RunVerification()
        {
            WUInityInput.LoadInput("evac_verification");
            EvacuationInput evacOptions = Engine.INPUT.Evacuation;
            WUInityInput wuinityOptions = Engine.INPUT;
            Engine.INPUT.Simulation.RunPedestrianModule = true;
            Engine.INPUT.Simulation.RunTrafficModule = false;
            Engine.INPUT.Simulation.RunFireModule = false;
            Engine.RUNTIME_DATA.Simulation.MultipleSimulations = true;

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
            WUInityInput.LoadInput("evac_verification");
            wuinityOptions = Engine.INPUT;
            evacOptions = Engine.INPUT.Evacuation;

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

