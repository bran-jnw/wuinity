using ImGuiNET;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI
{    
    public static class SimulationInputTab
    {
        private static Vector2 _latLon, _domainSize, _newLatLon, _newDomainSize;

        public static void Draw(PREACT.Input.SimulationInput input)
        {
            ImGui.InputText(nameof(input.Name), ref input.Name, 128);
        
            _latLon.x = (float)input.LowerLeftLatLon.x;
            _latLon.y = (float)input.LowerLeftLatLon.y;
            ImGui.InputFloat2(nameof(input.LowerLeftLatLon), ref _latLon);
            _domainSize.x = (float)input.DomainSize.x;
            _domainSize.y = (float)input.DomainSize.y;
            ImGui.InputFloat2(nameof(input.DomainSize), ref _domainSize);
            if(ImGui.TreeNode("Redefine domain"))
            {
                ImGui.InputFloat2("New " + nameof(input.LowerLeftLatLon), ref _newLatLon);
                ImGui.InputFloat2("New " + nameof(input.DomainSize), ref _newDomainSize);
                if (ImGui.Button("Apply")) 
                {
                    input.LowerLeftLatLon = new PREACT.Math.Vector2d(_newLatLon.x, _newLatLon.y);
                    input.DomainSize = new PREACT.Math.Vector2d(_newDomainSize.x, _newDomainSize.y);
                }

                ImGui.TreePop();
            }

            ImGui.InputFloat(nameof(input.DeltaTime), ref input.DeltaTime);

            CustomTypes.InputDateTimePopup(nameof(input.StartDateTime), ref input.StartDateTime);
            CustomTypes.InputDateTimePopup(nameof(input.EndDateTime), ref input.EndDateTime);

            ImGui.Checkbox(nameof(input.StopWhenEvacuated), ref input.StopWhenEvacuated);
        }

        private static void DrawVector2d(string name, ref PREACT.Math.Vector2d value)
        {
            Vector2 displayValue = new Vector2((float)value.x, (float)value.y);
            ImGui.InputFloat2(name, ref displayValue);
            value.x = displayValue.x;
            value.y = displayValue.y;
        }
    }
}
