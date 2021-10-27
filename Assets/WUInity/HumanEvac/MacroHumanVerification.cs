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

            //P3, need more curves
            SaveWUI.LoadInput("evac_verification");
            wuinityOptions = WUInity.WUINITY_IN;
            evacOptions = WUInity.WUINITY_IN.evac;

            wuinityOptions.simName = "P3_1";
            evacOptions.overrideTotalPopulation = true;
            evacOptions.totalPopulation = 1000;
            WUInity.WUINITY_SIM.StartSimFromGUI();

            wuinityOptions.simName = "P3_2";
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 500f;
            WUInity.WUINITY_SIM.StartSimFromGUI();

            wuinityOptions.simName = "P3_3";
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 1000f;
            WUInity.WUINITY_SIM.StartSimFromGUI();

            //P4 - can't do it

            //PT1            
            evacOptions.overrideTotalPopulation = true;
            evacOptions.totalPopulation = 100;
            evacOptions.responseCurves[0].dataPoints[0].timeMinMax.y = 1f;
            for (int i = 0; i < 3; i++)
            {
                wuinityOptions.simName = "PT1_mod" + (i + 1);
                evacOptions.walkingDistanceModifier = (i + 1);
                WUInity.WUINITY_SIM.StartSimFromGUI();
            }            
        }
    }
}

