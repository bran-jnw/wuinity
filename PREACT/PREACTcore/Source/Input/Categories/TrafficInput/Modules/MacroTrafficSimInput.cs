//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Numerics;
using PREACT.Traffic;

namespace PREACT.Input
{
    [System.Serializable] 
    public class MacroTrafficSimInput
    {
        public enum RoutingPriority { Fastest, Closest, Random, EvacGroup };

        public string RoadTypesFile;
        public float StallSpeed = 5f;        
        public RoutingPriority Routing = RoutingPriority.Closest;
        public Vector2 BackGroundDensityMinMax = Vector2.Zero;
        public List<TrafficAccident> TrafficAccidents = new List<TrafficAccident>();
        public List<ReverseLanes> ReverseLanes = new List<ReverseLanes>();
        public List<TrafficInjection> TrafficInjections = new List<TrafficInjection>();
        public TrafficProbe[] TrafficProbes;

        //TODO: write parser
    }
}
