//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using PREACT.Evacuation;
using LIBSUMO = Eclipse.Sumo.Libsumo;
using PREACT.Utility.Math;

namespace PREACT.Traffic
{
    public class SUMOVehicle : TrafficModuleVehicle
    {
        string _sumoId;      
        double rotation;
        bool active;
        bool directControlled;
        Vector2 lastPos;
        Vector3 oldVisualPos;
        Vector3 newVisualPos;
        float oldRotation, newRotation;

        public SUMOVehicle(uint carID, string sumoID, LIBSUMO.TraCIPosition initialPos, double angle, uint peopleInCar, EvacuationDestination goal) : base(carID, peopleInCar, goal)
        {
            _vehicleId = carID;
            _sumoId = sumoID;
            _worldPosition = new Vector2d(initialPos.x, initialPos.y);  
            active = true;
            directControlled = false;
            lastPos = new Vector2((float)initialPos.x, (float)initialPos.y);

            oldVisualPos = new Vector3((float)_worldPosition.x, 0.0f, (float)_worldPosition.y);
            newVisualPos = oldVisualPos;
            rotation = (float)angle;
        }

        public string GetSumoVehicleID()
        {
            return _sumoId;
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

        public void SetWorldPosItionAndRotation(LIBSUMO.TraCIPosition localPos, double angle, Vector2d offset)
        {
            //position
            lastPos.X = (float)_worldPosition.x;
            lastPos.Y = (float)_worldPosition.y;

            _worldPosition = new Vector2d(localPos.x, localPos.y) + offset;

            oldVisualPos = newVisualPos;
            newVisualPos = new Vector3((float)_worldPosition.x, 0.0f, (float)_worldPosition.y);

            //rotation
            oldRotation = (float)rotation;
            rotation = angle;
            newRotation = (float)angle;

            _speedRatio = (float)(LIBSUMO.Vehicle.getSpeed(_sumoId) / LIBSUMO.Vehicle.getAllowedSpeed(_sumoId)); //include? LIBSUMO.Vehicle.getSpeedFactor(_sumoId) *
        }

        public bool IsActive()
        {
            return active;
        }

        public override void Arrive(float deltaTime, float currentTime)
        {
            active = false;
            if(_destination != null)
            {
                _destination.CarArrives(this, deltaTime, currentTime);
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

