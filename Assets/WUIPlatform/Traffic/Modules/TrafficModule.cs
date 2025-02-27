//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Numerics;
using WUIPlatform.Evacuation;

namespace WUIPlatform.Traffic
{
    public abstract class TrafficModule : SimulationModule
    {
        protected List<float> arrivalData;
        protected List<InjectedCar> carsToInject;
        protected Vector4[] carsToRender;

        protected struct InjectedCar
        {
            public Vector2d startLatLong;
            public EvacuationGoal evacuationGoal;
            public uint numberOfPeopleInCar;

            public InjectedCar(Vector2d startLatLong, EvacuationGoal evacuationGoal, uint numberOfPeopleInCar)
            {
                this.startLatLong = startLatLong;
                this.evacuationGoal = evacuationGoal;
                this.numberOfPeopleInCar = numberOfPeopleInCar;
            }
        }

        public TrafficModule()
        {
            arrivalData = new List<float>();
            carsToInject = new List<InjectedCar>();
            carsToRender = new Vector4[1];
            carsToRender[0].W = -1f;
        }

        /// <summary>
        /// Inject new car into the simulation and puts it in a waiting list (as this happens during a simulation step). Must be "consumed" later with PostUpdate().
        /// </summary>
        /// <param name="startLatLong"></param>
        /// <param name="evacuationGoal"></param>
        /// <param name="routeData"></param>
        /// <param name="numberOfPeopleInCar"></param>
        public void InsertNewCar(Vector2d startLatLong, EvacuationGoal evacuationGoal, uint numberOfPeopleInCar)
        {
            carsToInject.Add(new InjectedCar(startLatLong, evacuationGoal, numberOfPeopleInCar));
        }

        public abstract void HandleNewCars();
        
        public abstract void InsertNewTrafficEvent(TrafficEvent tE);
        public abstract int GetTotalCarsSimulated();        
        public abstract int GetNumberOfCarsInSystem();
        public abstract void UpdateEvacuationGoals();
        public abstract Vector4[] GetCarWorldPositionsStatesCarIDs();
        public abstract void SaveToFile(int runNumber);

        private static uint carCount = 0;
        protected static uint GetNewCarID()
        {
            ++carCount;
            return carCount;
        }
        public List<float> GetArrivalData()
        {
            return arrivalData;
        }

        public abstract void HandleIgnitedFireCells(List<Vector2int> cellIndices);

        public abstract bool IsNetworkReachable(Vector2d startLatLong);
    }
}