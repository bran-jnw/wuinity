using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string dT, nrRuns, convergenceMaxDifference, convergenceMinSequence;
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

            if (!WUInity.DATA_STATUS.HaveInput)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run folder"))
                {
                    OpenRunFolder();
                }
                ++buttonIndex;

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
                    WUInityInput.SaveInput();
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
            wO.simDataName = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), wO.simDataName);
            ++buttonIndex;   
            
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            WUInity.RUNTIME_DATA.MultipleSimulations = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.RUNTIME_DATA.MultipleSimulations, "Multiple runs");
            ++buttonIndex;
            if (WUInity.RUNTIME_DATA.MultipleSimulations)
            {
                //number of runs
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
                ++buttonIndex;
                nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
                ++buttonIndex;

                WUInity.INPUT.stopAfterConverging = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.stopAfterConverging, "Stop after converging");
                ++buttonIndex;

                if(WUInity.INPUT.stopAfterConverging)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Convergence criteria:");
                    ++buttonIndex;
                    convergenceMaxDifference = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), convergenceMaxDifference);
                    ++buttonIndex;
                }
            }

            WUInity.INPUT.runEvacSim = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runEvacSim, "Simulate pedestrians");
            ++buttonIndex;

            WUInity.INPUT.runTrafficSim = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runTrafficSim, "Simulate traffic");
            ++buttonIndex;

            WUInity.INPUT.runFireSim = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runFireSim, "Simulate fire spread");
            ++buttonIndex;

            WUInity.INPUT.runSmokeSim = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.runSmokeSim, "Simulate smoke spread");
            ++buttonIndex;            

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData(wO);  
                if (!WUInity.DATA_STATUS.CanRunSimulation())
                {
                    WUInity.LOG("ERROR: Could not start simulation, see error log.");
                }
                else
                {
                    menuChoice = ActiveMenu.Output;
                    WUInity.INSTANCE.StartSimulation();                   
                }
            }
            ++buttonIndex;            
        }

        void CleanMainMenu(WUInityInput wO)
        {
            mainMenuDirty = false;
            dT = wO.deltaTime.ToString();
            nrRuns = WUInity.RUNTIME_DATA.General.NumberOfRuns.ToString();
            convergenceMaxDifference = WUInity.RUNTIME_DATA.convergenceMaxDifference.ToString();
            convergenceMinSequence = WUInity.RUNTIME_DATA.convergenceMinSequence.ToString();
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
            int.TryParse(nrRuns, out WUInity.RUNTIME_DATA.General.NumberOfRuns);
            float.TryParse(convergenceMaxDifference, out WUInity.RUNTIME_DATA.convergenceMaxDifference);
            int.TryParse(convergenceMinSequence, out WUInity.RUNTIME_DATA.convergenceMinSequence);
        }

        void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            WUInityInput wO = WUInity.INPUT;
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.simDataName + ".wui", "Save file", "Save");
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
            wO.simDataName = name;

            WUInityInput.SaveInput();
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = WUInity.DATA_FOLDER;
            if (WUInity.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            }
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }

        void LoadInput(string[] paths)
        {
            WUInityInput.LoadInput(paths[0]);
            mainMenuDirty = true;
        }            

        void CancelSaveLoad()
        {
            creatingNewFile = false;
        }

        void OpenRunFolder()
        {
            FileBrowser.SetFilters(true);
            string initialPath = WUInity.DATA_FOLDER;
            if (WUInity.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            }
            FileBrowser.ShowLoadDialog(RunFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Run all files in folder", "Run");
        }

        void RunFolder(string[] paths)
        {
            WUInity.INSTANCE.RunAllCasesInFolder(paths[0]);
        }

        
    }
}

