using Assets.WUInity.GUI.DearIMGUI.Editors;
using ImGuiNET;
using NUnit.Framework.Internal;
using PREACT;
using PREACT.Evacuation;
using PREACT.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI
{ 
    public static class EvacuationTabs
    {
        public static string[] TrafficModulesStrings;
        public static string[] PedestrianModulesStrings;
        static int _pedestrianModuleIndex, _trafficModuleIndex;

        

        static EvacuationTabs()
        {
            PedestrianModulesStrings = Enum.GetNames(typeof(PedestrianModuleInput.PedestrianModules));
            TrafficModulesStrings = Enum.GetNames(typeof(TrafficModuleInput.TrafficModules));            
        }

        public static void Draw(PREACTInput input, EvacuationInput eInput, PedestrianModuleInput pInput, TrafficModuleInput tInput)
        {
            if (ImGui.BeginTabItem("General"))
            {
                ImGui.Checkbox(nameof(eInput.UseTriggerBufferEvacuation), ref eInput.UseTriggerBufferEvacuation);
                if(eInput.UseTriggerBufferEvacuation)
                {
                    if (ImGui.Button("Select trigger buffer file")) { }
                    ImGui.InputText(nameof(eInput.TriggerBufferFile), ref eInput.TriggerBufferFile, 256);                    
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Evacuation modules"))
            {
                ImGui.SeparatorText("Pedestrian");
                _pedestrianModuleIndex = (int)pInput.Module;
                ImGui.Combo(nameof(PedestrianModuleInput.PedestrianModules), ref _pedestrianModuleIndex, PedestrianModulesStrings, PedestrianModulesStrings.Length);
                pInput.Module = (PedestrianModuleInput.PedestrianModules)_pedestrianModuleIndex;
                if (pInput.Module != PedestrianModuleInput.PedestrianModules.None)
                {
                    if (ImGui.Button("Module settings")) { }
                }

                ImGui.SeparatorText("Traffic");
                _trafficModuleIndex = (int)tInput.Module;
                ImGui.Combo(nameof(TrafficModuleInput.TrafficModules), ref _trafficModuleIndex, TrafficModulesStrings, TrafficModulesStrings.Length);
                tInput.Module = (TrafficModuleInput.TrafficModules)_trafficModuleIndex;
                if (tInput.Module != TrafficModuleInput.TrafficModules.None)
                {
                    if (ImGui.Button("Module settings")) { }
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Population"))
            {
                if (ImGui.Button("Select population file")) { }
                ImGui.InputText(nameof(input.Population.PopulationFile), ref input.Population.PopulationFile, 256);
                ImGui.Checkbox(nameof(input.Population.CullOutsideGroups), ref input.Population.CullOutsideGroups);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Demographics"))
            {
                if (ImGui.Button("New demographics")) { DemographicsInputEditorWindow.Open(input.Population.Demographics, null); }

                foreach (KeyValuePair<string, DemographicsInput> kV in input.Population.Demographics)
                {
                    DemographicsInput demo = kV.Value;
                    if (ImGui.TreeNode(demo.Name))
                    {                        
                        if (ImGui.Button("Edit")) { DemographicsInputEditorWindow.Open(input.Population.Demographics, demo); }
                        if (ImGui.Button("Remove")) { }

                        ImGui.TreePop();
                    }
                }

                ImGui.EndTabItem();
            }            

            if (ImGui.BeginTabItem("Destinations"))
            {
                if (ImGui.Button("New destination")) { DestinationInputEditWindow.Open(eInput.EvacuationDestinationInputs, null); }

                foreach (KeyValuePair<string, EvacuationDestinationInput> kV in eInput.EvacuationDestinationInputs)
                {
                    EvacuationDestinationInput dest = kV.Value;
                    if (ImGui.TreeNode(dest.Name))
                    {
                        if (ImGui.Button("Edit")) { DestinationInputEditWindow.Open(eInput.EvacuationDestinationInputs, dest); }
                        ImGui.SameLine();
                        if (ImGui.Button("Zoom to")) { }
                        ImGui.SameLine();
                        if (ImGui.Button("Remove")) { }

                        ImGui.TreePop();
                    }
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Response curves"))
            {
                if (ImGui.Button("New response curve")) { }

                foreach (KeyValuePair<string, ResponseCurve> kV in eInput.ResponseCurves)
                {
                    ResponseCurve rC = kV.Value;
                    if (ImGui.TreeNode(rC.Name))
                    {
                        if (ImGui.Button("Edit")) { }
                        if (ImGui.Button("Remove")) { }

                        ImGui.TreePop();
                    }
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Evacuation groups"))
            {
                if (ImGui.Button("New evacuation group")) { }

                foreach (KeyValuePair<string, EvacuationGroupInput> kV in eInput.EvacuationGroupInputs)
                {
                    EvacuationGroupInput eGI = kV.Value; ;
                    if (ImGui.TreeNode(eGI.Name))
                    {
                        if (ImGui.Button("Edit")) { }
                        if (ImGui.Button("Zoom to")) { }
                        if (ImGui.Button("Remove")) { }

                        ImGui.TreePop();
                    }
                }

                ImGui.EndTabItem();
            }       
        }
    }
}
