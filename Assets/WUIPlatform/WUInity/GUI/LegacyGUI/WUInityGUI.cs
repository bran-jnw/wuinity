using UnityEngine;
using PREACT;
using System.Collections.Generic;

namespace WUInity.UI
{
    public partial class WUInityGUI : MonoBehaviour
    {
        [System.Serializable]
        public class MenuButton
        {
            private string _text;
            private Rect _rect;

            static int MENU_COUNT;

            public MenuButton(int buttonHeight, string text)
            {
                int buttonIndex = MENU_COUNT;
                ++MENU_COUNT;

                _rect = new Rect();
                this._text = text;

                _rect.x = 10;
                _rect.y = buttonIndex * (buttonHeight + 5) + 10;
                this._rect.height = buttonHeight;
                this._rect.width = 100;// text.Length * 8;
            }

            public bool Pressed()
            {
                return GUI.Button(_rect, _text);
            }
        }

        [SerializeField] Texture2D verticalColorGradient;
        [SerializeField] GUIStyle styleAlignedRight; 
        [SerializeField] GUIStyle styleAlignedCenter;

        public enum ActiveMenu { None, MainMenu, Map, Tools, Evac, Traffic, Farsite, Output, Fire, Routing }
        ActiveMenu menuChoice = ActiveMenu.MainMenu;

        int menuBarHeight;
        const int menuBarWidth = 120;

        const int buttonHeight = 20;
        const int subMenuXOrigin = menuBarWidth;
        const int buttonColumnStart = subMenuXOrigin + 20;

        int columnWidth = 200;

        Vector2 scrollPosition;
        const int consoleHeight = 160;


        MenuButton mainMenu;
        MenuButton mapMenu;
        MenuButton toolsMenu;
        MenuButton fireMenu;
        MenuButton evacMenu;
        MenuButton routingMenu;
        MenuButton trafficMenu;
        MenuButton outputMenu;
        MenuButton hideMenu;
        MenuButton exitMenu;
        MenuButton swapGUI;

        string[] wuiFilter = new string[] { ".wui" };
        string[] lcpFilter = new string[] { ".lcp", ".tif", ".tiff" };
        string[] fuelModelsFilter = new string[] { ".fuel" };
        
        string[] populationMapFilter = new string[] { ".pop" };
        string[] gpwFilter = new string[] { ".gpw" };


        private void Start()
        {
            menuBarHeight = Screen.height - consoleHeight;

            //this builds the order
            mainMenu = new MenuButton(buttonHeight, "Main Menu");            
            mapMenu = new MenuButton(buttonHeight, "Map");  
            fireMenu = new MenuButton(buttonHeight, "Fire spread");
            evacMenu = new MenuButton(buttonHeight, "Evacuation");
            routingMenu = new MenuButton(buttonHeight, "Routing");
            trafficMenu = new MenuButton(buttonHeight, "Traffic");
            toolsMenu = new MenuButton(buttonHeight, "Tools");
            outputMenu = new MenuButton(buttonHeight, "Output");
            hideMenu = new MenuButton(buttonHeight, "Hide Menu");
            exitMenu = new MenuButton(buttonHeight, "Exit");
            swapGUI = new MenuButton(buttonHeight, "New GUI");
        }

        WUInityManager _wuinityManager;
        Engine _engine;
        private PREACT.IO.PREACTInput _input;
        private PREACT.Runtime.WorkingData _workingData;
        public void SetManager(WUInityManager wuinityManager, Engine engine, PREACT.Runtime.WorkingData workingData)
        {
            _wuinityManager = wuinityManager;
            _engine = engine;
            _workingData = workingData;
        }

        public void UpdateInput(PREACT.IO.PREACTInput input)
        {
            _input = input;
            SetDirty();
        }

        List<string> _messages = new List<string>();
        public void NewMessage(string message)
        {
            _messages.Add(message);
        }

        bool _simulationRunning = false;
        public void SimulationStarted()
        {
            _simulationRunning = true;
        }

        public void SimulationStopped()
        {
            _simulationRunning = true;
        }

