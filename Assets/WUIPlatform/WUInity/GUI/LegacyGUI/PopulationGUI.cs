using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string desiredPopulation;
        bool populationMenuDirty = true;
        bool reScaling = false;

        void PopulationMenu()
        {
            PopulationInput popIn = WUIEngine.INPUT.Population;
            if (populationMenuDirty)
            {
                populationMenuDirty = false;
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            string localPopStatus = "Population data NOT loaded";
            if (WUIEngine.POPULATION.IsPopulationLoaded())
            {
                localPopStatus = "Population data loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), localPopStatus);
            ++buttonIndex;

            string popRouteStatus = "Population NOT adjusted to routes";
            if (WUIEngine.POPULATION.IsPopulationCorrectedForRoutes())
            {
                popRouteStatus = "Population adjusted to routes";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), popRouteStatus);
            ++buttonIndex;

            if(WUIEngine.DATA_STATUS.PopulationLoaded)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + WUIEngine.POPULATION.GetTotalPopulation());
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total active cells: " + WUIEngine.POPULATION.GetTotalActiveCells());
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
            if (!WUInityEngine.INSTANCE.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create/edit custom pop."))
                {
                    WUInityEngine.INSTANCE.StartPainter(Painter.PaintMode.CustomPopulation);
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
                    WUInityEngine.Painter.SetCustomGPWColor(true);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove cell"))
                {
                    WUInityEngine.Painter.SetCustomGPWColor(false);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Finish editing"))
                {
                    int totalPop;
                    bool success = int.TryParse(desiredPopulation, out totalPop);
                    if (!success)
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Error, " Total population not a number, ignoring changes.");
                    }
                    else
                    {
                        WUIEngine.POPULATION.GetPopulationData().PlaceUniformPopulation(totalPop);
                    }
                    WUInityEngine.INSTANCE.StopPainter();
                    WUInityEngine.INSTANCE.DisplayPopulation();
                }
                ++buttonIndex;
                ++buttonIndex;
            }

            if (WUIEngine.POPULATION.IsPopulationLoaded())
            {
                if (!WUIEngine.POPULATION.GetPopulationData().correctedForRoutes && WUIEngine.RUNTIME_DATA.Routing.RouteCollections != null)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Correct for route access"))
                    {
                        WUIEngine.POPULATION.UpdatePopulationBasedOnRoutes(WUIEngine.RUNTIME_DATA.Routing.RouteCollections);
                        WUInityEngine.INSTANCE.DisplayPopulation();
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
                            WUIEngine.POPULATION.ScaleTotalPopulation(newPop);
                            WUInityEngine.INSTANCE.DisplayPopulation();
                        }
                        else
                        {
                            WUIEngine.LOG(WUIEngine.LogType.Error, " New population count not a number.");
                        }
                        reScaling = false;
                    }
                    ++buttonIndex;
                }   

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide population data"))
                {
                    WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.Population);
                    WUInityEngine.INSTANCE.DisplayPopulation();
                    WUInityEngine.INSTANCE.ToggleEvacDataPlane();
                    
                }
                ++buttonIndex;
            }

            //GPW stuff
            ++buttonIndex;
            string localGPWStatus = "Local GPW data NOT loaded";
            if (WUIEngine.DATA_STATUS.LocalGPWLoaded)
            {
                localGPWStatus = "Local GPW data loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), localGPWStatus);
            ++buttonIndex;

            string globalGPWStatus = "Global GPW data NOT found";
            if (WUIEngine.DATA_STATUS.GlobalGPWAvailable)
            {
                globalGPWStatus = "Global GPW data found";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), globalGPWStatus);
            ++buttonIndex;

            if (WUIEngine.DATA_STATUS.LocalGPWLoaded)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW total population: " + WUIEngine.POPULATION.GetLocalGPWTotalPopulation());
                ++buttonIndex;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Set global GPW folder"))
            {
                OpenSetGPWFolder();
            }
            ++buttonIndex;

            if (WUIEngine.DATA_STATUS.GlobalGPWAvailable)
            {
                string buildGPWstring = "Build local GPW file";
                if (WUIEngine.POPULATION.IsLocalGPWLoaded())
                {
                    buildGPWstring = "Re-build local GPW file";
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), buildGPWstring))
                {
                    WUIEngine.POPULATION.CreateLocalGPW();
                }
                ++buttonIndex;
            }            

            if (WUIEngine.DATA_STATUS.LocalGPWLoaded)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide GPW data"))
                {
                    WUIEngine.POPULATION.Visualizer.ToggleLocalGPWVisibility();
                }
                ++buttonIndex;
            }            
        }

        void OpenLoadPopulation()
        {
            FileBrowser.SetFilters(false, populationFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(LoadPopulation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load population file", "Load");
        }

        void LoadPopulation(string[] paths)
        {            
            WUIEngine.POPULATION.LoadPopulationFromFile(paths[0], true);
            populationMenuDirty = true;
        }

        void OpenCreatePopulationFromLocalGPW()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreatePopulationFromLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load local GPW file", "Load");
        }

        void CreatePopulationFromLocalGPW(string[] paths)
        {
            WUIEngine.POPULATION.CreatePopulationFromLocalGPW(paths[0]);
            populationMenuDirty = true;
        }

        void OpenSetGPWFolder()
        {
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(SetGPWFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Set GPW folder", "Set");
        }

        void SetGPWFolder(string[] paths)
        {
            WUIEngine.RUNTIME_DATA.Population.LoadGlobalGPWFolder(paths[0], true);
        }
    }
}