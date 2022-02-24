using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string desiredPopulation;
        bool populationInputDirty = true;
        bool reScaling = false;

        void PopulationMenu()
        {
            PopulationInput popIn = WUInity.INPUT.population;
            if (populationInputDirty)
            {
                populationInputDirty = false;
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            string localPopStatus = "Status: Population data NOT loaded";
            if (WUInity.POPULATION.IsPopulationLoaded())
            {
                localPopStatus = "Status: Population data loaded";
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
                        WUInity.LogMessage("ERROR: Total population not a number, ignoring changes.");
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
                if(!WUInity.POPULATION.GetPopulationData().correctedForRoutes && WUInity.SIM.GetRouteCollection() != null)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Correct for route access"))
                    {
                        WUInity.POPULATION.UpdatePopulationBasedOnRoutes(WUInity.SIM.GetRouteCollection());
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
                            WUInity.LogMessage("ERROR: New population count not a number.");
                        }
                        reScaling = false;
                    }
                    ++buttonIndex;
                }     

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Total population: " + WUInity.POPULATION.GetTotalPopulation());
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide population data"))
                {
                    WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.Population);
                    WUInity.INSTANCE.DisplayPopulation();
                    WUInity.INSTANCE.ToggleEvacDataPlane();
                    
                }
                ++buttonIndex;
            }

            //GPW stuff
            string globalGPWStatus = "Global GPW data NOT found";
            if (WUInity.DATA_STATUS.globalGPWAvailable)
            {
                globalGPWStatus = "Global GPW data found";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), globalGPWStatus);
            ++buttonIndex;

            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Set global GPW folder"))
            {
                OpenSetGPWFolder();
            }
            ++buttonIndex;

            if (WUInity.DATA_STATUS.globalGPWAvailable)
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

            if (WUInity.DATA_STATUS.localGPWLoaded)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW total population: " + WUInity.POPULATION.GetLocalGPWTotalPopulation());
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide GPW data"))
                {
                    WUInity.POPULATION.ToggleLocalGPWVisibility();
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
            WUInity.INPUT.population.populationFile = paths[0];
            WUInity.POPULATION.LoadPopulationFromFile();
            populationInputDirty = true;
        }

        void OpenCreatePopulationFromLocalGPW()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreatePopulationFromLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load local GPW file", "Load");
        }

        void CreatePopulationFromLocalGPW(string[] paths)
        {
            WUInity.INPUT.population.localGPWFile = paths[0];
            WUInity.POPULATION.CreatePopulationFromLocalGPW();
            populationInputDirty = true;
        }

        void OpenSetGPWFolder()
        {
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(SetGPWFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Set GPW folder", "Set");
        }

        void SetGPWFolder(string[] paths)
        {
            WUInity.INPUT.population.gpwDataFolder = paths[0];
            WUInity.DATA_STATUS.globalGPWAvailable = GPW.LocalGPWData.IsGPWAvailable();
        }
    }
}