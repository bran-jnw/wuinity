using ImGuiNET;
using PREACT;
using System;
using PREACT.Input;
using PREACT.Dispersion;

namespace Assets.WUInity.GUI.DearIMGUI.Input
{
    internal class HazardsInputGUI
    {
        static string[] WildfireModulesStrings;
        static int wildfireModuleIndex = 0;

        static string[] SmokeModulesStrings;
        static int smokeModulIndex = 0;

        static HazardsInputGUI()
        {
            WildfireModulesStrings = Enum.GetNames(typeof(WildfireModuleInput.WildfireModules));
            SmokeModulesStrings = Enum.GetNames(typeof(SmokeInput.SmokeModules));
        }


        public static void Draw(PREACTInput input)
        {
            ImGui.SeparatorText("Wildfire spread");
            ImGui.Combo(nameof(input.WildfireModule), ref wildfireModuleIndex, WildfireModulesStrings, WildfireModulesStrings.Length);
            input.WildfireModule.Module = (WildfireModuleInput.WildfireModules)wildfireModuleIndex;
            if(input.WildfireModule.Module != WildfireModuleInput.WildfireModules.None)
            {
                if (ImGui.Button("Module settings")) { }
            }

            ImGui.SeparatorText("Wildfire smoke");
            ImGui.Combo(nameof(input.SmokeModule), ref smokeModulIndex, SmokeModulesStrings, SmokeModulesStrings.Length);
            input.SmokeModule.Module = (SmokeInput.SmokeModules)smokeModulIndex;
            if(input.SmokeModule.Module != SmokeInput.SmokeModules.None)
            {
                if (ImGui.Button("Module settings")) { }
            }
        }
    }
}
