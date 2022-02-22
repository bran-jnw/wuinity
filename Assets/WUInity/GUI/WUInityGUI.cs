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

        public enum ActiveMenu { None, MainMenu, Population, Evac, Traffic, Farsite, Output, Fire }
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
        //GUIButton farsiteMenu = new GUIButton(1, buttonHeight, "Farsite Menu");
        MenuButton fireMenu = new MenuButton(1, buttonHeight, "Fire spread");
        MenuButton gpwMenu = new MenuButton(2, buttonHeight, "Population");
        MenuButton evacMenu = new MenuButton(3, buttonHeight, "Evacuation");
        MenuButton trafficMenu = new MenuButton(4, buttonHeight, "Traffic");
        MenuButton outputMenu = new MenuButton(5, buttonHeight, "Output");
        MenuButton hideMenu = new MenuButton(6, buttonHeight, "Hide Menu");
        MenuButton exitMenu = new MenuButton(7, buttonHeight, "Exit");

        string[] wuiFilter = new string[] { ".wui" };
        string[] lcpFilter = new string[] { ".lcp" };
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
            if (GUI.Button(mainMenu.rect, mainMenu.text))
            {
                if (menuChoice == ActiveMenu.MainMenu)
                {
                    //menuChoice = ActiveMenu.None;
                }
                else
                {
                    menuChoice = ActiveMenu.MainMenu;
                    WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                }
            }

            if(WUInity.INSTANCE.haveInput)
            {
                if (GUI.Button(gpwMenu.rect, gpwMenu.text))
                {
                    if (menuChoice == ActiveMenu.Population)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.Population;
                        WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.GPW);
                    }
                }

                if (GUI.Button(evacMenu.rect, evacMenu.text))
                {
                    if (menuChoice == ActiveMenu.Evac)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.Evac;
                        WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                    }
                }

                if (GUI.Button(trafficMenu.rect, trafficMenu.text))
                {
                    if (menuChoice == ActiveMenu.Traffic)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.Traffic;
                        WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                    }
                }

                if (GUI.Button(fireMenu.rect, fireMenu.text))
                {
                    if (menuChoice == ActiveMenu.Fire)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.Fire;
                        WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                    }
                }

                if (WUInity.SIM.showResults)
                {
                    if (GUI.Button(outputMenu.rect, outputMenu.text))
                    {
                        if (menuChoice == ActiveMenu.Output)
                        {
                            //menuChoice = ActiveMenu.None;
                        }
                        else
                        {
                            menuChoice = ActiveMenu.Output;
                            WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
                        }
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

            //console
            GUI.Box(new Rect(0, Screen.height - consoleHeight, Screen.width, consoleHeight), "");
            GUI.BeginGroup(new Rect(0, Screen.height - consoleHeight, Screen.width, consoleHeight), "");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(consoleHeight));
            List<string> log = WUInity.SIM.GetSimLog();
            for (int i = log.Count - 1; i >= 0; i--)
            {                
                GUILayout.Label(log[i]);
            }            
            GUILayout.EndScrollView();
            GUI.EndGroup();

            //call correct menu
            if (menuChoice == ActiveMenu.MainMenu)
            {
                MainMenu();
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

            DataSampleWindow();
        }

        const int dataSampleWindowWidth = 600;
        const int dataSampleWindowHeight = 20;
        private void DataSampleWindow()
        {
            
            GUI.Box(new Rect(Screen.width - dataSampleWindowWidth, 0, dataSampleWindowWidth, dataSampleWindowHeight), WUInity.INSTANCE.GetDataSampleString());
        }   
    }
}