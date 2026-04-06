using Assets.WUInity.GUI.DearIMGUI;
using ImGuiNET;
using PREACT;
using PREACT.Evacuation;
using System.Collections.Generic;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class PopulationInputTab
    {
        public static void Draw(PREACT.Input.PopulationInput input)
        {
            ImGui.InputText(nameof(input.PopulationFile), ref input.PopulationFile, 256);
            ImGui.Checkbox(nameof(input.CullOutsideGroups), ref input.CullOutsideGroups);

            if (ImGui.CollapsingHeader("Demographics"))
            {
                if (ImGui.Button("Create demographics")){ }
                
                foreach (KeyValuePair<string, DemographicsInput> kV in input.Demographics)
                {
                    DemographicsInput demo = kV.Value;
                    if (ImGui.TreeNode(demo.Name))
                    {                        
                        ImGui.InputText(nameof(demo.Name), ref demo.Name, 64);
                        ImGui.Checkbox(nameof(demo.AllowMoreThanOneCar), ref demo.AllowMoreThanOneCar);
                        if(demo.AllowMoreThanOneCar)
                        {
                            ImGui.InputInt(nameof(demo.MaxCars), ref demo.MaxCars);
                            ImGui.InputFloat(nameof(demo.MaxCarsProbability), ref demo.MaxCarsProbability);
                        }
                        if (ImGui.Button("Remove")) { }

                        ImGui.TreePop();
                    }
                }
            }
        }
    }
}
