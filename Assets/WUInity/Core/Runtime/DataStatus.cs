using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.WUInity;

namespace WUInity
{
    public class DataStatus
    {
        public bool HaveInput;

        public bool MapLoaded;

        public bool PopulationLoaded;
        public bool PopulationCorrectedForRoutes;
        public bool GlobalGPWAvailable;
        public bool LocalGPWLoaded;

        public bool OpticalDensityLoaded;

        public bool RouteCollectionLoaded;
        public bool RouterDbLoaded;

        public bool OsmFileValid;

        public bool LcpLoaded, FuelModelsLoaded;

        public bool ResponseCurvesValid;

        public bool CanRunSimulation()
        {
            bool canRun = true;
            if (!MapLoaded)
            {
                canRun = false;
                LOG("ERROR: Map is not loaded.");
            }

            if (!PopulationLoaded && (!LocalGPWLoaded || !GlobalGPWAvailable))
            {
                canRun = false;
                LOG("ERROR: Population is not loaded and no local nor global GPW file is found to build it from.");
            }

            if (!RouterDbLoaded && !OsmFileValid)
            {
                canRun = false;
                LOG("ERROR: No router database loaded and no valid OSM file was found to build it from.");
            }

            if (INPUT.Simulation.RunFireSim)
            {
                if (!LcpLoaded)
                {
                    canRun = false;
                    LOG("ERROR: No LCP file loaded but fire spread is activated.");
                }
            }

            if (INPUT.Simulation.RunEvacSim)
            {
                if (RUNTIME_DATA.Evacuation.ResponseCurves == null)
                {
                    canRun = false;
                    LOG("ERROR: No valid response curves have been loaded.");
                }

            }

            return canRun;
        }

        public void Reset()
        {
            //haveInput = false; //can never lose input after getting it once
            MapLoaded = false;

            PopulationLoaded = false;
            PopulationCorrectedForRoutes = false;
            GlobalGPWAvailable = false;
            LocalGPWLoaded = false;

            RouteCollectionLoaded = false;
            RouterDbLoaded = false;
            OsmFileValid = false;

            LcpLoaded = false;
            FuelModelsLoaded = false;
        }
    }
}

