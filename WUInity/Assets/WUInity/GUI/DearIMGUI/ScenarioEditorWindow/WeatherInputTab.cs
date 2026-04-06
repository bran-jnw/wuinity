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
            ImGui.InputText(nameof(input.WeatherFile), ref input.WeatherFile, 256);
        }
    }
}
