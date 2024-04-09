namespace WUIEngine.Population
{
    public abstract class PopulationVisualizer
    {
        protected PopulationManager owner;

        public PopulationVisualizer(PopulationManager owner)
        {
            this.owner = owner;
        }

        public abstract void SetDataPlane(bool setActive);
        public abstract bool IsDataPlaneActive();
        public abstract object GetPopulationTexture();
        public abstract void CreateTexture();
        public abstract void CreateGPWTexture();
        public abstract bool ToggleLocalGPWVisibility();
    }
}


