using PREACT.Evacuation;
using System.Collections.Generic;
using PREACT.Math;
using System;
using SGPdotNET.Observation;

namespace PREACT.Detection
{
    /// <summary>
    /// This class is supposed to collect all the communication between different sub-modules, 
    /// e.g. traffic simulation needing information from the smoke or fire simulation.
    /// This is done to not clutter up the simulation class itself.
    /// </summary>
    public class DetectionManager
    {
        private Simulation _simulation;

        private DroneModule _droneModule;
        private ViirsTracking _viirs;

        SatelliteDetectionStatus[] _satelliteDetectionStatus;

        public SatelliteDetectionStatus[] SatelliteStatus { get => _satelliteDetectionStatus; }

        public DetectionManager(Simulation simulation)
        {
            _simulation = simulation;
            _viirs = new ViirsTracking();
        }

        public List<SimulationModule> CreateModules(WeatherManager weather, TimeManager time, out bool success)
        {
            List<SimulationModule> createdModules = new List<SimulationModule>();            

            CreateDroneModule(out success);
            if (success && _droneModule != null)
            {
                createdModules.Add(_droneModule);
            }
            else
            {
                return createdModules;
            }

            return createdModules;
        }

        private void CreateDroneModule(out bool success)
        {
            //Panos
            success = true;
        }

        List<(Vector2d, double)> _ignitions = new List<(Vector2d, double)>(10);
        public void RegisterFireIgnition(Vector2d latLon, double elevation)
        {
            _ignitions.Add((latLon, elevation));
        }

        bool _first = true;
        float _lastCheckTimer = 30f;
        public void PostStep(TimeManager time, float deltaTime)
        {
            bool checkSatelliteDetection = false;
            _lastCheckTimer += deltaTime;
            if(_lastCheckTimer >= 30f)
            {
                _lastCheckTimer = 0;
                checkSatelliteDetection = true;
            }

            if(checkSatelliteDetection)
            {
                for (int i = 0; i < _ignitions.Count; ++i)
                {
                    double fireArea = _simulation.Hazards.Wildfire.CurrentBurnArea;
                    _satelliteDetectionStatus = _viirs.UpdateDetectionStatus(time.CurrentUTCDateTime, _ignitions[i].Item1, _ignitions[i].Item2, fireArea);
                    for (int j = 0; j < _satelliteDetectionStatus.Length; ++j)
                    {
                        if (_satelliteDetectionStatus[j].IsInside && _satelliteDetectionStatus[j].CanDetect)
                        {
                            //Engine.Message(_simulation, Engine.LogType.Log, $"Ignition at lat/lon ({_ignitions[i].Item1.ToString()}) was observed by {_satelliteDetectionStatus[j].Satellite.Name} at {_satelliteDetectionStatus[j].OffNadirAngle} degrees off nadir.");
                        }
                    }
                }
                //_ignitions.Clear();
            }
            
        }
    }
}

