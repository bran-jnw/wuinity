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
        EngineTask _engineTask;

        void MainMenu()
        {
            //whenever we load a file we need to set the new data for the GUI
            if (mainMenuDirty)
            {
                CleanMainMenu();
            }

            GUI.Box(new Rect(subMenuXOrigin, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            if (!_wuinityManager.Map.IsAccessTokenValid)
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

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run folder"))
            {
                OpenRunFolder();
            }
            ++buttonIndex;

            if(_input == null)
            {
                return;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save"))
            {
                if (_wuinityManager.Engine.WorkingFile== null)
                {
                    OpenSaveInput();
                }
                else
                {
                    ParseMainData();
                    PREACT.IO.PREACTInput.SaveToDisk(_input, _wuinityManager.WorkingFolder);
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
            _input.Simulation.Name = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.Simulation.Name);
            ++buttonIndex;   
            
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            _engineTask.MultipleSimulations = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engineTask.MultipleSimulations, "Multiple runs");
            ++buttonIndex;
            if (_engineTask.MultipleSimulations)
            {
                //number of runs
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
                ++buttonIndex;
                nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
                ++buttonIndex;

                _engineTask.StopAfterConverging = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engineTask.StopAfterConverging, "Stop after converging");
                ++buttonIndex;

                if(_engineTask.StopAfterConverging)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Convergence criteria:");
                    ++buttonIndex;
                    convergenceMaxDifference = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), convergenceMaxDifference);
                    ++buttonIndex;
                }
            }

            _input.Simulation.RunPedestrianModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.Simulation.RunPedestrianModule, "Simulate pedestrians");
            ++buttonIndex;

            _input.Simulation.RunTrafficModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.Simulation.RunTrafficModule, "Simulate traffic");
            ++buttonIndex;

            _input.Simulation.RunFireModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.Simulation.RunFireModule, "Simulate fire spread");
            ++buttonIndex;

            _input.Simulation.RunSmokeModule = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.Simulation.RunSmokeModule, "Simulate smoke spread");
            ++buttonIndex;            

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData();
                menuChoice = ActiveMenu.Output;
                _wuinityManager.RunSimulation(new EngineTask(true, 1, true, 10, 0.02f));
            }
            ++buttonIndex;            
        }

        void CleanMainMenu()
        {
            mainMenuDirty = false;
            if(_input != null)
            {
                dT = _input.Simulation.DeltaTime.ToString();
                nrRuns = _engineTask.NumberOfRuns.ToString();
                convergenceMaxDifference = _engineTask.ConvergenceMaxDifference.ToString();
                convergenceMinSequence = _engineTask.ConvergenceMinSequence.ToString();
            }          
        }

        public void ParseMainData()
        {
            ParseEvacInput();
            ParseTrafficInput();

            if (mainMenuDirty)
            {
                return;
            }

            float.TryParse(dT, out _input.Simulation.DeltaTime);
            int.TryParse(nrRuns, out _engineTask.NumberOfRuns);
            float.TryParse(convergenceMaxDifference, out _engineTask.ConvergenceMaxDifference);
            int.TryParse(convergenceMinSequence, out _engineTask.ConvergenceMinSequence);
        }

        void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, ".wui", "Save file", "Save");
        }   
        void SaveInput(string[] paths)
        {
            mainMenuDirty = true;
            ParseMainData();
            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            _input.Simulation.Name = name;

            PREACT.IO.PREACTInput.SaveToDisk(_input, paths[0]);
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = _engine.WorkingFolder;
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }

        void LoadInput(string[] paths)
        {
            bool success;
            _engine.LoadInputFromFile(paths[0], out success);
            if(success)
            {
                mainMenuDirty = true;
            }            
        }            

        void CancelSaveLoad()
        {
            creatingNewFile = false;
        }

        void OpenRunFolder()
        {
            FileBrowser.SetFilters(true);
            string initialPath = _input.RootFolder;
            FileBrowser.ShowLoadDialog(RunFolder, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Run all files in folder", "Run");
        }

        void RunFolder(string[] paths)
        {
            _wuinityManager.RunAllCasesInFolder(paths[0], _engineTask);
        }

        
    }
}

