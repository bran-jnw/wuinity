using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.WUInity.GUI.DearIMGUI.Input
{
    public static class WeatherInputTab
    {
        public static void Draw(PREACT.Input.WeatherInput input)
        {
            if (ImGui.Button("Select weather file")) { }
            ImGui.SameLine();
            ImGui.InputText(nameof(input.WeatherFile), ref input.WeatherFile, 256);            
        }
    }
}
