using PREACT.Math;

namespace PREACT
{
    public class WeatherManager
    {
        IO.PREACTInput _input;

        public WeatherManager(IO.PREACTInput input) 
        {
            _input = input;
        }

        public void Step(float currentTime)
        {
            //TODO: update all relevant data
        }

        public float GetTemperature(Vector3d simulationPosition)
        {
            return 0f;
        }

        public float GetRelativeHumidity(Vector3d simulationPosition)
        {
            return 0f;
        }

        public float GetPrecipitation(Vector3d simulationPosition)
        {
            return 0f;
        }

        public void GetCanadianFBPMoisture(out double FFMC, out double BUI)
        {
            FFMC = 0;
            BUI = 0;
        }

        public double GetBUI(float time)
        {
            return 0;
        }

        public void GetWind(out double speed, out double direction)
        {
            speed = 0f;
            direction = 0f;
        }

        public void GetWind(Vector2d simulationPosition, out double speed, out double direction)
        {
            speed = 0f;
            direction = 0f;
        }

        public void GetWind(Vector3d simulationPosition, out double speed, out double direction)
        {
            speed = 0f;
            direction = 0f;
        }
    }
}
