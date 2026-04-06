
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace PREACT.Traffic
{
    public class SumoEdge
    {
        public string Id;
        public string From;
        public string To;
        public List<(double x, double y)> Shape = new List<(double, double)>();
        public List<string> Lanes = new List<string>();
    }

    public class SumoLane
    {
        public string Id;
        public string EdgeId;
        public List<(double x, double y)> Shape = new List<(double, double)>();
    }

    public class SumoConfig
    {
        private SumoNetwork _network;

        public SumoNetwork Network { get => _network; }

        public SumoConfig(string filePath, bool readNetwork)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlElement root = doc.DocumentElement;
            if (root == null)
            {
                throw new Exception("Invalid .sumocg file");
            }


            // The net-file entry is inside <input> … <net-file value="..."/>
            XmlNode inputNode = root.SelectSingleNode("//input");
            if (inputNode == null)
            {
                throw new Exception("No <input> element found in .sumocfg");
            }

            XmlNode netFileNode = inputNode.SelectSingleNode("net-file");
            if (netFileNode == null)
            {
                throw new Exception("No <net-file> element found in .sumocfg");
            }

            XmlAttribute valueAttr = netFileNode.Attributes["value"];
            if (valueAttr == null)
            {
                throw new Exception("<net-file> exists but has no 'value' attribute");
            }

            string netEditFilePath = Path.Combine(Path.GetDirectoryName(filePath), valueAttr.Value);
            _network = new SumoNetwork(netEditFilePath, readNetwork);
        }
    }  

    public class SumoNetwork
    {
        public Math.Vector2d UTMOffset;
        public Dictionary<string, SumoEdge> Edges = new Dictionary<string, SumoEdge>();
        public Dictionary<string, SumoLane> Lanes = new Dictionary<string, SumoLane>();


        public SumoNetwork(string filePath, bool readNetwork)
        {
            XmlDocument doc;

            bool isGzip = filePath.ToLower().EndsWith("gz");
            if (isGzip)
            {
                //string xmlPath = filePath.Remove(filePath.Length - 3, 3);
                using var input = File.OpenRead(filePath);
                using var gzip = new GZipStream(input, CompressionMode.Decompress);
                doc = new XmlDocument();
                doc.Load(gzip);
            }
            else
            {
                doc = new XmlDocument();
                doc.Load(filePath);
            }            

            XmlElement root = doc.DocumentElement;
            if (root == null)
            {
                throw new Exception("Invalid net.xml file");
            }

            //save UTM offset
            XmlNode location = root.SelectSingleNode("//location");
            if (location == null)
            {
                throw new Exception("Could not find location in file.");
            }
            XmlAttribute netOffset = location.Attributes["netOffset"];
            if(netOffset != null)
            {
                string[] offset = netOffset.Value.Split(',');
                double.TryParse(offset[0], out double offsetX);
                double.TryParse(offset[1], out double offsetY);
                UTMOffset = new Math.Vector2d(offsetX, offsetY);  
            }

            if(!readNetwork)
            {
                return;
            }

            //do edges
            XmlNodeList edgeNodes = root.SelectNodes("//edge");
            foreach (XmlNode edgeNode in edgeNodes)
            {
                XmlAttribute idAttr = edgeNode.Attributes["id"];
                if (idAttr == null)
                {
                    continue;
                }

                string id = idAttr.Value;

                // Skip internal edges
                if (id.StartsWith(":"))
                {
                    continue;
                }

                SumoEdge edge = new SumoEdge();
                edge.Id = id;

                XmlAttribute fromAttr = edgeNode.Attributes["from"];
                XmlAttribute toAttr = edgeNode.Attributes["to"];

                edge.From = fromAttr != null ? fromAttr.Value : null;
                edge.To = toAttr != null ? toAttr.Value : null;

                XmlAttribute shapeAttr = edgeNode.Attributes["shape"];
                if (shapeAttr != null)
                {
                    edge.Shape = ParseShape(shapeAttr.Value);
                }

                Edges[id] = edge;

                // Parse lane children
                XmlNodeList laneNodes = edgeNode.SelectNodes("lane");
                foreach (XmlNode laneNode in laneNodes)
                {
                    XmlAttribute laneIdAttr = laneNode.Attributes["id"];
                    if (laneIdAttr == null)
                    {
                        continue;
                    }

                    string laneId = laneIdAttr.Value;

                    SumoLane lane = new SumoLane();
                    lane.Id = laneId;
                    lane.EdgeId = id;

                    XmlAttribute laneShapeAttr = laneNode.Attributes["shape"];
                    if (laneShapeAttr != null)
                    {
                        lane.Shape = ParseShape(laneShapeAttr.Value);
                    }

                    Lanes[laneId] = lane;
                    edge.Lanes.Add(laneId);
                }
            }
        }

        private static List<(double x, double y)> ParseShape(string shape)
        {
            List<(double x, double y)> list = new List<(double, double)>();

            if (string.IsNullOrEmpty(shape))
            {
                return list;
            }

            string[] pairs = shape.Split(' ');
            foreach (string p in pairs)
            {
                string[] xy = p.Split(',');
                if (xy.Length == 2)
                {
                    double x = double.Parse(xy[0], CultureInfo.InvariantCulture);
                    double y = double.Parse(xy[1], CultureInfo.InvariantCulture);
                    list.Add((x, y));
                }
            }

            return list;
        }
    }
}
