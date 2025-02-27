//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using WUIPlatform.IO;
using System.Numerics;

namespace WUIPlatform.Pedestrian
{
    /// <summary>
    /// Class that holds all households that are contained at the start within this cell
    /// Also contains the route/routes from this cell to any evac goal that is reachable
    /// </summary>
    public class HumanEvacCell
    {
        public MacroHousehold[] macroHouseholds;
        public RouteCollection routeCollection;
        public Vector2d cellWorldSize;
        //public double cellDensity; //persons / km2
        public Vector2d closestNodeSimulationSpace;
        public bool cellIsEvacuated;
        int cellIndex;
        private Vector2d _roadAccesLatLon;

        /// <summary>
        /// Creates human evac cell that keeps track of households in the cell and the routes they can use after reaching their car.
        /// </summary>
        /// <param name="nodeCenter"></param>
        /// <param name="cellWorldSize"></param>
        /// <param name="routeCollection"></param>
        /// <param name="personsInCell"></param>
        public HumanEvacCell(Vector2d nodeCenter, Vector2d cellWorldSize, Vector2d roadAccessLatLon, int personsInCell, int cellIndex)
        {
            PopulationInput eO = WUIEngine.INPUT.Population;

            this.cellWorldSize = cellWorldSize;
            this.cellIndex = cellIndex;
            _roadAccesLatLon = roadAccessLatLon;

            int peopleWithoutHouseHold = personsInCell;
            List<int> personsPerHousehold = new List<int>();
            while (peopleWithoutHouseHold > 0)
            {
                int p = Random.Range(eO.MinHouseholdSize, eO.MaxHouseholdSize);
                if (p > peopleWithoutHouseHold)
                {
                    p = peopleWithoutHouseHold;
                }
                personsPerHousehold.Add(p);
                peopleWithoutHouseHold -= p;
            }

            macroHouseholds = new MacroHousehold[personsPerHousehold.Count];

            closestNodeSimulationSpace = GeoConversions.GeoToWorldPosition(roadAccessLatLon.x, roadAccessLatLon.y, WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale);
            for (int i = 0; i < macroHouseholds.Length; ++i)
            {
                int evacGroupIndex = WUIEngine.RUNTIME_DATA.Evacuation.EvacGroupIndices[i];
                //macroHouseholds[i] = new MacroHousehold(this, nodeCenter, personsPerHousehold[i], MacroHouseholdSim.GetRandomWalkingSpeed(), MacroHouseholdSim.GetRandomResponseTime(evacGroupIndex));
            }
        }

        public int GetCellIndex()
        {
            return cellIndex;
        }

        public Vector2d GetRoadAccessLatLon()
        {
            return _roadAccesLatLon;
        }
    }
}

