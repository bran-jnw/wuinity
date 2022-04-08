using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI : MonoBehaviour
    {
        [System.Serializable]
        public class MenuButton
        {
            public string text;
            public Rect rect;

            public MenuButton(int buttonIndex, int buttonHeight, string text)
            {
                rect = new Rect();
                this.text = text;

                rect.x = 10;
                rect.y = buttonIndex * (buttonHeight + 5) + 10;
                this.rect.height = buttonHeight;
                this.rect.width = 100;// text.Length * 8;
            }
        }

        [SerializeField] GUIStyle style;

        public enum ActiveMenu { None, MainMenu, Map, Population, Evac, Traffic, Farsite, Output, Fire }
        ActiveMenu menuChoice = ActiveMenu.MainMenu;

        int menuBarHeight;
        const int menuBarWidth = 120;

        const int buttonHeight = 20;
        const int subMenuXOrigin = menuBarWidth;
        const int buttonColumnStart = subMenuXOrigin + 20;

        int columnWidth = 200;

        Vector2 scrollPosition;
        const int consoleHeight = 160;


        MenuButton mainMenu = new MenuButton(0, buttonHeight, "Main Menu");
        MenuButton mapMenu = new MenuButton(1, buttonHeight, "Map");
        MenuButton populationMenu = new MenuButton(2, buttonHeight, "Population");
        //GUIButton farsiteMenu = new GUIButton(1, buttonHeight, "Farsite Menu");
        MenuButton fireMenu = new MenuButton(3, buttonHeight, "Fire spread");
        MenuButton evacMenu = new MenuButton(4, buttonHeight, "Evacuation");
        MenuButton trafficMenu = new MenuButton(5, buttonHeight, "Traffic");
        MenuButton outputMenu = new MenuButton(6, buttonHeight, "Output");
        MenuButton hideMenu = new MenuButton(7, buttonHeight, "Hide Menu");
        MenuButton exitMenu = new MenuButton(8, buttonHeight, "Exit");

        string[] wuiFilter = new string[] { ".wui" };
        string[] lcpFilter = new string[] { ".lcp" };
        string[] fuelModelsFilter = new string[] { ".fuel" };
        string[] osmFilter = new string[] { ".pbf" };
        string[] populationFilter = new string[] { ".pop" };
        string[] gpwFilter = new string[] { ".gpw" };

        private void Start()
        {
            menuBarHeight = Screen.height - consoleHeight;
        }

        void OnGUI()
        {
            //select menu
            GUI.Box(new Rect(0, 0, menuBarWidth, menuBarHeight), "");
            if (GUI.Button(mainMenu.rect, mainMenu.text) && !WUInity.SIM.isRunning)
            {
                menuChoice = ActiveMenu.MainMenu;
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
            }

            if(WUInity.DATA_STATUS.haveInput)
            {
                if (GUI.Button(mapMenu.rect, mapMenu.text) && !WUInity.SIM.isRunning)
                {                    
                    menuChoice = ActiveMenu.Map;
                }

                if (GUI.Button(populationMenu.rect, populationMenu.text) && !WUInity.SIM.isRunning)
                {
                    menuChoice = ActiveMenu.Population;
                    //WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.GPW);
                }

                if (GUI.Button(evacMenu.rect, evacMenu.text) && !WUInity.SIM.isRunning)
                {
                    menuChoice = ActiveMenu.Evac;
                    WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                }

                if (GUI.Button(trafficMenu.rect, trafficMenu.text) && !WUInity.SIM.isRunning)
                {
                    menuChoice = ActiveMenu.Traffic;
                    WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                }

                if (GUI.Button(fireMenu.rect, fireMenu.text) && !WUInity.SIM.isRunning)
                {
                    menuChoice = ActiveMenu.Fire;
                    WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                }

                if (WUInity.SIM.haveResults)
                {
                    if (GUI.Button(outputMenu.rect, outputMenu.text))
                    {
                        menuChoice = ActiveMenu.Output;
                        WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                    }
                }
            }                  

            /*if (GUI.Button(hideMenu.rect, hideMenu.text))
            {
                menuChoice = ActiveMenu.None;
            }*/

            if (GUI.Button(exitMenu.rect, exitMenu.text))
            {
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
            else if (menuChoice == ActiveMenu.Population)
            {
                PopulationMenu();
            }
            else if (menuChoice == ActiveMenu.Evac)
            {
                EvacMenu();
            }
            else if (menuChoice == ActiveMenu.Traffic)
            {
                TrafficMenu();
            }
            else if (menuChoice == ActiveMenu.Farsite)
            {
                FarsiteMenu();
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
            List<string> log = WUInity.GetLog();
            for (int i = log.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(log[i]);
            }
            GUILayout.EndScrollView();
            GUI.EndGroup();
        }

        public void SetGUIDirty()
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
            GUI.Box(new Rect(Screen.width - dataSampleWindowWidth, 0, dataSampleWindowWidth, dataSampleWindowHeight), WUInity.INSTANCE.GetDataSampleString());
        }   
    }
}