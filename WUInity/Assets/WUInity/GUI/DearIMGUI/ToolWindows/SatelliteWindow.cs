using ImGuiNET;
using PREACT.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class SatelliteWindow
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

            ImGui.Begin("Satellite status", ref _isOpen, PreactGUI.NoDockingNoCollapse);

            if(PreactGUI.Engine.Simulation != null && PreactGUI.Engine.Simulation.Detection.SatelliteStatus != null)
            {
                ImGui.Text($"Current UTC date/time: {PreactGUI.Engine.Simulation.Time.CurrentUTCDateTime}");
                ImGui.Separator();
                for (int i = 0; i < PreactGUI.Engine.Simulation.Detection.SatelliteStatus.Length; ++i)
                {
                    PREACT.Detection.SatelliteDetectionStatus status = PreactGUI.Engine.Simulation.Detection.SatelliteStatus[i];
                    ImGui.Text($"Satellite {status.Satellite.Name} status:");
                    ImGui.BulletText($"Position: Lat ({status.GeoCoord.Latitude.Degrees}), Lon ({status.GeoCoord.Longitude.Degrees}), Alt. [km] ({status.GeoCoord.Altitude})");
                    ImGui.BulletText($"Inside view area: {status.IsInside}");
                    ImGui.BulletText($"Detection possible: {status.CanDetect}");
                    ImGui.BulletText($"Wildfire off nadir angle: {status.OffNadirAngle}");
                    ImGui.BulletText($"Angle to wildfire edge: {status.ToEdgeAngle}");
                    ImGui.Separator();
                }                    
            }   
            else
            {
                ImGui.Text($"No active satllites.");
            }

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }
    }
}
