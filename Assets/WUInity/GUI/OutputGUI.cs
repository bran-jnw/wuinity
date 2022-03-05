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

            dummy = WUInity.SIM.GetMacroHumanSim().GetPeopleStaying();
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "People staying: " + dummy);
            ++buttonIndex;

            //toatl cars
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total cars: " + WUInity.SIM.GetMacroHumanSim().GetTotalCars());
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Smoke dispersion"))
            {
                WUInity.INSTANCE.DisplaySmokeDispersion();
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Staying population"))
            {
                WUInity.INSTANCE.DisplayStayingPop();
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.Staying);
            }
            ++buttonIndex;

            float timeRange = WUInity.OUTPUT.totalEvacTime - WUInity.SIM.StartTime;
            float time = sliderVtraffic * timeRange + WUInity.SIM.StartTime;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic density"))
            {
                WUInity.INSTANCE.DisplayClosestDensityData(time);
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

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/Hide data"))
            {
                WUInity.INSTANCE.ToggleEvacDataPlane();
            }
            ++buttonIndex;

            if (WUInity.INPUT.runEvacSim && WUInity.SIM.GetMacroHumanSim() != null)
            {
                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Pedestrians left: " + WUInity.SIM.GetMacroHumanSim().GetPeopleLeft() + " / " + WUInity.POPULATION.GetTotalPopulation());
                ++buttonIndex;

                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars reached: " + WUInity.SIM.GetMacroHumanSim().GetCarsReached());
                ++buttonIndex;
            }

            //cars still left
            if (WUInity.INPUT.runTrafficSim && WUInity.SIM.GetMacroTrafficSim() != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars left: " + WUInity.SIM.GetMacroTrafficSim().GetCarsInSystem() + " / " + WUInity.SIM.GetMacroTrafficSim().GetTotalCarsSimulated());
                ++buttonIndex;
            }

            int totalEvacuated = 0;
            for (int i = 0; i < WUInity.INPUT.traffic.evacuationGoals.Length; i++)
            {
                totalEvacuated += WUInity.INPUT.traffic.evacuationGoals[i].currentPeople;
                string name = WUInity.INPUT.traffic.evacuationGoals[i].name;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), name + ": " + WUInity.INPUT.traffic.evacuationGoals[i].currentPeople + " (" + WUInity.INPUT.traffic.evacuationGoals[i].cars.Count + ")");
                ++buttonIndex;
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evacuated: " + totalEvacuated + " / " + (WUInity.POPULATION.GetTotalPopulation() - WUInity.SIM.GetMacroHumanSim().GetPeopleStaying()));
            ++buttonIndex;

            //fire output stuff
            if (WUInity.INPUT.runFireSim)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind speed: " + WUInity.SIM.GetFireWindSpeed() + " m/s");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind direction: " + WUInity.SIM.GetFireWindDirection() + " degrees");
                ++buttonIndex;
            }

            if(WUInity.SIM.isRunning)
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
                    WUInity.SIM.StopSim("STOP: Stopped simulation as requested by user.");
                }
                ++buttonIndex;
            }            
        }
    }
}
