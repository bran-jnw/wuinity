//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.IO;
using System.IO;
using System.Collections.Generic;
using System;
using PREACT.Utility.Analysis;
using PREACT.Runtime;

namespace PREACT
{    
    public class Engine
    {
        private static Engine _ENGINE;
        private IExternalManager _externalManager;
        private Simulation[] _simulations;
        private Simulation _mainSimulation; //this one talks to any visualizer         
        private PREACTInput _input;
        private DataStatus _dataStatus;
        private PREACTOutput _output;        
        private string _workingFile;
        private Visualization.WUIShowCommunicator _wuiShow;
        private WorkingData _workingData;

        public Simulation Simulation { get => _mainSimulation; }
        //public PREACTScenario Scenario { get => _scenario; }
        public DataStatus DataStatus { get => _dataStatus; }        
        public string WorkingFile { get => _workingFile; }
        public string WorkingFolder
        {
            get
            {
                if(_input != null)
                {
                    return _input.RootFolder;
                }
                else
                {
                    return AppDomain.CurrentDomain.BaseDirectory;
                }
            }
        }
        public string OutputFolder
        {
            get
            {
                //DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(WorkingFile).ToString(), Input.Simulation.Name + "_output"));
                //return path.ToString();
                if(_input != null)
                {
                    string path = Path.Combine(WorkingFolder, _input.Simulation.Name + "_output");
                    if(!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    return Path.Combine(WorkingFolder, _input.Simulation.Name + "_output");
                }
                else
                {
                    string path = Path.Combine(WorkingFolder, "_output");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    return path;
                }
            }
        }
        public Visualization.WUIShowCommunicator WUIShow { get => _wuiShow; }        
        public PREACTOutput Output { get => _output; }     
        public WorkingData WorkingData { get => _workingData; }

        public Engine(IExternalManager externalManager, bool mainEngine = true)
        {
            //needed for proper reading of input files on all systems
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _output = new PREACTOutput();
            _workingData = new WorkingData();
            _externalManager = externalManager;
            if(mainEngine)
            {
                _ENGINE = this;
            }            
        }      
       
        public async void RunSimulations(EngineTask engineTask, bool runInParallel = false)
        {
            try
            {    
                System.Threading.Tasks.Task task;
                if(runInParallel)
                {
                    task = System.Threading.Tasks.Task.Run(() => RunSimulationsParallel(engineTask));
                }
                else
                {
                    task = System.Threading.Tasks.Task.Run(() => RunSimulationsSerial(engineTask));
                }
                await task;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        bool _stopSimulations = false;
        private void RunSimulationsSerial(EngineTask engineTask)
        {
            PreSimulations(engineTask);
            
            for (int i = 0; i < _simulations.Length; ++i)
            {
                _simulations[i] = new Simulation(this, _input, i);
                _mainSimulation = _simulations[i];
                _simulations[i].Run();
                CollectSimulationStatistics(_simulations[i], engineTask);
                if (_stopSimulations)
                {
                    break;
                }
            }

            PostSimulations();
        }

        private void RunSimulationsParalellProcess()
        {
            //run one sim in this process
            _simulations[0].Run();

            //run the rest as new processes so that SUMO works
            var tcs = new System.Threading.Tasks.TaskCompletionSource<int>();

            var process = new System.Diagnostics.Process
            {
                StartInfo = { FileName = "wuinity.exe", Arguments= WorkingFile },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            //collect the data written to disk to create output
        }

        const int _parallellRunCount = 4;        
        private void RunSimulationsParallel(EngineTask engineTask)
        {
            PreSimulations(engineTask);

            //Currently this will not work as SUMO can only run one instance per process, need to find workaround
            int batches = engineTask.NumberOfRuns / _parallellRunCount + engineTask.NumberOfRuns % _parallellRunCount > 0 ? 1 : 0;
            for (int i = 0; i < batches; ++i)
            {
                int startIndex = batches * _parallellRunCount;
                int endIndex = Math.Min(startIndex + _parallellRunCount, engineTask.NumberOfRuns);
                System.Threading.Tasks.Parallel.For(startIndex, endIndex, index =>
                {
                    try
                    {
                        _simulations[index].Run();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                });

                for(int j = startIndex; j < endIndex; ++j)
                {
                    CollectSimulationStatistics(_simulations[j], engineTask);
                }
                if(_stopSimulations)
                {
                    break;
                }
            }                

            PostSimulations();
        }

        private void PreSimulations(EngineTask engineTask, bool parallel = false)
        {
            _stopSimulations = false;

            if (trafficArrivalDataCollection != null)
            {
                trafficArrivalDataCollection.Clear();
            }
            else
            {
                trafficArrivalDataCollection = new List<List<float>>();
            }

            if (parallel)
            {
                _simulations = new Simulation[engineTask.NumberOfRuns];
                for (int i = 0; i < _simulations.Length; ++i)
                {
                    _simulations[i] = new Simulation(this, _input, i);
                }
                //by default simulation 0 will be the main to be displayed
                SetMainSimulation(0);
            }            

            if (_input.WUIShow.SendDataToWUIShow && _input.Simulation.RunTrafficModule)
            {
                _wuiShow = new Visualization.WUIShowCommunicator(this, _input.WUIShow.WuiShowServerIP, _input.WUIShow.WuiShowServerPort, 0, _input.Simulation.LowerLeftLatLon.y, _input.Simulation.LowerLeftLatLon.x);
            }
        }   
        
        public void SetMainSimulation(int simulationIndex)
        {
            if (simulationIndex >= 0 && simulationIndex < _simulations.Length)
            {
                for (int i = 0; i < _simulations.Length; ++i)
                {
                    _simulations[i].SetAsBackgroundSimulation();
                }
                _mainSimulation = _simulations[simulationIndex];
                _mainSimulation.SetAsMainSimulation();
            }
        }
        
        private void PostSimulations()
        {
            //save functional analysis
            int actualRuns = trafficArrivalDataCollection.Count;
            if (actualRuns > 0)
            {
                float[] averageCurve = FunctionalAnalysis.CalculateAverageCurve(trafficArrivalDataCollection, FunctionalAnalysis.DimensionScalingMode.Average);
                SaveAverageCurve(averageCurve);
                //plot results
                double[] xData = new double[averageCurve.Length];
                double[] yData = new double[averageCurve.Length];
                for (int i = 0; i < averageCurve.Length; i++)
                {
                    xData[i] = averageCurve[i] / 3600.0f;
                    yData[i] = i + 1;
                }
                CreatePlotData(xData, yData);
            }

            if (convergedInSequence >= 10)
            {
                Message(null, LogType.Log, " Average total evacuation time: " + cumulativeTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations before converging according to user set criteria.");

            }
            else
            {
                Message(null, LogType.Log, " Average total evacuation time: " + cumulativeTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulation/s.");
            }

            PREACTOutput.SaveLogToDisk(_consoleLog, Path.Combine(OutputFolder, _input.Simulation.Name + ".log"));
        }

        float cumulativeTotalEvacTime = 0.0f;
        int convergedInSequence = 0;
        List<List<float>> trafficArrivalDataCollection;
        /// <summary>
        /// Each simulation calls this function when it is done to see if evacuation time has vonverged and simulations should be stopped.
        /// </summary>
        /// <param name="simulation"></param>
        private void CollectSimulationStatistics(Simulation simulation, EngineTask engineTask)
        {
            lock(trafficArrivalDataCollection)
            {
                if (simulation.TrafficModule != null)
                {
                    trafficArrivalDataCollection.Add(simulation.TrafficModule.GetArrivalData());
                }
                else
                {
                    return;
                }

                int resultCount = trafficArrivalDataCollection.Count;
                //need at least 2 simulations to have valid average
                if (resultCount > 1)
                {
                    float pastAverage = cumulativeTotalEvacTime / (resultCount - 1);
                    cumulativeTotalEvacTime += simulation.CurrentTime;
                    float currentAverage = cumulativeTotalEvacTime / resultCount;
                    float convergenceCriteria = (currentAverage - pastAverage) / currentAverage;
                    //if convergence met we can stop
                    if (convergenceCriteria < engineTask.ConvergenceMaxDifference)
                    {
                        ++convergedInSequence;
                        //we are done
                        if (engineTask.StopAfterConverging && convergedInSequence > engineTask.ConvergenceMinSequence)
                        {
                            _stopSimulations = true; //needed for serial run
                            CloseSimulations(false); //needed for parallel run
                        }
                    }
                    else
                    {
                        convergedInSequence = 0;
                    }
                }
                else
                {
                    cumulativeTotalEvacTime += simulation.CurrentTime;
                }
            }            
        }       

        public void SetInput(PREACTInput input)
        {
            _dataStatus.HaveInput = true;
            _input = input;
            _workingFile = null;
            _dataStatus.Reset();
            _dataStatus.HaveInput = true;
            UpdateExternalManager(_input);
        }

        public void LoadInputFromFile(string filePath, out bool success)
        {
            _dataStatus.HaveInput = false;
            PREACTInput input = PREACTInput.LoadFromDisk(filePath, out success);
            if(success)
            {
                _input = input;
                _workingFile = filePath;
                _dataStatus.Reset();
                _dataStatus.HaveInput = true;   
                UpdateExternalManager(_input);
            }
        }

        private void UpdateExternalManager(PREACTInput input)
        {
            if (_externalManager != null)
            {
                _externalManager.UpdateInput(input);
            }
        }
                
        public enum LogType { Log, Warning, SimulationError, InputError, Event, Debug };
        private List<string> _consoleLog = new List<string>();
        /// <summary>
        /// Receives all the information from a WUINITY session, used by GUI.
        /// </summary>
        /// <param name="message"></param>
        public static void Message(Simulation simulation, LogType logType, string message)
        {
            //TODO: ReaderWriteLock
            if (_ENGINE == null)
            {
                return;
            }

            if (simulation != null && simulation.State == Simulation.SimulationState.Running)
            {
                message = "[Simulation# " + simulation.SimulationIndex + ", " +(int)simulation.CurrentTime + "s] " + message;
            }

            if (logType == LogType.Warning)
            {
                message = "WARNING: " + message;
            }
            else if (logType == LogType.SimulationError || logType == LogType.InputError)
            {
                message = "ERROR: " + message;
            }
            else if (logType == LogType.Event)
            {
                message = "EVENT: " + message;
            }
            else if(logType == LogType.Debug)
            {
                message = "!!!DEBUG!!!: " + message;
            }
            else
            {
                message = "LOG: " + message;
            }

            _ENGINE._consoleLog.Add("[" + DateTime.Now.ToLongTimeString() + "] " + message);

            if(_ENGINE._externalManager != null)
            {
                _ENGINE._externalManager.NewLogMessage(message);
            }

            if (logType == LogType.SimulationError)
            {
                _ENGINE.CloseSimulations(true);
            }           
        }

        static string[] logBuffer;
        public string[] GetMessageLog()
        {
            if (logBuffer == null || logBuffer.Length != _consoleLog.Count)
            {
                logBuffer = _consoleLog.ToArray();
            }

            return logBuffer;
        }

        public void ClearLog()
        {
            _consoleLog.Clear();
        }

        public void PauseSimulations()
        {
            for (int i = 0; i < _simulations.Length; ++i)
            {
                _simulations[i].SetPause(true);
            }
        }

        public void UnpauseSimulations()
        {
            for (int i = 0; i < _simulations.Length; ++i)
            {
                _simulations[i].SetPause(false);
            }
        }

        public void CloseSimulations(bool stoppedDueToError)
        {
            _stopSimulations = true;
            for (int i = 0; i < _simulations.Length; ++i)
            {
                if(_simulations[i] != null)
                {
                    if(stoppedDueToError)
                    {
                        _simulations[i].Stop("Critical error in simulation, aborting.", true);
                    }
                    else
                    {
                        _simulations[i].Stop("User has requested closing.", false);
                    }
                }
            }
        }

        //TODO: these below here are misplaced, but need to figure out where they fit better
        private void SaveAverageCurve(float[] data)
        {
            string[] output = new string[data.Length + 2];
            output[0] = "Time [s],ArrivalIndex [-]";
            output[1] = "0.0, 0";
            for (int i = 0; i < data.Length; i++)
            {
                output[i + 2] = data[i].ToString() + "," + (i + 1).ToString();
            }
            string path = Path.Combine(OutputFolder, _input.Simulation.Name + "_traffic_average.csv");
            System.IO.File.WriteAllLines(path, output);
        }

        byte[] _plotBytes;
        void CreatePlotData(double[] xData, double[] yData)
        {
            if (xData.Length > 0 && yData.Length > 0)
            {
                ScottPlot.Plot timeTraffic = new ScottPlot.Plot(512, 512);
                timeTraffic.AddScatterLines(xData, yData);
                timeTraffic.Title("Average cumulative arrival of cars");
                timeTraffic.YLabel("Number of cars [-]");
                timeTraffic.XLabel("Time [h]");
                //string plotPath = timeTraffic.SaveFig(System.IO.Path.Combine(WUIEngine.OUTPUT_FOLDER, "traffic_avg.png"));
                byte[] byteData = timeTraffic.GetImageBytes();
            }
        }

        /// <summary>
        /// Returns bytes for bitmap to draw a plot of average curve for evacuation.
        /// </summary>
        /// <returns></returns>
        public byte[] GetArrivalPlotBytes()
        {
            return _plotBytes;
        }
    }
}

