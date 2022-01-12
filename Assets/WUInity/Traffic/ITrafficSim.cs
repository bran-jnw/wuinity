using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Traffic
{
    /// <summary>
    /// Interface for all traffic simulation modules
    /// </summary>
    public interface ITrafficSim
    {
        /// <summary>
        /// Advances the traffic simulation with the give delta time.
        /// </summary>
        /// <param name="deltaTime">Time step advancement.</param>
        /// <param name="currentTime">Time since entire  simulation started (fire tstarts at time 0).</param>
        public void AdvanceTrafficSimulation(float deltaTime, float currentTime);
    }
}

