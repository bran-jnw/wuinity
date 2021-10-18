using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Evac
{
    public static class MacroHumanVerification
    {
        public static void RunVerification()
        {
            SaveWUI.LoadInput("evac_verification");
            EvacInput evacOptions = WUInity.WUINITY_IN.evac;
            WUInityInput wuinityOptions = WUInity.WUINITY_IN;
            WUInity.WUINITY_SIM.runEvacSim = true;
            WUInity.WUINITY_SIM.runTrafficSim = false;
            WUInity.WUINITY_SIM.runFireSim = false;
            wuinityOptions.runInRealTime = false;

            WUInity.WUINITY.LoadGPW();
            WUInity.WUINITY_SIM.LoadItineroDatabase();

            //P1
            wuinityOptions.simName = "P1";
            evacOptions.overrideTotalPopulation = true;
            evacOptions.totalPopulation = 1000;
            WUInity.WUINITY_SIM.StartSimFromGUI();

            //P2
            for (int i = 1; i < 6; i++)
            {
                evacOptions.totalPopulation = 1000 * i;
                evacOptions.allowMoreThanOneCar = true;
                evacOptions.minHouseholdSize = i;
                evacOptions.maxHouseholdSize = i;
                evacOptions.maxCars = i;
                evacOptions.maxCarsChance = 1f;
                wuinityOptions.simName = "P2_" + i;
                WUInity.WUINITY_SIM.StartSimFromGUI();
            }

            
        }
    }
}

