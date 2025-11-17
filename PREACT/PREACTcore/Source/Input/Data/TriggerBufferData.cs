using PREACT.IO;
using PREACT.Fire;
using System.IO;

namespace PREACT.IO
{
    public class TriggerBufferData
    {

        private InitialFuelMoistureLibrary _kPERILInitialFuelMoistureData;

        public InitialFuelMoistureLibrary kPERILInitialFuelMoistureData { get => _kPERILInitialFuelMoistureData; }


        public TriggerBufferData() 
        {
            
        }

        public void LoadAll(SimulationInput simulationInput, TriggerBufferInput triggerBufferInput, string rootFolder, out bool success)
        {
            success = false;

            if (triggerBufferInput.CalculateTriggerBuffer
                && triggerBufferInput.TriggerBuffer == TriggerBufferInput.TriggerBufferChoice.kPERIL
                && triggerBufferInput.kPERILInput.CalculateROSFromBehave)
            {

                    string filePath = Path.Combine(rootFolder, triggerBufferInput.kPERILInput.InitialFuelMoistureFile);
                    LoadPERILInitialFuelMoistureData(filePath, false, out success);
                    if (!success)
                    {
                        return;
                    }
            }

            success = true;
        }

        private void LoadPERILInitialFuelMoistureData(string filePath, bool updateInput, out bool success)
        {
            _kPERILInitialFuelMoistureData = InitialFuelMoistureLibrary.LoadInitialFuelMoistureDataFile(filePath, out success);
        }
    }
}