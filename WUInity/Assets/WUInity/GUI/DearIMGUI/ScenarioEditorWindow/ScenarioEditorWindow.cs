using Assets.WUInity.GUI.DearIMGUI.Input;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public  static class ScenarioEditorWindow
    {
        private static bool _isOpen;
        private static PREACT.Input.PREACTInput _input;

        public static PREACT.Input.PREACTInput Input{ get => _input; }
        public static bool HasInput { get => _input == null ? false : true; }

        public static void SetInput(PREACT.Input.PREACTInput input)
        {
            _input = input;
            Open();
        }

        public static void ClearInput()
        {
            _input = null;
        }

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
            if(_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
            _isOpen = false;            
        }

        public static void Draw()
        {
            if(!_isOpen || _input == null)
            {
                return;
            }

            ImGui.Begin("Scenario editor", ref _isOpen, PreactGUI.NoDockingNoCollapse);

            if (ImGui.BeginTabBar("Scenario"))
            {
                if (ImGui.BeginTabItem("Run"))
                {
                    RunTab.Draw();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Simulation"))
                {
                    if (ImGui.BeginTabBar("SimulationBar"))
                    {
                        if (ImGui.BeginTabItem("General"))
                        {
                            SimulationInputTab.Draw(_input.Simulation);
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Map"))
                        {
                            MapInputTab.Draw(_input.Map);
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Weather"))
                        {
                            WeatherInputTab.Draw(_input.Weather);
                            ImGui.EndTabItem();
                        }

                        ImGui.EndTabBar();
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Evacuation"))
                {
                    if (ImGui.BeginTabBar("EvacuationBar"))
                    {
                        EvacuationTabs.Draw(_input, _input.Evacuation, _input.PedestrianModule, _input.TrafficModule);

                        ImGui.EndTabBar();
                    }                        

                    ImGui.EndTabItem();

                }
                                  

                if (ImGui.BeginTabItem("Hazards"))
                {
                    HazardsInputTab.Draw(_input);
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }            

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }

        public static void SaveInput()
        {
            //ParseMainData();
            PREACT.Input.PREACTInput.SaveToDisk(_input, PreactGUI.Engine.WorkingFile);
        }

        public static void SaveNewInput(string[] paths)
        {
            //ParseMainData();
            PREACT.Input.PREACTInput.SaveToDisk(_input, paths[0]);
            PreactGUI.Engine.LoadInputFromFile(paths[0], out bool success);
        }
    }
}
