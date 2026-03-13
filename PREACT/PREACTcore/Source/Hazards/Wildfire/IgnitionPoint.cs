//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Math;

namespace PREACT.Wildfire
{
    [System.Serializable]
    public struct IgnitionPoint
    {
        float _ignitionTime;
        Vector2d _simulationPos;
        Vector2d _latLon;
        private bool _hasBeenIgnited;

        public Vector2d LatLon { get => _latLon; }
        public Vector2d SimulationPos { get => _simulationPos; }
        public float IgnitionTime { get => _ignitionTime; }    

        public bool HasBeenIgnited()
        {
            return _hasBeenIgnited;
        }

        public void Ignite()
        {
            _hasBeenIgnited = true;
        }

        /// <summary>
        /// Called to create ignition point from input.
        /// </summary>
        /// <param name="simulation"></param>
        /// <param name="input"></param>
        public IgnitionPoint(Simulation simulation, IgnitionPointInput input)
        {
            _latLon = input.LatLon;
            _ignitionTime = input.IgnitionTime;
            _simulationPos = simulation.Spatial.GetSimulationPosition(_latLon);

            _hasBeenIgnited = false;
        }

        /// <summary>
        /// Called when wanting to create igbition point while running simulation (real-time interaction).
        /// </summary>
        /// <param name="simulation"></param>
        /// <param name="simulationPos"></param>
        public IgnitionPoint(Simulation simulation, Vector2d simulationPos)
        {
            _latLon = simulation.Input.Simulation.Data.GetWGS84FromSimulationPosition(simulationPos);
            _simulationPos = simulationPos;
            _ignitionTime = simulation.Time.SimulationTime;

            _hasBeenIgnited = false;
        }
    }
}
