//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace PREACT.Runtime
{
    /// <summary>
    /// Contains all static data that is derived from input and needed for the simulation.
    /// </summary>
    public class PREACTData
    {
        public GeoData Geo;
        public EvacuationData Evacuation;
        public PopulationData Population;
        public TrafficData Traffic;
        public FireData Fire;
        public SmokeData Smoke;

        public PREACTData(IO.PREACTInput input)
        {
            Geo = new GeoData(input);         
            Evacuation = new EvacuationData(input);
            Population = new PopulationData();
            Traffic = new TrafficData();
            Fire = new FireData();
            Smoke = new SmokeData();            
        }       

        public void LoadAll(IO.PREACTInput input, string rootFolder, out bool success)
        {
            //need to load evacuation goals before routing as they rely on evacuation goals
            Population.LoadAll(input, rootFolder, out success);
            Evacuation.LoadAll(input, rootFolder, out success);
            Traffic.LoadAll(input, rootFolder, out success);
            Fire.LoadAll(input, rootFolder, Geo.UTMOrigin, out success);
            Smoke.LoadAll(input, rootFolder, out success); //does nothing right now
        }
    }
}

