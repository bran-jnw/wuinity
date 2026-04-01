//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace PREACT.Pedestrian
{
    public abstract class PedestrianModule : SimulationModule
    {
        public PedestrianModule(Simulation simulation) : base(simulation) 
        {
            
        }

        public abstract void ReactToWildfire(double simulationTime);
        public abstract int GetTotalCars();
        public abstract int GetPeopleStaying();
        public abstract int GetPeopleLeft();
        public abstract int GetCarsReached();
        public abstract int GetTotalPopulation();
        public abstract int GetTotalHouseHolds();
    }
}

