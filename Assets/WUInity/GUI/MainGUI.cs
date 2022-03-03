using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string dT, nrRuns;
        bool mainMenuDirty = true, creatingNewFile = false;

        void MainMenu()
        {
            WUInityInput wO = WUInity.INPUT;

            //whenever we load a file we need to set the new data for the GUI
            if (mainMenuDirty)
            {
                CleanMainMenu(wO);
            }

            GUI.Box(new Rect(subMenuXOrigin, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            if (!WUInity.MAP.IsAccessTokenValid)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "ERROR: Mapbox token not valid.");
                return;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "New file"))
            {
                creatingNewFile = true;
                OpenSaveInput();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load file"))
            {
                OpenLoadInput();
            }
            ++buttonIndex;

            //will remove default and use example instead
            /*if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load defaults"))
            {
                SaveLoadWUI.LoadDefaultInputs();
                mainInputDirty = true;
            }
            ++buttonIndex;*/

            if (!WUInity.DATA_STATUS.haveInput)
            {
                return;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save"))
            {
                if (WUInity.WORKING_FILE == null)
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

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save as"))
            {
                OpenSaveInput();
            }
            buttonIndex += 2;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Simulation name:");
            ++buttonIndex;
            wO.simName = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), wO.simName);
            ++buttonIndex;   
            
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            //number of runs
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
            ++buttonIndex;
            nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
            ++buttonIndex;

            WUInity.INPUT.runEvacSim = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runEvacSim, "Simulate pedestrians");
            ++buttonIndex;

            WUInity.INPUT.runTrafficSim = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runTrafficSim, "Simulate traffic");
            ++buttonIndex;

            WUInity.INPUT.runFireSim = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runFireSim, "Simulate fire spread");
            ++buttonIndex;

            WUInity.INPUT.runInRealTime = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runInRealTime, "Update sim in GUI");
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData(wO);  
                if (!WUInity.DATA_STATUS.CanRunSimulation())
                {
                    WUInity.WUI_LOG("ERROR: Could not start simulation, see error log.");
                }
                else
                {
                    WUInity.WUI_LOG("LOG: Simulation started, please wait.");
                    menuChoice = ActiveMenu.Output;
                    WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.TrafficDens);
                    WUInity.INSTANCE.SetEvacDataPlane(true);
                    WUInity.SIM.StartSimulation();                    
                }
            }
            ++buttonIndex;
        }

        void CleanMainMenu(WUInityInput wO)
        {
            mainMenuDirty = false;
            dT = wO.deltaTime.ToString();
            nrRuns = wO.numberOfRuns.ToString();
        }

        void ParseMainData(WUInityInput wO)
        {
            ParseEvacInput();
            ParseTrafficInput();

            if (mainMenuDirty)
            {
                return;
            }

            float.TryParse(dT, out wO.deltaTime);
            int.TryParse(nrRuns, out wO.numberOfRuns);
        }

        void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            WUInityInput wO = WUInity.INPUT;
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.simName + ".wui", "Save file", "Save");
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = WUInity.DATA_FOLDER;
            if (WUInity.DATA_STATUS.haveInput)
            {
                initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            }            
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }             

        void SaveInput(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;

            WUInity.WORKING_FILE = paths[0];
            if (creatingNewFile)
            {
                mainMenuDirty = true;
                WUInity.INSTANCE.CreateNewInputData();
                wO = WUInity.INPUT; //have to update this since we are creating a new one
            }
            else
            {
                ParseMainData(wO);
            }
            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            wO.simName = name;

            SaveLoadWUI.SaveInput();
        }

        void LoadInput(string[] paths)
        {
            SaveLoadWUI.LoadInput(paths[0]);
            mainMenuDirty = true;
        }            

        void CancelSaveLoad()
        {
            creatingNewFile = false;
        }
    }
}

