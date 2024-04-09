using UnityEngine;
using WUIEngine;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string outputTime;
        float sliderVtraffic = 1f;
        bool displayArrivalPlot = false;

        void OutputMenu()
        {
            if(Engine.SIM.State == Simulation.SimulationState.Error || Engine.SIM.State == Simulation.SimulationState.Initializing)
            {
                return;
            }

            int buttonColumnStart = 140;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int dummy = (int)Engine.OUTPUT.totalEvacTime;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evac time: " + dummy + " s");
            ++buttonIndex;

            dummy = Engine.POPULATION.GetTotalPopulation();
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + dummy);
            ++buttonIndex;

            dummy = Engine.SIM.PedestrianModule.GetPeopleStaying();
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "People staying: " + dummy);
            ++buttonIndex;

            //toatl cars
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total cars: " + Engine.SIM.PedestrianModule.GetTotalCars());
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

            float timeRange = Engine.OUTPUT.totalEvacTime - Engine.SIM.StartTime;
            float time = sliderVtraffic * timeRange + Engine.SIM.StartTime;
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
                //WUInity.INSTANCE.DisplayClosestDensityData(time);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Hide visual data"))
            {
                WUInity.INSTANCE.SetEvacDataPlane(false);
                WUInity.INSTANCE.SetFireDataPlane(false);
            }
            ++buttonIndex;

            if (Engine.INPUT.Simulation.RunPedestrianModule && Engine.SIM.PedestrianModule != null)
            {
                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Pedestrians left: " + Engine.SIM.PedestrianModule.GetPeopleLeft() + " / " + Engine.POPULATION.GetTotalPopulation());
                ++buttonIndex;

                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars reached: " + Engine.SIM.PedestrianModule.GetCarsReached());
                ++buttonIndex;
            }

            //cars still left
            if (Engine.INPUT.Simulation.RunTrafficModule && Engine.SIM.TrafficModule != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars left: " + Engine.SIM.TrafficModule.GetCarsInSystem() + " / " + Engine.SIM.TrafficModule.GetTotalCarsSimulated());
                ++buttonIndex;
            }

            for (int i = 0; i < Engine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                string name = Engine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), name + ": " + Engine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentPeople + " (" + Engine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].cars.Count + ")");
                ++buttonIndex;
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evacuated: " + Engine.RUNTIME_DATA.Evacuation.GetTotalEvacuated() + " / " + (Engine.POPULATION.GetTotalPopulation() - Engine.SIM.PedestrianModule.GetPeopleStaying()));
            ++buttonIndex;

            //fire output stuff
            if (Engine.INPUT.Simulation.RunFireModule && Engine.SIM.State == Simulation.SimulationState.Running)
            {               
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind speed: " + Engine.SIM.FireModule.GetCurrentWindData().speed + " m/s");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind direction: " + Engine.SIM.FireModule.GetCurrentWindData().direction + " degrees");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Active cells (FireMesh): " + Engine.SIM.FireModule.GetActiveCellCount());
                ++buttonIndex;

                //fire visual mode
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fire display mode");
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fireline intensity"))
                {
                    WUInity.FIRE_VISUALS.SetFireDisplayMode(Visualization.FireRenderer.FireDisplayMode.FirelineIntensity);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    WUInity.FIRE_VISUALS.SetFireDisplayMode(Visualization.FireRenderer.FireDisplayMode.FuelModelNumber);
                }
                ++buttonIndex;
            }

            if(Engine.SIM.State == Simulation.SimulationState.Running)
            {
                ++buttonIndex;

                string pauseState = "Simulation running";
                string pauseButton = "Pause simulation";
                if (Engine.SIM.IsPaused)
                {
                    pauseState = "Simulation paused";
                    pauseButton = "Cont. simulation";
                }

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseState);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseButton))
                {
                    Engine.SIM.TogglePause();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop simulation"))
                {                    
                    WUInity.INSTANCE.StopSimulation();
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Step execution time [ms]: " + Engine.SIM.StepExecutionTime);
                ++buttonIndex;

                LegendGUI();                
            }

            if (plotHasArrived)  
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

        byte[] plotByteData;
        Texture2D plotFig;
        bool plotHasArrived;
        public void SetArrivalPlotBytes(byte[] byteData)
        {
            plotByteData = byteData;            
            displayArrivalPlot = true;
            plotHasArrived = true;            
        }

        private void CreateArrivalTexture()
        {
            plotFig = new Texture2D(2, 2);
            ImageConversion.LoadImage(plotFig, plotByteData);
        }

        void ResetOutputGUI()
        {
            displayArrivalPlot = false;
            plotHasArrived = false;
        }

        void LegendGUI()
        {
            if (Engine.INPUT.Simulation.RunFireModule)
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

            if (Engine.INPUT.Simulation.RunSmokeModule)
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
