using ImGuiNET;
using PREACT.Math;
using System;
using System.Threading.Tasks;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class DownloadWindow
    {
        private static Vector2d _lowerLeftLatLon, _upperRightLatLon, _domainSize;
        private static DateTime _startDateTime, _endDateTime;
        private static bool _isOpen;
        private static bool _folderSet;
        private static string _downloadFolder = string.Empty;
        private static bool _useAnderson13 = true;
        //private static string[] _landfireYears = new string[] { "2016", "2020", "2023", "2024" };
 
        public static void Open()
        {
            if (!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;

            if(ScenarioEditorWindow.HasInput)
            {
                _lowerLeftLatLon = ScenarioEditorWindow.Input.Simulation.LowerLeftLatLon;
                _upperRightLatLon = ScenarioEditorWindow.Input.Simulation.LowerLeftLatLon;
                _startDateTime = ScenarioEditorWindow.Input.Simulation.StartDateTime;
                _endDateTime = ScenarioEditorWindow.Input.Simulation.EndDateTime;
            }
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

            if(!_folderSet)
            {
                if (ImGui.Button("Set download folder")) { OpenSetDownloadFolder(); }
                return;
            }

            ImGui.Text("Download folder set to:" + _downloadFolder);

            ImGui.SeparatorText("Area of interest (AIO)");
            CustomTypes.InputDouble2(nameof(PREACT.Input.SimulationInput.LowerLeftLatLon), ref _lowerLeftLatLon);
            CustomTypes.InputDouble2("UpperRightLatLon", ref _upperRightLatLon);
            CustomTypes.InputDouble2(nameof(PREACT.Input.SimulationInput.DomainSize), ref _domainSize);

            ImGui.SeparatorText("Time period of interest");
            CustomTypes.InputDateTimePopup(nameof(PREACT.Input.SimulationInput.StartDateTime), ref _startDateTime);
            CustomTypes.InputDateTimePopup(nameof(PREACT.Input.SimulationInput.EndDateTime), ref _endDateTime);

            if (ImGui.Button("Download all")) { DownloadAll(); }

            ImGui.SeparatorText("Landfire data");
            ImGui.Text("Downloads data from Landfire for the specified AIO.");
            ImGui.Checkbox("Use 13 Anderson FBFM?", ref _useAnderson13);
            if (ImGui.Button("Download landscape")) { Task.Run(() => PREACT.Tools.LandfireLandscapeDownloader.DownloadLandscape(_startDateTime.Year, _useAnderson13, _lowerLeftLatLon, _upperRightLatLon, _downloadFolder)); }

            ImGui.SeparatorText("Weather data");
            ImGui.Text("Downloads data from Open-Meteo at the center of AIO and for the entire year of interest.");
            if (ImGui.Button("Download weather")) { }

            ImGui.SeparatorText("OpenStreetMap data");
            ImGui.Text("Downloads OSM data via Overpass for the specified AIO.");
            if (ImGui.Button("Download OSM")) { }

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }

        private static void DownloadAll()
        {
            Task.Run(() => PREACT.Tools.LandfireLandscapeDownloader.DownloadLandscape(_startDateTime.Year, _useAnderson13, _lowerLeftLatLon, _upperRightLatLon, _downloadFolder));
        }

        private static void OpenSetDownloadFolder()
        {
            string initialFolder = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(SetRootFolder, FileBrowser.CancelSaveLoad, SimpleFileBrowser.FileBrowser.PickMode.Folders, false, initialFolder, null, "Set download folder", "Set");
        }
        private static void SetRootFolder(string[] paths)
        {
            _folderSet = true;
            _downloadFolder = paths[0];
        }
    }
}
