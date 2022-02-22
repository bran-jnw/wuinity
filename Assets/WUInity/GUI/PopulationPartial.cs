using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string desiredPopulation;
        bool populationInputDirty = true;

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

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load population file"))
            {
                OpenLoadPopulation();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Set GPW folder"))
            {
                OpenSetGPWFolder();
            }
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.population.gpwDataFolder);
            ++buttonIndex;

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

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Populate from local GPW"))
            {
                OpenCreatePopulationFromLocalGPW();
            }
            ++buttonIndex;

            if (WUInity.POPULATION.IsPopulationLoaded())
            {
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

            if (WUInity.POPULATION.IsLocalGPWLoaded())
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW total population: " + WUInity.POPULATION.GetLocalGPWTotalPopulation());
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide GPW data"))
                {
                    WUInity.POPULATION.ToggleLocalGPWVisibility();
                }
                ++buttonIndex;
            }

            ++buttonIndex;
            if (!WUInity.INSTANCE.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create/edit custom GPW"))
                {
                    WUInity.INSTANCE.StartPainter(WUInityPainter.PaintMode.CustomPopulation);
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
                                
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop editing"))
                {
                    int totalPop;
                    bool success = int.TryParse(desiredPopulation, out totalPop);
                    if (!success)
                    {
                        WUInity.SIM.LogMessage("ERROR: Total population not a number, ignoring changes.");
                    }
                    else
                    {
                        WUInity.POPULATION.GetPopulationData().PlaceUniformPopulation(totalPop);
                    }
                    WUInity.INSTANCE.StopPainter();
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
            populationInputDirty = true;
        }
    }
}