//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using WUIPlatform.IO;
using System.IO;

namespace WUIPlatform.Runtime
{
    public class EvacuationData
    {
        private Vector2int _cellCount;
        public Vector2int CellCount
        {
            get
            {
                WUIEngineInput input = WUIEngine.INPUT;
                _cellCount.x = Mathf.CeilToInt((float)input.Simulation.DomainSize.x / input.Evacuation.PaintCellSize);
                _cellCount.y = Mathf.CeilToInt((float)input.Simulation.DomainSize.y / input.Evacuation.PaintCellSize);
                return _cellCount;
            }
        }

        BlockGoalEvent[] _blockGoalEvents;
        public BlockGoalEvent[] BlockGoalEvents
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

        private List<EvacuationGoal> _evacuationGoals;
        public List<EvacuationGoal> EvacuationGoals
        {
            get
            {
                return _evacuationGoals;
            }
        }

        private EvacGroup[] _evacuationGroups;
        public EvacGroup[] EvacuationGroups
        {
            get
            {
                return _evacuationGroups;
            }
        }

        public void LoadAll()
        {            
            //need goals and curves before can load groups    
            LoadResponseCurves();
            LoadEvacuationGoals();
            LoadEvacuationGroups();
            //need to load groups before indices
            LoadEvacGroupIndices();
            LoadBlockGoalEvents();            
        }

        public bool LoadBlockGoalEvents()
        {
            bool success;
            _blockGoalEvents = BlockGoalEvent.LoadBlockGoalEvents(out success);

            return success;
        }

        public bool LoadEvacGroupIndices()
        {
            bool success;
            //fills with first group if "failed", as in could not load but creates default
            EvacGroup.LoadEvacGroupIndices(out success);

            return success;
        }

        public bool LoadResponseCurves()
        {
            bool success;
            _responseCurves = ResponseCurve.LoadResponseCurves(out success);

            return success;
        }

        public bool LoadEvacuationGoals()
        {
            bool success;
            _evacuationGoals = EvacuationGoal.LoadEvacuationGoalFiles(out success);

            return success;
        }        

        public void AddEvacuationGoal(EvacuationGoal newGoal)
        {
            if(_evacuationGoals == null)
            {
                _evacuationGoals = new List<EvacuationGoal>();
            }
            _evacuationGoals.Add(newGoal);
        }

        public void RemoveEvacuationGoal(EvacuationGoal goal)
        {
            if (_evacuationGoals != null)
            {
                _evacuationGoals.Remove(goal);
            }
        }

        public void ClearAndAddEvacuationGoals(EvacuationGoal[] evacGoals)
        {
            if (_evacuationGoals == null)
            {
                _evacuationGoals = new List<EvacuationGoal>();
            }
            else
            {
                _evacuationGoals.Clear();
            }

            for (int i = 0; i < evacGoals.Length; i++)
            {
                _evacuationGoals.Add(evacGoals[i]);
            }            
        }

        public bool LoadEvacuationGroups()
        {
            bool success;
            _evacuationGroups = EvacGroup.LoadEvacGroupFiles(out success);

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

        public int GetEvacGoalIndexFromName(string name)
        {
            int index = -1;
            for (int i = 0; i < EvacuationGoals.Count; i++)
            {
                if (name == EvacuationGoals[i].name)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, " User has specified an evacuation goal named " + name + " but no such evacuation goal has been defined.");
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
                WUIEngine.LOG(WUIEngine.LogType.Warning, " User has specified a response curve named " + name + " but no such response curve has been defined.");
            }

            return index;
        }

        public EvacGroup GetEvacGroup(int cellIndex)
        {
            WUIEngineInput input = WUIEngine.INPUT;
            if (WUIEngine.RUNTIME_DATA.Evacuation.EvacGroupIndices.Length < WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y)
            {
                return null;
            }

            cellIndex = WUIEngine.RUNTIME_DATA.Evacuation.EvacGroupIndices[cellIndex];

            return EvacuationGroups[cellIndex];
        }

        public EvacGroup GetEvacGroup(int x, int y)
        {
            WUIEngineInput input = WUIEngine.INPUT;
            if (WUIEngine.RUNTIME_DATA.Evacuation.EvacGroupIndices.Length < WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y)
            {
                return null;
            }

            int index = x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x;
            index = WUIEngine.RUNTIME_DATA.Evacuation.EvacGroupIndices[index];
            return EvacuationGroups[index];
        }

        public uint GetTotalEvacuated()
        {
            uint result = 0;
            for (int i = 0; i < EvacuationGoals.Count; i++)
            {
                result += EvacuationGoals[i].currentPeople;
            }

            return result;
        }
    }
}