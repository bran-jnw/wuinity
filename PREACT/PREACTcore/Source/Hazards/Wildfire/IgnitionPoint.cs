//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;

namespace PREACT.Wildfire
{
    [System.Serializable]
    public struct IgnitionPoint
    {
        public Vector2d LatLon;
        public float IgnitionTime;

        private int x;
        private int y;

        private bool _hasBeenIgnited;

        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public bool HasBeenIgnited()
        {
            return _hasBeenIgnited;
        }

        public void MarkAsIgnited()
        {
            _hasBeenIgnited = true;
        }

        /// <summary>
        /// Only used for testing, creates ignition point directly on mesh
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public IgnitionPoint(int x, int y, float ignitionTime)
        {
            this.x = x;
            this.y = y;
            this.IgnitionTime = ignitionTime;

            LatLon = Vector2d.zero;

            _hasBeenIgnited = false;
        }

        public IgnitionPoint(IgnitionPointInput input)
        {
            LatLon = input.LatLon;
            x = -1;
            y = -1;
            IgnitionTime = input.IgnitionTime;

            _hasBeenIgnited = false;
        }

        public IgnitionPoint(Vector2d latLong, float ignitionTime)
        {
            this.LatLon = latLong;
            x = -1;
            y = -1;
            this.IgnitionTime = ignitionTime;

            _hasBeenIgnited = false;
        }

        /// <summary>
        /// Used when creating something dynamically during runtime.
        /// </summary>
        /// <param name="latLong"></param>
        /// <param name="mesh"></param>
        public IgnitionPoint(Simulation simulation, Vector2d latLong, FireMesh mesh, float ignitionTime)
        {
            this.LatLon = latLong;

            Vector2d pos = simulation.GetSimulationPosition(latLong);

            x = (int)(pos.x / mesh._cellSize.x);
            y = (int)(pos.y / mesh._cellSize.y);

            this.IgnitionTime = ignitionTime;

            _hasBeenIgnited = false;
        }

        /// <summary>
        /// Called when starting fire since we only specify lat/long in input file
        /// </summary>
        /// <param name="mesh"></param>
        public void CalculateMeshIndex(Simulation simulation, FireMesh mesh)
        {
            if (x < 0 && y < 0)
            {
                Vector2d pos = simulation.GetSimulationPosition(LatLon);

                x = (int)(pos.x / mesh._cellSize.x);
                y = (int)(pos.y / mesh._cellSize.y);
            }
        }

        public bool IsInsideFire(Vector2int cells)
        {
            if (x >= 0 && x < cells.x && y >= 0 && y < cells.y)
            {
                return true;
            }
            return false;
        }
    }
}
