
using ImGuiNET;
using PREACT;
using System.Globalization;
using UnityEngine;
using WUInity;
using WUInity.Visualization;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class OutputWindow
    {
        private static bool _isOpen;

        static OutputWindow()
        {

        }

        public static void Open()
        {
            if(!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;
        }

        public static void Draw()
        {
            if (!_isOpen)
            {
                return;
            }            
            
            if (PreactGUI.Engine.Simulation == null || PreactGUI.Engine.Simulation.State == Simulation.SimulationState.Error || PreactGUI.Engine.Simulation.State == Simulation.SimulationState.Initializing)
            {
                return;
            }

            ImGui.Begin("Output", ref _isOpen, ImGuiWindowFlags.NoDocking);
            Simulation sim = PreactGUI.Engine.Simulation;

            ImGui.SeparatorText("General");
            ImGui.BulletText($"{nameof(sim.Time.SimulationTime)}: {(int)sim.Time.SimulationTime} s");
            ImGui.BulletText($"{nameof(sim.Time.CurrentDateTime)}: {sim.Time.CurrentDateTime.ToString(CultureInfo.CurrentCulture)}");
            ImGui.BulletText($"{nameof(sim.Input.Population.Data.TotalPopulation)}: {sim.Input.Population.Data.TotalPopulation}");                        

            if (sim.Evacuation.PedestrianModule != null)
            {
                ImGui.BulletText($"People staying: {sim.Evacuation.PedestrianModule.GetPeopleStaying()}");
                ImGui.BulletText($"Total vehicles: {sim.Evacuation.PedestrianModule.GetTotalCars()}");
            }

            ImGui.SeparatorText("Simulation state");
            if (sim.State == Simulation.SimulationState.Running)
            {

                string pauseState = "Simulation running";
                string pauseButton = "Pause simulation";
                if (sim.IsPaused)
                {
                    pauseState = "Simulation paused";
                    pauseButton = "Cont. simulation";
                }

                ImGui.BulletText(pauseState);

                if (ImGui.Button(pauseButton)) { sim.TogglePause(); }

                if (ImGui.Button("Stop simulation")) { PreactGUI.WUInity.StopSimulations(); }

                if (ImGui.Button("Toggle realtime")) { sim.ToggleRealtime(); }

                ImGui.BulletText("Step execution time [ms]: " + sim.StepExecutionTime.ToString("F1", CultureInfo.InvariantCulture));
            }

            ImGui.SeparatorText("Weather");
            ImGui.BulletText($"Temp.: {sim.Weather.GetTemperature()}");
            ImGui.BulletText($"RH: {sim.Weather.GetRelativeHumidity()}");
            ImGui.BulletText($"Wind speed: {sim.Weather.WindSpeed}");
            ImGui.BulletText($"Wind dir.: {sim.Weather.WindDirection}");

            ImGui.SeparatorText("Display controls");
            if (ImGui.Button("Toggle household rendering")) { PreactGUI.WUInity.ToggleHouseholdRendering(); }
            if (ImGui.Button("Toggle traffic rendering")) { PreactGUI.WUInity.ToggleTrafficRendering(); }
            if (ImGui.Button("Toggle wildfire spread rendering")) 
            {
                PreactGUI.WUInity.ToggleFireSpreadRendering();
                PreactGUI.WUInity.SetSampleMode(DataSampleMode.None);
            }
            if (ImGui.Button("Toggle wildfire smoke rendering"))
            {
                PreactGUI.WUInity.ToggleSootRendering();
                PreactGUI.WUInity.SetSampleMode(DataSampleMode.None);
            }
            if (ImGui.Button("Disable rendering"))
            {
                PreactGUI.WUInity.SimulationDomainVisualizer.SetVisibility(false);
                PreactGUI.WUInity.SimulationDomainVisualizer.SetGPWVisibility(false);
                PreactGUI.WUInity.FireDomainVisualizer.SetVisibility(false);
            }

            ImGui.SeparatorText("Evacuation");

            if (sim.Input.PedestrianModule.Enabled && sim.Evacuation.PedestrianModule != null)
            {
                ImGui.BulletText($"Pedestrians left: {sim.Evacuation.PedestrianModule.GetPeopleLeft()} /  {sim.Evacuation.PedestrianModule.GetTotalPopulation()}");
                ImGui.BulletText($"Vehicles reached: {sim.Evacuation.PedestrianModule.GetCarsReached()}");                
            }

            //vehicles still left
            if (sim.Input.TrafficModule.Enabled && sim.Evacuation.TrafficModule != null)
            {
                ImGui.BulletText($"Vehicles left: {sim.Evacuation.TrafficModule.GetNumberOfCarsInSystem()} / {sim.Evacuation.TrafficModule.GetTotalCarsSimulated()}");                
            }

            if (sim.Input.PedestrianModule.Enabled)
            {
                for (int i = 0; i < sim.Evacuation.Destinations.Count; i++)
                {
                    string name = sim.Evacuation.Destinations[i].Name;
                    ImGui.BulletText($"{name}: {sim.Evacuation.Destinations[i].CurrentPeople} ({ sim.Evacuation.Destinations[i].Vehicles.Count})");
                    
                }
                ImGui.BulletText($"Total evacuated: {sim.Evacuation.GetTotalEvacuated()} / {sim.Evacuation.PedestrianModule.GetTotalPopulation() - sim.Evacuation.PedestrianModule.GetPeopleStaying()}");                
            }

            ImGui.SeparatorText("Wildfire spread");
            if (sim.Input.WildfireModule.Enabled && sim.State == Simulation.SimulationState.Running)
            {
                ImGui.BulletText($"Active cells (FireMesh): {sim.Hazards.Wildfire.GetActiveCellCount()}");
                //fire visual mode
                ImGui.SeparatorText("Fire display mode");
                
                if (ImGui.Button("Fireline intensity")) { PreactGUI.WUInity.FireRenderer.SetFireDisplayMode(FireRenderer.FireDisplayMode.FirelineIntensity); }                
                if (ImGui.Button("Fuel model")){ PreactGUI.WUInity.FireRenderer.SetFireDisplayMode(FireRenderer.FireDisplayMode.FuelModelNumber); }                
            }

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }
    }
}
