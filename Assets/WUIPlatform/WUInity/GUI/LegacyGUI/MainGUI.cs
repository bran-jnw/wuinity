using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string dT, nrRuns, convergenceMaxDifference, convergenceMinSequence;
        bool mainMenuDirty = true, creatingNewFile = false;

        void MainMenu()
        {
            WUIEngineInput wO = WUIEngine.INPUT;

            //whenever we load a file we need to set the new data for the GUI
            if (mainMenuDirty)
            {
                CleanMainMenu(wO);
            }

            GUI.Box(new Rect(subMenuXOrigin, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            if (!WUInityEngine.MAP.IsAccessTokenValid)
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

            if (!WUIEngine.DATA_STATUS.HaveInput)
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
                if (WUIEngine.WORKING_FILE== null)
                {
                    OpenSaveInput();
                }
                else
                {
                    ParseMainData(wO);
                    WUIEngineInput.SaveInput();
                }
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save as"))
            {
                OpenSaveInput();
            }
            buttonIndex += 2;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Simulation ID:");
            ++buttonIndex;
            wO.Simulation.SimulationID = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), wO.Simulation.SimulationID);
            ++buttonIndex;   
            
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations, "Multiple runs");
            ++buttonIndex;
            if (WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations)
            {
                //number of runs
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
                ++buttonIndex;
                nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
                ++buttonIndex;

                WUIEngine.INPUT.Simulation.StopAfterConverging = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.INPUT.Simulation.StopAfterConverging, "Stop after converging");
                ++buttonIndex;

                if(WUIEngine.INPUT.Simulation.StopAfterConverging)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Convergence criteria:");
                    ++buttonIndex;
                    convergenceMaxDifference = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), convergenceMaxDifference);
                    ++buttonIndex;
                }
            }

            WUIEngine.INPUT.Simulation.RunPedestrianModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.INPUT.Simulation.RunPedestrianModule, "Simulate pedestrians");
            ++buttonIndex;

            WUIEngine.INPUT.Simulation.RunTrafficModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.INPUT.Simulation.RunTrafficModule, "Simulate traffic");
            ++buttonIndex;

            WUIEngine.INPUT.Simulation.RunFireModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.INPUT.Simulation.RunFireModule, "Simulate fire spread");
            ++buttonIndex;

            WUIEngine.INPUT.Simulation.RunSmokeModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.INPUT.Simulation.RunSmokeModule, "Simulate smoke spread");
            ++buttonIndex;            

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData(wO);  
                if (!WUIEngine.DATA_STATUS.CanRunSimulation())
                {
                    WUIEngine.LOG(WUIEngine.LogType.Error, " Could not start simulation, see error log.");
                }
                else
                {
                    menuChoice = ActiveMenu.Output;
                    WUInityEngine.INSTANCE.StartSimulation();                   
                }
            }
            ++buttonIndex;            
        }

        void CleanMainMenu(WUIEngineInput wO)
        {
            mainMenuDirty = false;
            dT = wO.Simulation.DeltaTime.ToString();
            nrRuns = WUIEngine.RUNTIME_DATA.Simulation.NumberOfRuns.ToString();
            convergenceMaxDifference = WUIEngine.RUNTIME_DATA.Simulation.ConvergenceMaxDifference.ToString();
            convergenceMinSequence = WUIEngine.RUNTIME_DATA.Simulation.ConvergenceMinSequence.ToString();
        }

        public void ParseMainData(WUIEngineInput wO)
        {
            ParseEvacInput();
            ParseTrafficInput();

            if (mainMenuDirty)
            {
                return;
            }

            float.TryParse(dT, out wO.Simulation.DeltaTime);
            int.TryParse(nrRuns, out WUIEngine.RUNTIME_DATA.Simulation.NumberOfRuns);
            float.TryParse(convergenceMaxDifference, out WUIEngine.RUNTIME_DATA.Simulation.ConvergenceMaxDifference);
            int.TryParse(convergenceMinSequence, out WUIEngine.RUNTIME_DATA.Simulation.ConvergenceMinSequence);
        }

        void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            WUIEngineInput wO = WUIEngine.INPUT;
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FILE);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.Simulation.SimulationID + ".wui", "Save file", "Save");
        }                  

        void SaveInput(string[] paths)
        {
            WUIEngineInput wO = WUIEngine.INPUT;

            WUIEngine.WORKING_FILE = paths[0];
            if (creatingNewFile)
            {
                mainMenuDirty = true;
                WUIEngine.ENGINE.CreateNewInputData();
                wO = WUIEngine.INPUT; //have to update this since we are creating a new one
            }
            else
            {
                ParseMainData(wO);
            }
            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            wO.Simulation.SimulationID = name;

            WUIEngineInput.SaveInput();
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = WUIEngine.DATA_FOLDER;
            if (WUIEngine.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            }
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }

        void LoadInput(string[] paths)
        {
            WUIEngineInput.LoadInput(paths[0]);
            mainMenuDirty = true;
        }            

        void CancelSaveLoad()
        {
            creatingNewFile = false;
        }

        void OpenRunFolder()
        {
            FileBrowser.SetFilters(true);
            string initialPath = WUIEngine.DATA_FOLDER;
            if (WUIEngine.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            }
            FileBrowser.ShowLoadDialog(RunFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Run all files in folder", "Run");
        }

        void RunFolder(string[] paths)
        {
            WUInityEngine.INSTANCE.RunAllCasesInFolder(paths[0]);
        }

        
    }
}

