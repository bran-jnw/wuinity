using System.Globalization;
using UnityEngine;
using PREACT;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string outputTime;
        float sliderVtraffic = 1f;
        bool displayArrivalPlot = false;

        void OutputMenu()
        {
            if(_engine.Simulation == null || _engine.Simulation.State == Simulation.SimulationState.Error || _engine.Simulation.State == Simulation.SimulationState.Initializing)
            {
                return;
            }

            int buttonColumnStart = 140;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int dummy = (int)_engine.Simulation.CurrentTime;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evac time: " + dummy + " s");
            ++buttonIndex;

            dummy = _engine.Simulation.Input.Population.Data.TotalPopulation;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + dummy);
            ++buttonIndex;

            if(_engine.Simulation.PedestrianModule != null)
            {
                dummy = _engine.Simulation.PedestrianModule.GetPeopleStaying();
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "People staying: " + dummy);
                ++buttonIndex;

                //toatl cars
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total cars: " + _engine.Simulation.PedestrianModule.GetTotalCars());
                ++buttonIndex;
            }         

            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Display options");
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Households"))
            {
                _wuinityManager.ToggleHouseholdRendering();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic"))
            {
                _wuinityManager.ToggleTrafficRendering();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fire spread"))
            {
                _wuinityManager.ToggleFireSpreadRendering();
                _wuinityManager.SetSampleMode(DataSampleMode.None);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Extinction coefficient"))
            {
                _wuinityManager.ToggleSootRendering();
                _wuinityManager.SetSampleMode(DataSampleMode.None);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Hide visual data"))
            {
                _wuinityManager.SimulationDomainVisualizer.SetVisibility(false);
                _wuinityManager.SimulationDomainVisualizer.SetGPWVisibility(false);
                _wuinityManager.FireDomainVisualizer.SetVisibility(false);
            }
            ++buttonIndex;

            if (_input.PedestrianModule.Enabled && _engine.Simulation.PedestrianModule != null)
            {
                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Pedestrians left: " + _engine.Simulation.PedestrianModule.GetPeopleLeft() + " / " + _engine.Simulation.PedestrianModule.GetTotalPopulation());
                ++buttonIndex;

                //cars reached by pedestrians
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars reached: " + _engine.Simulation.PedestrianModule.GetCarsReached());
                ++buttonIndex;
            }

            //cars still left
            if (_input.TrafficModule.Enabled && _engine.Simulation.TrafficModule != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars left: " + _engine.Simulation.TrafficModule.GetNumberOfCarsInSystem() + " / " + _engine.Simulation.TrafficModule.GetTotalCarsSimulated());
                ++buttonIndex;
            }

            if(_input.PedestrianModule.Enabled)
            {
                for (int i = 0; i < _engine.Simulation.Evacuation.Destinations.Length; i++)
                {
                    string name = _engine.Simulation.Evacuation.Destinations[i].Name;
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), name + ": " + _engine.Simulation.Evacuation.Destinations[i].CurrentPeople + " (" + _engine.Simulation.Evacuation.Destinations[i].Vehicles.Count + ")");
                    ++buttonIndex;
                }
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evacuated: " + _engine.Simulation.Evacuation.GetTotalEvacuated() + " / " + (_engine.Simulation.PedestrianModule.GetTotalPopulation() - _engine.Simulation.PedestrianModule.GetPeopleStaying()));
                ++buttonIndex;
            }            

            //fire output stuff
            if (_input.WildfireModule.Enabled && _engine.Simulation.State == Simulation.SimulationState.Running)
            {               
                /*GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind speed: " + _engine.Simulation.FireModule.GetCurrentWindData().speed + " m/s");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind direction: " + _engine.Simulation.FireModule.GetCurrentWindData().direction + " degrees");
                ++buttonIndex;*/
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Active cells (FireMesh): " + _engine.Simulation.FireModule.GetActiveCellCount());
                ++buttonIndex;

                //fire visual mode
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fire display mode");
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fireline intensity"))
                {
                    _wuinityManager.FireRenderer.SetFireDisplayMode(Visualization.FireRenderer.FireDisplayMode.FirelineIntensity);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    _wuinityManager.FireRenderer.SetFireDisplayMode(Visualization.FireRenderer.FireDisplayMode.FuelModelNumber);
                }
                ++buttonIndex;
            }

            if(_engine.Simulation.State == Simulation.SimulationState.Running)
            {
                ++buttonIndex;

                string pauseState = "Simulation running";
                string pauseButton = "Pause simulation";
                if (_engine.Simulation.IsPaused)
                {
                    pauseState = "Simulation paused";
                    pauseButton = "Cont. simulation";
                }

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseState);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), pauseButton))
                {
                    _engine.Simulation.TogglePause();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop simulation"))
                {                    
                    _wuinityManager.StopSimulations();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle realtime"))
                {
                    _engine.Simulation.ToogleRealtime();
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Step execution time [ms]: " + _engine.Simulation.StepExecutionTime.ToString("F1", CultureInfo.InvariantCulture));
                ++buttonIndex;

                LegendGUI();                
            }

            if (_engine.Simulation.State == Simulation.SimulationState.Finished)  
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Display usage map"))
                {
                    _wuinityManager.DisplayTrafficUsageMap();
                }
                ++buttonIndex;

                if (plotFig == null)
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

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle k-PERIL results"))
                {
                    _wuinityManager.FireDomainVisualizer.DisplayTriggerBuffer(_engine.Simulation.Output.GetTriggerBufferOutput(_engine.Simulation.SimulationIndex, out success));
                }
                ++buttonIndex;
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
            //ImageConversion.LoadImage(plotFig, _engine.Simulation.GetArrivalPlotBytes());
        }

        void ResetOutputGUI()
        {
            displayArrivalPlot = false;
        }

        void LegendGUI()
        {
            if (_input.WildfireModule.Enabled)
            {
                GUI.BeginGroup(new Rect(Screen.width - 125, Screen.height * 0.5f - 305, 120, 300));

                GUI.Box(new Rect(0, 0, 120, 300), "Fireline int.");
                GUI.DrawTexture(new Rect(40, 50, 40, 200), verticalColorGradient);
                string upperLimit = _wuinityManager.FireRenderer.GetUpperFirelineIntensityLimit().ToString("f1") + " [kW/m]";
                GUI.Label(new Rect(0, 20, 120, 20), upperLimit, styleAlignedCenter);
                string lowerLimit = _wuinityManager.FireRenderer.GetLowerFirelineIntensityLimit().ToString("f1") + " [kW/m]";
                GUI.Label(new Rect(0, 260, 120, 20), lowerLimit, styleAlignedCenter);

                GUI.EndGroup();
            }

            if (_input.SmokeModule.Enabled)
            {
                GUI.BeginGroup(new Rect(Screen.width - 125, Screen.height * 0.5f + 5, 120, 300));

                GUI.Box(new Rect(0, 0, 120, 300), "Ext. coeff.");
                GUI.DrawTexture(new Rect(40, 50, 40, 200), verticalColorGradient);
                string upperLimit = _wuinityManager.FireRenderer.GetUpperExtCoeff().ToString("e3") + " [-/m]";
                GUI.Label(new Rect(0, 20, 120, 20), upperLimit, styleAlignedCenter);
                string lowerLimit = _wuinityManager.FireRenderer.GetLowerExtCoeff().ToString("e3") + " [-/m]";
                GUI.Label(new Rect(0, 260, 120, 20), lowerLimit, styleAlignedCenter);

                GUI.EndGroup();
            }            
        }
    }
}
