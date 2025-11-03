//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.IO;
using PREACT.Evacuation;
using PREACT.Utility.Math;

namespace PREACT.Scenario
{
    public class EvacuationData
    {
        Input _input;

        public EvacuationData(Input input)
        {
            _input = input;
        }

        private Vector2int _cellCount;
        public Vector2int CellCount
        {
            get
            {
                _cellCount.x = Mathf.CeilToInt((float)_input.Simulation.DomainSize.x / _input.Evacuation.PaintCellSize);
                _cellCount.y = Mathf.CeilToInt((float)_input.Simulation.DomainSize.y / _input.Evacuation.PaintCellSize);
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

        private ResponseCurve[] _responseCurves;
        public ResponseCurve[] ResponseCurves
        {
            get
            {                
                return _responseCurves;
            }
        }

        private EvacuationGroup[] _evacuationGroups;
        public EvacuationGroup[] EvacuationGroups { get=> _evacuationGroups; }

        public void LoadAll(Input input, string rootFolder)
        {
            Engine.MESSAGE(null, Engine.LogType.Log, "Loading Evacuation data...");
            
            if(input.Simulation.RunPedestrianModule)
            {
                //need goals and curves before can load groups
                //need to load groups before indices
                LoadEvacGroupIndices(input, rootFolder);
                LoadBlockGoalEvents(input, rootFolder);
            }                       
        }

        public bool LoadBlockGoalEvents(Input input, string rootFolder)
        {
            bool success;
            _blockGoalEvents = BlockDestinationEvent.LoadBlockGoalEvents(input, rootFolder, out success);

            return success;
        }

        public bool LoadEvacGroupIndices(Input input, string workingFolder)
        {
            bool success;
            //fills with first group if "failed", as in could not load but creates default
            string path = System.IO.Path.Combine(workingFolder, input.Evacuation.EvacuationGroupsMapFile);
            EvacuationGroup.LoadEvacGroupIndices(path, out success);

            return success;
        }

        public void UpdateEvacGroupIndices(int[] indices)
        {
            _evacGroupIndices = new int[CellCount.x * CellCount.y];
            for (int y = 0; y < CellCount.y; y++)
            {
                for (int x = 0; x < CellCount.x; x++)
                {
                    int index = x + y * CellCount.x;
                    if (indices != null)
                    {
                        _evacGroupIndices[index] = indices[index];
                    }
                    else
                    {
                        //default
                        _evacGroupIndices[index] = 0;
                    }
                }
            }
        }

        public int GetEvacGoalIndexFromName(string name, Simulation simulation)
        {
            int index = -1;
            for (int i = 0; i < simulation.Destinations.Count; i++)
            {
                if (name == simulation.Destinations[i].Name)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, " User has specified an evacuation goal named " + name + " but no such evacuation goal has been defined.");
            }

            return index;
        }

        public int GetResponseCurveIndexFromName(string name)
        {
            int index = -1;
            for (int i = 0; i < ResponseCurves.Length; i++)
            {
                if (name == ResponseCurves[i].name)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, " User has specified a response curve named " + name + " but no such response curve has been defined.");
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