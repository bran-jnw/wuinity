using UnityEngine;
using ImGuiNET;
using UImGui;
using System.IO;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public class PREACT_IMGUI : MonoBehaviour
    {
        [SerializeField]
        private bool darkTheme = false;
        [SerializeField]
        string fontFilePath = Path.Combine(Application.dataPath, $"WUInity\\GUI\\DearIMGUI\\Fonts", "AdobeClean-Regular.otf"); //MS_Sans_Serif.ttf

        private void OnEnable()
        {
            UImGuiUtility.Layout += OnLayout;
            ApplyTheme();
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
        }

        //draws menu
        private void OnLayout(UImGui.UImGui obj)
        {
            MainMenu();
            NewScenario.Draw();
        }

        public void ApplyTheme()
        {
            Themes.ApplyAdobeSpectrum(darkTheme);
        }

        public void AddFont(ImGuiIOPtr io)
        {
            // Clear default fonts if you want only your custom one            
            io.Fonts.Clear();
            io.Fonts.AddFontFromFileTTF(fontFilePath, 14);
        }

        private void MainMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New scenario")) { NewScenario._draw = true; }
                    if (ImGui.MenuItem("Load scenario")) { }
                    if (ImGui.MenuItem("Save scenario")) { }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Data download"))
                {
                    //IMGUI_DEMO_MARKER("Menu/Edit");
                    if (ImGui.MenuItem("Download OSM data")) { }
                    if (ImGui.MenuItem("Download WorldPop data")) { }
                    if (ImGui.MenuItem("Download Landfire data")) { }
                    //ImGui.Separator();

                    ImGui.EndMenu();
                }
                
                ImGui.EndMainMenuBar();
            }
        }
    }
}
