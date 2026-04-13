using ImGuiNET;
using PREACT.Input;
using System.IO;

namespace Assets.WUInity.GUI.DearIMGUI
{ 
    static public class GlobalSmokeInputEditorWindow
    {
        private static bool _isOpen;
        private static GlobalSmokeInput _input;

        public static void Open(GlobalSmokeInput input)
        {
            _input = input;
            if(!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;
        }

        public static void Draw()
        {
            ImGui.Begin(nameof(GlobalSmokeInput), ref _isOpen, PreactGUI.NoDockingNoCollapse);
                        
            if (ImGui.Button("Create global smoke file")) { }
            ImGui.SameLine();
            if (ImGui.Button("Set global smoke file")) { FileBrowser.OpenSetFilePath(path => _input.ExtinctionFile = path); }
            ImGui.InputText(nameof(_input.ExtinctionFile), ref _input.ExtinctionFile, 256);

            ImGui.Separator();
            if (ImGui.Button("Apply")) { _isOpen = false; }

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }
    }
}
