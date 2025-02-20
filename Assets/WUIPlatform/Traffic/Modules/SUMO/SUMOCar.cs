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
        double xPos, yPos;
        double rotation;
        bool active;
        bool directControlled;
        Vector2 lastPos;
        Vector3 oldVisualPos;
        Vector3 newVisualPos;
        float oldRotation, newRotation;
        float speedRatio;

        public SUMOCar(uint carID, string sumoID, LIBSUMO.TraCIPosition initialPos, double angle, uint peopleInCar, EvacuationGoal goal) : base(carID, peopleInCar, goal)
        {
            this.carID = carID;
            this.sumoID = sumoID;
            xPos = initialPos.x;
            yPos = initialPos.y;
            active = true;
            directControlled = false;
            lastPos = new Vector2((float)initialPos.x, (float)initialPos.y);

            oldVisualPos = new Vector3((float)xPos, 0.0f, (float)yPos);
            newVisualPos = oldVisualPos;
            rotation = (float)angle;
        }

        public string GetSumoVehicleID()
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

        public void SetPosRot(double x, double y, double angle)
        {
            //position
            lastPos.X = (float)xPos;
            lastPos.Y = (float)yPos;

            xPos = x;
            yPos = y;

            oldVisualPos = newVisualPos;
            newVisualPos = new Vector3((float)xPos, 0.0f, (float)yPos);

            //rotation
            oldRotation = (float)rotation;
            rotation = angle;
            newRotation = (float)angle;
        }

        public void SetLocalPosRot(LIBSUMO.TraCIPosition pos, double angle)
        {
            SetPosRot(pos.x, pos.y, angle);
        }

        Vector4 _positionAndSpeed;
        public override Vector4 GetWorldPositionSpeedCarID(bool updateData)
        {
            return GetPositionSpeedCarID(updateData, WUIEngine.SIM.TrafficModule.GetOriginOffset());
        }
        public Vector4 GetPositionSpeedCarID(bool updateData, Vector2d originOffset)
        {
            if (updateData)
            {
                float speedRatio = (float)(LIBSUMO.Vehicle.getSpeed(sumoID) / LIBSUMO.Vehicle.getAllowedSpeed(sumoID));
                _positionAndSpeed = new Vector4((float)(xPos + originOffset.x), (float)(yPos + originOffset.y), speedRatio, carID);
            }

            return _positionAndSpeed;
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

