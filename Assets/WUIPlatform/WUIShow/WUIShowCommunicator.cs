using System;
using System.Numerics;
using System.Net.Sockets;

namespace WUIPlatform.Visualization
{
    public class WUIShowCommunicator
    {
        private int timesCarSent = 0;
        private float lastTime = 0f;
        private UdpClient udpClient;

        public WUIShowCommunicator(string serverIP, int serverPort)
        {
            udpClient = new UdpClient(serverIP, serverPort);
        }

        public void SendData(float currentTime)
        {
            //this should only contain cars of interest/active, should not track only "moving" cars as that might not visualize queueing cars correctly
            Vector4[] cars = WUIEngine.SIM.TrafficModule.GetCarWorldPositionsStatesCarIDs();

            if (cars.Length > 0 && currentTime > lastTime + WUIEngine.INPUT.Visualization.wuiShowDeltaTime)
            {
                byte[] sendBytes = new byte[cars.Length * 16];
                int i = 0;
                void addBytes(byte[] bytes)
                {
                    for (int j = 0; j < bytes.Length; j++)
                    {
                        sendBytes[i + j] = bytes[j];
                    }
                    i += bytes.Length;
                }

                for (int j = 0; j < cars.Length; j++)
                {
                    Vector4 carData = cars[j];

                    //add id
                    /* byte[] carIdBytes = BitConverter.GetBytes(car.carId);
                    for(int j = 0; j<carIdBytes.Length;j++)
                    {
                        sendBytes[i + j] = carIdBytes[j];
                    }
                    i+=4;*/

                    //data contained in W is a uint cast to float, should be enough to identify with the amount of cars we have
                    addBytes(BitConverter.GetBytes(carData.W));
                    addBytes(BitConverter.GetBytes(carData.X));
                    addBytes(BitConverter.GetBytes(carData.Y));
                    addBytes(BitConverter.GetBytes(carData.Z));

                    if (timesCarSent == 1)
                    {
                        //   Debug.Log("id: " + car.carID + ", position and speed: " + car.GetUnityPositionAndSpeed(false));
                    }
                }

                int maxChunkSize = 16 * 1024; //send max 1024 cars at a time
                for (int x = 0; x < sendBytes.Length; x += maxChunkSize)
                {
                    int targetSize = 0;
                    if (sendBytes.Length < maxChunkSize + x)
                    {
                        targetSize = sendBytes.Length - x;
                    }
                    else
                    {
                        targetSize = maxChunkSize;
                    }
                    byte[] chunk = new byte[targetSize];
                    Array.Copy(sendBytes, x, chunk, 0, targetSize);
                    udpClient.Send(chunk, chunk.Length);
                }

                lastTime = currentTime;
                timesCarSent++;
            }
        }
    }
}

