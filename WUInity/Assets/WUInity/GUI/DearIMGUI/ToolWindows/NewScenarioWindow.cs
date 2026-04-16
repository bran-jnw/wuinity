using ImGuiNET;
using PREACT;
using PREACT.Input;
using PREACT.Math;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class NewScenarioWindow
    {
        private static bool _isOpen;

        private static Vector2d _latLon;
        private static PREACTInput _input;
        private static bool _folderSet;
        private static bool _havePopulation, _haveSumo, _haveWildfireLandscape, _haveWeather;
        private static bool _wantPedestrian, _wantTraffic, _wantWildfire, _wantSmoke;

        public static void Open(bool resetInput)
        {
            if (!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;
            if(resetInput)
            {
                _folderSet = false;
                _latLon = Vector2d.zero;
            }
            ScenarioEditorWindow.ClearInput();
            PreactGUI.WUInity.ShowWebMercatorMap();
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
            if(!_isOpen)
            {
                return;
            }

            ImGui.Begin("New scenario creator", ref _isOpen, PreactGUI.NoDockingNoCollapse);

            if(!_folderSet)
            {
                if (ImGui.Button("Set root folder")) 
                {
                    OpenSetRootFolder();                    
                }
                return;
            }
            ImGui.Text($"{nameof(_input.RootFolder)}: {_input.RootFolder}");

            SimulationInput simIn = _input.Simulation;

            ImGui.SeparatorText("Basic scenario data");
            ImGui.InputText(nameof(simIn.Name), ref simIn.Name, 128);

            ImGui.SeparatorText("Area of interest (AIO)");
            if (ImGui.Button("Set AIO on map"))
            {
                Close();
                PreactGUI.WUInity.PickBoundingBoxOnMap(SetAIO);
            }
            if(CustomTypes.InputDouble2(nameof(simIn.LowerLeftLatLon), ref _latLon))
            {
                simIn.LowerLeftLatLon = _latLon;
            }
            CustomTypes.InputDouble2(nameof(simIn.DomainSize), ref simIn.DomainSize);

            CustomTypes.InputDateTimePopup(nameof(simIn.StartDateTime), ref simIn.StartDateTime);
            CustomTypes.InputDateTimePopup(nameof(simIn.EndDateTime), ref simIn.EndDateTime);

            ImGui.SeparatorText("Evacuation");
            ImGui.Checkbox("Pedestrian evacuation?", ref _wantPedestrian);
            if(_wantPedestrian)
            {
                ImGui.Checkbox("Have population?", ref _havePopulation);
                if(_havePopulation)
                {
                    if (ImGui.Button("Select population file")) { }
                }
                else
                {
                    if (ImGui.Button("Step 1: Download WorldPop")) { }
                    if (ImGui.Button("Step 2: Download OSM data")) { }
                    if (ImGui.Button("Step 3: Build RouterDb")) { }
                    if (ImGui.Button("Step 4: Generate population")) { }
                }
            }

            ImGui.Separator();

            ImGui.Checkbox("Vehicle evacuation?", ref _wantTraffic);
            if (_wantTraffic)
            {
                ImGui.Checkbox("Have SUMO input?", ref _haveSumo);
                if (_haveSumo)
                {
                    if (ImGui.Button("Set SUMO input file")) { FileBrowser.OpenSetFilePath(path => _input.TrafficModule.SumoInput.ConfigurationFile = path); }
                }
                else
                {
                    if (ImGui.Button("Step 1: Download OSM data")) { }
                    if (ImGui.Button("Step 2: Generate SUMO input")) { }
                }
            }

            ImGui.SeparatorText("Hazards");
            ImGui.Checkbox("Wildfire spread?", ref _wantWildfire);
            if (_wantWildfire)
            {
                ImGui.Checkbox("Have wildfire landscape?", ref _haveWildfireLandscape);
                if(_haveWildfireLandscape)
                {
                    if (ImGui.Button("Set landscape file")) { }
                }
                else
                {
                    if (ImGui.Button("Step 1: Download Landfire data")) { }
                }

                ImGui.Checkbox("Have weather?", ref _haveWeather);
                if (_haveWeather)
                {
                    if (ImGui.Button("Select weather file")) { FileBrowser.OpenSetFilePath(path => _input.Weather.WeatherFile = path); }
                }
                else
                {
                    if (ImGui.Button("Step 1: Download weather file")) { }
                }
            }

            ImGui.Separator();

            ImGui.Checkbox("Smoke spread?", ref _wantSmoke);
            if (_wantSmoke)
            {
                if(!_wantWildfire)
                {
                    ImGui.Text("Smoke dispersion needs wildfire spread active as source term.");
                }                  
            }

            ImGui.SeparatorText("Finished?");

            if (ImGui.Button("Generate scenario")) { GenerateScenario(); }

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }

        private static void SetAIO(Vector2d[] latLons)
        {
            _latLon = new Vector2d(Mathd.Min(latLons[0].x, latLons[1].x), Mathd.Min(latLons[0].y, latLons[1].y));
            _input.Simulation.LowerLeftLatLon = _latLon;
            Vector2d _upperRightLatLon = new Vector2d(Mathd.Max(latLons[0].x, latLons[1].x), Mathd.Max(latLons[0].y, latLons[1].y));
            var lower = PREACT.Utility.LatLngUTMConverter.WGS84.convertLatLngToUtm(_latLon.x, _latLon.y);
            var upper = PREACT.Utility.LatLngUTMConverter.WGS84.convertLatLngToUtm(_upperRightLatLon.x, _upperRightLatLon.y);
            _input.Simulation.DomainSize = new Vector2d(upper.Easting - lower.Easting, upper.Northing - lower.Northing); 
            Open(false);
        }

        private static void GenerateScenario()
        {
            if(_input.Simulation.Name == string.Empty)
            {
                Engine.Message(null, Engine.LogType.InputError, $"Parameter {nameof(_input.Simulation.Name)} needs to be properly set.");
                return;
            }
            _isOpen = false;
            _folderSet = false;
            string filePath = Path.Combine(_input.RootFolder, _input.Simulation.Name, ".wui");
            PREACTInput.SaveToDisk(_input, filePath);
            PreactGUI.Engine.SetInput(_input, filePath);     
        }
        private static void OpenSetRootFolder()
        {
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(SetRootFolder, FileBrowser.CancelSaveLoad, SimpleFileBrowser.FileBrowser.PickMode.Folders, false, null, null, "Set root folder", "Set");
        }
        private static void SetRootFolder(string[] paths)
        {
            _folderSet = true;
            _input = new PREACTInput(string.Empty);
            _input.RootFolder = paths[0];
        }
    }
}
