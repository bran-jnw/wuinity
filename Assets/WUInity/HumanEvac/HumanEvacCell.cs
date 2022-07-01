using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Evac
{
    /// <summary>
    /// Class that holds all households that are contained at the start within this cell
    /// Also contains the route/routes from this cell to any evac goal that is reachable
    /// </summary>
    public class HumanEvacCell
    {
        public MacroHousehold[] macroHouseholds;
        public RouteCollection routeCollection;
        public Vector2D cellWorldSize;
        //public double cellDensity; //persons / km2
        public Vector2D closestNodeUnitySpace;
        public bool cellIsEvacuated;
        int cellIndex;

        /// <summary>
        /// Creates human evac cell ythat keeps track of households in the cell and the routes they can use after reaching their car.
        /// </summary>
        /// <param name="nodeCenter"></param>
        /// <param name="cellWorldSize"></param>
        /// <param name="route"></param>
        /// <param name="personsInCell"></param>
        public HumanEvacCell(Vector2D nodeCenter, Vector2D cellWorldSize, RouteCollection route, int personsInCell, int cellIndex)
        {
            EvacInput eO = WUInity.INPUT.Evacuation;

            this.cellWorldSize = cellWorldSize;
            this.routeCollection = route;
            this.cellIndex = cellIndex;

            int peopleWithoutHouseHold = personsInCell;
            List<int> personsPerHousehold = new List<int>();
            while (peopleWithoutHouseHold > 0)
            {
                int p = Random.Range(eO.minHouseholdSize, eO.maxHouseholdSize + 1);
                if (p > peopleWithoutHouseHold)
                {
                    p = peopleWithoutHouseHold;
                }
                personsPerHousehold.Add(p);
                peopleWithoutHouseHold -= p;
            }

            macroHouseholds = new MacroHousehold[personsPerHousehold.Count];

            Mapbox.Utils.Vector2d v = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(routeCollection.GetSelectedRoute().route.Shape[0].Latitude, routeCollection.GetSelectedRoute().route.Shape[0].Longitude, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
            closestNodeUnitySpace = new Vector2D(v.x, v.y);
            for (int i = 0; i < macroHouseholds.Length; ++i)
            {
                int evacGroupIndex = WUInity.RUNTIME_DATA.evacGroupIndices[i];
                macroHouseholds[i] = new MacroHousehold(this, nodeCenter, personsPerHousehold[i], MacroHumanSim.GetRandomWalkingSpeed(), MacroHumanSim.GetRandomResponseTime(evacGroupIndex));
            }
        }

        public int GetCellIndex()
        {
            return cellIndex;
        }
    }
}

