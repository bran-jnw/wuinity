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
                    if (ImGui.MenuItem("New scenario")) { NewScenarioWindow.Open(); }
                    if (ImGui.MenuItem("Load scenario")) { FileBrowserBackend.OpenLoadInput(); }
                    if (ImGui.MenuItem("Save", ScenarioEditorWindow.HasInput)) { ScenarioEditorWindow.SaveInput(); }
                    if (ImGui.MenuItem("Save as", ScenarioEditorWindow.HasInput)) { FileBrowserBackend.OpenSaveInput(); }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Scenario", ScenarioEditorWindow.HasInput))
                {
                    bool edit = false;
                    if (PreactGUI.Engine.Simulation == null || (PreactGUI.Engine.Simulation.State != PREACT.Simulation.SimulationState.Running && PreactGUI.Engine.Simulation.State != PREACT.Simulation.SimulationState.Initializing))
                    {
                        edit = true;
                    }

                    if (ImGui.MenuItem("Run/edit", edit)) { ScenarioEditorWindow.Open(); }

                    bool output = false;
                    if(PreactGUI.Engine.Simulation != null && (PreactGUI.Engine.Simulation.State == PREACT.Simulation.SimulationState.Running || PreactGUI.Engine.Simulation.State == PREACT.Simulation.SimulationState.Completed))
                    {
                        output = true;
                    }
                    if (ImGui.MenuItem("Output", output)) { OutputWindow.Open(); }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Console", ScenarioEditorWindow.HasInput))
                {
                    if (ImGui.MenuItem("Open")) { ConsoleWindow.Open(); }
                    if (ImGui.MenuItem("Clear")) { }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Tools"))
                {
                    ImGui.SeparatorText("Download tools");
                    if (ImGui.MenuItem("Download OSM data")) { }
                    if (ImGui.MenuItem("Download WorldPop data")) { }
                    if (ImGui.MenuItem("Download Landfire data")) { }

                    ImGui.SeparatorText("Edit tools");
                    if (ImGui.MenuItem("Landscape editor")) { }

                    ImGui.SeparatorText("Viewing tools");
                    if (ImGui.MenuItem("Visualize population")) { }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Themes"))
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
