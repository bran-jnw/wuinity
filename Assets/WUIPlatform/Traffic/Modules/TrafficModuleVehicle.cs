//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using WUIPlatform.Evacuation;

namespace WUIPlatform.Traffic
{
    public abstract class TrafficModuleVehicle
    {
        protected uint _vehicleId;
        protected Vector2d _worldPosition;
        protected uint _numberOfPeople;
        protected float _totalTravelTime;
        protected string _vehicleType;
        protected EvacuationDestination _destination;
        protected float _speedRatio;

        public uint VehicleId { get => _vehicleId; }
        public Vector2d WorldPosition { get => _worldPosition; }
        public uint NumberOfPeople { get => _numberOfPeople; }
        public float TotalTravelTime { get => _totalTravelTime; }
        public string VehicleType { get => _vehicleType; }
        public EvacuationDestination Destination { get => _destination; }
        public float SpeedRatio { get => _speedRatio; }


        public TrafficModuleVehicle(uint carId, uint numberOfPeopleInCar, EvacuationDestination destination)
        {
            _vehicleId = carId;
            _numberOfPeople = numberOfPeopleInCar;
            _destination = destination;
        }

        public abstract void Arrive();
    }
}

    
