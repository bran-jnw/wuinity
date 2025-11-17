using PREACT.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PREACT
{
    internal class PREACTexecute :IExternalManager
    {
        Engine _engine;
        PREACTInput? _input;
        bool _isDone;

        public Engine Engine { get => _engine; }
        public bool IsDone { get => _isDone; }


        public PREACTexecute()
        {
            _engine = new Engine(this, true);
            _isDone = true;
        }

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Incorrect input parameters.");
                return;
            }
            else if(!File.Exists(args[0]))
            {
                Console.WriteLine("Specified file does not exist.");
            }

            bool success;
            _engine.LoadInputFromFile(args[0], out success);
            if (success)
            {
                EngineTask engineTask = null;
                //simple serial run
                if(args.Length == 1)
                {
                    engineTask = new EngineTask(EngineTask.ExecutionMode.Serial, 1, 0);
                }
                //parallel run and its following processes
                else if(args.Length >= 3)
                {
                    int numberOfRuns, batchSize, simulationIndexOffset;

                    int.TryParse(args[1], out numberOfRuns);
                    int.TryParse(args[2], out batchSize);
                    int.TryParse(args[3], out simulationIndexOffset);

                    if (numberOfRuns > 1)
                    {
                        engineTask = new EngineTask(EngineTask.ExecutionMode.ParallelProcess, numberOfRuns, simulationIndexOffset, batchSize);
                    }
                    else
                    {
                        engineTask = new EngineTask(EngineTask.ExecutionMode.Serial, numberOfRuns, simulationIndexOffset, batchSize);
                    }
                }                
                 
                if(engineTask != null)
                {
                    _isDone = false;
                    _engine.RunSimulations(engineTask);
                }                               
            }
            else
            {
                Console.WriteLine("Failed to read loaded file");
            }
        }

        public void NewLogMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void PauseSimulations()
        {
            throw new NotImplementedException();
        }

        public void SimulationStarted()
        {
            throw new NotImplementedException();
        }

        public void SimulationsFinished()
        {
            _isDone = true;
        }

        public void StopSimulations()
        {
            throw new NotImplementedException();
        }

        public void UpdateInput(PREACTInput input)
        {
            _input = input;
        }
    }
}
