using ImGuiNET;
using PREACT;
using PREACT.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI.Editors
{ 
    public static class DestinationInputEditWindow
    {
        private static bool _isOpen;
        private static EvacuationDestinationInput _input;
        private static Dictionary<string, EvacuationDestinationInput> _inputs;
        public static string[] DestinationTypesStrings;
        static int _destinationTypeIndex;
        static string _oldKey = string.Empty;

        static DestinationInputEditWindow()
        {
            DestinationTypesStrings = Enum.GetNames(typeof(DestinationTypes));
        }

        public static void Open(Dictionary<string, EvacuationDestinationInput> inputs, EvacuationDestinationInput input)
        {
            if(!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;

            _inputs = inputs;
            if(input == null)
            {
                _input = new EvacuationDestinationInput();
                _oldKey = string.Empty;
            }
            else
            {
                _input = input;
                _oldKey = _input.Name;
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
            if(!_isOpen)
            {
                return;
            }

            ImGui.Begin("Evacuation destination editor", ref _isOpen, PreactGUI.NoDockingNoCollapse);

            ImGui.InputText(nameof(_input.Name), ref _input.Name, 64);

            if(ImGui.Button("Set on map")) 
            {
                Close();
                PreactGUI.WUInity.PickPosOnMap(SetDestinationPos); 
            }
            ImGui.SameLine();
            Vector2 latLon = new Vector2((float)_input.LatLon.x, (float)_input.LatLon.y);
            if(ImGui.InputFloat2(nameof(_input.LatLon), ref latLon))
            {
                _input.LatLon.x = latLon.x;
                _input.LatLon.y = latLon.y;
            }            

            _destinationTypeIndex = (int)_input.Type;
            ImGui.Combo(nameof(_input.Type), ref _destinationTypeIndex, DestinationTypesStrings, DestinationTypesStrings.Length);
            _input.Type = (DestinationTypes)_destinationTypeIndex;

            if(_input.Type == DestinationTypes.Shelter)
            {
                ImGui.InputFloat(nameof(_input.MaxFlow), ref _input.MaxFlow);
                ImGui.InputInt(nameof(_input.MaxVehicles), ref _input.MaxVehicles);
                ImGui.InputInt(nameof(_input.MaxPeople), ref _input.MaxPeople);
            }            

            Vector3 color = new Vector3((float)_input.Color.r, (float)_input.Color.g, (float)_input.Color.b);
            ImGui.ColorEdit3(nameof(_input.Color), ref color);
            _input.Color.r = color.x;
            _input.Color.g = color.y;
            _input.Color.b = color.z;

            if (ImGui.Button("OK")) 
            {                
                _inputs.Remove(_oldKey);
                _inputs.Add(_input.Name, _input);
                _isOpen = false;
            }

            ImGui.End();
            if(!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }    
        
        private static void SetDestinationPos(PREACT.Math.Vector2d simulationPos)
        {
            _input.LatLon = ScenarioEditorWindow.Input.Simulation.Data.GetWGS84FromSimulationPosition(simulationPos);
            Open(_inputs, _input);
        }
    }
}