        void OnGUI()
        {
            if (_wuinityManager == null)
            {
                return;
            }

            //keep track if menu state has changed to kill things like the painter
            ActiveMenu lastMenu = menuChoice;

            //select menu
            GUI.Box(new Rect(0, 0, menuBarWidth, menuBarHeight), "");
            if (mainMenu.Pressed() && !_simulationRunning)
            {
                menuChoice = ActiveMenu.MainMenu;
                _wuinityManager.SetSampleMode(DataSampleMode.None);
            }
            
            if(!_simulationRunning)
            {
                if(_input != null)
                {
                    if (mapMenu.Pressed())
                    {
                        menuChoice = ActiveMenu.Map;
                    }

                    if (evacMenu.Pressed())
                    {
                        menuChoice = ActiveMenu.Evac;
                        _wuinityManager.SetSampleMode(DataSampleMode.None);
                    }

                    if (routingMenu.Pressed())
                    {
                        menuChoice = ActiveMenu.Routing;
                        _wuinityManager.SetSampleMode(DataSampleMode.None);
                    }

                    if (trafficMenu.Pressed())
                    {
                        menuChoice = ActiveMenu.Traffic;
                        _wuinityManager.SetSampleMode(DataSampleMode.None);
                    }

                    if (fireMenu.Pressed())
                    {
                        menuChoice = ActiveMenu.Fire;
                        _wuinityManager.SetSampleMode(DataSampleMode.None);
                    }
                }                

                if (toolsMenu.Pressed())
                {
                    menuChoice = ActiveMenu.Tools;
                }
            }                

            if (_wuinityManager.Engine.Simulation != null && _wuinityManager.Engine.Simulation.HaveResults)
            {
                if (outputMenu.Pressed())
                {
                    menuChoice = ActiveMenu.Output;
                    _wuinityManager.SetSampleMode(DataSampleMode.None);
                }
            }
            
            //if menu has changed we might have to kill a few things
            if(lastMenu != menuChoice)
            {
                _wuinityManager.StopPainter();
                ResetFireGUI();
            }

            if (exitMenu.Pressed())
            {
                _wuinityManager.StopSimulations();
                Application.Quit();
            }

            //call correct menu
            if (menuChoice == ActiveMenu.MainMenu)
            {
                MainMenu();
            }
            else if (menuChoice == ActiveMenu.Map)
            {
                MapMenu();
            }
            else if (menuChoice == ActiveMenu.Tools)
            {
                ToolsMenu();
            }
            else if (menuChoice == ActiveMenu.Evac)
            {
                EvacMenu();
            }
            else if (menuChoice == ActiveMenu.Routing)
            {
                RoutingMenu();
            }
            else if (menuChoice == ActiveMenu.Traffic)
            {
                TrafficMenu();
            }
            else if (menuChoice == ActiveMenu.Fire)
            {
                FireMenu();
            }
            else if (menuChoice == ActiveMenu.Output)
            {
                OutputMenu();
            }

            UpdateConsole();
            DataSampleWindow();
        }
                
        void UpdateConsole()
        {
            //console
            GUI.Box(new Rect(0, Screen.height - consoleHeight, Screen.width, consoleHeight), "");
            GUI.BeginGroup(new Rect(0, Screen.height - consoleHeight, Screen.width, consoleHeight), "");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(consoleHeight));
            
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(_messages[i]);
            }
            
            GUILayout.EndScrollView();
            GUI.EndGroup();
        }

        public void SetDirty()
        {
            mainMenuDirty = true;
            mapMenuDirty = true;
            populationMenuDirty = true;
            evacMenuDirty = true;
            trafficMenuDirty = true;
        }

        const int dataSampleWindowWidth = 600;
        const int dataSampleWindowHeight = 20;
        private void DataSampleWindow()
        {
            GUI.Box(new Rect(Screen.width - dataSampleWindowWidth, 0, dataSampleWindowWidth, dataSampleWindowHeight), _wuinityManager.GetDataSampleString());
        }   
    }
}