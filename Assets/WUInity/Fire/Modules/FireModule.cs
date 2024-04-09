using System.Collections.Generic;

namespace WUIEngine.Fire
{
    public abstract class FireModule : SimulationModule
    {
        /// <summary>
        /// The fire module might be able to take longer time steps compared to other modules, so this information is neede dif only doing fire simulation.
        /// </summary>
        /// <returns></returns>
        public abstract float GetInternalDeltaTime();

        /// <summary>
        /// Get ROS in the eight cardinal directions
        /// </summary>
        public abstract int[,] GetMaxROS();

        public abstract int GetCellCountX();
        public abstract int GetCellCountY();
        public abstract float GetCellSizeX();
        public abstract float GetCellSizeY();
        public abstract float[] GetFireLineIntensityData();
        public abstract float[] GetFuelModelNumberData();
        public abstract float[] GetSootProduction();
        public abstract int GetActiveCellCount();
        public abstract List<Vector2int> GetIgnitedFireCells();
        public abstract void ConsumeIgnitedFireCells();

        /// <summary>
        /// Returns state of cell on mesh based on lat/long. Returns dead if outside of mesh.
        /// </summary>
        /// <param name="latLong"></param>
        /// <returns></returns>
        public abstract FireCellState GetFireCellState(Vector2d latLong);
        public abstract WindData GetCurrentWindData();
    }
}

