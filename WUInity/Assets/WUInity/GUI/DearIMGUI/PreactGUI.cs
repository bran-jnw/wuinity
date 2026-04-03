using ImGuiNET;
using PREACT;
using System.Collections.Generic;
using System.IO;
using UImGui;
using UnityEngine;
using WUInity;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public class PreactGUI : MonoBehaviour
    {
        [SerializeField]
        private bool darkTheme = false;
        [SerializeField]
        string fontFilePath = Path.Combine(Application.dataPath, $"WUInity\\GUI\\DearIMGUI\\Fonts", "AdobeClean-Regular.otf"); //MS_Sans_Serif.ttf

        static WUInityManager _wuinityManager;
        static Engine _engine;

        public static WUInityManager WUInity { get => _wuinityManager; }
        public static Engine Engine { get => _engine; }

        private void OnEnable()
        {
            UImGuiUtility.Layout += OnLayout;
            ApplyTheme();
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
        }

        //draws menus
        private void OnLayout(UImGui.UImGui obj)
        {
            if(_wuinityManager == null)
            {
                return;
            }

            MenuBarGUI.Draw();
            NewScenarioWindow.Draw();
            ScenarioEditorGUI.Draw();
        }

        public void SetManager(WUInityManager wuinityManager, Engine engine, PREACT.Runtime.WorkingData workingData)
        {
            _wuinityManager = wuinityManager;
            _engine = engine;
            //_workingData = workingData;
        }

        public void UpdateInput(PREACT.Input.PREACTInput input)
        {
            ScenarioEditorGUI.SetInput(input);
            //_workingData.SetSimulatonData(input.Simulation.LowerLeftLatLon, input.Simulation.DomainSize);
        }

        LinkedList<string> _messages = new LinkedList<string>();
        public void NewMessage(string message)
        {
            _messages.AddFirst(message);
            if (_messages.Count > 50)
            {
                _messages.RemoveLast();
            }
        }

        bool _simulationRunning = false;
        public void SimulationStarted()
        {
            _messages.Clear();
            _simulationRunning = true;
        }

        public void SimulationsFinished()
        {
            _simulationRunning = false;
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
    }
}
