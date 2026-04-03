using ImGuiNET;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI
{    
    public static class SimulationInputGUI
    {
        public static Vector2 _latLon, _domainSize;

        public static void Draw(PREACT.Input.SimulationInput input)
        {
            ImGui.InputText(nameof(input.Name), ref input.Name, 128);
            ImGui.InputFloat2(nameof(input.LowerLeftLatLon), ref _latLon);
            ImGui.InputFloat2(nameof(input.DomainSize), ref _domainSize);
            ImGui.InputFloat(nameof(input.DeltaTime), ref input.DeltaTime);
        }
    }
}
