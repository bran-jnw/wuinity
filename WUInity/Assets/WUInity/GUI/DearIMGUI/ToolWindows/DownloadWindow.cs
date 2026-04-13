using ImGuiNET;
using PREACT.Math;
using System;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class DownloadWindow
    {
        private static Vector2d _latLon, _domainSize;
        private static DateTime _startDateTime, _endDateTime;
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

            ImGui.Begin("Download tool", ref _isOpen, PreactGUI.NoDockingNoCollapse);

            ImGui.SeparatorText("Region of interest");
            //CustomTypes.InputVector2d(nameof(PREACT.Input.SimulationInput.LowerLeftLatLon), ref _latLon);
            //CustomTypes.InputVector2d(nameof(PREACT.Input.SimulationInput.DomainSize), ref _domainSize);

            ImGui.SeparatorText("Time period of interest");
            CustomTypes.InputDateTimePopup(nameof(PREACT.Input.SimulationInput.StartDateTime), ref _startDateTime);
            CustomTypes.InputDateTimePopup(nameof(PREACT.Input.SimulationInput.EndDateTime), ref _endDateTime);

            ImGui.SeparatorText("Landfire downloader");

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }
    }
}
