using System.Globalization;
using UnityEngine;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string outputTime;
        float sliderVtraffic = 1f;
        bool displayArrivalPlot = false;

        void OutputMenu()
        {
            if(WUIEngine.SIM.State == Simulation.SimulationState.Error || WUIEngine.SIM.State == Simulation.SimulationState.Initializing)
            {
                return;
            }

            int buttonColumnStart = 140;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int dummy = (int)WUIEngine.OUTPUT.totalEvacTime;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evac time: " + dummy + " s");
            ++buttonIndex;

            dummy = WUIEngine.POPULATION.GetTotalPopulation();
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + dummy);
            ++buttonIndex;

            if(WUIEngine.SIM.PedestrianModule != null)
            {
                dummy = WUIEngine.SIM.PedestrianModule.GetPeopleStaying();
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "People staying: " + dummy);
                ++buttonIndex;

                //toatl cars
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total cars: " + WUIEngine.SIM.PedestrianModule.GetTotalCars());
                ++buttonIndex;
            }         

            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Display options");
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Households"))
            {
                WUInityEngine.INSTANCE.ToggleHouseholdRendering();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic"))
            {
                WUInityEngine.INSTANCE.ToggleTrafficRendering();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fire spread"))
            {
                WUInityEngine.INSTANCE.ToggleFireSpreadRendering();
                WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density"))
            {
                WUInityEngine.INSTANCE.ToggleSootRendering();
                WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
            }
            ++buttonIndex;

            float timeRange = WUIEngine.OUTPUT.totalEvacTime - WUIEngine.SIM.StartTime;
            float time = sliderVtraffic * timeRange + WUIEngine.SIM.StartTime;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic density"))
            {
                WUInityEngine.INSTANCE.DisplayClosestDensityData(time);
                WUInityEngine.INSTANCE.ToggleEvacDataPlane();
                WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.TrafficDens);
            }
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic density: " + (int)time + " seconds");
            ++buttonIndex;
            sliderVtraffic = GUI.HorizontalSlider(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sliderVtraffic, 0.0f, 1.0f);
            ++buttonIndex;
            if (WUInityEngine.INSTANCE.dataSampleMode == WUInityEngine.DataSampleMode.TrafficDens)
            {
                //WUInity.INSTANCE.DisplayClosestDensityData(time);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Hide visual data"))
            {
                WUInityEngine.INSTANCE.SetEvacDataPlane(false);
                WUInityEngine.INSTANCE.SetFireDataPlane(false);
            }
            ++buttonIndex;

            if (WUIEngine.INPUT.Simulation.RunPedestrianModule && WUIEngine.SIM.PedestrianModule != null)
            {
                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Pedestrians left: " + WUIEngine.SIM.PedestrianModule.GetPeopleLeft() + " / " + WUIEngine.POPULATION.GetTotalPopulation());
                ++buttonIndex;

                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars reached: " + WUIEngine.SIM.PedestrianModule.GetCarsReached());
                ++buttonIndex;
            }

            //cars still left
            if (WUIEngine.INPUT.Simulation.RunTrafficModule && WUIEngine.SIM.TrafficModule != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars left: " + WUIEngine.SIM.TrafficModule.GetCarsInSystem() + " / " + WUIEngine.SIM.TrafficModule.GetTotalCarsSimulated());
                ++buttonIndex;
            }

            if(WUIEngine.INPUT.Simulation.RunPedestrianModule)
            {
                for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
                {
                    string name = WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name;
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), name + ": " + WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentPeople + " (" + WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].cars.Count + ")");
                    ++buttonIndex;
                }
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evacuated: " + WUIEngine.RUNTIME_DATA.Evacuation.GetTotalEvacuated() + " / " + (WUIEngine.POPULATION.GetTotalPopulation() - WUIEngine.SIM.PedestrianModule.GetPeopleStaying()));
                ++buttonIndex;
            }            

            //fire output stuff
            if (WUIEngine.INPUT.Simulation.RunFireModule && WUIEngine.SIM.State == Simulation.SimulationState.Running)
            {               
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind speed: " + WUIEngine.SIM.FireModule.GetCurrentWindData().speed + " m/s");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind direction: " + WUIEngine.SIM.FireModule.GetCurrentWindData().direction + " degrees");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Active cells (FireMesh): " + WUIEngine.SIM.FireModule.GetActiveCellCount());
                ++buttonIndex;

                //fire visual mode
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fire display mode");
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fireline intensity"))
                {
                    WUInityEngine.FIRE_VISUALS.SetFireDisplayMode(Visualization.FireRenderer.FireDisplayMode.FirelineIntensity);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    WUInityEngine.FIRE_VISUALS.SetFireDisplayMode(Visualization.FireRenderer.FireDisplayMode.FuelModelNumber);
                }
                ++buttonIndex;
            }

            if(WUIEngine.SIM.State == Simulation.SimulationState.Running)
            {
                ++buttonIndex;

                string pauseState = "Simulation running";
                string pauseButton = "Pause simulation";
                if (WUIEngine.SIM.IsPaused)
                {
                    pauseState = "Simulation paused";
                    pauseButton = "Cont. simulation";
                }

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseState);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseButton))
                {
                    WUIEngine.SIM.TogglePause();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop simulation"))
                {                    
                    WUInityEngine.INSTANCE.StopSimulation();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle realtime"))
                {
                    WUIEngine.SIM.ToogleRealtime();
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Step execution time [ms]: " + WUIEngine.SIM.StepExecutionTime.ToString("F1", CultureInfo.InvariantCulture));
                ++buttonIndex;

                LegendGUI();                
            }

            if (WUIEngine.SIM.State == Simulation.SimulationState.Finished)  
            {
                if(plotFig == null)
                {
                    CreateArrivalTexture();
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle arrival output"))
                {
                    displayArrivalPlot = !displayArrivalPlot;
                }
                ++buttonIndex;

                if (displayArrivalPlot)
                {
                    plotWindowRect = GUI.Window(0, plotWindowRect, ShowPlot, "Arrival output");
                }                
            }
        }

        static int headerHeight;
        static Vector2Int plotWindowSize = new Vector2Int(532, 532);
        Rect plotWindowRect = new Rect(0.5f * (Screen.width - plotWindowSize.x), 0.5f * (Screen.height - plotWindowSize.y), plotWindowSize.x, plotWindowSize.y);
        Rect plotWindowGroup = new Rect(headerHeight, 0, plotWindowSize.x, plotWindowSize.y);
        static Rect dragWindowRect = new Rect(0, 0, 10000, headerHeight);
        Vector2 scrollPlot;
        Rect closeWindowRect = new Rect(plotWindowSize.x - 20, 0, 20, 20);


        void ShowPlot(int windowID)
        {   
            if (GUI.Button(closeWindowRect, "X", styleAlignedCenter))
            {
                displayArrivalPlot = false;
            }

            //GUI.BeginGroup(plotWindowGroup);
            //scrollPlot = GUI.BeginScrollView(plotWindowGroup, scrollPlot, new Rect(0, 0, 532, 1024));
            

            GUI.DrawTexture(new Rect(0, 20, 512, 512), plotFig);


            //GUI.EndScrollView();
            //GUI.EndGroup();

            GUI.DragWindow(dragWindowRect);
        }

        Texture2D plotFig;
        private void CreateArrivalTexture()
        {
            plotFig = new Texture2D(2, 2);
            ImageConversion.LoadImage(plotFig, WUIEngine.SIM.GetArrivalPlotBytes());
        }

        void ResetOutputGUI()
        {
            displayArrivalPlot = false;
        }

        void LegendGUI()
        {
            if (WUIEngine.INPUT.Simulation.RunFireModule)
            {
                GUI.BeginGroup(new Rect(Screen.width - 125, Screen.height * 0.5f - 305, 120, 300));

                GUI.Box(new Rect(0, 0, 120, 300), "Fireline int.");
                GUI.DrawTexture(new Rect(40, 50, 40, 200), verticalColorGradient);
                string upperLimit = WUInityEngine.FIRE_VISUALS.GetUpperFirelineIntensityLimit().ToString("f1") + " [kW/m]";
                GUI.Label(new Rect(0, 20, 120, 20), upperLimit, styleAlignedCenter);
                string lowerLimit = WUInityEngine.FIRE_VISUALS.GetLowerFirelineIntensityLimit().ToString("f1") + " [kW/m]";
                GUI.Label(new Rect(0, 260, 120, 20), lowerLimit, styleAlignedCenter);

                GUI.EndGroup();
            }

            if (WUIEngine.INPUT.Simulation.RunSmokeModule)
            {
                GUI.BeginGroup(new Rect(Screen.width - 125, Screen.height * 0.5f + 5, 120, 300));

                GUI.Box(new Rect(0, 0, 120, 300), "Optical dens.");
                GUI.DrawTexture(new Rect(40, 50, 40, 200), verticalColorGradient);
                string upperLimit = WUInityEngine.FIRE_VISUALS.GetUpperOpticalDensityLimit().ToString("e3") + " [-/m]";
                GUI.Label(new Rect(0, 20, 120, 20), upperLimit, styleAlignedCenter);
                string lowerLimit = WUInityEngine.FIRE_VISUALS.GetLowerOpticalDensityLimit().ToString("e3") + " [-/m]";
                GUI.Label(new Rect(0, 260, 120, 20), lowerLimit, styleAlignedCenter);

                GUI.EndGroup();
            }            
        }
    }
}
