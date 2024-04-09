using WUIEngine.Population;
using WUIEngine.Runtime;
using WUIEngine.IO;
using System.IO;
using System.Collections.Generic;
using System;

namespace WUIEngine
{    
    public class Engine
    {        
        private WUInityInput _input;
        private WUInityOutput _output;
        private Simulation _sim;
        private PopulationManager _populationManager;
        string _workingFilePath;
        private DataStatus _dataStatus;

        private struct ValidCriticalData
        {
            public Vector2d lowerLeftLatLong;
            public Vector2d size;
            public float routeCellSize;

            public ValidCriticalData(WUInityInput input)
            {
                lowerLeftLatLong = input.Simulation.LowerLeftLatLong;
                size = input.Simulation.Size;
                routeCellSize = input.Evacuation.RouteCellSize;
            }
        }
        ValidCriticalData validInput;
                
        private Engine()
        {
            //needed for proper reading of input files on all systems
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        private static readonly Engine _engine = new Engine();
        public static Engine ENGINE { get => _engine; }

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
                if (ENGINE._runtimeData == null)
                {
                    ENGINE._runtimeData = new RuntimeData();
                }
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

        public static WUInityInput INPUT
        {
            get
            {
                /*if (ENGINE._input == null)
                {
                    ENGINE._input = new WUInityInput();
                }*/
                return ENGINE._input;
            }
        }

        public static WUInityOutput OUTPUT
        {
            get
            {
                if (ENGINE._output == null)
                {
                    ENGINE._output = new WUInityOutput();
                }
                return ENGINE._output;
            }
        }

        public static PopulationManager POPULATION
        {
            get
            {
                if (ENGINE._populationManager == null)
                {
                    ENGINE._populationManager = new PopulationManager();
                }
                return ENGINE._populationManager;
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
                DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(WORKING_FILE).ToString(), "output"));
                return path.ToString();
            }
        }

        /// <summary>
        /// Load an existing file and try to validate all of the associated data.
        /// If data is valid it is also loaded.
        /// </summary>
        /// <param name="input"></param>
        public void SetNewInputData(IO.WUInityInput input)
        {
            DATA_STATUS.Reset();
            DATA_STATUS.HaveInput = true;
            if (input == null)
            {
                _input = new IO.WUInityInput();
            }
            else
            {
                _input = input;
            }

            validInput = new ValidCriticalData(_input);

            //transform input to actual data
            RUNTIME_DATA.Population.LoadAll();
            RUNTIME_DATA.Evacuation.LoadAll();
            //need to load evacuation goals before routing as they rely on evacuation goals
            RUNTIME_DATA.Routing.LoadAll();
            RUNTIME_DATA.Traffic.LoadAll();
            RUNTIME_DATA.Fire.LoadAll();

            UpdateMapResourceStatus();

#if USING_UNITY
            WUInity.WUInity.GUI.SetDirty();
            //this needs map and evac goals
            WUInity.WUInity.INSTANCE.SpawnMarkers();
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
            DATA_STATUS.MapLoaded = RUNTIME_DATA.LoadMapbox();

#if USING_UNITY
            WUInity.WUInity.INSTANCE.UpdateSimBorders();
            WUInity.WUInity.INSTANCE.WUICamera.SetCameraStartPosition(INPUT.Simulation.Size);
#endif
            bool coordinatesAreDirty = true;
            bool sizeIsDirty = true;

            if (validInput.lowerLeftLatLong.x == INPUT.Simulation.LowerLeftLatLong.x
                && validInput.lowerLeftLatLong.y == INPUT.Simulation.LowerLeftLatLong.y)
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
            validInput.lowerLeftLatLong = INPUT.Simulation.LowerLeftLatLong;
        }

        public void UpdateEvacResourceStatus()
        {
            bool cellSizeIsDirty = true;

            if (validInput.routeCellSize == _input.Evacuation.RouteCellSize)
            {
                cellSizeIsDirty = false;
            }
        }
        
        public enum LogType { Log, Warning, Error, Event };
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
            else
            {
                message = "LOG: " + message;
            }

            ENGINE.simLog.Add("[" + DateTime.Now.ToLongTimeString() + "] " + message);

#if USING_UNITY
            if (UnityEngine.Application.isEditor || UnityEngine.Debug.isDebugBuild)
            {
                UnityEngine.Debug.Log(message);
            }
#endif

            if (logType == LogType.Error)
            {
                SIM.StopSim("Simulation can't run, please check log.");
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
    }
}

