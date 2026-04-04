using ImGuiNET;
using PREACT.Math;
using PREACT.Tools;
using System;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class NewScenarioWindow
    {
        private static bool _open;

        public static string scenarioId = string.Empty;
        public static Vector2 _latLon, _domainSize;
        public static int _year = DateTime.Today.Year;

        public static void Open()
        {
            _open = true;
        }

        public static void Draw()
        {
            if(!_open)
            {
                return;
            }

            ImGui.Begin("New scenario", ref _open, PreactGUI.NoDockingNoCollapse);

            ImGui.InputText("Scenario name", ref scenarioId, 128);
            ImGui.InputFloat2("Lower left lat/lon", ref _latLon);
            ImGui.InputFloat2("Domain size", ref _domainSize);
            ImGui.InputInt("Year of interest", ref _year);

            if(ImGui.Button("Create scenario")){ SelectFolderAndSave(); }

            ImGui.End();
        }

        private static void SelectFolderAndSave()
        {
            //CreateBaseScenario();
            string rootFolder = string.Empty;
            PREACT.Input.PREACTInput input = new PREACT.Input.PREACTInput(rootFolder);
            input.Simulation.Name = scenarioId;
            input.Simulation.LowerLeftLatLon = new Vector2d(_latLon.x, _latLon.y);
            input.Simulation.DomainSize = new Vector2d(_domainSize.x, _domainSize.y);

            ScenarioEditorWindow.SetInput(input);
            _open = false;
        }

        /*public static async void CreateBaseData(string[] paths)
        {
            _workingData.SetSimulatonData(lowerLatLon, domainSize);
            Vector2d upperLatLon = _workingData.SimulationInput.Data.GetWGS84FromSimulationPosition(domainSize);

            int.TryParse(_minHouseholdSize, out int min);
            int.TryParse(_maxHouseholdSize, out int max);

            int.TryParse(_yearOfInterest, out int year);

            await PopulationTools.CreateBaseScenario(paths[0], _scenarioId, min, max, lowerLatLon, upperLatLon, year);
        }*/
    }
}
