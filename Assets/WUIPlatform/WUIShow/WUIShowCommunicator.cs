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
        //private TcpClient tcpClient;

        private Vector4[] previouslySentCars;
        private int numberOfBlockedCars = 0;
        private double origoLongitude;
        private double origoLatitude;
        private Vector2d offset;
        private int maxNumberOfCars;

        public WUIShowCommunicator(string serverIP, int serverPort, double origoLongitude = -105.104505, double origoLatitude = 39.409924, int maxNumberOfCars = 10000)
        {
            udpClient = new UdpClient(serverIP, serverPort);
            //tcpClient = new TcpClient(serverIP, serverPort);


            this.origoLongitude = origoLongitude;
            this.origoLatitude = origoLatitude;

            this.offset = WUIEngine.SIM.TrafficModule.GetOriginOffset();
            this.maxNumberOfCars = maxNumberOfCars;
            previouslySentCars = new Vector4[maxNumberOfCars];
            
        }

        public void SendData(float currentTime)
        {
            //this should only contain cars of interest/active, should not track only "moving" cars as that might not visualize queueing cars correctly
            Vector4[] cars = WUIEngine.SIM.TrafficModule.GetCarWorldPositionsStatesCarIDs();

            //we only have dummy data
            if(cars.Length == 1 && cars[0].W < 0)
            {
                return;
            }

            if (cars.Length > 0 && currentTime > lastTime + WUIEngine.INPUT.WUIShow.WuiShowDeltaTime)
            {
                byte[] sendBytes = new byte[cars.Length * 16];
                int i = 0;
                void addBytes(byte[] bytes)
                {
                    for (int b = 0; b < bytes.Length; b++)
                    {
                        sendBytes[i + b] = bytes[b];
                    }
                    i += bytes.Length;
                }

                numberOfBlockedCars = 0;
                for (int j = 0; j < cars.Length; j++)
                {
                    Vector4 carData = cars[j];
                    uint carId = (uint)carData.W;

                    if (carId < maxNumberOfCars && carData != previouslySentCars[carId]) // only send the cars that have changed
                    {
                        previouslySentCars[carId] = carData;
                        addBytes(BitConverter.GetBytes(carId));

                        //sending geodata, wgs84
                        if (true)
                        {
                            
                            LIBSUMO.TraCIPosition wgs84 = LIBSUMO.Simulation.convertGeo(carData.X - offset.x, carData.Y - offset.y, false);

                            //Make the lon/lat coordinates relative to conserve precision during cast to float
                            //SUMO defines lon as x and lat as y
                            double longitude = wgs84.x - origoLongitude;
                            double latitude = wgs84.y - origoLatitude;
                            addBytes(BitConverter.GetBytes((float)longitude));
                            addBytes(BitConverter.GetBytes((float)latitude));
                        }
                        else
                        {
                            addBytes(BitConverter.GetBytes(carData.X));
                            addBytes(BitConverter.GetBytes(carData.Y));
                        }

                        addBytes(BitConverter.GetBytes(carData.Z));

                        if (timesCarSent == 1)
                        {
                            //   Debug.Log("id: " + car.carID + ", position and speed: " + car.GetUnityPositionAndSpeed(false));
                        }
                    }
                    else
                    {
                        numberOfBlockedCars++;
                    }

                }
                int numberofCarsToSend = sendBytes.Length - (numberOfBlockedCars * 16);
                int maxChunkSize = 16 * 1024; //send max 1024 cars at a time
                for (int x = 0; x < numberofCarsToSend; x += maxChunkSize)
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

