using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public class WUInityGUI : MonoBehaviour
    {
        [System.Serializable]
        public class GUIButton
        {
            public string text;
            public Rect rect;

            public GUIButton(int buttonIndex, int buttonHeight, string text)
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

        public enum ActiveMenu { None, MainMenu, GPWMenu, EvacMenu, TrafficMenu, FarsiteMenu, OutputMenu, FireMenu }
        ActiveMenu menuChoice = ActiveMenu.MainMenu;

        static int buttonHeight = 20;
        static int buttonColumnStart = 140;

        GUIButton mainMenu = new GUIButton(0, buttonHeight, "Main Menu");
        GUIButton farsiteMenu = new GUIButton(1, buttonHeight, "Farsite Menu");
        GUIButton fireMenu = new GUIButton(2, buttonHeight, "Fire spread");
        GUIButton gpwMenu = new GUIButton(3, buttonHeight, "GPW Menu");
        GUIButton evacMenu = new GUIButton(4, buttonHeight, "Evac Menu");
        GUIButton trafficMenu = new GUIButton(5, buttonHeight, "Traffic Menu");
        GUIButton outputMenu = new GUIButton(6, buttonHeight, "Output Menu");
        GUIButton hideMenu = new GUIButton(7, buttonHeight, "Hide Menu");
        GUIButton exitMenu = new GUIButton(8, buttonHeight, "Exit");

        string[] wuiFilter = new string[] { ".wui" };
        string[] lcpFilter = new string[] { ".lcp" };

        void OnGUI()
        {
            //select menu
            GUI.Box(new Rect(0, 0, 120, Screen.height), "");
            if (GUI.Button(mainMenu.rect, mainMenu.text))
            {
                if (menuChoice == ActiveMenu.MainMenu)
                {
                    //menuChoice = ActiveMenu.None;
                }
                else
                {
                    menuChoice = ActiveMenu.MainMenu;
                    WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.None);
                }
            }

            if(WUInity.WUINITY.haveInput)
            {
                if (GUI.Button(gpwMenu.rect, gpwMenu.text))
                {
                    if (menuChoice == ActiveMenu.GPWMenu)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.GPWMenu;
                        WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.GPW);
                    }
                }

                if (GUI.Button(evacMenu.rect, evacMenu.text))
                {
                    if (menuChoice == ActiveMenu.EvacMenu)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.EvacMenu;
                        WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.None);
                    }
                }

                if (GUI.Button(trafficMenu.rect, trafficMenu.text))
                {
                    if (menuChoice == ActiveMenu.TrafficMenu)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.TrafficMenu;
                        WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.None);
                    }
                }

                if (GUI.Button(farsiteMenu.rect, farsiteMenu.text))
                {
                    if (menuChoice == ActiveMenu.FarsiteMenu)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.FarsiteMenu;
                        WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.Farsite);
                    }
                }

                if (GUI.Button(fireMenu.rect, fireMenu.text))
                {
                    if (menuChoice == ActiveMenu.FireMenu)
                    {
                        //menuChoice = ActiveMenu.None;
                    }
                    else
                    {
                        menuChoice = ActiveMenu.FireMenu;
                        WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.None);
                    }
                }

                if (WUInity.WUINITY_SIM.showResults)
                {
                    if (GUI.Button(outputMenu.rect, outputMenu.text))
                    {
                        if (menuChoice == ActiveMenu.OutputMenu)
                        {
                            //menuChoice = ActiveMenu.None;
                        }
                        else
                        {
                            menuChoice = ActiveMenu.OutputMenu;
                            WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.None);
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

            //info menu
            GUI.Box(new Rect(120, Screen.height - buttonHeight, Screen.width - 120, buttonHeight), infoMessage);

            //call correct menu
            if (menuChoice == ActiveMenu.MainMenu)
            {
                MainMenu();
            }
            else if (menuChoice == ActiveMenu.GPWMenu)
            {
                GPWMenu();
            }
            else if (menuChoice == ActiveMenu.EvacMenu)
            {
                EvacMenu();
            }
            else if (menuChoice == ActiveMenu.TrafficMenu)
            {
                TrafficMenu();
            }
            else if (menuChoice == ActiveMenu.FarsiteMenu)
            {
                FarsiteMenu();
            }
            else if (menuChoice == ActiveMenu.FireMenu)
            {
                FireMenu();
            }
            else if (menuChoice == ActiveMenu.OutputMenu)
            {
                OutputMenu();
            }
        }

        string dT, nrRuns, Lat, Long, sizeX, sizeY, zoom;
        bool mainInputDirty = true;
        void MainMenu()
        {
            WUInityInput wO = WUInity.WUINITY_IN;
            if (mainInputDirty)
            {
                mainInputDirty = false;
                dT = wO.deltaTime.ToString();
                nrRuns = wO.numberOfRuns.ToString();
                Lat = wO.lowerLeftLatLong.x.ToString();
                Long = wO.lowerLeftLatLong.y.ToString();
                sizeX = wO.size.x.ToString();
                sizeY = wO.size.y.ToString();
                zoom = wO.zoomLevel.ToString();
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - buttonHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load inputs"))
            {
                OpenLoadInput();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load defaults"))
            {
                SaveLoadWUI.LoadDefaultInputs();
                mainInputDirty = true;
            }
            ++buttonIndex;

            if(!WUInity.WUINITY.haveInput)
            {
                return;
            }

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Simulation name:");
            ++buttonIndex;
            wO.simName = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), wO.simName);
            ++buttonIndex;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save input file"))
            {
                if(wO.simName == "default")
                {
                    OpenSaveInput();
                }
                else
                {
                    ParseMainData(wO);
                    SaveLoadWUI.SaveInput();
                }                
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save as"))
            {
                OpenSaveInput();
            }
            ++buttonIndex;

            //LatLong
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map LL Lat.:");
            ++buttonIndex;
            Lat = GUI.TextField(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Lat);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map LL Long.:");
            ++buttonIndex;
            Long = GUI.TextField(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Long);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map size x [m]:");
            ++buttonIndex;
            sizeX = GUI.TextField(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sizeX);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map size y [m]:");
            ++buttonIndex;
            sizeY = GUI.TextField(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sizeY);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map zoom level:");
            ++buttonIndex;
            zoom = GUI.TextField(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), zoom);
            ++buttonIndex;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load Map"))
            {
                ParseMainData(wO);
                WUInity.WUINITY.LoadMapbox();
            }
            ++buttonIndex;

            buttonIndex += 2;
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            //number of runs
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
            ++buttonIndex;
            nrRuns = GUI.TextField(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
            ++buttonIndex;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData(wO);
                WUInity.WUINITY_SIM.LogMessage("Simulation started, please wait.");
                if (WUInity.WUINITY_SIM.StartSimFromGUI())
                {
                    menuChoice = ActiveMenu.OutputMenu;
                }
            }
            ++buttonIndex;
        }

        void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            WUInityInput wO = WUInity.WUINITY_IN;
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.simName + ".wui", "Save file", "Save");
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load file", "Load");
        }

        void OpenLoadLCP()
        {
            FileBrowser.SetFilters(false, lcpFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadLCP, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load file", "Load");
        }

        void SaveInput(string[] paths)
        {
            WUInity.WORKING_FILE = paths[0];
            WUInityInput wO = WUInity.WUINITY_IN;
            ParseMainData(wO);
            SaveLoadWUI.SaveInput();
        }

        void LoadInput(string[] paths)
        {
            SaveLoadWUI.LoadInput(paths[0]);
            mainInputDirty = true;
        }

        void LoadLCP(string[] paths)
        {
            WUInityInput wO = WUInity.WUINITY_IN; 
            wO.fire.lcpFile = paths[0];
            WUInity.WUINITY_SIM.UpdateLCPFile();
        }

        void CancelSaveLoad()
        {

        }

        void ParseMainData(WUInityInput wO)
        {
            ParseEvacInput();
            ParseTrafficInput();

            if(mainInputDirty)
            {
                return;
            }

            float.TryParse(dT, out wO.deltaTime);
            int.TryParse(nrRuns, out wO.numberOfRuns);
            double.TryParse(Lat, out wO.lowerLeftLatLong.x);
            double.TryParse(Long, out wO.lowerLeftLatLong.y);
            double.TryParse(sizeX, out wO.size.x);
            double.TryParse(sizeY, out wO.size.y);
            int.TryParse(zoom, out wO.zoomLevel);            
        }

        bool gpwInputDirty = true;
        void GPWMenu()
        {
            GPWInput gpwIn = WUInity.WUINITY_IN.gpw;
            if (gpwInputDirty)
            {
                gpwInputDirty = false;
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - buttonHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW data filename:");
            ++buttonIndex;
            gpwIn.localGPWFilename = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), gpwIn.localGPWFilename);
            ++buttonIndex;

            /*if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Apply Changes"))
            {
                gpwIn.localGPWFilename = gpwName;
            }
            ++buttonIndex;*/

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load GPW Data"))
            {
                WUInity.WUINITY.LoadGPW();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/Hide GPW data"))
            {
                WUInity.WUINITY.ToggleGPWViewer();
            }
            ++buttonIndex;
        }


        string totalPop, cellSize, maxCars, maxCarsProb, minHousehold, maxHousehold, walkingDistMod, walkSpeedMin, walkSpeedMax, walkSpeedMod, evacOrderTime;
        bool evacInputDirty = true;
        int columnWidth = 200;
        void EvacMenu()
        {
            EvacInput eO = WUInity.WUINITY_IN.evac;
            if (evacInputDirty)
            {
                evacInputDirty = false;
                totalPop = eO.totalPopulation.ToString();
                cellSize = eO.routeCellSize.ToString();
                maxCars = eO.maxCars.ToString();
                maxCarsProb = eO.maxCarsChance.ToString();
                minHousehold = eO.minHouseholdSize.ToString();
                maxHousehold = eO.maxHouseholdSize.ToString();
                walkSpeedMin = eO.walkingSpeedMinMax.x.ToString();
                walkSpeedMax = eO.walkingSpeedMinMax.y.ToString();
                walkSpeedMod = eO.walkingSpeedModifier.ToString();
                walkingDistMod = eO.walkingDistanceModifier.ToString();
                evacOrderTime = eO.evacuationOrderStart.ToString();

            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - buttonHeight), "");
            int buttonIndex = 0;

            int buttonColumnStart = 140;

            //
            eO.overrideTotalPopulation = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), eO.overrideTotalPopulation, "Override population count");
            ++buttonIndex;

            //
            if(eO.overrideTotalPopulation)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population [-]:");
                ++buttonIndex;
                totalPop = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), totalPop);
                ++buttonIndex;
            }            

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size [m]");
            ++buttonIndex;
            cellSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), cellSize);
            ++buttonIndex;

            //
            eO.allowMoreThanOneCar = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), eO.allowMoreThanOneCar, "Allow more than one car");
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Max cars [-]");
            ++buttonIndex;
            maxCars = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), maxCars);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Probability for max cars");
            ++buttonIndex;
            maxCarsProb = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), maxCarsProb);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Min. persons per household");
            ++buttonIndex;
            minHousehold = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), minHousehold);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Max. persons per household");
            ++buttonIndex;
            maxHousehold = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), maxHousehold);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Min. walking speed");
            ++buttonIndex;
            walkSpeedMin = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkSpeedMin);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Max. walking speed");
            ++buttonIndex;
            walkSpeedMax = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkSpeedMax);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Walking speed mod.");
            ++buttonIndex;
            walkSpeedMod = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkSpeedMod);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Walking distance mod.");
            ++buttonIndex;
            walkingDistMod = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkingDistMod);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Evacuation order [time after fire]");
            ++buttonIndex;
            evacOrderTime = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), evacOrderTime);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run evac verification"))
            {
                Evac.MacroHumanVerification.RunVerification();
            }
            ++buttonIndex;

            if (WUInity.WUINITY.IsPainterActive())
            {
                for (int i = 0; i < WUInity.WUINITY_IN.evac.evacGroups.Length; i++)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint group " + (i + 1)))
                    {
                        WUInity.PAINTER.SetEvacGroupColor(i);
                    }
                    ++buttonIndex;
                }

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop painting"))
                {
                    WUInity.WUINITY.StopPainter();
                }
                ++buttonIndex;
            }
            else
            {    
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint evac group"))
                {
                    WUInity.WUINITY.StartPainter(WUInityPainter.PaintMode.EvacGroup);
                }
                ++buttonIndex;
            }  
        }

        void ParseEvacInput()
        {
            if (evacInputDirty)
            {
                return;
            }

            EvacInput eO = WUInity.WUINITY_IN.evac;

            int.TryParse(totalPop, out eO.totalPopulation);
            float.TryParse(cellSize, out eO.routeCellSize);
            int.TryParse(maxCars, out eO.maxCars);
            float.TryParse(maxCarsProb, out eO.maxCarsChance);
            int.TryParse(minHousehold, out eO.minHouseholdSize);
            int.TryParse(maxHousehold, out eO.maxHouseholdSize);
            float.TryParse(walkSpeedMin, out eO.walkingSpeedMinMax.x);
            float.TryParse(walkSpeedMax, out eO.walkingSpeedMinMax.y);
            float.TryParse(walkSpeedMod, out eO.walkingSpeedModifier);
            float.TryParse(walkingDistMod, out eO.walkingDistanceModifier);
            float.TryParse(evacOrderTime, out eO.evacuationOrderStart);
        }

        string opticalDensity, stallSpeed, borderSize;
        bool trafficInputDirty = true;
        void TrafficMenu()
        {
            TrafficInput tO = WUInity.WUINITY_IN.traffic;
            ItineroInput iO = WUInity.WUINITY_IN.itinero;
            if (trafficInputDirty)
            {
                trafficInputDirty = false;
                stallSpeed = tO.stallSpeed.ToString();
                opticalDensity = tO.opticalDensity.ToString();
                borderSize = iO.osmBorderSize.ToString();
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - buttonHeight), "");
            int buttonIndex = 0;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM data filename");
            ++buttonIndex;
            iO.osmDataName = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), iO.osmDataName);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM border size [m]");
            ++buttonIndex;
            borderSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), borderSize);
            ++buttonIndex;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Itinero routerDB name");
            ++buttonIndex;
            iO.routerDatabaseName = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), iO.routerDatabaseName);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load Router Database"))
            {
                ParseTrafficInput();
                WUInity.WUINITY_SIM.LoadItineroDatabase();
            }
            ++buttonIndex;

            //route collections
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Pre-calc routes filename");
            ++buttonIndex;
            tO.precalcRoutesName = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), tO.precalcRoutesName);
            ++buttonIndex;
            /*if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load pre-calc routes"))
            {
                ParseTrafficInput();
                WUInity.WUINITY_SIM.LoadRouteCollections();
            }
            ++buttonIndex;*/

            //jam speed
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Capacity speed");
            ++buttonIndex;
            stallSpeed = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), stallSpeed);
            ++buttonIndex;

            //smoke stuff
            tO.visibilityAffectsSpeed = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), tO.visibilityAffectsSpeed, "Speed is affected by smoke");
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density [1/m]");
            ++buttonIndex;
            opticalDensity = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), opticalDensity);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run traffic verification"))
            {
                Traffic.MacroTrafficVerification.RunTrafficVerificationTests();
            }
            ++buttonIndex;
        }

        void ParseTrafficInput()
        {
            if(trafficInputDirty)
            {
                return;
            }

            TrafficInput tO = WUInity.WUINITY_IN.traffic;
            ItineroInput iO = WUInity.WUINITY_IN.itinero;

            float.TryParse(stallSpeed, out tO.stallSpeed);
            float.TryParse(opticalDensity, out tO.opticalDensity);
            float.TryParse(borderSize, out iO.osmBorderSize);
        }

        float sliderValue = 0f;
        Farsite.FarsiteViewer fV;
        void FarsiteMenu()
        {
            FarsiteInput fI = WUInity.WUINITY_IN.farsite;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - buttonHeight), "");
            int buttonIndex = 0;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Output file name:");
            ++buttonIndex;
            fI.outputPrefix = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), fI.outputPrefix);
            ++buttonIndex;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Import Farsite Data"))
            {
                WUInity.WUINITY.LoadFarsite();
            }
            ++buttonIndex;

            if (WUInity.WUINITY_FARSITE != null)
            {
                if (fV == null)
                {
                    fV = WUInity.WUINITY_FARSITE;
                }
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time: " + (int)fV.actualTime + " hours after ignition");
                ++buttonIndex;
                sliderValue = GUI.HorizontalSlider(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sliderValue, 0.0F, 1.0f);
                fV.SetTime(sliderValue);
                ++buttonIndex;

                //TODO: add condition if farsite data loaded
                if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/Hide Farsite Data"))
                {
                    fV.ToggleTerrain();
                }
                ++buttonIndex;
            }
        }

        void FireMenu()
        {
            FireInput fI = WUInity.WUINITY_IN.fire;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - buttonHeight), "");
            int buttonIndex = 0;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "LCP file:");
            ++buttonIndex;
            fI.lcpFile = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), fI.lcpFile);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load LCP file"))
            {
                OpenLoadLCP();
            }
            ++buttonIndex;

            if (WUInity.WUINITY.IsPainterActive())
            {
                if(WUInity.PAINTER.GetPaintMode() == WUInityPainter.PaintMode.WUIArea)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add WUI area"))
                    {
                        WUInity.PAINTER.SetWUIAreaColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove WUI area"))
                    {
                        WUInity.PAINTER.SetWUIAreaColor(false);
                    }
                    ++buttonIndex;
                }
                else if (WUInity.PAINTER.GetPaintMode() == WUInityPainter.PaintMode.RandomIgnitionArea)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add random ignition area"))
                    {
                        WUInity.PAINTER.SetRandomIgnitionAreaColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove random ignition area"))
                    {
                        WUInity.PAINTER.SetRandomIgnitionAreaColor(false);
                    }
                    ++buttonIndex;
                }
                else if (WUInity.PAINTER.GetPaintMode() == WUInityPainter.PaintMode.RandomIgnitionArea)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add initial ignition"))
                    {
                        WUInity.PAINTER.SetInitialIgnitionAreaColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove initial ignition"))
                    {
                        WUInity.PAINTER.SetInitialIgnitionAreaColor(false);
                    }
                    ++buttonIndex;
                }

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop painting"))
                {
                    WUInity.WUINITY.StopPainter();
                }
                ++buttonIndex;
            }
            else
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint WUI area"))
                {
                    WUInity.WUINITY.StartPainter(WUInityPainter.PaintMode.WUIArea);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint random ignition area"))
                {
                    WUInity.WUINITY.StartPainter(WUInityPainter.PaintMode.RandomIgnitionArea);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint initial ignition"))
                {
                    WUInity.WUINITY.StartPainter(WUInityPainter.PaintMode.InitialIgnition);
                }
                ++buttonIndex;
            }
        }

        string outputTime;
        float sliderVtraffic = 1f;
        void OutputMenu()
        {
            int buttonColumnStart = 140;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - buttonHeight), "");
            int buttonIndex = 0;

            int dummy = (int)WUInity.WUINITY_OUT.totalEvacTime;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evac time: " + dummy + " s");
            ++buttonIndex;

            dummy = WUInity.WUINITY_OUT.evac.actualTotalEvacuees;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + dummy);
            ++buttonIndex;

            dummy = WUInity.WUINITY_OUT.evac.stayingPeople;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "People staying: " + dummy);
            ++buttonIndex;

            //toatl cars
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total cars: " + WUInity.WUINITY_SIM.GetMacroHumanSim().GetTotalCars());
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Interpolated population density"))
            {
                WUInity.WUINITY.DisplayRawPop();
                WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.Raw);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stuck population"))
            {
                WUInity.WUINITY.DisplayStuckPop();
                WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.None);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Redist. population density"))
            {
                WUInity.WUINITY.DisplayRelocatedPop();
                WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.Relocated);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Staying population"))
            {
                WUInity.WUINITY.DisplayStayingPop();
                WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.Staying);
            }
            ++buttonIndex;

            float timeRange = WUInity.WUINITY_OUT.totalEvacTime - WUInity.WUINITY_SIM.StartTime;
            float time = sliderVtraffic * timeRange + WUInity.WUINITY_SIM.StartTime;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic density"))
            {
                WUInity.WUINITY.DisplayClosestDensityData(time);
                WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.TrafficDens);
            }
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Traffic density: " + (int)time + " seconds");
            ++buttonIndex;
            sliderVtraffic = GUI.HorizontalSlider(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sliderVtraffic, 0.0f, 1.0f);
            ++buttonIndex;
            if (WUInity.WUINITY.dataSampleMode == WUInity.DataSampleMode.TrafficDens)
            {
                WUInity.WUINITY.DisplayClosestDensityData(time);
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/Hide data"))
            {
                WUInity.WUINITY.ToggleDataPlane();
            }
            ++buttonIndex;

            if(WUInity.WUINITY_IN.runEvacSim && WUInity.WUINITY_SIM.GetMacroHumanSim() != null)
            {
                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Pedestrians left: " + WUInity.WUINITY_SIM.GetMacroHumanSim().GetPeopleLeft() + " / " + WUInity.WUINITY_OUT.evac.actualTotalEvacuees);
                ++buttonIndex;

                //pedestrians still left
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars reached: " + WUInity.WUINITY_SIM.GetMacroHumanSim().GetCarsReached());
                ++buttonIndex;
            }            

            //cars still left
            if (WUInity.WUINITY_IN.runTrafficSim &&  WUInity.WUINITY_SIM.GetMacroTrafficSim() != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cars left: " + WUInity.WUINITY_SIM.GetMacroTrafficSim().GetCarsInSystem() + " / " + WUInity.WUINITY_SIM.GetMacroTrafficSim().GetTotalCarsSimulated());
                ++buttonIndex;
            }            

            int totalEvacuated = 0;
            for (int i = 0; i < WUInity.WUINITY_IN.traffic.evacuationGoals.Length; i++)
            {
                totalEvacuated += WUInity.WUINITY_IN.traffic.evacuationGoals[i].currentPeople;
                string name = WUInity.WUINITY_IN.traffic.evacuationGoals[i].name;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), name + ": " + WUInity.WUINITY_IN.traffic.evacuationGoals[i].currentPeople + " (" + WUInity.WUINITY_IN.traffic.evacuationGoals[i].cars.Count + ")");                
                ++buttonIndex;
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total evacuated: " + totalEvacuated + " / " + (WUInity.WUINITY_OUT.evac.actualTotalEvacuees - WUInity.WUINITY_OUT.evac.stayingPeople));
            ++buttonIndex;

            //fire output stuff
            if (WUInity.WUINITY_IN.runFireSim)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind speed: " + WUInity.WUINITY_SIM.GetFireWindSpeed() + " m/s");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Wind direction: " + WUInity.WUINITY_SIM.GetFireWindDirection() + " degrees");
                ++buttonIndex;
            }            
        }

        string infoMessage;
        public void PrintInfo(string message)
        {
            if (GUI.tooltip != null)
            {

            }
            infoMessage = message;
        }
    }
}