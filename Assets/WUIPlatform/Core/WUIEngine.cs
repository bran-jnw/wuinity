//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.Population;
using WUIPlatform.Runtime;
using WUIPlatform.IO;
using System.IO;
using System.Collections.Generic;
using System;

namespace WUIPlatform
{    
    public class WUIEngine
    {        
        private WUIEngineInput _input;
        public static WUIEngineInput INPUT { get => ENGINE._input; }
        private WUIEngineOutput _output;
        private Simulation _sim;
        private string _workingFilePath;
        private DataStatus _dataStatus;       

        private struct ValidCriticalData
        {
            public Vector2d lowerLeftLatLong;
            public Vector2d size;
            public float routeCellSize;

            public ValidCriticalData(WUIEngineInput input)
            {
                lowerLeftLatLong = input.Simulation.LowerLeftLatLon;
                size = input.Simulation.DomainSize;
                routeCellSize = input.Evacuation.PaintCellSize;
            }
        }
        ValidCriticalData validInput;
          
        private static readonly WUIEngine _ENGINE = new WUIEngine();
        public static WUIEngine ENGINE { get => _ENGINE; }

        private WUIEngine()
        {
            //needed for proper reading of input files on all systems
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        public static Simulation SIM
        {
            get
            {
                if (ENGINE._sim == null)
                {
                    ENGINE._sim = new Simulation();
                }
                return ENGINE._sim;
            }
        }

        RuntimeData _runtimeData;
        public static RuntimeData RUNTIME_DATA
        {
            get
            {
                return ENGINE._runtimeData;
            }
        }

        public static DataStatus DATA_STATUS
        {
            get
            {
                if (ENGINE._dataStatus == null)
                {
                    ENGINE._dataStatus = new DataStatus();
                }
                return ENGINE._dataStatus;
            }
        }

        

        public static WUIEngineOutput OUTPUT
        {
            get
            {
                if (ENGINE._output == null)
                {
                    ENGINE._output = new WUIEngineOutput();
                }
                return ENGINE._output;
            }
        }

        public static string DATA_FOLDER
        {
            get
            {
#if USING_UNITY
                return Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).ToString(), "external_data");
#endif
            }
        }

        public static string WORKING_FILE
        {
            get => ENGINE._workingFilePath;        
            set => ENGINE._workingFilePath = value;
        }

        public static string WORKING_FOLDER { get => Path.GetDirectoryName(WORKING_FILE); }

