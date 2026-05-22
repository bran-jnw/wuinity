//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Evacuation;
using PREACT.Math;

namespace PREACT.Traffic
{
    public abstract class TrafficModule : SimulationModule
    {
        protected List<double> _arrivalData;
        protected List<InjectedCar> _carsToInject;
        protected Dictionary<uint, TrafficModuleVehicle> _activeVehicles;

        protected struct InjectedCar
        {
            public Vector2d startLatLon;
            public EvacuationDestination evacuationDestination;
            public uint numberOfPeopleInCar;

            public InjectedCar(Vector2d startLatLon, EvacuationDestination evacuationGoal, uint numberOfPeopleInCar)
            {
                this.startLatLon = startLatLon;
                this.evacuationDestination = evacuationGoal;
                this.numberOfPeopleInCar = numberOfPeopleInCar;
            }
        }

        public TrafficModule(Simulation simulation) : base(simulation)
        {
            _arrivalData = new List<double>();
            _carsToInject = new List<InjectedCar>();
            _activeVehicles = new Dictionary<uint, TrafficModuleVehicle>();
        }

        /// <summary>
        /// Inject new car into the simulation and puts it in a waiting list (as this happens during a simulation step). Must be "consumed" later with PostUpdate().
        /// </summary>
        /// <param name="startLatLon"></param>
        /// <param name="evacuationGoal"></param>
        /// <param name="routeData"></param>
        /// <param name="numberOfPeopleInCar"></param>
        public void InsertNewCar(Vector2d startLatLon, EvacuationDestination evacuationGoal, uint numberOfPeopleInCar)
        {
            _carsToInject.Add(new InjectedCar(startLatLon, evacuationGoal, numberOfPeopleInCar));
        }

        public abstract void HandleNewCars();
        
        public abstract void InsertNewTrafficEvent(TrafficEvent tE);
        public abstract int GetTotalCarsSimulated();        
        public abstract int GetNumberOfCarsInSystem();
        public abstract void UpdateDestinations();
        public Dictionary<uint, TrafficModuleVehicle> GetActiveVehicles()
        {
            return _activeVehicles;
        }
        public abstract void SaveToFile(int simulationIndex);

        private static uint carCount = 0;
        protected static uint GetNewCarID()
        {
            ++carCount;
            return carCount;
        }
        public List<double> GetArrivalData()
        {
            return _arrivalData;
        }

        public abstract void HandleIgnitedFireCells(List<Vector2int> cellIndices);

        public abstract bool IsNetworkReachable(Vector2d startLatLong);

        public List<TrafficModuleVehicle> GetVehiclesInBoundingBox(Vector2d lowerLeftSimulationPos, Vector2d upperRightSimulationPos)
        {
            List<TrafficModuleVehicle> vehicles = new List<TrafficModuleVehicle>();

            foreach (TrafficModuleVehicle vehicle in _activeVehicles.Values)
            {
                if(vehicle.SimulationPos.x >= lowerLeftSimulationPos.x && vehicle.SimulationPos.x <= upperRightSimulationPos.x && vehicle.SimulationPos.y >= lowerLeftSimulationPos.y && vehicle.SimulationPos.y <= upperRightSimulationPos.y)
                {
                    vehicles.Add(vehicle);
                }
            }

            return vehicles;
        }

        public List<TrafficModuleVehicle> GetVehiclesInRadius(List<TrafficModuleVehicle> vehicles, Vector2d center, double radius)
        {
            vehicles.Clear();
            double radiusSqrd = radius * radius;
            foreach (TrafficModuleVehicle vehicle in _activeVehicles.Values)
            {
                double distanceSqrd = Vector2d.SqrMagnitude(center - vehicle.SimulationPos);
                if (distanceSqrd <= radiusSqrd)
                {
                    vehicles.Add(vehicle);
                }
            }

            return vehicles;
        }

        public abstract void SetManualDestination(List<TrafficModuleVehicle> vehicles, Vector2d simulationPos, EvacuationDestination evacuationDestination);
    }
}