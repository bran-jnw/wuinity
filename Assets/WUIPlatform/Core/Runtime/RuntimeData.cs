//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.Population;
using System;
using WUIPlatform.Utility;

namespace WUIPlatform.Runtime
{
    /// <summary>
    /// Contains all data that gets created during runtime before simulation starts.
    /// </summary>
    public class RuntimeData
    {
        public SimulationData Simulation;
        public EvacuationData Evacuation;
        public PopulationData Population;
        public RoutingData Routing;
        public TrafficData Traffic;
        public FireData Fire;
        public SmokeData Smoke;

        public RuntimeData()
        {
            Simulation = new SimulationData();
            //Map = new MapData();
            //Visualization = new VisualizationData();         
            Evacuation = new EvacuationData();
            Population = new PopulationData();
            Routing = new RoutingData();
            Traffic = new TrafficData();
            Fire = new FireData();
            Smoke = new SmokeData();
        }       
    }
}

