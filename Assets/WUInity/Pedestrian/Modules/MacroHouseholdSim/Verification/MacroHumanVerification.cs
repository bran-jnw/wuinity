namespace WUInity.Pedestrian
{
    public static class MacroHumanVerification
    {
        public static void RunVerification()
        {
            WUInityInput.LoadInput("evac_verification");
            EvacuationInput evacOptions = WUInity.INPUT.Evacuation;
            WUInityInput wuinityOptions = WUInity.INPUT;
            WUInity.INPUT.Simulation.RunPedestrianModule = true;
            WUInity.INPUT.Simulation.RunTrafficModule = false;
            WUInity.INPUT.Simulation.RunFireModule = false;
            WUInity.RUNTIME_DATA.Simulation.MultipleSimulations = true;

            //WUInity.INSTANCE.LoadGPW(); //TODO: fix this input 
            //WUInity.SIM_DATA.LoadRouterDatabase();

            //P1
            wuinityOptions.Simulation.SimulationID = "P1";
            //evacOptions.overrideTotalPopulation = true;
            //evacOptions.totalPopulation = 1000;
            WUInity.SIM.Start();

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
                WUInity.SIM.Start();
            }

            //P3, need more curves
            WUInityInput.LoadInput("evac_verification");
            wuinityOptions = WUInity.INPUT;
            evacOptions = WUInity.INPUT.Evacuation;

            wuinityOptions.Simulation.SimulationID = "P3_1";
            //evacOptions.overrideTotalPopulation = true;
            //evacOptions.totalPopulation = 1000;
            WUInity.SIM.Start();

            //TODO_broken after chnage of response curve input
            /*wuinityOptions.simName = "P3_2";
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 500f;
            WUInity.SIM.StartSimulation();

            wuinityOptions.simName = "P3_3";
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 1000f;
            WUInity.SIM.StartSimulation();

            //P4 - can't do it

            //PT1            
            //evacOptions.overrideTotalPopulation = true;
            //evacOptions.totalPopulation = 1000;
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 1f;
            for (int i = 0; i < 3; i++)
            {
                wuinityOptions.simName = "PT1_mod" + (i + 1);
                evacOptions.walkingDistanceModifier = (i + 1);
                WUInity.SIM.StartSimulation();
            }        */    
        }
    }
}

