using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WUIPlatform.Visualization
{
    public class WUIShowCommunicator
    {
        private int timesCarSent = 0;
        private float lastTime = 0f;
        private UdpClient udpClient;
        private TcpServer tcpServer;

        private Dictionary<uint, Vector2d> previouslySentPositions;
        private Queue<Traffic.TrafficModuleVehicle> _newVehiclesNotSent;
        private int numberOfBlockedCars = 0;
        private double origoLongitude;
        private double origoLatitude;
        private Vector2d offset;
        private int maxNumberOfCars;
        bool _readingData;

        public WUIShowCommunicator(string serverIP, int udpPort, int tcpPort = 0, double origoLongitude = -105.104505, double origoLatitude = 39.409924, int maxNumberOfCars = 10000)
        {
            WUIEngine.SIM.SetPause(true);

            udpClient = new UdpClient(serverIP, udpPort);
            Task.Run(() => TcpServer.StartServer(tcpPort == 0 ? udpPort + 1 : tcpPort, HandleTcpRequest)); 

            this.origoLongitude = origoLongitude;
            this.origoLatitude = origoLatitude;

            this.offset = WUIEngine.SIM.TrafficModule.GetOriginOffset();
            this.maxNumberOfCars = maxNumberOfCars;
            previouslySentPositions = new Dictionary<uint, Vector2d>();
            _newVehiclesNotSent = new Queue<Traffic.TrafficModuleVehicle>();
            
        }

        private byte[] GetTriggerBufferData()
        {
            byte[] result = null;
            float[,] data = WUIEngine.SIM.GetTriggerBufferData();

            if (data != null)
            {
                int xDim = data.GetLength(0);
                int yDim = data.GetLength(1);

                result = new byte[2 * sizeof(int) + xDim * yDim * sizeof(float)];

                byte[] bytes = BitConverter.GetBytes(xDim);
                Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
                bytes = BitConverter.GetBytes(yDim);
                Buffer.BlockCopy(bytes, 0, result, sizeof(int), bytes.Length);

                int offset = 2 * sizeof(int);
                for (int y = 0; y < yDim; ++y)
                {
                    for (int x = 0; x < xDim; x++)
                    {
                        bytes = BitConverter.GetBytes(data[x, y]);
                        Buffer.BlockCopy(bytes, 0, result, offset, bytes.Length);
                        offset += bytes.Length;
                    }
                }
            }

            return result;
        }

        private void GetUsageMapData()
        {

        }

        const int _maxNameLengths = 32;
        private byte[] GetNewVehiclesData()
        {
            _readingData = true;
            //vehicle Id, number of people, type of vehicle, destination name
            List<byte> data = new List<byte>();
            while(_newVehiclesNotSent.Count > 0)
            {
                Traffic.TrafficModuleVehicle vehicle = _newVehiclesNotSent.Dequeue();
                data.AddRange(BitConverter.GetBytes(vehicle.VehicleId));
                data.AddRange(BitConverter.GetBytes(vehicle.NumberOfPeople));
                data.AddRange(Encoding.UTF8.GetBytes(vehicle.VehicleClass.PadRight(_maxNameLengths)));
                data.AddRange(Encoding.UTF8.GetBytes(vehicle.Destination.Name.PadRight(_maxNameLengths)));
            }
            byte[] result = data.ToArray();
            _readingData = false;

            return result;
        }

        private byte[] GetDestinationsData()
        {            
            List<Evacuation.EvacuationDestination> destinations = WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals;
            //name, type, total cars, total people, total travel time, average travel time
            List<byte> data = new List<byte>();
            for(int i = 0; i < destinations.Count; ++i)
            {
                data.AddRange(Encoding.UTF8.GetBytes(destinations[i].Name.PadRight(_maxNameLengths)));
                data.AddRange(Encoding.UTF8.GetBytes(destinations[i].goalType.ToString().PadRight(_maxNameLengths)));
                data.AddRange(BitConverter.GetBytes(destinations[i].cars.Count));
                data.AddRange(BitConverter.GetBytes(destinations[i].currentPeople));
                data.AddRange(BitConverter.GetBytes(destinations[i].TotalTravelTime));
                data.AddRange(BitConverter.GetBytes(destinations[i].AverageTravelTime));
            }
            byte[] result = data.ToArray();

            return result;
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
                WUIEngine.SIM.SetPause(true);
            }
            else if (request == "START")
            {
                headerMessage = "STARTED";
                WUIEngine.SIM.SetPause(false);
            }
            else if (request == "TriggerBuffer")
            {
                headerMessage = "int, int, float[]";
                data = GetTriggerBufferData();
            }
            else if(request == "GetNewVehicles")
            {
                headerMessage = "NewVehicles";
                data = GetNewVehiclesData();
            }
            else if (request == "GetDestinations")
            {
                headerMessage = "Destinations";
                data = GetDestinationsData();
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
            if(_readingData)
            {
                return;
            }

            //this should only contain cars of interest/active, should not track only "moving" cars as that might not visualize queueing cars correctly
            Dictionary<uint, Traffic.TrafficModuleVehicle> vehicles = WUIEngine.SIM.TrafficModule.GetActiveVehicles();

            //we only have dummy data
            if(vehicles.Count == 0)
            {
                return;
            }

            if (currentTime > lastTime + WUIEngine.INPUT.WUIShow.WuiShowDeltaTime)
            {
                byte[] sendBytes = new byte[vehicles.Count * 16];
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
                uint vehicleCount = 0;
                foreach(Traffic.TrafficModuleVehicle vehicle in vehicles.Values)
                {
                    if (vehicleCount < maxNumberOfCars)
                    {
                        bool sendData = false;
                        Vector2d oldWorldPos;
                        bool foundVehicle = previouslySentPositions.TryGetValue(vehicle.VehicleId, out oldWorldPos);
                        //new vehicle not sent before
                        if (!foundVehicle)
                        {
                            previouslySentPositions.Add(vehicle.VehicleId, vehicle.WorldPosition);
                            _newVehiclesNotSent.Enqueue(vehicle);
                            sendData = true;
                        }
                        //only send if position has changed
                        else if (vehicle.WorldPosition != oldWorldPos)
                        {
                            previouslySentPositions[vehicle.VehicleId] = vehicle.WorldPosition;
                            sendData = true;
                        } 

                        if(sendData)
                        {
                            addBytes(BitConverter.GetBytes(vehicle.VehicleId));
                            //sending geodata, wgs84
                            LIBSUMO.TraCIPosition wgs84 = LIBSUMO.Simulation.convertGeo(vehicle.WorldPosition.x - offset.x, vehicle.WorldPosition.y - offset.y, false);
                            //Make the lon/lat coordinates relative to conserve precision during cast to float
                            //SUMO defines lon as x and lat as y
                            double longitude = wgs84.x - origoLongitude;
                            double latitude = wgs84.y - origoLatitude;
                            addBytes(BitConverter.GetBytes((float)longitude));
                            addBytes(BitConverter.GetBytes((float)latitude));
                            addBytes(BitConverter.GetBytes(vehicle.SpeedRatio));

                            ++vehicleCount;
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

