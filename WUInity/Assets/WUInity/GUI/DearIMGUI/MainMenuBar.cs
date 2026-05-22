using ImGuiNET;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class MainMenuBar
    {
        public static void Draw()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {                    
                    if (ImGui.MenuItem("Load scenario")) { FileBrowser.OpenLoadInput(); }
                    if (ImGui.MenuItem("Save", ScenarioEditorWindow.HasInput)) { ScenarioEditorWindow.SaveInput(); }
                    if (ImGui.MenuItem("Save as", ScenarioEditorWindow.HasInput)) { FileBrowser.OpenSaveInput(); }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Scenario"))
                {
                    bool isRunning = false;
                    if (PreactGUI.Engine.Simulation != null && PreactGUI.Engine.Simulation.IsRunning)
                    {
                        isRunning = true;
                    }  
                    if (ImGui.MenuItem("New scenario", !isRunning)) { NewScenarioWindow.Open(true); }

                    ImGui.SeparatorText("Loaded scenario");
                    bool canEdit = false;
                    if (ScenarioEditorWindow.HasInput && !isRunning)
                    {
                        canEdit = true;
                    }
                    if (ImGui.MenuItem("Run/edit", canEdit)) { ScenarioEditorWindow.Open(); }

                    //placeholder
                    if (ImGui.BeginMenu("Detection", ScenarioEditorWindow.HasInput))
                    {
                        if (ImGui.MenuItem("Satellites")) { SatelliteWindow.Open(); }
                        if (ImGui.MenuItem("Drones")) { }

                        ImGui.EndMenu();
                    }

                    bool haveOutput = false;
                    if(PreactGUI.Engine.Simulation != null && (PreactGUI.Engine.Simulation.State == PREACT.Simulation.SimulationState.Running || PreactGUI.Engine.Simulation.State == PREACT.Simulation.SimulationState.Completed))
                    {
                        haveOutput = true;
                    }
                    if (ImGui.MenuItem("Output", haveOutput)) { OutputWindow.Open(); }

                    ImGui.EndMenu();
                }

                

                if (ImGui.BeginMenu("Console"))
                {
                    if (ImGui.MenuItem("Open")) { ConsoleWindow.Open(); }
                    if (ImGui.MenuItem("Clear")) { }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Utilities"))
                {
                    ImGui.SeparatorText("Download");
                    if (ImGui.MenuItem("Download data")) { DownloadDataWindow.Open(); }

                    ImGui.SeparatorText("Edit");
                    if (ImGui.MenuItem("Population editor")) { PopulationEditWindow.Open(); }
                    if (ImGui.MenuItem("Landscape editor")) { }

                    ImGui.SeparatorText("Viewing");
                    if (ImGui.MenuItem("Visualize population")) { }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Theme"))
                {
                    if (ImGui.MenuItem("Dark theme")) { Themes.ApplyAdobeSpectrum(true); }
                    if (ImGui.MenuItem("Light Theme")) { Themes.ApplyAdobeSpectrum(false); }

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }
    }
}
