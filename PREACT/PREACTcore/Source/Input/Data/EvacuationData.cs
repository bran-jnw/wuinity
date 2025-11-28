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
        public List<ResponseCurve> ResponseCurves;
        public List<EvacuationDestinationInput> EvacuationDestinationInputs;        
        public List<EvacuationGroup> EvacuationGroups;

        public EvacuationData(SimulationInput simulationInput, EvacuationInput evacuationInput)
        {
            _simulationInput = simulationInput;
            _evacuationInput = evacuationInput;
            ResponseCurves = new List<ResponseCurve>();
            EvacuationDestinationInputs = new List<EvacuationDestinationInput>();
            EvacuationGroups = new List<EvacuationGroup>();
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
            
            //need goals and curves before can load groups
            //need to load groups before indices
            LoadResponseCurves(rootFolder, _evacuationInput.ResponseCurveFiles, out success);
            LoadEvacuationDestinations(rootFolder, _evacuationInput.EvacuationDestinationFiles, out success);
            LoadEvacGroupFiles(rootFolder, this, _evacuationInput.EvacuationGroupFiles, out success);
            string filePath = Path.Combine(rootFolder, _evacuationInput.EvacuationGroupsMapFile);
            LoadEvacGroupIndices(filePath, out success);

            success = true;
        }

        public void LoadResponseCurves(string rootFolder, List<string> responseCurveFiles, out bool success)
        {
            ResponseCurves = ResponseCurve.LoadResponseCurves(rootFolder, responseCurveFiles, out success);
        }

        public void LoadEvacuationDestinations(string rootFolder, List<string> evacuationGoalFiles, out bool success)
        {
            EvacuationDestinationInputs = EvacuationDestinationInput.LoadEvacuationDestinationFiles(rootFolder, evacuationGoalFiles, out success);
        }

        public void LoadEvacGroupFiles(string rootFolder, EvacuationData evacuationData, List<string> evacuationGroupFiles, out bool success)
        {
            EvacuationGroups = EvacuationGroup.LoadEvacGroupFiles(rootFolder, this, evacuationGroupFiles, out success);
        }        

        public void LoadEvacGroupIndices(string filePath, out bool success)
        {
            //fills with first group if "failed", as in could not load but creates default            
            EvacuationGroup.LoadEvacGroupIndices(filePath, this, out _evacGroupIndices, out success);
            if(!success)
            {       
                //TODO: this is bad, fix in better way
                DefaultEvacGroupIndices();
            }            
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

        public int GetEvacGoalIndexFromName(string name)
        {
            int index = -1;
            for (int i = 0; i < EvacuationDestinationInputs.Count; i++)
            {
                if (name == EvacuationDestinationInputs[i].Name)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                Engine.Message(null, Engine.LogType.Warning, " User has specified an evacuation goal named " + name + " but no such evacuation goal has been defined.");
            }

            return index;
        }

        public int GetResponseCurveIndexFromName(string name)
        {
            int index = -1;
            for (int i = 0; i < ResponseCurves.Count; i++)
            {
                if (name == ResponseCurves[i].name)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                Engine.Message(null, Engine.LogType.Warning, " User has specified a response curve named " + name + " but no such response curve has been defined.");
            }

            return index;
        }

        public EvacuationGroup GetEvacGroup(int cellIndex)
        {
            if (EvacGroupIndices.Length < CellCount.x * CellCount.y)
            {
                return null;
            }

            cellIndex = EvacGroupIndices[cellIndex];

            return EvacuationGroups[cellIndex];
        }

        public EvacuationGroup GetEvacGroup(int x, int y)
        {
            if (EvacGroupIndices.Length < CellCount.x * CellCount.y)
            {
                return null;
            }

            int index = x + y * CellCount.x;
            index = EvacGroupIndices[index];

            return EvacuationGroups[index];
        }        
    }
}