using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using PREACT;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string _dT, _nrRuns, _convergenceMaxDifference, _convergenceMinSequence, _minimumRunSequence;
        bool mainMenuDirty = true, creatingNewFile = false, _multipleSimulations;
        EngineTask _engineTask = new EngineTask();

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
                    PREACT.Input.PREACTInput.SaveToDisk(_input, _wuinityManager.WorkingFolder);
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
            _dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _dT);
            ++buttonIndex;

            _multipleSimulations = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _multipleSimulations, "Multiple runs");
            ++buttonIndex;
            if (_multipleSimulations)
            {
                //number of runs
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
                ++buttonIndex;
                _nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _nrRuns);
                ++buttonIndex;

                _engineTask.StopAfterConverging = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _engineTask.StopAfterConverging, "Stop after converging");
                ++buttonIndex;

                if (_engineTask.StopAfterConverging)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Convergence criteria:");
                    ++buttonIndex;
                    _convergenceMaxDifference = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _convergenceMaxDifference);
                    ++buttonIndex;

                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Minimum runs:");
                    ++buttonIndex;
                    _minimumRunSequence = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _minimumRunSequence);
                    ++buttonIndex;
                }
            }            

            _input.PedestrianModule.Enabled = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.PedestrianModule.Enabled, "Simulate pedestrians");
            ++buttonIndex;

            _input.TrafficModule.Enabled = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.TrafficModule.Enabled, "Simulate traffic");
            ++buttonIndex;

            _input.WildfireModule.Enabled = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.WildfireModule.Enabled, "Simulate fire spread");
            ++buttonIndex;

            _input.SmokeModule.Enabled = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.SmokeModule.Enabled, "Simulate smoke spread");
            ++buttonIndex;            

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData();
                menuChoice = ActiveMenu.Output;
                if(_multipleSimulations)
                {
                    int.TryParse(_nrRuns, out _engineTask.NumberOfRuns);
                    float.TryParse(_convergenceMaxDifference, out _engineTask.ConvergenceMaxDifference);
                    int.TryParse(_minimumRunSequence, out _engineTask.ConvergenceMinSequence);
                    if(_engineTask.NumberOfRuns > 1)
                    {
                        _engineTask.Execution = EngineTask.ExecutionMode.ParallelProcess;
                    }
                }
                else
                {
                    _engineTask.NumberOfRuns = 1;
                    _engineTask.StopAfterConverging = true;
                    _engineTask.Execution = EngineTask.ExecutionMode.Serial;
                }
                _wuinityManager.RunSimulation(_engineTask);
            }
            ++buttonIndex;            
        }

        void CleanMainMenu()
        {
            mainMenuDirty = false;
            if(_input != null)
            {
                _dT = _input.Simulation.DeltaTime.ToString();
                _nrRuns = _engineTask.NumberOfRuns.ToString();
                _convergenceMaxDifference = _engineTask.ConvergenceMaxDifference.ToString();
                _convergenceMinSequence = _engineTask.ConvergenceMinSequence.ToString();
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

            float.TryParse(_dT, out _input.Simulation.DeltaTime);
            int.TryParse(_nrRuns, out _engineTask.NumberOfRuns);
            float.TryParse(_convergenceMaxDifference, out _engineTask.ConvergenceMaxDifference);
            int.TryParse(_convergenceMinSequence, out _engineTask.ConvergenceMinSequence);
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

            PREACT.Input.PREACTInput.SaveToDisk(_input, paths[0]);
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

