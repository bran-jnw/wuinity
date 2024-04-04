using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string desiredPopulation;
        bool populationMenuDirty = true;
        bool reScaling = false;

        void PopulationMenu()
        {
            PopulationInput popIn = WUInity.INPUT.Population;
            if (populationMenuDirty)
            {
                populationMenuDirty = false;
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            string localPopStatus = "Population data NOT loaded";
            if (WUInity.POPULATION.IsPopulationLoaded())
            {
                localPopStatus = "Population data loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), localPopStatus);
            ++buttonIndex;

            string popRouteStatus = "Population NOT adjusted to routes";
            if (WUInity.POPULATION.IsPopulationCorrectedForRoutes())
            {
                popRouteStatus = "Population adjusted to routes";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), popRouteStatus);
            ++buttonIndex;

            if(WUInity.DATA_STATUS.PopulationLoaded)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + WUInity.POPULATION.GetTotalPopulation());
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total active cells: " + WUInity.POPULATION.GetTotalActiveCells());
                ++buttonIndex;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load population file"))
            {
                OpenLoadPopulation();
            }
            ++buttonIndex;                     

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Populate from local GPW"))
            {
                OpenCreatePopulationFromLocalGPW();
            }
            ++buttonIndex;

            //custom population creation
            if (!WUInity.INSTANCE.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create/edit custom pop."))
                {
                    WUInity.INSTANCE.StartPainter(Painter.PaintMode.CustomPopulation);
                }
                ++buttonIndex;
            }
            else
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Desired population:");
                ++buttonIndex;
                desiredPopulation = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), desiredPopulation);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add cell"))
                {
                    WUInity.PAINTER.SetCustomGPWColor(true);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove cell"))
                {
                    WUInity.PAINTER.SetCustomGPWColor(false);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop editing"))
                {
                    int totalPop;
                    bool success = int.TryParse(desiredPopulation, out totalPop);
                    if (!success)
                    {
                        WUInity.CONSOLE(WUInity.LogType.Error, " Total population not a number, ignoring changes.");
                    }
                    else
                    {
                        WUInity.POPULATION.GetPopulationData().PlaceUniformPopulation(totalPop);
                    }
                    WUInity.INSTANCE.StopPainter();
                    WUInity.INSTANCE.DisplayPopulation();
                }
                ++buttonIndex;
                ++buttonIndex;
            }

            if (WUInity.POPULATION.IsPopulationLoaded())
            {
                if (!WUInity.POPULATION.GetPopulationData().correctedForRoutes && WUInity.RUNTIME_DATA.Routing.RouteCollections != null)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Correct for route access"))
                    {
                        WUInity.POPULATION.UpdatePopulationBasedOnRoutes(WUInity.RUNTIME_DATA.Routing.RouteCollections);
                        WUInity.INSTANCE.DisplayPopulation();
                    }
                    ++buttonIndex;
                }

                //re-scaling
                if(!reScaling)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Re-scale population"))
                    {
                        reScaling = true;
                    }
                    ++buttonIndex;
                }
                else
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Desired population:");
                    ++buttonIndex;
                    desiredPopulation = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), desiredPopulation);
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Apply re-scale"))
                    {
                        int newPop;
                        if(int.TryParse(desiredPopulation, out newPop))
                        {
                            WUInity.POPULATION.ScaleTotalPopulation(newPop);
                            WUInity.INSTANCE.DisplayPopulation();
                        }
                        else
                        {
                            WUInity.CONSOLE(WUInity.LogType.Error, " New population count not a number.");
                        }
                        reScaling = false;
                    }
                    ++buttonIndex;
                }   

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide population data"))
                {
                    WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.Population);
                    WUInity.INSTANCE.DisplayPopulation();
                    WUInity.INSTANCE.ToggleEvacDataPlane();
                    
                }
                ++buttonIndex;
            }

            //GPW stuff
            ++buttonIndex;
            string localGPWStatus = "Local GPW data NOT loaded";
            if (WUInity.DATA_STATUS.LocalGPWLoaded)
            {
                localGPWStatus = "Local GPW data loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), localGPWStatus);
            ++buttonIndex;

            string globalGPWStatus = "Global GPW data NOT found";
            if (WUInity.DATA_STATUS.GlobalGPWAvailable)
            {
                globalGPWStatus = "Global GPW data found";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), globalGPWStatus);
            ++buttonIndex;

            if (WUInity.DATA_STATUS.LocalGPWLoaded)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW total population: " + WUInity.POPULATION.GetLocalGPWTotalPopulation());
                ++buttonIndex;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Set global GPW folder"))
            {
                OpenSetGPWFolder();
            }
            ++buttonIndex;

            if (WUInity.DATA_STATUS.GlobalGPWAvailable)
            {
                string buildGPWstring = "Build local GPW file";
                if (WUInity.POPULATION.IsLocalGPWLoaded())
                {
                    buildGPWstring = "Re-build local GPW file";
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), buildGPWstring))
                {
                    WUInity.POPULATION.CreateLocalGPW();
                }
                ++buttonIndex;
            }            

            if (WUInity.DATA_STATUS.LocalGPWLoaded)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide GPW data"))
                {
                    WUInity.POPULATION.Visualizer.ToggleLocalGPWVisibility();
                }
                ++buttonIndex;
            }            
        }

        void OpenLoadPopulation()
        {
            FileBrowser.SetFilters(false, populationFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(LoadPopulation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load population file", "Load");
        }

        void LoadPopulation(string[] paths)
        {            
            WUInity.POPULATION.LoadPopulationFromFile(paths[0], true);
            populationMenuDirty = true;
        }

        void OpenCreatePopulationFromLocalGPW()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreatePopulationFromLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load local GPW file", "Load");
        }

        void CreatePopulationFromLocalGPW(string[] paths)
        {
            WUInity.POPULATION.CreatePopulationFromLocalGPW(paths[0]);
            populationMenuDirty = true;
        }

        void OpenSetGPWFolder()
        {
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(SetGPWFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Set GPW folder", "Set");
        }

        void SetGPWFolder(string[] paths)
        {
            WUInity.RUNTIME_DATA.Population.LoadGlobalGPWFolder(paths[0], true);
        }
    }
}