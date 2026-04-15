using ImGuiNET;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class StartWindow
    {
        private static bool _isOpen;

        public static void Open()
        {
            if (!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;
        }

        public static void Close()
        {
            if (_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
            _isOpen = false;
        }

        public static void Draw()
        {
            if (!_isOpen)
            {
                return;
            }

            ImGui.Begin("Welcome to PREACT", ref _isOpen, PreactGUI.NoDockingNoCollapse);

            ImGui.Text("Load or create a new .wui file to run simulations.");
            if (ImGui.Button("OK")) { Close(); }

            ImGui.SeparatorText("User set PATHs");
            ImGui.Text("Make sure that the listed paths below are set correctly, otherwise PREACT will not work properly.");
            ImGui.Separator();
            ImGui.Text($"SUMO (including GDAL) path is set to {PreactGUI.Engine.SumoPath}");
            ImGui.Text($"PROJ_LIB path is set to {PreactGUI.Engine.ProjLibPath}");
            ImGui.Text($"PROJ_DATA path is set to {PreactGUI.Engine.ProjDataPath}");

            ImGui.End();    
            if(!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }

        }
    }
}
