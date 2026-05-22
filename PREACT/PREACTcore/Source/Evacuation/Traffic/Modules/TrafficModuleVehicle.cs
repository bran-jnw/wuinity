//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Evacuation;
using PREACT.Math;

namespace PREACT.Traffic
{
    public abstract class TrafficModuleVehicle
    {
        protected uint _vehicleId;
        protected Vector2d _worldPosition;
        protected uint _numberOfPeople;
        protected float _totalTravelTime;
        protected string _vehicleClass;
        protected EvacuationDestination _destination;
        protected float _speedRatio;       

        public uint VehicleId { get => _vehicleId; }
        public Vector2d SimulationPos { get => _worldPosition; }
        public uint NumberOfPeople { get => _numberOfPeople; }
        public float TotalTravelTime { get => _totalTravelTime; }
        public string VehicleClass { get => _vehicleClass; }
        public EvacuationDestination Destination { get => _destination; }
        public float SpeedRatio { get => _speedRatio; }      


        public TrafficModuleVehicle(uint vehicleId, uint numberOfPeopleInCar, EvacuationDestination destination, string vehicleClass = "passenger")
        {
            _vehicleId = vehicleId;
            _numberOfPeople = numberOfPeopleInCar;
            _destination = destination;
            _vehicleClass = vehicleClass;
        }

        public void UpdateDestination(EvacuationDestination newDestination)
        {
            _destination = newDestination;
        }

        public abstract bool TryToArrive(double deltaTime, double currentTime);
    }
}

    
