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
using System.Diagnostics;

namespace WUIPlatform
{    
    public class WUIEngine
    {        
        private WUIEngineInput _input;
        private WUIEngineOutput _output;
        private Simulation _sim;
        private string _workingFilePath;
        private DataStatus _dataStatus;       
        private Stopwatch _stopWatch;
        public Stopwatch StopWatch { get => _stopWatch; }

        private struct ValidCriticalData
        {
            public Vector2d lowerLeftLatLong;
            public Vector2d size;
            public float routeCellSize;

            public ValidCriticalData(WUIEngineInput input)
            {
                lowerLeftLatLong = input.Simulation.LowerLeftLatLon;
                size = input.Simulation.Size;
                routeCellSize = input.Evacuation.RouteCellSize;
            }
        }
        ValidCriticalData validInput;
          
        private static readonly WUIEngine _ENGINE = new WUIEngine();
        public static WUIEngine ENGINE { get => _ENGINE; }


        private WUIEngine()
        {
            //needed for proper reading of input files on all systems
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _stopWatch = new Stopwatch();
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

        public static WUIEngineInput INPUT
        {
            get
            {
                return ENGINE._input;
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
            get
            {
                return ENGINE._workingFilePath;
            }
            set
            {
                ENGINE._workingFilePath = value;
            }
        }

        public static string WORKING_FOLDER
        {
            get
            {
                return Path.GetDirectoryName(WORKING_FILE);
            }
        }

        public static string OUTPUT_FOLDER
        {
            get
            {
                DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(WORKING_FILE).ToString(), INPUT.Simulation.SimulationID + "_output"));
                return path.ToString();
            }
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
            RUNTIME_DATA.Evacuation.LoadAll();
            RUNTIME_DATA.Population.LoadAll();
            RUNTIME_DATA.Routing.LoadAll();
            //need to load evacuation goals before routing as they rely on evacuation goals
            RUNTIME_DATA.Traffic.LoadAll();
            RUNTIME_DATA.Fire.LoadAll();

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
            WUInity.WUInityEngine.INSTANCE.WUICamera.SetCameraStartPosition(INPUT.Simulation.Size);
#endif
            bool coordinatesAreDirty = true;
            bool sizeIsDirty = true;

            if (validInput.lowerLeftLatLong.x == INPUT.Simulation.LowerLeftLatLon.x
                && validInput.lowerLeftLatLong.y == INPUT.Simulation.LowerLeftLatLon.y)
            {
                coordinatesAreDirty = false;
            }

            if (validInput.size.x == INPUT.Simulation.Size.x
                && validInput.size.y == INPUT.Simulation.Size.y)
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
            validInput.size = INPUT.Simulation.Size;
            validInput.lowerLeftLatLong = INPUT.Simulation.LowerLeftLatLon;
        }

        public void UpdateEvacResourceStatus()
        {
            bool cellSizeIsDirty = true;

            if (validInput.routeCellSize == _input.Evacuation.RouteCellSize)
            {
                cellSizeIsDirty = false;
            }
        }
        
        public enum LogType { Log, Warning, Error, Event, Debug };
        private List<string> simLog = new List<string>();
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
            else if (logType == LogType.Error)
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

            ENGINE.simLog.Add("[" + DateTime.Now.ToLongTimeString() + "] " + message);

#if USING_UNITY
            if (UnityEngine.Application.isEditor) // || UnityEngine.Debug.isDebugBuild
            {
                UnityEngine.Debug.Log(message);
            }
#endif

            if (logType == LogType.Error)
            {
                SIM.Stop("Simulation can't run, please check log.", true);
            }           
        }

        static string[] logBuffer;
        public static string[] GetLog()
        {
            if (logBuffer == null || logBuffer.Length != ENGINE.simLog.Count)
            {
                logBuffer = ENGINE.simLog.ToArray();
            }

            return logBuffer;
        }

        public static void ClearLog()
        {
            ENGINE.simLog.Clear();
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

