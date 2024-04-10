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
        }

        public void SetPosRot(LIBSUMO.TraCIPosition pos, float angle)
        {
            SetPosRot((float)pos.x, (float)pos.y, angle);
        }

        Vector4 positionAndSpeed;
        public Vector4 GetPositionAndSpeed(bool updateData)
        {
            if (updateData)
            {
                float speedRatio = (float)(LIBSUMO.Vehicle.getSpeed(sumoID) / LIBSUMO.Vehicle.getAllowedSpeed(sumoID));
                positionAndSpeed = new Vector4(xPos, yPos, speedRatio, 0f);
            }

            return positionAndSpeed;
        }

        public bool IsActive()
        {
            return active;
        }

        public override void Arrive()
        {
            active = false;
            goal.CarArrives(this, WUIEngine.SIM.CurrentTime, WUIEngine.INPUT.Simulation.DeltaTime);
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

