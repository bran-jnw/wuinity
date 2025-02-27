//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Numerics;
using WUIPlatform.Traffic;

namespace WUIPlatform.IO
{
    [System.Serializable] 
    public class MacroTrafficSimInput
    {
        public string RoadTypesFile;
        public float StallSpeed = 5f;
        public enum RoutingChoice { Fastest, Closest, Random, EvacGroup };
        public RoutingChoice Routing = RoutingChoice.Closest;
        public Vector2 BackGroundDensityMinMax = Vector2.Zero;
        public TrafficAccident[] TrafficAccidents;// = TrafficAccident.GetDummy();
        public ReverseLanes[] ReverseLanes;// = Traffic.ReverseLanes.GetDummy();
        public TrafficInjection[] TrafficInjections;// = TrafficInjection.GetTemplate();
        public TrafficProbe[] TrafficProbes;// = TrafficProbe.GetTemplate();

        //TODO: write parser
    }
}
