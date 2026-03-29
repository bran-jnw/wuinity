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
        private WildfireModule _wildfireModule;
        private SmokeModule _smokeModule;
        private Simulation _simulation;

        public WildfireModule WildfireModule { get => _wildfireModule; }
        public SmokeModule SmokeModule { get => _smokeModule; }

        public HazardManager(Simulation simulation)
        {
            _simulation = simulation;
        }

        public List<SimulationModule> CreateModules(WeatherManager weather, TimeManager time, out bool success)
        {
            List<SimulationModule> createdModules = new List<SimulationModule>();

            CreateWildfireModule(_simulation, _simulation.Input, weather, time, out success);
            if(success && _wildfireModule != null)
            {
                createdModules.Add(_wildfireModule);
            }
            else
            {
                return createdModules;
            }
            
            CreateSmokeModule(_simulation, _simulation.Input, weather, time, out success);
            if (success && _smokeModule != null)
            {
                createdModules.Add(_smokeModule);
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
                    _wildfireModule = new AscFireImport(simulation);
                    Engine.Message(simulation, Engine.LogType.Log, "Fire module AscImport initiated.");
                }
                else if (input.WildfireModule.Module == WildfireModuleInput.WildfireModules.SimpleWildfireCA)
                {
                    //_wildfireModule = new CellParticleHybrid(simulation, input.WildfireModule.Data.LandscapeData, input.WildfireModule.Data.WuiArea, input.WildfireModule.Data.FuelModelsData, input.WildfireModule.Data.InitialFuelMoistureData, input.WildfireModule.Data.IgnitionPoints);
                    _wildfireModule = new SimpleWildfireCA(simulation, input.WildfireModule.Data.LandscapeData, input.WildfireModule.Data.IgnitionPoints, weather, time);
                    Engine.Message(simulation, Engine.LogType.Log, "Fire module CellParticleHybrid initiated.");
                }
                else if (input.WildfireModule.Module == WildfireModuleInput.WildfireModules.ElmClone)
                {
                    _wildfireModule = new ElmClone(simulation, input.WildfireModule.Data.LandscapeData, input.WildfireModule.Data.IgnitionPoints, weather, time);
                    Engine.Message(simulation, Engine.LogType.Log, "Fire module NarrowLevelSet initiated.");
                }
                else
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "Could not initiate fire module, aborting.");
                }
            }
            else
            {

                success = true;
                Engine.Message(simulation, Engine.LogType.Log, "No fire module was enabled.");
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
                    _smokeModule = new GlobalSmoke(simulation, input.SmokeModule.Data.ExtinctionRamp);
                    success = true;
                    return;
                }

                if (!input.WildfireModule.Enabled)
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "Smoke module that needs fire as source was enabled but no fire module was enabled, aborting.");
                }
                else
                {
                    if (input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuseMixingLayer)
                    {
                        _smokeModule = new AdvectDiffuseMixingLayer(simulation);
                    }
                    else if (input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuse3D)
                    {
                        _smokeModule = new AdvectDiffuse3D(simulation);
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
        }

        /// <summary>
        /// Returns optical density at ground level and location in simulation space.
        /// </summary>
        /// <returns></returns>
        public float GetExtinctionCoefficientAtPos(Vector2d pos)
        {
            float result = 0f;
            if(_smokeModule != null)
            {
                result = _smokeModule.GetSootDensityAtPos(pos) * 8700f; //TODO: user specified mass specific extinction coefficient
            }

            return result;
        }
    }
}
