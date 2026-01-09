//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Evacuation;
using PREACT.Math;
using System.Collections.Generic;
using System.IO;

namespace PREACT.IO
{
    public class EvacuationData
    {
        SimulationInput _simulationInput;
        EvacuationInput _evacuationInput;

        public EvacuationData(SimulationInput simulationInput, EvacuationInput evacuationInput)
        {
            _simulationInput = simulationInput;
            _evacuationInput = evacuationInput;
            //ResponseCurves = new List<ResponseCurve>();
            //EvacuationDestinationInputs = new List<EvacuationDestinationInput>();
            //EvacuationGroups = new List<EvacuationGroup>();
        }

        //TODO: fix this getter, want to get rid of
        private Vector2int _cellCount;
        public Vector2int CellCount
        {
            get
            {
                _cellCount.x = Mathf.CeilToInt((float)_simulationInput.DomainSize.x / _evacuationInput.PaintCellSize);
                _cellCount.y = Mathf.CeilToInt((float)_simulationInput.DomainSize.y / _evacuationInput.PaintCellSize);
                return _cellCount;
            }
        }

        BlockDestinationEvent[] _blockGoalEvents;
        public BlockDestinationEvent[] BlockGoalEvents
        {
            get
            {                
                return _blockGoalEvents;
            }
        }

        int[] _evacGroupIndices;
        public int[] EvacGroupIndices
        {
            get
            {
                return _evacGroupIndices;
            }
        }               

        public void LoadAll(string rootFolder, out bool success)
        {
            success = false;
            Engine.Message(null, Engine.LogType.Log, "Loading Evacuation data...");

            string filePath = Path.Combine(rootFolder, _evacuationInput.EvacuationGroupsMapFile);

            success = true;
        }   

        private void DefaultEvacGroupIndices()
        {
            Engine.Message(null, Engine.LogType.Warning, "Creating default group map.");
            _evacGroupIndices = new int[CellCount.x * CellCount.y];
            for (int y = 0; y < CellCount.y; y++)
            {
                for (int x = 0; x < CellCount.x; x++)
                {
                    int index = x + y * CellCount.x;
                    _evacGroupIndices[index] = 0;
                }
            }
        }      
    }
}