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
                    if (ImGui.MenuItem("Load scenario")) { FileBrowserGUI.OpenLoadInput(); }
                    if (ImGui.MenuItem("Save", ScenarioEditorGUI.HasInput)) { ScenarioEditorGUI.SaveInput(); }
                    if (ImGui.MenuItem("Save as", ScenarioEditorGUI.HasInput)) { FileBrowserGUI.OpenSaveInput(); }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Scenario", ScenarioEditorGUI.HasInput))
                {
                    if (ImGui.MenuItem("Run/edit")) { ScenarioEditorGUI.Open(); }

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
