//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Numerics;
using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform.Fire
{
    public class CellVector : FireModule
    {
        private struct CellVectorCell
        {
            public enum CellState { Idle, Burning, Dead};
            public CellState state;
            public Vector2[] vertexPositions;
            public float initialFuelMoisture;
            public float maxSpreadRate, maxSpreadRateDirection, reactionIntensity, maxReactionIntensity, firelineIntensity, maxFirelineIntensity;
            public InitialFuelMoisture fuelMoisture;
            public LandscapeStruct lcp;
            float[] spreadDirectionAngles;
            Vector2[] spreadDirections;
            List<Vector2> ignitionVertices;

            public void InitiateVertices()
            {
                vertexPositions = new Vector2[4];
                spreadDirectionAngles = new float[4];
                spreadDirections = new Vector2[4];
                if(lcp.fuel_model > 13)
                {
                    state = CellState.Dead;
                }
                else
                {
                    state = CellState.Idle;
                }

                ignitionVertices = new List<Vector2>();
            }

            public float[] GetSpreadDirectionDegrees()
            {         
                float dx = vertexPositions[0].X - cellCorners[0].X;
                float dy = vertexPositions[0].Y - cellCorners[0].Y;
                spreadDirectionAngles[0] = Mathf.Atan(dx / dy) + Mathf.PI;

                dx = cellCorners[1].X - vertexPositions[1].X;
                dy = vertexPositions[1].Y - cellCorners[1].Y;
                spreadDirectionAngles[1] = Mathf.PI - Mathf.Atan(dx / dy);

                dx = cellCorners[2].X - vertexPositions[2].X;
                dy = cellCorners[2].Y - vertexPositions[2].Y;
                spreadDirectionAngles[2] = Mathf.Atan(dx / dy);

                dx = vertexPositions[3].X - cellCorners[3].X;
                dy = cellCorners[3].Y - vertexPositions[3].Y;
                spreadDirectionAngles[3] = 2f * Mathf.PI - Mathf.Atan(dx / dy);

                return spreadDirectionAngles;
            }

            public Vector2[] GetSpreadDirections()
            {
                spreadDirections[0] = Vector2.Normalize(cellCorners[0] - vertexPositions[0]);
                spreadDirections[1] = Vector2.Normalize(cellCorners[1] - vertexPositions[1]);
                spreadDirections[2] = Vector2.Normalize(cellCorners[2] - vertexPositions[2]);
                spreadDirections[3] = Vector2.Normalize(cellCorners[3] - vertexPositions[3]);

                return spreadDirections;
            }

            public void AddIgnition(Vector2 ignitionVertex)
            {
                ignitionVertices.Add(ignitionVertex);
            }

            public void HandleIgnitions()
            {
                if(ignitionVertices.Count == 0)
                {
                    return;
                }

                state = CellState.Burning;
            }

            public void UpdateCellSpreadRate(float spreadRate, float spreadDirection, float firelineIntensity, float reactionIntensity)
            {
                if (spreadRate > maxSpreadRate)
                {
                    maxSpreadRate = spreadRate;
                    maxSpreadRateDirection = spreadDirection;
                }

                this.firelineIntensity = firelineIntensity;
                if (firelineIntensity > maxFirelineIntensity)
                {
                    maxFirelineIntensity = firelineIntensity;
                }

                this.reactionIntensity = reactionIntensity;
                if (reactionIntensity > maxReactionIntensity)
                {
                    maxReactionIntensity = reactionIntensity;
                }
            }
        }

        int _cellsX, _cellsY;
        float _cellSizeX, _cellSizeY;
        CellVectorCell[] _cells;  
        LCPData _lcpData;
        WeatherInput _weather;
        WindInput _wind;
        InitialFuelMoistureList _initialFuelMoisture;
        FuelModelSet _fuelModelSet;
        Surface _surfaceFire;
        Crown _crownFire;
        IgnitionPoint[] _ignitionPoints;
        Vector2d offset;
        int bufferSize;

        static Vector2[] cellCorners;
        static Vector2 north = new Vector2(0f, 1f);


        TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
        BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
        WindHeightInputMode windHeightInputMode = WindHeightInputMode.DirectMidflame;
        BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
        BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
        BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
        BehaveUnits.SpeedUnits.SpeedUnitsEnum speedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
        WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;

        public CellVector()
        {
            _lcpData = WUIEngine.RUNTIME_DATA.Fire.LCPData;
            _weather = WUIEngine.RUNTIME_DATA.Fire.WeatherInput;
            _wind = WUIEngine.RUNTIME_DATA.Fire.WindInput;
            _initialFuelMoisture = WUIEngine.RUNTIME_DATA.Fire.InitialFuelMoistureData;
            _ignitionPoints = WUIEngine.RUNTIME_DATA.Fire.IgnitionPoints;

            _cellSizeX = (float)_lcpData.RasterCellResolutionX;
            _cellSizeY = (float)_lcpData.RasterCellResolutionY;
            _cellsX = _lcpData.Header.numeast;
            _cellsY = _lcpData.Header.numnorth;

            Vector2d lcpUTM = new Vector2d(_lcpData.Header.WestUtm, _lcpData.Header.SouthUtm);
            offset = lcpUTM - WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin;

            bufferSize = _cellsX * _cellsY;
            _cells = new CellVectorCell[bufferSize];

            for (int i = 0; i < bufferSize; i++)
            {
                CellVectorCell cell = _cells[i];
                int xIndex = i % _cellsX;
                int yIndex = i / _cellsX;
                LandscapeStruct lcp = _lcpData.GetCellData(xIndex, yIndex);
                cell.lcp = lcp;
                cell.InitiateVertices();
                _cells[i] = cell;
            }

            //south-west, south-east, nort-west, north-east
            cellCorners = new Vector2[4];
            cellCorners[0] = new Vector2(-_cellSizeX * 0.5f, -_cellSizeY * 0.5f);
            cellCorners[1] = new Vector2(-_cellSizeX * 0.5f, _cellSizeY * 0.5f);
            cellCorners[2] = new Vector2(-_cellSizeX * 0.5f, _cellSizeY * 0.5f);
            cellCorners[3] = new Vector2(_cellSizeX * 0.5f, _cellSizeY * 0.5f);

            UpdateInput();
        }

        void UpdateInput()
        {
            _fuelModelSet = new FuelModelSet();
            //set custom fuel models if present
            if (WUIEngine.DATA_STATUS.FuelModelsLoaded)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Adding custom fuel model specifications.");
                for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.FuelModelsData.Fuels.Count; i++)
                {
                    _fuelModelSet.setFuelModelRecord(WUIEngine.RUNTIME_DATA.Fire.FuelModelsData.Fuels[i]);
                }
            }
            _surfaceFire = new Surface(_fuelModelSet);
            _crownFire = new Crown(_fuelModelSet);
        }

        public override void Step(float currentTime, float deltaTime)
        {
            MoveVertices(deltaTime);
            CheckIgnitions();
            HandleIgnitions();
        }


        private void MoveVertices(float deltaTime)
        {
            for (int i = 0; i < bufferSize; i++)
            {
                UpdateMaxSpreadrate(i);

                double maxSpreadRate = _surfaceFire.getSpreadRate(speedUnits);
                double directionOfMaxSpread = _surfaceFire.getDirectionOfMaxSpread();   
                double eccentricity_ = _surfaceFire.getFireEccentricity();
                double reactionIntensity = _surfaceFire.getReactionIntensity(BehaveUnits.HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum.KilowattsPerSquareMeter);
                double firelineIntensity = _surfaceFire.getFirelineIntensity(BehaveUnits.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilowattsPerMeter);                
                double heatPerUnitArea = _surfaceFire.getHeatPerUnitArea() * 11.356538527057f; //comes in btu/sq.feet, now in kJ/m2

                _cells[i].UpdateCellSpreadRate((float)maxSpreadRate, (float)directionOfMaxSpread, (float)firelineIntensity, (float)reactionIntensity);

                //directions are -x, +x, -y, +y
                float[] spreadDirectionDegrees = _cells[i].GetSpreadDirectionDegrees();
                Vector2[] spreadDirections = _cells[i].GetSpreadDirections();
                for (int j = 0; j < spreadDirectionDegrees.Length; j++)
                {
                    double directionSpreadRate = maxSpreadRate;

                    double beta = abs(directionOfMaxSpread - spreadDirectionDegrees[j]);
                    // Calculate the fire spread rate in this azimuth
                    // if it deviates more than a tenth degree from the maximum azimuth
                    if (beta > 180.0)
                    {
                        beta = (360.0 - beta);
                    }
                    if (abs(beta) > 0.1)
                    {
                        double radians = beta * M_PI / 180.0;
                        directionSpreadRate = maxSpreadRate * (1.0 - eccentricity_) / (1.0 - eccentricity_ * cos(radians));                  
                    }
                    _cells[i].vertexPositions[j] += spreadDirections[j] * (float)directionSpreadRate;
                }
            }
        }

        void CheckIgnitions()
        {
            for (int i = 0; i < bufferSize; i++)
            {
                int x = i % _cellsX;
                int y = i / _cellsX;

                //left
                if (x > 0)
                {                    
                    if(_cells[i - 1].state == CellVectorCell.CellState.Idle)
                    {
                        if (_cells[i].vertexPositions[0].X < cellCorners[0].X)
                        {
                            _cells[i - 1].AddIgnition(_cells[i].vertexPositions[0]);
                        }
                        if (_cells[i].vertexPositions[2].X < cellCorners[2].X)
                        {
                            _cells[i - 1].AddIgnition(_cells[i].vertexPositions[2]);
                        }
                    }
                }

                //right
                if (x < _cellsX - 1)
                {                    
                    if (_cells[i + 1].state == CellVectorCell.CellState.Idle)
                    {
                        if (_cells[i].vertexPositions[1].X > cellCorners[1].X)
                        {
                            _cells[i + 1].AddIgnition(_cells[i].vertexPositions[1]);
                        }
                        if (_cells[i].vertexPositions[3].X > cellCorners[3].X)
                        {
                            _cells[i + 1].AddIgnition(_cells[i].vertexPositions[3]);
                        }
                    }
                }

                //down
                if (y > 0)
                {                    
                    if (_cells[i - _cellsX].state == CellVectorCell.CellState.Idle)
                    {
                        if (_cells[i].vertexPositions[0].Y < cellCorners[0].Y)
                        {
                            _cells[i - _cellsX].AddIgnition(_cells[i].vertexPositions[0]);
                        }
                        if (_cells[i].vertexPositions[1].Y < cellCorners[1].Y)
                        {
                            _cells[i - _cellsX].AddIgnition(_cells[i].vertexPositions[1]);
                        }
                    }
                }

                //up
                if (y < _cellsY - 1)
                {
                    if (_cells[i + _cellsX].state == CellVectorCell.CellState.Idle)
                    {
                        if (_cells[i].vertexPositions[2].Y > cellCorners[2].Y)
                        {
                            _cells[i + _cellsX].AddIgnition(_cells[i].vertexPositions[2]);
                        }
                        if (_cells[i].vertexPositions[3].Y > cellCorners[3].Y)
                        {
                            _cells[i + _cellsX].AddIgnition(_cells[i].vertexPositions[3]);
                        }
                    }
                }
            }
        }

        void HandleIgnitions()
        {
            for (int i = 0; i < bufferSize; i++)
            {
                _cells[i].HandleIgnitions();
            }
        }

        private void UpdateMaxSpreadrate(int index)
        {
            //update surface spread rate
            double moistureOneHour = _cells[index].fuelMoisture.OneHour;// 6.0;
            double moistureTenHour = _cells[index].fuelMoisture.TenHour; //7.0;
            double moistureHundredHour = _cells[index].fuelMoisture.HundredHour; //8.0;
            double moistureLiveHerbaceous = _cells[index].fuelMoisture.LiveHerbaceous; //60.0;
            double moistureLiveWoody = _cells[index].fuelMoisture.LiveWoody; //90.0;

            int fuelModelNumber = _cells[index].lcp.fuel_model;
            int slope = _cells[index].lcp.slope;
            int aspect = _cells[index].lcp.aspect;

            double windSpeed = GetCurrentWindData().speed;
            double windDirection = GetCurrentWindData().direction;
            // Wind adjustment factor parameters
            double canopyCover = _cells[index].lcp.canopy_cover;
            double canopyHeight = _cells[index].lcp.crown_canopy_height;
            double crownRatio = _cells[index].lcp.crown_bulk_density; //TODO: is this correct?

             //fireMesh.surfaceFire.getWindAndSpreadOrientationMode(); //WindAndSpreadOrientationMode.RelativeToUpslope;

            //feed new data
            _surfaceFire.updateSurfaceInputs(fuelModelNumber,
                moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody, moistureUnits,
                windSpeed, speedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode,
                slope, slopeUnits, aspect,
                canopyCover, coverUnits, canopyHeight, lengthUnits, crownRatio);

            //do new calc
            _surfaceFire.doSurfaceRunInDirectionOfMaxSpread();            
        }

        public override bool IsSimulationDone()
        {
            throw new System.NotImplementedException();
        }

        public override float GetInternalDeltaTime()
        {
            throw new System.NotImplementedException();
        }

        public override int[,] GetMaxROS()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCellCountX()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCellCountY()
        {
            throw new System.NotImplementedException();
        }

        public override float GetCellSizeX()
        {
            throw new System.NotImplementedException();
        }

        public override float GetCellSizeY()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetFireLineIntensityData()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetFuelModelNumberData()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetSootProduction()
        {
            throw new System.NotImplementedException();
        }

        public override int GetActiveCellCount()
        {
            throw new System.NotImplementedException();
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            throw new System.NotImplementedException();
        }

        public override void ConsumeIgnitedFireCells()
        {
            throw new System.NotImplementedException();
        }

        public override FireCellState GetFireCellState(Vector2d latLong)
        {
            throw new System.NotImplementedException();
        }

        public override WindData GetCurrentWindData()
        {
            throw new System.NotImplementedException();
        }
    }
}

