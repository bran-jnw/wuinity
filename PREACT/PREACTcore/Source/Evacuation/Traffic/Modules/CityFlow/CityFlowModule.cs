//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using PREACT.Math;
using CityFlowCore;

namespace PREACT.Traffic
{
    public class CityFlowModule : TrafficModule
    {
        CityFlowCore.Engine _cityFlow;

        public CityFlowModule(Simulation simulation) : base(simulation)
        {
            SWIGTYPE_p_std__string configFile;
            //configFile = simulation.Input.Traffic.CityFlowInput.ConfigurationFile;
            //_cityFlow = new CityFlowCore.Engine(configFile, 4);
        }

        public override void Step(double currentTime, double deltaTime)
        {
            _cityFlow.nextStep();
            var vehicles = _cityFlow.getVehicles(true);
            //GetVehicles(engine);
            /*for (int j = 0; j < vehicles; ++j)
            {
                IntPtr b = _cityFlow.veh
                string c = Marshal.PtrToStringAnsi(b);
                //MonoBehaviour.print(c);
                //print(vehicles);
            }*/
        }

        public override bool IsSimulationDone()
        {
            throw new NotImplementedException();
        }

        public override void InsertNewTrafficEvent(TrafficEvent tE)
        {
            throw new NotImplementedException();
        }

        public override int GetTotalCarsSimulated()
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfCarsInSystem()
        {
            throw new NotImplementedException();
        }

        public override void UpdateDestinations()
        {
            throw new NotImplementedException();
        }

        public override void SaveToFile(int simulationIdentifier)
        {
            throw new NotImplementedException();
        }

        public override void HandleNewCars()
        {
            throw new NotImplementedException();
        }

        public override void HandleIgnitedFireCells(List<Vector2int> cellIndices)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsNetworkReachable(Vector2d startLatLong)
        {
            throw new NotImplementedException();
        }

        public override void SetManualDestination(List<TrafficModuleVehicle> vehicles, Vector2d simulationPos, Evacuation.EvacuationDestination evacuationDestination)
        {
            throw new NotImplementedException();
        }
    }
}

