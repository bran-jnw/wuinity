using PREACT.IO;
using PREACT.Runtime;

namespace PREACT
{
    public class PREACTScenario
    {
        private PREACTInput _input;
        private PREACTData _data;

        public PREACTInput Input { get => _input; }
        public PREACTData Data { get => _data; }

        public PREACTScenario(PREACTInput input, PREACTData data)
        {
            _input = input;
            _data = data;
        }

        public static PREACTScenario LoadFromDisk(string filePath, out bool success)
        {
            success = false;
            PREACTScenario scenario = null;

            PREACTInput input = PREACTInput.LoadFromDisk(filePath, out success);
            if (success)
            {
                string rootFolder = System.IO.Path.GetDirectoryName(filePath);
                scenario = LoadFromInput(input, rootFolder, out success);
            }

            return scenario;
        }

        public static PREACTScenario LoadFromInput(PREACTInput input, string rootFolder, out bool success)
        {
            success = false;
            PREACTScenario scenario = null;

            PREACTData data = CreateDataFromInput(input, rootFolder, out success);
            if (success)
            {
                scenario = new PREACTScenario(input, data);
            }

            return scenario;
        }

        /// <summary>
        /// Reads all the data from referenced files in input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rootFolder"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        private static PREACTData CreateDataFromInput(PREACTInput input, string rootFolder, out bool success)
        {
            success = false;
            Engine.MESSAGE(null, Engine.LogType.Log, "Loading referenced data from input file...");
            PREACTData data = new PREACTData(input); //Geo gets loaded automatically            
            //need to load evacuation goals before routing as they rely on evacuation goals
            data.Population.LoadAll(input, rootFolder, out success);
            data.Evacuation.LoadAll(input, rootFolder, out success);
            //RUNTIME_DATA.Routing.LoadAll(); //this does nothing right now                
            data.Traffic.LoadAll(input, rootFolder);
            data.Fire.LoadAll(input, rootFolder, data.Geo.UTMOrigin, out success);
            if (!success)
            {
                return null;
            }
            data.Smoke.LoadAll(input, rootFolder); //does nothing right now

            success = true;
            return data;
        }
    }
}