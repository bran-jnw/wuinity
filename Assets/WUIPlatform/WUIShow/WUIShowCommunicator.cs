using System;
using System.Numerics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEditor.Experimental.GraphView;

namespace WUIPlatform.Visualization
{
    public class WUIShowCommunicator
    {
        private int timesCarSent = 0;
        private float lastTime = 0f;
        private UdpClient udpClient;
        private TcpServer tcpServer;

        private Vector4[] previouslySentCars;
        private int numberOfBlockedCars = 0;
        private double origoLongitude;
        private double origoLatitude;
        private Vector2d offset;
        private int maxNumberOfCars;

        public WUIShowCommunicator(string serverIP, int udpPort, int tcpPort = 0, double origoLongitude = -105.104505, double origoLatitude = 39.409924, int maxNumberOfCars = 10000)
        {
            udpClient = new UdpClient(serverIP, udpPort);

            Task.Run(() => TcpServer.StartServer(tcpPort == 0 ? udpPort + 1 : tcpPort, HandleTcpRequest)); 

            this.origoLongitude = origoLongitude;
            this.origoLatitude = origoLatitude;

            this.offset = WUIEngine.SIM.TrafficModule.GetOriginOffset();
            this.maxNumberOfCars = maxNumberOfCars;
            previouslySentCars = new Vector4[maxNumberOfCars];
            
        }

        public byte[] HandleTcpRequest(string request)
        {
            request = request.TrimEnd('\0');
            string headerMessage = "UnknownRequest"; //must not exceed 24 characters
            byte[] data = new byte[0];
            if (request == "GetOrigin")
            {
                headerMessage = "Origin";
                data = new byte[16];
                Buffer.BlockCopy(BitConverter.GetBytes(origoLongitude), 0, data, 0, 8);
                Buffer.BlockCopy(BitConverter.GetBytes(origoLatitude), 0, data, 8, 8);
            }
            else if (request == "PAUSE")
            {
                headerMessage = "PAUSED";
                //do some command to pause the simulation
            }
            else if (request == "START")
            {
                headerMessage = "STARTED";
                //do some command to start the simulation
            }
            byte[] header = CreateTcpHeader(headerMessage, data.Length);

            byte[] combinedData = new byte[header.Length + data.Length];
            Array.Copy(header, 0, combinedData, 0, header.Length);
            Array.Copy(data, 0, combinedData, header.Length, data.Length);
            return combinedData;
        }

        public byte[] CreateTcpHeader(string message, long dataLength)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] paddedMessage = new byte[24];
            Array.Copy(messageBytes, 0, paddedMessage, 0, Math.Min(messageBytes.Length, 24));

            byte[] lengthBytes = BitConverter.GetBytes(dataLength);

            byte[] header = new byte[32];
            Array.Copy(paddedMessage, 0, header, 0, 24);
            Array.Copy(lengthBytes, 0, header, 24, 8);

            return header;
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
                    byte[] chunk = new byte[targetSize+4]; //add 4 bytes for the currentTime
                    Array.Copy(BitConverter.GetBytes((float)currentTime), 0, chunk, 0, 4); //add the currentTime first in the chunk
                    Array.Copy(sendBytes, x, chunk, 4, targetSize);
                    udpClient.Send(chunk, chunk.Length);
                }

                lastTime = currentTime;
                timesCarSent++;
            }
        }

        //TcpServerStuff
        public delegate byte[] HandleRequestDelegate(string receivedRequest);
        class TcpServer
        {
            public static async Task StartServer(int port, HandleRequestDelegate handleRequestMethod)
            {
                TcpListener server = new TcpListener(IPAddress.Any, port);

                server.Start();
                WUIEngine.LOG(WUIEngine.LogType.Log, "TCP Server started on port: " + port);

                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();
                    _ = HandleClientAsync(client, stream, handleRequestMethod);
                }
            }

            static async Task HandleClientAsync(TcpClient client, NetworkStream stream, HandleRequestDelegate handleRequestMethod)
            {
                try {
                    while(client.Connected)
                    {
                        byte[] buffer = new byte[24];//commands from wuishow cannot exceed 24 characters

                        int totalBytesRead = 0;

                        while (totalBytesRead < 24)
                        {
                            int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, 24 - totalBytesRead);
                            if (bytesRead == 0) break;
                            totalBytesRead += bytesRead;
                        }

                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                        WUIEngine.LOG(WUIEngine.LogType.Log, "TCP server received message: " + receivedMessage);
                        byte[] response = handleRequestMethod(receivedMessage);
                        await stream.WriteAsync(response, 0, response.Length);
                    }
                }
                catch (Exception e)
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, "Error handling wuishow TCP request: " + e.Message);
                }
                finally
                {
                    stream.Close();
                    client.Close();
                }
            }
        }
    }
}

