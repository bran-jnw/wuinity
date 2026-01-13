//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Collections.Generic;

namespace PREACT.IO
{
    [System.Serializable]
    public class SimulationOutput
    {
        private Simulation _simulation;

        private float _totalAverageEvacTime;
        public float TotalAverageEvacTime { get => _totalAverageEvacTime; }
        private EvacOutput _evac;
        public EvacOutput Evac { get => _evac; }
        private List<float> _averageEvacTimes;

        Dictionary<int, float[,]> _triggerBuffers = new Dictionary<int, float[,]>();


        public SimulationOutput(Simulation simulation)
        {
            _evac = new EvacOutput();
            _averageEvacTimes = new List<float>();

            _simulation = simulation;
        }

        List<float> _emptyArrivalData = new List<float>();
        public List<float> GetTrafficArrivalData()
        {
            if (_simulation.TrafficModule != null)
            {
                return _simulation.TrafficModule.GetArrivalData();
            }
            else
            {
                return _emptyArrivalData;
            }
        }

        public void AddEvacTime(float totalEvacTime)
        {
            _averageEvacTimes.Add(totalEvacTime);
            for (int i = 0; i < _averageEvacTimes.Count; i++)
            {
                _totalAverageEvacTime += _averageEvacTimes[i];
            }
            _totalAverageEvacTime /= _averageEvacTimes.Count;
        }

        public void AddTriggerBufferOutput(float[,] triggerBufferOutput, int simulationIndex)
        {
            _triggerBuffers.Add(simulationIndex, triggerBufferOutput);
        }

        public float[,] GetTriggerBufferOutput(int simulationIndex, out bool success)
        {
            float[,] buffer = null;
            success = _triggerBuffers.TryGetValue(simulationIndex, out buffer);
            return buffer;
        }

        public void CalculateAverageTriggerBuffer()
        {
            float bufferCountInverse = 1f / _triggerBuffers.Count;
            float[,] perilOutput = null;         
            foreach (KeyValuePair<int, float[,]> buffer in _triggerBuffers)
            {
                int xDim = buffer.Value.GetLength(0);
                int yDim = buffer.Value.GetLength(1);

                if(perilOutput == null)
                {
                    perilOutput = new float[xDim, yDim];
                }

                for(int y = 0; y < yDim; ++y)
                {
                    for (int x = 0; x < xDim; ++x)
                    {
                        perilOutput[x, y] += buffer.Value[x, y] * bufferCountInverse;
                    }
                }
            }
        }

        /// <summary>
        /// Path includes filename
        /// </summary>
        /// <param name="log"></param>
        /// <param name="path"></param>
        public static void SaveLogToDisk(List<string> log, string path)
        {            
            File.WriteAllLines(path, log);
        }
    }

    [System.Serializable]
    public class EvacOutput
    {        
        public int actualTotalEvacuees;    
        public int stayingPeople;

        [System.NonSerialized] public int[] rawPopulation;
    }
}

