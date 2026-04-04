using ImGuiNET;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class ConsoleWindow
    {
        private static bool _open;

        private static ImGuiWindowFlags consoleWindow = ImGuiWindowFlags.NoCollapse;

        public static void Open()
        {
            _open = true;
        }

        public static void Draw(LinkedList<string> messages)
        {
            if (!_open)
            {
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.zero);
            ImGui.Begin("Console", ref _open, consoleWindow);           

            foreach (string message in messages)
            {
                ImGui.Text(message);
            }        

            ImGui.End();
            ImGui.PopStyleVar(1);
        }
    }
}
