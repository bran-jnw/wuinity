using ImGuiNET;
using PREACT;
using System;
using static System.Math;


namespace Assets.WUInity.GUI.DearIMGUI.Input
{
    public static class RunTab
    {
        static EngineTask _engineTask = new EngineTask();

        static string[] ExecutionModeStrings;
        static int executionMode;

        static RunTab()
        {
            ExecutionModeStrings = Enum.GetNames(typeof(EngineTask.ExecutionMode));
        }

        public static void Draw()
        {
            ImGui.Combo(nameof(EngineTask.ExecutionMode), ref executionMode, ExecutionModeStrings, ExecutionModeStrings.Length);
            _engineTask.Execution = (EngineTask.ExecutionMode)executionMode;
            ImGui.SliderInt(nameof(EngineTask.NumberOfRuns), ref _engineTask.NumberOfRuns, 1, 100);
            if (_engineTask.NumberOfRuns > 1)
            {
                ImGui.InputInt(nameof(EngineTask.BatchSize), ref _engineTask.BatchSize);
                ImGui.Checkbox(nameof(EngineTask.StopAfterConverging), ref _engineTask.StopAfterConverging);
                if (_engineTask.StopAfterConverging)
                {
                    ImGui.SliderInt(nameof(EngineTask.ConvergenceMinSequence), ref _engineTask.ConvergenceMinSequence, 1, 30);
                    ImGui.SliderFloat(nameof(EngineTask.ConvergenceMaxDifference), ref _engineTask.ConvergenceMaxDifference, 0.01f, 0.2f);
                }
            }

            ImGui.Separator();

            if (ImGui.Button("Start run"))
            {
                ScenarioEditorWindow.Close();
                PreactGUI.WUInity.RunSimulation(_engineTask);
                OutputWindow.Open();
            }
        }        
    }
}
