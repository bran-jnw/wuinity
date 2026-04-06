using ImGuiNET;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class ConsoleWindow
    {
        private static bool _open = true;

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
            ImGui.Begin("Console", ref _open, ImGuiWindowFlags.NoCollapse);

            LinkedListNode<string> node = messages.First;
            while (node != null)
            {
                ImGui.Text(node.Value);
                node = node.Next;
            }

            ImGui.End();
            ImGui.PopStyleVar(1);
        }
    }
}
