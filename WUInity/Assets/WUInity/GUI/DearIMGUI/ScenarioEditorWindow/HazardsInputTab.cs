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
            wildfireModuleIndex = (int)input.WildfireModule.Module;
            ImGui.Combo(nameof(input.WildfireModule), ref wildfireModuleIndex, WildfireModulesStrings, WildfireModulesStrings.Length);
            input.WildfireModule.Module = (WildfireModuleInput.WildfireModules)wildfireModuleIndex;
            if(input.WildfireModule.Module != WildfireModuleInput.WildfireModules.None)
            {
                if (ImGui.Button("Module settings###1")) 
                {
                    if (input.WildfireModule.Module == WildfireModuleInput.WildfireModules.SimpleWildfireCA) { }
                }
            }

            ImGui.SeparatorText("Wildfire smoke");
            smokeModuleIndex = (int)input.SmokeModule.Module;
            ImGui.Combo(nameof(input.SmokeModule), ref smokeModuleIndex, SmokeModulesStrings, SmokeModulesStrings.Length);
            input.SmokeModule.Module = (SmokeInput.SmokeModules)smokeModuleIndex;
            if(input.SmokeModule.Module != SmokeInput.SmokeModules.None)
            {
                if (ImGui.Button("Module settings###2")) 
                {
                    PREACT.Engine.Message(null, PREACT.Engine.LogType.Debug, "CLICK");
                    if (input.SmokeModule.Module == SmokeInput.SmokeModules.GlobalSmoke) { GlobalSmokeInputEditorWindow.Open(input.SmokeModule.GlobalSmokeInput);}
                }
            }
        }
    }
}
