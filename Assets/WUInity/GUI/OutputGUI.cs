using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string outputTime;
        float sliderVtraffic = 1f;
        void OutputMenu()
        {
            int buttonColumnStart = 140;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int dummy = (int)WUInity.OUTPUT.totalEvacTime;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evac time: " + dummy + " s");
            ++buttonIndex;

            dummy = WUInity.POPULATION.GetTotalPopulation();
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + dummy);
            ++buttonIndex;

            dummy = WUInity.SIM.MacroHumanSim().GetPeopleStaying();
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "People staying: " + dummy);
            ++buttonIndex;

            //toatl cars
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total cars: " + WUInity.SIM.MacroHumanSim().GetTotalCars());
            ++buttonIndex;

            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Display options");
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Households"))
            {
                WUInity.INSTANCE.ToggleHouseholdRendering();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic"))
            {
                WUInity.INSTANCE.ToggleTrafficRendering();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fire spread"))
            {
                WUInity.INSTANCE.ToggleFireSpreadRendering();
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density"))
            {
                WUInity.INSTANCE.ToggleSootRendering();
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
            }
            ++buttonIndex;

            float timeRange = WUInity.OUTPUT.totalEvacTime - WUInity.SIM.StartTime;
            float time = sliderVtraffic * timeRange + WUInity.SIM.StartTime;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic density"))
            {
                WUInity.INSTANCE.DisplayClosestDensityData(time);
                WUInity.INSTANCE.ToggleEvacDataPlane();
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.TrafficDens);
            }
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic density: " + (int)time + " seconds");
            ++buttonIndex;
            sliderVtraffic = GUI.HorizontalSlider(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sliderVtraffic, 0.0f, 1.0f);
            ++buttonIndex;
            if (WUInity.INSTANCE.dataSampleMode == WUInity.DataSampleMode.TrafficDens)
            {
                WUInity.INSTANCE.DisplayClosestDensityData(time);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Hide visual data"))
            {
                WUInity.INSTANCE.SetEvacDataPlane(false);
                WUInity.INSTANCE.SetFireDataPlane(false);
            }
            ++buttonIndex;

            if (WUInity.INPUT.runEvacSim && WUInity.SIM.MacroHumanSim() != null)
            {
                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Pedestrians left: " + WUInity.SIM.MacroHumanSim().GetPeopleLeft() + " / " + WUInity.POPULATION.GetTotalPopulation());
                ++buttonIndex;

                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars reached: " + WUInity.SIM.MacroHumanSim().GetCarsReached());
                ++buttonIndex;
            }

            //cars still left
            if (WUInity.INPUT.runTrafficSim && WUInity.SIM.MacroTrafficSim() != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars left: " + WUInity.SIM.MacroTrafficSim().GetCarsInSystem() + " / " + WUInity.SIM.MacroTrafficSim().GetTotalCarsSimulated());
                ++buttonIndex;
            }

            int totalEvacuated = 0;
            for (int i = 0; i < WUInity.SIM_DATA.EvacuationGoals.Length; i++)
            {
                totalEvacuated += WUInity.SIM_DATA.EvacuationGoals[i].currentPeople;
                string name = WUInity.SIM_DATA.EvacuationGoals[i].name;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), name + ": " + WUInity.SIM_DATA.EvacuationGoals[i].currentPeople + " (" + WUInity.SIM_DATA.EvacuationGoals[i].cars.Count + ")");
                ++buttonIndex;
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evacuated: " + totalEvacuated + " / " + (WUInity.POPULATION.GetTotalPopulation() - WUInity.SIM.MacroHumanSim().GetPeopleStaying()));
            ++buttonIndex;

            //fire output stuff
            if (WUInity.INPUT.runFireSim)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind speed: " + WUInity.SIM.GetFireWindSpeed() + " m/s");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind direction: " + WUInity.SIM.GetFireWindDirection() + " degrees");
                ++buttonIndex;
            }

            if(WUInity.SIM.IsRunning)
            {
                ++buttonIndex;

                string pauseState = "Simulation running";
                string pauseButton = "Pause simulation";
                if (WUInity.INSTANCE.IsPaused())
                {
                    pauseState = "Simulation paused";
                    pauseButton = "Cont. simulation";
                }

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseState);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseButton))
                {
                    WUInity.INSTANCE.TogglePause();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop simulation"))
                {                    
                    WUInity.INSTANCE.StopSimulation();
                }
                ++buttonIndex;

                LegendGUI();
            }            
        }

        void LegendGUI()
        {
            if (WUInity.INPUT.runFireSim)
            {
                GUI.BeginGroup(new Rect(Screen.width - 125, Screen.height * 0.5f - 305, 120, 300));

                GUI.Box(new Rect(0, 0, 120, 300), "Fireline int.");
                GUI.DrawTexture(new Rect(40, 50, 40, 200), verticalColorGradient);
                string upperLimit = WUInity.FIRE_VISUALS.GetUpperFirelineIntensityLimit().ToString("f1") + " [kW/m]";
                GUI.Label(new Rect(0, 20, 120, 20), upperLimit, styleAlignedCenter);
                string lowerLimit = WUInity.FIRE_VISUALS.GetLowerFirelineIntensityLimit().ToString("f1") + " [kW/m]";
                GUI.Label(new Rect(0, 260, 120, 20), lowerLimit, styleAlignedCenter);

                GUI.EndGroup();
            }

            if (WUInity.INPUT.runSmokeSim)
            {
                GUI.BeginGroup(new Rect(Screen.width - 125, Screen.height * 0.5f + 5, 120, 300));

                GUI.Box(new Rect(0, 0, 120, 300), "Optical dens.");
                GUI.DrawTexture(new Rect(40, 50, 40, 200), verticalColorGradient);
                string upperLimit = WUInity.FIRE_VISUALS.GetUpperOpticalDensityLimit().ToString("e3") + " [-/m]";
                GUI.Label(new Rect(0, 20, 120, 20), upperLimit, styleAlignedCenter);
                string lowerLimit = WUInity.FIRE_VISUALS.GetLowerOpticalDensityLimit().ToString("e3") + " [-/m]";
                GUI.Label(new Rect(0, 260, 120, 20), lowerLimit, styleAlignedCenter);

                GUI.EndGroup();
            }            
        }
    }
}
