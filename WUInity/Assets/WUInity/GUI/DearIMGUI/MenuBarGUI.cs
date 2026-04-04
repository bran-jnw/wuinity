using ImGuiNET;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class MenuBarGUI
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
                    if (ImGui.MenuItem("Run/edit")) { ScenarioEditorWindow.Open(); }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Console", ScenarioEditorWindow.HasInput))
                {
                    if (ImGui.MenuItem("Open")) { ConsoleWindow.Open(); }
                    if (ImGui.MenuItem("Clear")) { }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Data download"))
                {
                    if (ImGui.MenuItem("Download OSM data")) { }
                    if (ImGui.MenuItem("Download WorldPop data")) { }
                    if (ImGui.MenuItem("Download Landfire data")) { }

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }
    }
}
