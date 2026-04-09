using ImGuiNET;
using System;
using PREACT.Input;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI.Input
{
    internal class HazardsInputTab
    {
        static string[] WildfireModulesStrings;
        static int wildfireModuleIndex = 0;

        static string[] SmokeModulesStrings;
        static int smokeModuleIndex = 0;

        static HazardsInputTab()
        {
            WildfireModulesStrings = Enum.GetNames(typeof(WildfireModuleInput.WildfireModules));
            SmokeModulesStrings = Enum.GetNames(typeof(SmokeInput.SmokeModules));
        }


        public static void Draw(PREACTInput input)
        {
            ImGui.SeparatorText("Wildfire spread");

            if (input.WildfireModule.Module != WildfireModuleInput.WildfireModules.None)
            {
                if (ImGui.Button("Module settings###1"))
                {
                    if (input.WildfireModule.Module == WildfireModuleInput.WildfireModules.SimpleWildfireCA) { }
                }
                ImGui.SameLine();
            }
            
            wildfireModuleIndex = (int)input.WildfireModule.Module;
            ImGui.Combo(nameof(input.WildfireModule), ref wildfireModuleIndex, WildfireModulesStrings, WildfireModulesStrings.Length);
            input.WildfireModule.Module = (WildfireModuleInput.WildfireModules)wildfireModuleIndex;
            

            ImGui.SeparatorText("Wildfire smoke");
            if (input.SmokeModule.Module != SmokeInput.SmokeModules.None)
            {
                if (ImGui.Button("Module settings###2"))
                {
                    if (input.SmokeModule.Module == SmokeInput.SmokeModules.GlobalSmoke) { GlobalSmokeInputEditorWindow.Open(input.SmokeModule.GlobalSmokeInput); }
                }
                ImGui.SameLine();
            }            
            smokeModuleIndex = (int)input.SmokeModule.Module;
            ImGui.Combo(nameof(input.SmokeModule), ref smokeModuleIndex, SmokeModulesStrings, SmokeModulesStrings.Length);
            input.SmokeModule.Module = (SmokeInput.SmokeModules)smokeModuleIndex;            
        }
    }
}
