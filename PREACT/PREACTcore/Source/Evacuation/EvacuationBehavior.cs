using System;
using System.Collections.Generic;
using PREACT.Input;
using PREACT.Utility.Math;
using PREACT.Evacuation;

namespace PREACT.Evacuation
{
    public class EvacuationBehavior
    {
        private string _name;
        private List<string> ResponseTriggers = new List<string>(8);
        private double _informationAwareness;
        private double _informationCompliance;

        private CDFSampler<ResponseCurve> _awakeResponseCurvesCDF;
        private CDFSampler<ResponseCurve> _asleepResponseCurvesCDF;
        private CDFSampler<ResponseCurve> _direResponseCurvesCDF;

        public EvacuationBehavior(Simulation simulation, EvacuationBehaviorInput input, Dictionary<string, ResponseCurve> allResponseCurves)
        {
            _name = input.Name;
            ResponseTriggers = input.ResponseTriggers;
            _informationAwareness = input.InformationAwareness;
            _informationCompliance = input.InformationCompliance;

            //awake
            ResponseCurve[] awakeResponseCurves = new ResponseCurve[input.AwakeResponseCurves.Count];
            double[] awakeResponseCurveProbabilities = new double[input.AwakeResponseCurves.Count];
            for (int i = 0; i < input.AwakeResponseCurves.Count; ++i)
            {
                ResponseCurve rC;
                if (allResponseCurves.TryGetValue(input.AwakeResponseCurves[i].Item1, out rC))
                {
                    awakeResponseCurves[i] = rC;
                    awakeResponseCurveProbabilities[i] = input.AwakeResponseCurves[i].Item2;
                }
                else
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "When creating evacuation groups a referenced response curve was not found.");
                }
            }
            _awakeResponseCurvesCDF = new CDFSampler<ResponseCurve>(awakeResponseCurveProbabilities, awakeResponseCurves);

            //asleep
            ResponseCurve[] asleepResponseCurves = new ResponseCurve[input.AsleepResponseCurves.Count];
            double[] asleepResponseCurveProbabilities = new double[input.AsleepResponseCurves.Count];
            for (int i = 0; i < input.AsleepResponseCurves.Count; ++i)
            {
                ResponseCurve rC;
                if (allResponseCurves.TryGetValue(input.AsleepResponseCurves[i].Item1, out rC))
                {
                    asleepResponseCurves[i] = rC;
                    asleepResponseCurveProbabilities[i] = input.AsleepResponseCurves[i].Item2;
                }
                else
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "When creating evacuation groups a referenced response curve was not found.");
                }
            }
            _asleepResponseCurvesCDF = new CDFSampler<ResponseCurve>(asleepResponseCurveProbabilities, asleepResponseCurves);

            //dire
            ResponseCurve[] direResponseCurves = new ResponseCurve[input.DireResponseCurves.Count];
            double[] direResponseCurveProbabilities = new double[input.DireResponseCurves.Count];
            for (int i = 0; i < input.DireResponseCurves.Count; ++i)
            {
                ResponseCurve rC;
                if (allResponseCurves.TryGetValue(input.DireResponseCurves[i].Item1, out rC))
                {
                    direResponseCurves[i] = rC;
                    direResponseCurveProbabilities[i] = input.DireResponseCurves[i].Item2;
                }
                else
                {
                    Engine.Message(simulation, Engine.LogType.SimulationError, "When creating evacuation groups a referenced response curve was not found.");
                }
            }
            _direResponseCurvesCDF = new CDFSampler<ResponseCurve>(direResponseCurveProbabilities, direResponseCurves);
        }

        public static Dictionary<string, EvacuationBehavior> CreateBehaviorsFromInput(Simulation simulation, Dictionary<string, EvacuationBehaviorInput> inputs, Dictionary<string, ResponseCurve> allResponseCurves)
        {
            Dictionary<string, EvacuationBehavior> behaviors = new Dictionary<string, EvacuationBehavior>(inputs.Count);
            foreach (KeyValuePair<string, EvacuationBehaviorInput> input in inputs)
            {
                behaviors.Add(input.Key, new EvacuationBehavior(simulation, input.Value, allResponseCurves));
            }
            return behaviors;
        }

        public bool GetAwareness(Pedestrian.MacroHousehold household)
        {
            bool becameAware = true;

            float f = Math.Random.valueF;
            if (f > _informationAwareness)
            {
                becameAware = false;
            }

            return becameAware;
        }

        public bool GetCompliance(Pedestrian.MacroHousehold household)
        {
            bool willComply = true;

            float f = Math.Random.valueF;
            if (f > _informationCompliance)
            {
                willComply = false;
            }

            return willComply;
        }

        public float GetProbableResponseTime(Pedestrian.MacroHousehold household)
        {
            float responseTime = float.MaxValue;            

            //determine which response curve to use based on the household state
            ResponseCurve responseCurve;
            if (household.CurrentAwarenessState == Pedestrian.MacroHousehold.AwarenessState.Asleep)
            {
                responseCurve = _asleepResponseCurvesCDF.DrawSample();
            }
            else if (household.CurrentAwarenessState == Pedestrian.MacroHousehold.AwarenessState.Distressed)
            {
                responseCurve = _direResponseCurvesCDF.DrawSample();
            }
            else //Awake
            {
                responseCurve = _awakeResponseCurvesCDF.DrawSample();
            }
            responseTime = responseCurve.DrawSample();

            return responseTime;
        }
           
    }
}
