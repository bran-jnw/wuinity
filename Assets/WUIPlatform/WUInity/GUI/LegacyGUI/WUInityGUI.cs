using UnityEngine;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI : MonoBehaviour
    {
        [System.Serializable]
        public class MenuButton
        {
            public string text;
            public Rect rect;

            static int MENU_COUNT;

            public MenuButton(int buttonHeight, string text)
            {
                int buttonIndex = MENU_COUNT;
                ++MENU_COUNT;

                rect = new Rect();
                this.text = text;

                rect.x = 10;
                rect.y = buttonIndex * (buttonHeight + 5) + 10;
                this.rect.height = buttonHeight;
                this.rect.width = 100;// text.Length * 8;
            }

            public bool Pressed()
            {
                return GUI.Button(rect, text);
            }
        }

        [SerializeField] Texture2D verticalColorGradient;
        [SerializeField] GUIStyle styleAlignedRight; 
        [SerializeField] GUIStyle styleAlignedCenter;

        public enum ActiveMenu { None, MainMenu, Map, Population, Evac, Traffic, Farsite, Output, Fire, Routing }
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
        MenuButton populationMenu;
        //GUIButton farsiteMenu = new GUIButton(1, buttonHeight, "Farsite Menu");
        MenuButton fireMenu;
        MenuButton evacMenu;
        MenuButton routingMenu;
        MenuButton trafficMenu;
        MenuButton outputMenu;
        MenuButton hideMenu;
        MenuButton exitMenu;
        MenuButton swapGUI;

        string[] wuiFilter = new string[] { ".wui" };
        string[] lcpFilter = new string[] { ".lcp" };
        string[] fuelModelsFilter = new string[] { ".fuel" };
        
        string[] populationFilter = new string[] { ".pop" };
        string[] gpwFilter = new string[] { ".gpw" };

        private void Start()
        {
            menuBarHeight = Screen.height - consoleHeight;

            mainMenu = new MenuButton(buttonHeight, "Main Menu");
            mapMenu = new MenuButton(buttonHeight, "Map");
            populationMenu = new MenuButton(buttonHeight, "Population");
            //GUIButton farsiteMenu = new GUIButton(1, buttonHeight, "Farsite Menu");
            fireMenu = new MenuButton(buttonHeight, "Fire spread");
            evacMenu = new MenuButton(buttonHeight, "Evacuation");
            routingMenu = new MenuButton(buttonHeight, "Routing");
            trafficMenu = new MenuButton(buttonHeight, "Traffic");
            outputMenu = new MenuButton(buttonHeight, "Output");
            hideMenu = new MenuButton(buttonHeight, "Hide Menu");
            exitMenu = new MenuButton(buttonHeight, "Exit");
            swapGUI = new MenuButton(buttonHeight, "New GUI");
        }

        string[] log;
        private void Update()
        {
            log = WUIEngine.GetLog();
        }

        void OnGUI()
        {
            //keep track if menu state has changed to kill things like the painter
            ActiveMenu lastMenu = menuChoice;

            //select menu
            GUI.Box(new Rect(0, 0, menuBarWidth, menuBarHeight), "");
            if (GUI.Button(mainMenu.rect, mainMenu.text) && WUIEngine.SIM.State != Simulation.SimulationState.Running)
            {
                menuChoice = ActiveMenu.MainMenu;
                WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
            }

            if(WUIEngine.DATA_STATUS.HaveInput)
            {
                if(WUIEngine.SIM.State != Simulation.SimulationState.Running)
                {
                    if (mapMenu.Pressed())
                    {
                        menuChoice = ActiveMenu.Map;
                    }

                    if (GUI.Button(populationMenu.rect, populationMenu.text))
                    {
                        menuChoice = ActiveMenu.Population;
                        //WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.GPW);
                    }

                    if (GUI.Button(evacMenu.rect, evacMenu.text))
                    {
                        menuChoice = ActiveMenu.Evac;
                        WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
                    }

                    if (GUI.Button(routingMenu.rect, routingMenu.text))
                    {
                        menuChoice = ActiveMenu.Routing;
                        WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
                    }

                    if (GUI.Button(trafficMenu.rect, trafficMenu.text))
                    {
                        menuChoice = ActiveMenu.Traffic;
                        WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
                    }

                    if (GUI.Button(fireMenu.rect, fireMenu.text))
                    {
                        menuChoice = ActiveMenu.Fire;
                        WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
                    }
                }                

                if (WUIEngine.SIM.HaveResults)
                {
                    if (GUI.Button(outputMenu.rect, outputMenu.text))
                    {
                        menuChoice = ActiveMenu.Output;
                        WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.None);
                    }
                }
            }    
            
            //if menu has changed we might have to kill a few things
            if(lastMenu != menuChoice)
            {
                WUInityEngine.INSTANCE.StopPainter();
                ResetFireGUI();
            }

            if (GUI.Button(exitMenu.rect, exitMenu.text))
            {
                Application.Quit();
            }

            if (GUI.Button(swapGUI.rect, swapGUI.text))
            {
                this.enabled = false;
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
            else if (menuChoice == ActiveMenu.Population)
            {
                PopulationMenu();
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
            /*else if (menuChoice == ActiveMenu.Farsite)
            {
                FarsiteMenu();
            }*/
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
                 
            if(log!= null)
            {
                for (int i = log.Length - 1; i >= 0; i--)
                {
                    GUILayout.Label(log[i]);
                }
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
            GUI.Box(new Rect(Screen.width - dataSampleWindowWidth, 0, dataSampleWindowWidth, dataSampleWindowHeight), WUInityEngine.INSTANCE.GetDataSampleString());
        }   
    }
}