using PREACT.Math;
using PREACT.Wildfire;
using PREACT.Dispersion;
using PREACT.Input;
using System.Collections.Generic;

namespace PREACT
{
    /// <summary>
    /// This class is supposed to collect all the communication between different sub-modules, 
    /// e.g. traffic simulation needing information from the smoke or fire simulation.
    /// This is done to not clutter up the simulation class itself.
    /// </summary>
    public class HazardManager
    {
        private WildfireModule _wildfire;
        private SmokeModule _smoke;
        private Simulation _simulation;

        //data products
        float[,] _wildfireFrontDistance;

        public WildfireModule Wildfire { get => _wildfire; }
        public SmokeModule Smoke { get => _smoke; }

        public HazardManager(Simulation simulation)
        {
            _simulation = simulation;
        }

        public void PostStep(float simulationTime)
        {
            CalculateWildfireDistanceTransform(simulationTime);
        }
                
        private void CalculateWildfireDistanceTransform(float simulationTime)
        {
            if(!_wildfire.Ignited() ||  (int)simulationTime % 300 != 0)
            {
                return;
            }

            float[,] front = _wildfire.GetMaxROS();
            if(_wildfireFrontDistance == null)
            {
                int xDim = front.GetLength(0);
                int yDim = front.GetLength(1);
                _wildfireFrontDistance = new float[xDim, yDim]; 
                for(int j = 0; j < yDim; ++j)
                {
                    for (int i = 0; i < xDim; ++i)
                    {
                        _wildfireFrontDistance[i, j] = float.MaxValue;
                    }
                }
                
            }
            Utility.Analysis.EuclideanDistanceTransform.ComputeEDT(front, _wildfireFrontDistance, _wildfire.GetCellSizeX(), _wildfire.GetCellSizeY(), 0f); 
        }

        public float DistanceToWildfire(Vector2d simulationPos)
        {
            float distance = float.MaxValue;

            Vector2int cellIndex = _simulation.Spatial.GetWildfireCellIndex(simulationPos, out bool inside);
            if(inside)
            {
                distance = _wildfireFrontDistance[cellIndex.x, cellIndex.y];
            }

            return distance;
        }

        public List<SimulationModule> CreateModules(WeatherManager weather, TimeManager time, out bool success)
        {
            List<SimulationModule> createdModules = new List<SimulationModule>();

            CreateWildfireModule(_simulation, _simulation.Input, weather, time, out success);
            if(success && _wildfire != null)
            {
                createdModules.Add(_wildfire);
            }
            else
            {
                return createdModules;
            }
            
            CreateSmokeModule(_simulation, _simulation.Input, weather, time, out success);
            if (success && _smoke != null)
            {
                createdModules.Add(_smoke);
            }
            else
            {
                return createdModules;
            }

            return createdModules;
        }

        private void CreateWildfireModule(Simulation simulation, PREACTInput input, WeatherManager weather, TimeManager time, out bool success)
        {
            success = false;

            if (input.WildfireModule.Enabled)
            {
                if (input.WildfireModule.Module == WildfireModuleInput.WildfireModules.AscImport)
                {
                    _wildfire = new AscFireImport(simulation);
                    Engine.Message(simulation, Engine.LogType.Log, $"Wildfire module {nameof(AscFireImport)} initiated.");
                }
                else if (input.WildfireModule.Module == WildfireModuleInput.WildfireModules.SimpleWildfireCA)
                {
                    //_wildfireModule = new CellParticleHybrid(simulation, input.WildfireModule.Data.LandscapeData, input.WildfireModule.Data.WuiArea, input.WildfireModule.Data.FuelModelsData, input.WildfireModule.Data.InitialFuelMoistureData, input.WildfireModule.Data.IgnitionPoints);
                    _wildfire = new SimpleWildfireCA(simulation, input.WildfireModule.Data.LandscapeData, input.WildfireModule.Data.IgnitionPoints, weather, time);
                    Engine.Message(simulation, Engine.LogType.Log, $"Wildfire module {nameof(SimpleWildfireCA)} initiated.");
                }
                else if (input.WildfireModule.Module == WildfireModuleInput.WildfireModules.ElmClone)
                {
                    _wildfire = new ElmClone(simulation, input.WildfireModule.Data.LandscapeData, input.WildfireModule.Data.IgnitionPoints, weather, time);
                    Engine.Message(simulation, Engine.LogType.Log, $"Wildfire module {nameof(ElmClone)} initiated.");
                }
                else
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "Could not initiate wildfire module, aborting.");
                }
            }
            else
            {

                success = true;
                Engine.Message(simulation, Engine.LogType.Log, "No fire module was enabled.");
            }

            if(_wildfire != null)
            {
                success = true;
            }
        }

        private void CreateSmokeModule(Simulation simulation, PREACTInput input, WeatherManager weather, TimeManager time, out bool success)
        {
            success = false;

            //can only run together for now
            if (input.SmokeModule.Enabled)
            {
                //simulation module does not need the fire
                if (input.SmokeModule.Module == SmokeInput.SmokeModules.GlobalSmoke)
                {
                    _smoke = new GlobalSmoke(simulation, input.SmokeModule.Data.ExtinctionRamp);
                }
                else if (!input.WildfireModule.Enabled)
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "Smoke module that needs wildfire as source was enabled but no wildfire module was enabled, aborting.");
                }
                else
                {
                    if (input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuseMixingLayer)
                    {
                        _smoke = new AdvectDiffuseMixingLayer(simulation);
                    }
                    else if (input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuse3D)
                    {
                        _smoke = new AdvectDiffuse3D(simulation);
                        Engine.Message(simulation, Engine.LogType.Log, "Smoke module AdvectDiffuse3D initiated.");
                    }
                    else if (input.SmokeModule.Module == SmokeInput.SmokeModules.BoxModel)
                    {
                        //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                    }
                }
            }
            else
            {
                success = true;
                Engine.Message(simulation, Engine.LogType.Log, "No smoke module was enabled.");
            }

            if (_smoke != null)
            {
                success = true;
            }
        }

        /// <summary>
        /// Returns optical density at ground level and location in simulation space.
        /// </summary>
        /// <returns></returns>
        public float GetExtinctionCoefficientAtPos(Vector2d pos)
        {
            float result = 0f;
            if(_smoke != null)
            {
                result = _smoke.GetSootDensityAtPos(pos) * 8700f; //TODO: user specified mass specific extinction coefficient
            }

            return result;
        }
    }
}
