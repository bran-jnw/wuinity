using Assets.WUInity.GUI.DearIMGUI;
using ImGuiNET;
using PREACT;
using PREACT.Evacuation;

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
                
                foreach (DemographicsInput d in input.Demographics.Values)
                {
                    if (ImGui.TreeNode(d.Name))
                    {                        
                        ImGui.InputText(nameof(d.Name), ref d.Name, 64);
                        ImGui.Checkbox(nameof(d.AllowMoreThanOneCar), ref d.AllowMoreThanOneCar);
                        if(d.AllowMoreThanOneCar)
                        {
                            ImGui.InputInt(nameof(d.MaxCars), ref d.MaxCars);
                            ImGui.InputFloat(nameof(d.MaxCarsProbability), ref d.MaxCarsProbability);
                        }
                        if (ImGui.Button("Remove")) { }

                        ImGui.TreePop();
                    }
                }
            }
        }
    }
}
