using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.WUInity.GUI.DearIMGUI.Input
{
    public static class WeatherInputGUI
    {
        public static void Draw(PREACT.Input.WeatherInput input)
        {
            ImGui.InputText("Weather file", ref input.WeatherFile, 256);
        }
    }
}
