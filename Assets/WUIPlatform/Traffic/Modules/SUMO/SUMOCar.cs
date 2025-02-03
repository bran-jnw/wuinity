//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;

namespace WUIPlatform.Traffic
{
    public class SUMOCar : TrafficModuleCar
    {
        string sumoID;        
        float xPos;
        float yPos;
        float rotation;
        bool active;
        bool directControlled;
        Vector2 lastPos;
        Vector3 oldVisualPos;
        Vector3 newVisualPos;
        float oldRotation, newRotation;
        private Vector2d offset; //Rik

        public SUMOCar(uint carID, string sumoID, LIBSUMO.TraCIPosition initialPos, double angle, uint peopleInCar, EvacuationGoal goal) : base(carID, peopleInCar, goal)
        {
            this.carID = carID;
            this.sumoID = sumoID;
            xPos = (float)initialPos.x;
            yPos = (float)initialPos.y;
            active = true;
            directControlled = false;
            lastPos = new Vector2((float)initialPos.x, (float)initialPos.y);

            oldVisualPos = new Vector3(xPos, 0.0f, yPos);
            newVisualPos = oldVisualPos;
            rotation = (float)angle;

            //Rik moved from SUMOMudule
            //need to use UTM projection in SUMO to match data, and since Mapbox is using Web mercator for calculations but UTM for tiles we need to do offset in UTM space
            Vector2d sumoUTM = new Vector2d(-WUIEngine.INPUT.Traffic.sumoInput.UTMoffset.x, -WUIEngine.INPUT.Traffic.sumoInput.UTMoffset.y);
            offset = sumoUTM - WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin;
        }

        public string GetVehicleID()
        {
            return sumoID;
        }

        /*public void UpdateVisualPosition(float lerpRatio)
        {
            if (model == null)
            {
                SpawnModel();
            }
            model.transform.position = Vector3.Lerp(oldVisualPos, newVisualPos, lerpRatio);
            model.transform.rotation = Quaternion.Slerp(oldRotation, newRotation, lerpRatio);
        }*/

        public void SetPosRot(float x, float y, float angle)
        {
            //position
            lastPos.X = xPos;
            lastPos.Y = yPos;

            xPos = x;
            yPos = y;

            oldVisualPos = newVisualPos;
            newVisualPos = new Vector3(xPos, 0.0f, yPos);

            //rotation
            oldRotation = rotation;
            rotation = angle;
            newRotation = angle;

            //Rik's
            if(lastPos.X != xPos || lastPos.Y!=yPos)
            {
                CarMoved();
            }
        }

        public void SetPosRot(LIBSUMO.TraCIPosition pos, float angle)
        {
            SetPosRot((float)pos.x, (float)pos.y, angle);
        }

        Vector4 positionAndSpeed;

        //Rik added 'override'
        public override Vector4 GetPositionAndSpeed(bool updateData)
        {
            if (updateData)
            {
                float speedRatio = (float)(LIBSUMO.Vehicle.getSpeed(sumoID) / LIBSUMO.Vehicle.getAllowedSpeed(sumoID));
                positionAndSpeed = new Vector4(xPos, yPos, speedRatio, 0f);
            }

            Vector4 pas = positionAndSpeed;
            pas.X += (float)offset.x;
            pas.Y += (float)offset.y;
            if (WUIEngine.INPUT.Simulation.ScaleToWebMercator)
            {
                pas.X *= (float)WUIEngine.RUNTIME_DATA.Simulation.UtmToMercatorScale.x;
                pas.Y *= (float)WUIEngine.RUNTIME_DATA.Simulation.UtmToMercatorScale.y;
            }

            return pas;
        }

        public bool IsActive()
        {
            return active;
        }

        public override void Arrive()
        {
            active = false;
            if(goal != null)
            {
                goal.CarArrives(this, WUIEngine.SIM.CurrentTime, WUIEngine.INPUT.Simulation.DeltaTime);
            }            
            //TODO: send message to WUI-nity
        }

        public void TakeDirectControl()
        {
            directControlled = true;
        }

        public void ReleaseDirectControl()
        {
            directControlled = false;
        }

        public bool IsDirectControlled()
        {
            return directControlled;
        }
    }
}

