using Assets.WUInity.GUI.DearIMGUI.Input;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public  static class ScenarioEditorGUI
    {
        private static bool _isOpen;
        private static PREACT.Input.PREACTInput _input;

        public static bool HasInput { get => _input == null ? false : true; }

        public static void SetInput(PREACT.Input.PREACTInput input)
        {
            _input = input;
            Open();
        }

        public static void Open()
        {
            _isOpen = true;
        }
        public static void Close()
        {
            _isOpen = false;
        }

        public static void Draw()
        {
            if(!_isOpen || _input == null)
            {
                return;
            }

            ImGui.Begin("Scenario editor", ref _isOpen, ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginTabBar(""))
            {
                if (ImGui.BeginTabItem("Run"))
                {
                    RunGUI.Draw();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Simulation"))
                {
                    SimulationInputGUI.Draw(_input.Simulation);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Weather"))
                {
                    WeatherInputGUI.Draw(_input.Weather);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Population"))
                {


                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Evacuation"))
                {


                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Hazards"))
                {
                    HazardsInputGUI.Draw(_input);
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }            

            ImGui.End();
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
