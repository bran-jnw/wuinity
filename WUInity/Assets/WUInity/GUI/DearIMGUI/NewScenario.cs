using ImGuiNET;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class NewScenario
    {
        public static bool _draw;

        public static string scenarioId = string.Empty;
        public static Vector2 _latLon, _domainSize;
        public static int _year;

        public static void Draw()
        {
            if(!_draw)
            {
                return;
            }

            // Create a window called "My First Tool", with a menu bar.
            ImGui.Begin("New scenario", ref _draw, ImGuiWindowFlags.MenuBar);

            ImGui.InputText("Scenario id", ref scenarioId, 128);
            ImGui.InputFloat2("Lower left lat/lon", ref _latLon);
            ImGui.InputFloat2("Domain size", ref _domainSize);
            ImGui.InputInt("Year of interest", ref _year);

            //if(ImGui.Button("Create scenario")){ PREACT.Tools.PopulationTools.CreateBaseScenario() };

            ImGui.End();
        }
    }
}