        public static string OUTPUT_FOLDER
        {
            get
            {
                DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(WORKING_FILE).ToString(), INPUT.Simulation.Id + "_output"));
                return path.ToString();
            }
        }

        Simulation[] _simulations;
        Simulation _mainSimulation; //this one talks to any visualizer 
        private Visualization.WUIShowCommunicator _wuiShow;
        public Visualization.WUIShowCommunicator WUIShow { get => _wuiShow; }
        public void RunSimulations()
        {    
            PreSimulations();

            System.Threading.Tasks.Parallel.For(0, _simulations.Length, index =>
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

            PostSimulations();
        }

        private void PreSimulations()
        {
            _simulations = new Simulation[RUNTIME_DATA.Simulation.NumberOfRuns];
            for (int i = 0; i < _simulations.Length; ++i)
            {
                _simulations[i] = new Simulation(this, _input, i);
            }
            //by default simulation 0 will be the main to be displayed
            SetMainSimulation(0);

            if (_input.WUIShow.SendDataToWUIShow && _input.Simulation.RunTrafficModule)
            {
                _wuiShow = new Visualization.WUIShowCommunicator(_input.WUIShow.WuiShowServerIP, _input.WUIShow.WuiShowServerPort, 0, _input.Simulation.LowerLeftLatLon.y, _input.Simulation.LowerLeftLatLon.x);
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
                WUIEngine.LOG(WUIEngine.LogType.Log, " Average total evacuation time: " + cumulativeTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations before converging according to user set criteria.");

            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Average total evacuation time: " + cumulativeTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulation/s.");
            }

            WUIEngineOutput.SaveLogToDisk(_consoleLog, _input.Simulation.Id);
        }

        float cumulativeTotalEvacTime = 0.0f;
        int convergedInSequence = 0;
        List<List<float>> trafficArrivalDataCollection = new List<List<float>>();
        /// <summary>
        /// Each simulation calls this function when it is done to see if evacuation time has vonverged and simulations should be stopped.
        /// </summary>
        /// <param name="simulation"></param>
        public void CollectSimulationStatistics(Simulation simulation)
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
                float pastAverage = cumulativeTotalEvacTime / (resultCount -1);
                cumulativeTotalEvacTime += simulation.CurrentTime;
                float currentAverage = cumulativeTotalEvacTime / resultCount;
                float convergenceCriteria = (currentAverage - pastAverage) / currentAverage;
                //if convergence met we can stop
                if (convergenceCriteria < RUNTIME_DATA.Simulation.ConvergenceMaxDifference)
                {
                    ++convergedInSequence;
                    //we are done
                    if (_input.Simulation.StopAfterConverging && convergedInSequence > WUIEngine.RUNTIME_DATA.Simulation.ConvergenceMinSequence)
                    {
                        StopSimulations();
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

        private void SaveAverageCurve(float[] data)
        {
            string[] output = new string[data.Length + 2];
            output[0] = "Time [s],ArrivalIndex [-]";
            output[1] = "0.0, 0";
            for (int i = 0; i < data.Length; i++)
            {
                output[i + 2] = data[i].ToString() + "," + (i + 1).ToString();
            }
            WUIEngineInput wuiIn = INPUT;
            string path = System.IO.Path.Combine(WUIEngine.OUTPUT_FOLDER, wuiIn.Simulation.Id + "_traffic_average.csv");
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

        /// <summary>
        /// Load an existing file and try to validate all of the associated data.
        /// If data is valid it is also loaded.
        /// </summary>
        /// <param name="input"></param>
        public void SetNewInputData(WUIEngineInput input)
        {      
            DATA_STATUS.Reset();
            DATA_STATUS.HaveInput = true;
            if (input == null)
            {
                _input = new WUIEngineInput();
            }
            else
            {
                _input = input;
            }

            validInput = new ValidCriticalData(_input);

            _runtimeData = new RuntimeData();
            //transform input to actual data
            LOG(LogType.Log, "Loading referenced data from input file...");
            RUNTIME_DATA.Evacuation.LoadAll();
            RUNTIME_DATA.Population.LoadAll();
            //RUNTIME_DATA.Routing.LoadAll(); //this does nothing right now
            //need to load evacuation goals before routing as they rely on evacuation goals
            RUNTIME_DATA.Traffic.LoadAll();
            RUNTIME_DATA.Fire.LoadAll();
            RUNTIME_DATA.Smoke.LoadAll(); //does nothing right now

            UpdateMapResourceStatus();

#if USING_UNITY
            WUInity.WUInityEngine.GUI.SetDirty();
            //this needs map and evac goals
            WUInity.WUInityEngine.INSTANCE.SpawnEvacuationGoalMarkers();
#endif


        }

        /// <summary>
        /// Called when the user want to create a new file from scratch in the GUI.
        /// </summary>
        /// <param name="input"></param>
        public void CreateNewInputData()
        {
            SetNewInputData(null);
        }


        public void UpdateMapResourceStatus()
        {
#if USING_UNITY
            DATA_STATUS.MapLoaded = WUInity.WUInityEngine.INSTANCE.LoadMapbox();
            WUInity.WUInityEngine.INSTANCE.UpdateSimBorders();
            WUInity.WUInityEngine.INSTANCE.WUICamera.SetCameraStartPosition(INPUT.Simulation.DomainSize);
#endif
            bool coordinatesAreDirty = true;
            bool sizeIsDirty = true;

            if (validInput.lowerLeftLatLong.x == INPUT.Simulation.LowerLeftLatLon.x
                && validInput.lowerLeftLatLong.y == INPUT.Simulation.LowerLeftLatLon.y)
            {
                coordinatesAreDirty = false;
            }

            if (validInput.size.x == INPUT.Simulation.DomainSize.x
                && validInput.size.y == INPUT.Simulation.DomainSize.y)
            {
                sizeIsDirty = false;
            }

            //fix any problems
            if (coordinatesAreDirty || sizeIsDirty)
            {
                //basically mark all data as not valid anymore
                DATA_STATUS.Reset();
                DATA_STATUS.MapLoaded = true;
            }

            //set cached data to be current data
            validInput.size = INPUT.Simulation.DomainSize;
            validInput.lowerLeftLatLong = INPUT.Simulation.LowerLeftLatLon;
        }

        public void UpdateEvacResourceStatus()
        {
            bool cellSizeIsDirty = true;

            if (validInput.routeCellSize == _input.Evacuation.PaintCellSize)
            {
                cellSizeIsDirty = false;
            }
        }
        
        public enum LogType { Log, Warning, SimError, InputError, Event, Debug };
        private List<string> _consoleLog = new List<string>();
        /// <summary>
        /// Receives all the information from a WUINITY session, used by GUI.
        /// </summary>
        /// <param name="message"></param>
        public static void LOG(LogType logType, string message)
        {
            if (SIM.State == Simulation.SimulationState.Running)
            {
                message = "[" + (int)SIM.CurrentTime + "s] " + message;
            }

            if (logType == LogType.Warning)
            {
                message = "WARNING: " + message;
            }
            else if (logType == LogType.SimError || logType == LogType.InputError)
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

            ENGINE._consoleLog.Add("[" + DateTime.Now.ToLongTimeString() + "] " + message);

#if USING_UNITY
            if (UnityEngine.Application.isEditor) //&& !WUInity.WUInityEngine.INSTANCE.SuppressMessages) // || UnityEngine.Debug.isDebugBuild
            {
                UnityEngine.Debug.Log(message);
            }
#endif

            if (logType == LogType.SimError)
            {
                SIM.Stop("Simulation can't run, please check log.", true);
            }           
        }

        static string[] logBuffer;
        public static string[] GetLog()
        {
            if (logBuffer == null || logBuffer.Length != ENGINE._consoleLog.Count)
            {
                logBuffer = ENGINE._consoleLog.ToArray();
            }

            return logBuffer;
        }

        public static void ClearLog()
        {
            ENGINE._consoleLog.Clear();
        }

        public void PauseSimulations()
        {
            for (int i = 0; i < _simulations.Length; ++i)
            {
                _simulations[i].SetPause(true);
            }
        }

        public void StopSimulations()
        {
            for (int i = 0; i < _simulations.Length; ++i)
            {
                _simulations[i].Stop("User requested stop.", false);
            }
        }

        public static void Exit()
        {
            if(ENGINE._sim != null)
            {
                ENGINE._sim.Stop("User has requested closing.", true);
            }
        }
    }
}

