using ImGuiNET;
using PREACT;
using PREACT.Input;
using PREACT.Math;
using PREACT.Tools;
using SimpleFileBrowser;
using System;
using System.IO;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class NewScenarioWindow
    {
        private static bool _isOpen;

        private static Vector2 _latLon, _domainSize;
        private static PREACTInput _input;
        private static bool _folderSet;
        private static bool _havePopulation, _haveSumo, _haveWildfireLandscape, _haveWeather;
        private static bool _wantPedestrian, _wantTraffic, _wantWildfire, _wantSmoke;

        public static void Open()
        {
            if (!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;
            _latLon = Vector2.zero;
            _domainSize = Vector2.zero;
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
                        
            ImGui.InputFloat2(nameof(simIn.LowerLeftLatLon), ref _latLon);  
            ImGui.InputFloat2(nameof(simIn.DomainSize), ref _domainSize);

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
                    if (ImGui.Button("Select SUMO input file")) { }
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
                    if (ImGui.Button("Select landscape file")) { };
                }
                else
                {
                    if (ImGui.Button("Step 1: Download Landfire data")) { }
                }

                ImGui.Checkbox("Have weather?", ref _haveWeather);
                if (_haveWeather)
                {
                    if (ImGui.Button("Select weather file")) { }
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
            ;

            ImGui.End();
            if (!_isOpen)
            {
                _folderSet = false;
                PreactGUI.CloseWindow(Draw);
            }
        }

        private static void GenerateScenario()
        {
            if(_input.Simulation.Name == string.Empty)
            {
                Engine.Message(null, Engine.LogType.InputError, $"Parameter {nameof(_input.Simulation.Name)} needs to be properly set.");
                return;
            }

            _input.Simulation.LowerLeftLatLon = new Vector2d(_latLon.x, _latLon.y);
            _input.Simulation.DomainSize = new Vector2d(_domainSize.x, _domainSize.y);
            _isOpen = false;
            _folderSet = false;
            string filePath = Path.Combine(_input.RootFolder, _input.Simulation.Name, ".wui");
            PREACTInput.SaveToDisk(_input, filePath);
            PreactGUI.Engine.SetInput(_input, filePath);     
        }
        private static void OpenSetRootFolder()
        {
            FileBrowser.ShowLoadDialog(SetRootFolder, FileBrowserBackend.CancelSaveLoad, FileBrowser.PickMode.Folders, false, null, null, "Set root folder", "Set");
        }
        private static void SetRootFolder(string[] paths)
        {
            _folderSet = true;
            _input = new PREACTInput(string.Empty);
            _input.RootFolder = paths[0];
        }

    }
}
