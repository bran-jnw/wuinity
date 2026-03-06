using PREACT.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire
{    
    public class NarrowLevelSet : WildfireModule
    {
        int _stepCount;
        NarrowBand _band;
        NarrowBandLevelSetSolver _solver;
        FastMarchingReinitializer _fmm;

        public int xDim { get; }
        public int yDim { get; }
        public double Dx { get; }
        public double Dy { get; }

        // Level set field
        public double[,] Phi { get; }
        public SpreadModel[,] Spread { get; }
     

        public NarrowLevelSet(Simulation simulation, LandscapeData landscape) : base(simulation)
        {
            xDim = simulation.Input.WildfireModule.Data.LandscapeData.GetCellCountX();
            yDim = simulation.Input.WildfireModule.Data.LandscapeData.GetCellCountY();
            Dx = simulation.Input.WildfireModule.Data.LandscapeData.RasterCellResolutionX;
            Dy = simulation.Input.WildfireModule.Data.LandscapeData.RasterCellResolutionY;

            Phi = new double[xDim, yDim];
            Spread = new SpreadModel[xDim, yDim];
            for (int y = 0; y < yDim; y++) 
            {
                for (int x = 0; x < xDim; x++)
                {
                    if(simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == IO.FireCellInput.SpreadRateModels.LookupROS)
                    {
                        Spread[x, y] = new SpreadModelLookupROS(landscape.GetCellData(x, y), simulation.Input.WildfireModule.Data.LookupROSTable);
                    }
                    else if (simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == IO.FireCellInput.SpreadRateModels.Behave)
                    {

                    }
                    else if (simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == IO.FireCellInput.SpreadRateModels.CanadianFBP)
                    {

                    }
                }
            }
            

            _band = new NarrowBand(xDim, yDim, 8);
            _solver = new NarrowBandLevelSetSolver(_band, 0.5);
            _fmm = new FastMarchingReinitializer(Dx, Dy);
        }

        public override void Step(float simulationTime, float deltaTime)
        {
            _solver.Step(this, deltaTime);

            // Periodic reinitialization (PDE or FMM)
            if (_stepCount % 20 == 0)
            {
                _fmm.Reinitialize(this);
            }            
            _stepCount++;
        }

        private void SetCircularIgnition(double x0, double y0, double radius)
        {

            for (int i = 0; i < xDim; i++)
            {
                double x = (i + 0.5) * Dx;
                for (int j = 0; j < yDim; j++)
                {
                    double y = (j + 0.5) * Dy;
                    double dist = Mathd.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0));
                    Phi[i, j] = dist - radius; // negative inside
                }
            }
        }

        public override void ConsumeIgnitedFireCells()
        {
            throw new NotImplementedException();
        }

        public override int GetActiveCellCount()
        {
            throw new NotImplementedException();
        }

        public override int GetCellCountX()
        {
            throw new NotImplementedException();
        }

        public override int GetCellCountY()
        {
            throw new NotImplementedException();
        }

        public override float GetCellSizeX()
        {
            throw new NotImplementedException();
        }

        public override float GetCellSizeY()
        {
            throw new NotImplementedException();
        }

        public override FireCellState GetFireCellState(Vector2d latLong)
        {
            throw new NotImplementedException();
        }

        public override float[] GetFireLineIntensityData()
        {
            throw new NotImplementedException();
        }

        public override float[] GetFuelModelNumberData()
        {
            throw new NotImplementedException();
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            throw new NotImplementedException();
        }

        public override float GetInternalDeltaTime()
        {
            throw new NotImplementedException();
        }

        public override float[,] GetMaxROS()
        {
            throw new NotImplementedException();
        }

        public override float[,] GetMaxROSAzimuth()
        {
            throw new NotImplementedException();
        }

        public override void GetOffsetAndSize(out Vector2d offset, out Vector2d size)
        {
            throw new NotImplementedException();
        }

        public override float[] GetSootProduction()
        {
            throw new NotImplementedException();
        }

        public override bool IsSimulationDone()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
