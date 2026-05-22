using ImGuiNET;
using PREACT.Math;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class DownloadDataWindow
    {
        private static Vector2d _lowerLeftLatLon, _upperRightLatLon, _domainSize;
        private static DateTime _startDateTime, _endDateTime;
        private static bool _isOpen;
        private static bool _folderSet;
        private static string _downloadFolder = string.Empty;
        private static bool _useAnderson13 = true;
        private static string _osmFileName = string.Empty, _weatherFileName = string.Empty, _worldPopFileName = string.Empty;
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

            if (ImGui.Button("Set download folder")) { OpenSetDownloadFolder(); }
            if (!_folderSet)
            {                
                return;
            }
            ImGui.Text("Download folder set to:" + _downloadFolder);

            ImGui.SeparatorText("Area of interest (AIO)");
            if (ImGui.Button("Set AIO on map")) 
            {
                Close();
                PreactGUI.WUInity.PickBoundingBoxOnMap(SetAIO); 
            }
            CustomTypes.InputDouble2(nameof(PREACT.Input.SimulationInput.LowerLeftLatLon), ref _lowerLeftLatLon);
            CustomTypes.InputDouble2("UpperRightLatLon", ref _upperRightLatLon);
            //CustomTypes.InputDouble2(nameof(PREACT.Input.SimulationInput.DomainSize), ref _domainSize);

            ImGui.SeparatorText("Time period of interest");
            CustomTypes.InputDateTimePopup(nameof(PREACT.Input.SimulationInput.StartDateTime), ref _startDateTime);
            CustomTypes.InputDateTimePopup(nameof(PREACT.Input.SimulationInput.EndDateTime), ref _endDateTime);

            //if (ImGui.Button("Download all")) { DownloadAll(); }

            ImGui.SeparatorText("Landfire data");
            ImGui.Text("Downloads data from Landfire for the specified AIO.");
            ImGui.Checkbox("Get 13 Anderson FBFM? (else 40 Scott and Burgan)", ref _useAnderson13);
            if (ImGui.Button("Download landscape")) { Task.Run(() => DownloadLandscape()); }

            ImGui.SeparatorText("Weather data");
            ImGui.Text("Downloads data from Open-Meteo at the center of AIO and for the entire year of interest.");
            ImGui.InputText("Weather filename", ref _weatherFileName, 64);
            if (ImGui.Button("Download weather")) 
            {
                Task.Run(() => PREACT.Tools.OpenMeteoDownloader.Download(0.5 * (_lowerLeftLatLon + _upperRightLatLon), _startDateTime, _endDateTime, Path.Combine(_downloadFolder, _weatherFileName + ".csv")));
            }

            ImGui.SeparatorText("OpenStreetMap data");
            ImGui.Text("Downloads OSM data via Overpass for the specified AIO.");
            ImGui.InputText("OSM filename", ref _osmFileName, 64);
            if (ImGui.Button("Download OSM")) 
            {
                Task.Run(() => PREACT.Tools.OSMDownloader.Download(_lowerLeftLatLon, _upperRightLatLon, Path.Combine(_downloadFolder, _osmFileName + ".osm.xml"))); 
            }

            ImGui.SeparatorText("WorldPop data");
            ImGui.Text("Downloads WorldPopData for the entire country of interest.");
            ImGui.InputText("WorldPop filename", ref _worldPopFileName, 64);
            if (ImGui.Button("Download WorldPop"))
            {
                Task.Run(() => PREACT.Tools.WorldPopDownloader.DownloadRegionUTM(_startDateTime.Year, _lowerLeftLatLon, _upperRightLatLon, _downloadFolder, _worldPopFileName));
            }

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }

        private static void SetAIO(Vector2d[] latLons)
        {
            _lowerLeftLatLon = new Vector2d(Mathd.Min(latLons[0].x, latLons[1].x), Mathd.Min(latLons[0].y, latLons[1].y));
            _upperRightLatLon = new Vector2d(Mathd.Max(latLons[0].x, latLons[1].x), Mathd.Max(latLons[0].y, latLons[1].y));
            Open();
        }

        private static async Task DownloadLandscape()
        {
            //verify that we are in US
            Vector2d center = 0.5 * (_lowerLeftLatLon + _upperRightLatLon);
            string iso3 = await PREACT.Tools.WorldPopDownloader.LatLonToISO3(center.x, center.y);
            if(iso3 != "USA")
            {
                PREACT.Engine.Message(null, PREACT.Engine.LogType.Log, "Specified region is outside of the USA, cannot download Landfire data.");
            }
            else
            {
                await PREACT.Tools.LandfireLandscapeDownloader.Download(_startDateTime.Year, _useAnderson13, _lowerLeftLatLon, _upperRightLatLon, _downloadFolder);
            }           
        }

        /*private static void DownloadAll()
        {
            Task.Run(() => PREACT.Tools.LandfireLandscapeDownloader.Download(_startDateTime.Year, _useAnderson13, _lowerLeftLatLon, _upperRightLatLon, _downloadFolder));
            Task.Run(() => PREACT.Tools.OpenMeteoDownloader.Download(0.5 * (_lowerLeftLatLon + _upperRightLatLon), _startDateTime, _endDateTime, Path.Combine(_downloadFolder, _weatherFileName + ".csv")));
            Task.Run(() => PREACT.Tools.OSMTools.DownloadOMSData(_lowerLeftLatLon, _upperRightLatLon, Path.Combine(_downloadFolder, _osmFileName + ".osm.xml")));
            Task.Run(() => PREACT.Tools.WorldPopDownloader.DownloadRegionUTM(_startDateTime.Year, _lowerLeftLatLon, _upperRightLatLon, Path.Combine(_downloadFolder, _worldPopFileName + ".tif")));
        }*/

        private static void OpenSetDownloadFolder()
        {
            string initialFolder = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(SetRootFolder, FileBrowser.Cancel, SimpleFileBrowser.FileBrowser.PickMode.Folders, false, initialFolder, null, "Set download folder", "Set");
        }
        private static void SetRootFolder(string[] paths)
        {
            _folderSet = true;
            _downloadFolder = paths[0];
        }
    }
}
