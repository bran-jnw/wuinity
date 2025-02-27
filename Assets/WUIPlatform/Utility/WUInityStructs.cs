//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.Evacuation;

namespace WUIPlatform
{
    [System.Serializable] 
    public enum RoutePriority { Fastest, Closest, Forced }    

    [System.Serializable]
    public class RouteData
    {
        //public string name;
        public Itinero.Route route;
        public EvacuationGoal evacGoal;

        public RouteData(Itinero.Route route, EvacuationGoal evacGoal)
        {
            this.route = route;
            this.evacGoal = evacGoal;
        }
    }

    [System.Serializable] public enum EvacGoalType { Exit, Refugee }
        
    [System.Serializable]
    public class TrafficCellData
    {
        public int carCount;
        public int peopleCount;
    }
}

