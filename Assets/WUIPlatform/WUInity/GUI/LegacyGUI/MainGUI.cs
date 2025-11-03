using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using PREACT;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string dT, nrRuns, convergenceMaxDifference, convergenceMinSequence;
        bool mainMenuDirty = true, creatingNewFile = false;

        void MainMenu()
        {
            PREACT.IO.Input wO = _engine.Input;

            //whenever we load a file we need to set the new data for the GUI
            if (mainMenuDirty)
            {
                CleanMainMenu(wO);
            }

            GUI.Box(new Rect(subMenuXOrigin, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            if (!_wuinityManager.Map.IsAccessTokenValid)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "ERROR: Mapbox token not valid.");
                return;
            }

            /*if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "New file"))
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
            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load defaults"))
            {
                SaveLoadWUI.LoadDefaultInputs();
                mainInputDirty = true;
            }
            ++buttonIndex;*/

            if (!_engine.DataStatus.HaveInput)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run folder"))
                {
                    OpenRunFolder();
                }
                ++buttonIndex;

                return;
            }

            /*if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save"))
            {
                if (_engine.WorkingFile== null)
                {
                    OpenSaveInput();
                }
                else
                {
                    ParseMainData(wO);
                    PREACT.IO.Input.SaveInput();
                }
            }
            ++buttonIndex;*/

            /*if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save as"))
            {
                OpenSaveInput();
            }
            buttonIndex += 2;*/

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Simulation ID:");
            ++buttonIndex;
            wO.Simulation.Id = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), wO.Simulation.Id);
            ++buttonIndex;   
            
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            _engine.ScenarioData.Simulation.MultipleSimulations = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engine.ScenarioData.Simulation.MultipleSimulations, "Multiple runs");
            ++buttonIndex;
            if (_engine.ScenarioData.Simulation.MultipleSimulations)
            {
                //number of runs
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
                ++buttonIndex;
                nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
                ++buttonIndex;

                _engine.Input.Simulation.StopAfterConverging = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engine.Input.Simulation.StopAfterConverging, "Stop after converging");
                ++buttonIndex;

                if(_engine.Input.Simulation.StopAfterConverging)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Convergence criteria:");
                    ++buttonIndex;
                    convergenceMaxDifference = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), convergenceMaxDifference);
                    ++buttonIndex;
                }
            }

            _engine.Input.Simulation.RunPedestrianModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engine.Input.Simulation.RunPedestrianModule, "Simulate pedestrians");
            ++buttonIndex;

            _engine.Input.Simulation.RunTrafficModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engine.Input.Simulation.RunTrafficModule, "Simulate traffic");
            ++buttonIndex;

            _engine.Input.Simulation.RunFireModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engine.Input.Simulation.RunFireModule, "Simulate fire spread");
            ++buttonIndex;

            _engine.Input.Simulation.RunSmokeModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engine.Input.Simulation.RunSmokeModule, "Simulate smoke spread");
            ++buttonIndex;            

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData(wO);  
                if (!_engine.DataStatus.CanRunSimulation())
                {
                    Engine.MESSAGE(null, Engine.LogType.SimError, " Could not start simulation, see error log.");
                }
                else
                {
                    menuChoice = ActiveMenu.Output;
                    _wuinityManager.RunSimulation();                   
                }
            }
            ++buttonIndex;            
        }

        void CleanMainMenu(PREACT.IO.Input wO)
        {
            mainMenuDirty = false;
            if(wO != null)
            {
                dT = wO.Simulation.DeltaTime.ToString();
            }
            else
            {
                dT = "-1.0";
            }
            
            if(_engine.ScenarioData != null)
            {
                nrRuns = _engine.ScenarioData.Simulation.NumberOfRuns.ToString();
                convergenceMaxDifference = _engine.ScenarioData.Simulation.ConvergenceMaxDifference.ToString();
                convergenceMinSequence = _engine.ScenarioData.Simulation.ConvergenceMinSequence.ToString();
            }
            else
            {
                nrRuns = "0";
                convergenceMaxDifference = "0";
                convergenceMinSequence = "0";
            }

                      
        }

        public void ParseMainData(PREACT.IO.Input wO)
        {
            ParseEvacInput();
            ParseTrafficInput();

            if (mainMenuDirty)
            {
                return;
            }

            float.TryParse(dT, out wO.Simulation.DeltaTime);
            int.TryParse(nrRuns, out _engine.ScenarioData.Simulation.NumberOfRuns);
            float.TryParse(convergenceMaxDifference, out _engine.ScenarioData.Simulation.ConvergenceMaxDifference);
            int.TryParse(convergenceMinSequence, out _engine.ScenarioData.Simulation.ConvergenceMinSequence);
        }

        /*void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            PREACT.IO.Input wO = _engine.Input;
            string initialPath = Path.GetDirectoryName(_engine.WorkingFile);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, "new.wui", "Save file", "Save");
        }              

        void SaveInput(string[] paths)
        {
            PREACT.IO.Input wO = _engine.Input;

            _engine.WorkingFile = paths[0];
            if (creatingNewFile)
            {
                mainMenuDirty = true;
                _engine.CreateNewInputData();
                wO = _engine.Input; //have to update this since we are creating a new one
            }
            else
            {
                ParseMainData(wO);
            }
            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            wO.Simulation.Id = name;

            PREACT.IO.Input.SaveInputToDisk();
        }*/

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = _engine.DataFolder;
            if (_engine.DataStatus.HaveInput)
            {
                initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            }
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }

        void LoadInput(string[] paths)
        {
            PREACT.IO.Input.LoadFromDisk(null, paths[0]);
            mainMenuDirty = true;
        }            

        void CancelSaveLoad()
        {
            creatingNewFile = false;
        }

        void OpenRunFolder()
        {
            FileBrowser.SetFilters(true);
            string initialPath = _engine.DataFolder;
            if (_engine.DataStatus.HaveInput)
            {
                initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            }
            FileBrowser.ShowLoadDialog(RunFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Run all files in folder", "Run");
        }

        void RunFolder(string[] paths)
        {
            _wuinityManager.RunAllCasesInFolder(paths[0]);
        }

        
    }
}

