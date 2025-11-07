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
            PREACTData data = new PREACTData(input); //Geo gets loaded automatically

            data.LoadAll(input, rootFolder, out success);            
            if (!success)
            {
                return null;
            }           

            success = true;
            return data;
        }
    }
}