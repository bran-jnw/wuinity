using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIEngine.IO;
using WUIEngine;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string dT, nrRuns, convergenceMaxDifference, convergenceMinSequence;
        bool mainMenuDirty = true, creatingNewFile = false;

        void MainMenu()
        {
            WUInityInput wO = Engine.INPUT;

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

            if (!Engine.DATA_STATUS.HaveInput)
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
                if (Engine.WORKING_FILE== null)
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
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Simulation ID:");
            ++buttonIndex;
            wO.Simulation.SimulationID = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), wO.Simulation.SimulationID);
            ++buttonIndex;   
            
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            Engine.RUNTIME_DATA.Simulation.MultipleSimulations = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.RUNTIME_DATA.Simulation.MultipleSimulations, "Multiple runs");
            ++buttonIndex;
            if (Engine.RUNTIME_DATA.Simulation.MultipleSimulations)
            {
                //number of runs
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
                ++buttonIndex;
                nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
                ++buttonIndex;

                Engine.INPUT.Simulation.StopAfterConverging = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.INPUT.Simulation.StopAfterConverging, "Stop after converging");
                ++buttonIndex;

                if(Engine.INPUT.Simulation.StopAfterConverging)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Convergence criteria:");
                    ++buttonIndex;
                    convergenceMaxDifference = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), convergenceMaxDifference);
                    ++buttonIndex;
                }
            }

            Engine.INPUT.Simulation.RunPedestrianModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.INPUT.Simulation.RunPedestrianModule, "Simulate pedestrians");
            ++buttonIndex;

            Engine.INPUT.Simulation.RunTrafficModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.INPUT.Simulation.RunTrafficModule, "Simulate traffic");
            ++buttonIndex;

            Engine.INPUT.Simulation.RunFireModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.INPUT.Simulation.RunFireModule, "Simulate fire spread");
            ++buttonIndex;

            Engine.INPUT.Simulation.RunSmokeModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.INPUT.Simulation.RunSmokeModule, "Simulate smoke spread");
            ++buttonIndex;            

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData(wO);  
                if (!Engine.DATA_STATUS.CanRunSimulation())
                {
                    Engine.LOG(Engine.LogType.Error, " Could not start simulation, see error log.");
                }
                else
                {
                    menuChoice = ActiveMenu.Output;
                    WUInityEngine.INSTANCE.StartSimulation();                   
                }
            }
            ++buttonIndex;            
        }

        void CleanMainMenu(WUInityInput wO)
        {
            mainMenuDirty = false;
            dT = wO.Simulation.DeltaTime.ToString();
            nrRuns = Engine.RUNTIME_DATA.Simulation.NumberOfRuns.ToString();
            convergenceMaxDifference = Engine.RUNTIME_DATA.Simulation.ConvergenceMaxDifference.ToString();
            convergenceMinSequence = Engine.RUNTIME_DATA.Simulation.ConvergenceMinSequence.ToString();
        }

        public void ParseMainData(WUInityInput wO)
        {
            ParseEvacInput();
            ParseTrafficInput();

            if (mainMenuDirty)
            {
                return;
            }

            float.TryParse(dT, out wO.Simulation.DeltaTime);
            int.TryParse(nrRuns, out Engine.RUNTIME_DATA.Simulation.NumberOfRuns);
            float.TryParse(convergenceMaxDifference, out Engine.RUNTIME_DATA.Simulation.ConvergenceMaxDifference);
            int.TryParse(convergenceMinSequence, out Engine.RUNTIME_DATA.Simulation.ConvergenceMinSequence);
        }

        void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            WUInityInput wO = Engine.INPUT;
            string initialPath = Path.GetDirectoryName(Engine.WORKING_FILE);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.Simulation.SimulationID + ".wui", "Save file", "Save");
        }                  

        void SaveInput(string[] paths)
        {
            WUInityInput wO = Engine.INPUT;

            Engine.WORKING_FILE = paths[0];
            if (creatingNewFile)
            {
                mainMenuDirty = true;
                Engine.ENGINE.CreateNewInputData();
                wO = Engine.INPUT; //have to update this since we are creating a new one
            }
            else
            {
                ParseMainData(wO);
            }
            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            wO.Simulation.SimulationID = name;

            WUInityInput.SaveInput();
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = Engine.DATA_FOLDER;
            if (Engine.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(Engine.WORKING_FOLDER);
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
            string initialPath = Engine.DATA_FOLDER;
            if (Engine.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(Engine.WORKING_FOLDER);
            }
            FileBrowser.ShowLoadDialog(RunFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Run all files in folder", "Run");
        }

        void RunFolder(string[] paths)
        {
            WUInityEngine.INSTANCE.RunAllCasesInFolder(paths[0]);
        }

        
    }
}

