using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/*
    Copyright (c) 2017 Sloan Kelly

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

/// <summary>
/// An OSM object that describes an arrangement of OsmNodes into a shape or road.
/// </summary>
class OsmWay : BaseOsm
{
    public enum HighwayType
    {        
        //Base
        motorway,
        trunk,
        primary,
        secondary,
        tertiary,
        unclassified,
        residential,
        service,
        //Links
        motorway_link,
        trunk_link,
        primary_link,
        secondary_link,
        tertiary_link,
        //Special
        living_street,
        pedestrian,
        track,
        bus_guideway,
        escape,
        raceway,
        road,
        //Paths
        footway,
        bridleway,
        steps,
        path,
        //Cycleway
        cycleway,
        //Lifecycle
        proposed,
        construction,
        //placeholder
        undefined
    }
    /// <summary>
    /// Way ID.
    /// </summary>
    public HighwayType highway { get; private set; }


    /// <summary>
    /// Way ID.
    /// </summary>
    public ulong ID { get; private set; }

    /// <summary>
    /// True if visible.
    /// </summary>
    public bool Visible { get; private set; }

    /// <summary>
    /// List of node IDs.
    /// </summary>
    public List<ulong> NodeIDs { get; private set; }

    /// <summary>
    /// True if the way is a boundary.
    /// </summary>
    public bool IsBoundary { get; private set; }

    /// <summary>
    /// True if the way is a building.
    /// </summary>
    public bool IsBuilding { get; private set; }

    /// <summary>
    /// True if the way is a road.
    /// </summary>
    public bool IsRoad { get; private set; }

    /// <summary>
    /// Height of the structure.
    /// </summary>
    public float Height { get; private set; }

    /// <summary>
    /// Height of the structure.
    /// </summary>
    public float Levels { get; private set; }

    /// <summary>
    /// The name of the object.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The number of lanes on the road. Default is 1 for contra-flow
    /// </summary>
    public int Lanes { get; private set; }

    /// <summary>
    /// Lane width.
    /// </summary>
    public float LaneWidth { get; private set; }

    /// <summary>
    /// Layer of the structure.
    /// </summary>
    public int Layer { get; private set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="node"></param>
    public OsmWay(XmlNode node)
    {
        NodeIDs = new List<ulong>();
        Height = 3.0f; // Default height for structures is 1 story (approx. 3m)
        Lanes = 1;      // Number of lanes either side of the divide 
        Name = "";
        Layer = 0;
        Levels = -1;
        highway = HighwayType.undefined;

        // Get the data from the attributes
        ID = GetAttribute<ulong>("id", node.Attributes);
        Visible = GetAttribute<bool>("visible", node.Attributes);

        // Get the nodes
        XmlNodeList nds = node.SelectNodes("nd");
        foreach(XmlNode n in nds)
        {
            ulong refNo = GetAttribute<ulong>("ref", n.Attributes);
            NodeIDs.Add(refNo);
        }

        if (NodeIDs.Count > 1)
        {
            IsBoundary = NodeIDs[0] == NodeIDs[NodeIDs.Count - 1];
        }

        // Read the tags
        XmlNodeList tags = node.SelectNodes("tag");
        foreach (XmlNode t in tags)
        {
            string key = GetAttribute<string>("k", t.Attributes);
            if (key == "building:levels")
            {
                Levels = GetAttribute<float>("v", t.Attributes);
            }
            else if (key == "height")
            {
                Height = 0.3048f * GetAttribute<float>("v", t.Attributes);
            }
            else if (key == "building")
            {
                IsBuilding = true; // GetAttribute<string>("v", t.Attributes) == "yes";
            }
            else if (key == "highway")
            {
                IsRoad = true;
                string highwayType = GetAttribute<string>("v", t.Attributes);
                SetHighwayType(highwayType);
            }
            else if (key=="lanes")
            {
                Lanes = GetAttribute<int>("v", t.Attributes);
            }
            else if (key == "name")
            {
                Name = GetAttribute<string>("v", t.Attributes);
            }
        }

        //make sense of some data
        Height = Mathf.Max(Levels * 3.0f, Height);
        Name = ID + "_" + Name;
    }

    //reference https://wiki.openstreetmap.org/wiki/Key:highway
    /// <summary>
    /// Sets highway type and width based on type, totally made up the actual widths. Find reference?
    /// </summary>
    private void SetHighwayType(string highwayString)
    {
        switch (highwayString)
        {
            case "motorway":
                highway = HighwayType.motorway;
                LaneWidth = 3.7f;
                break;
            case "trunk":
                highway = HighwayType.trunk;
                LaneWidth = 3.5f;
                break;
            case "primary":
                highway = HighwayType.primary;
                LaneWidth = 3.7f;
                break;
            case "secondary":
                highway = HighwayType.secondary;
                LaneWidth = 3.2f;
                break;
            case "tertiary":
                highway = HighwayType.tertiary;
                LaneWidth = 3.0f;
                break;
            case "unclassified":
                highway = HighwayType.unclassified;
                LaneWidth = 3.2f;
                break;
            case "residential":
                highway = HighwayType.residential;
                LaneWidth = 3.0f;
                break;
            case "service":
                highway = HighwayType.service;
                LaneWidth = 3.2f;
                break;
            //Link roads
            case "motorway_link":
                highway = HighwayType.motorway_link;
                LaneWidth = 3.5f;
                break;
            case "trunk_link":
                highway = HighwayType.trunk_link;
                LaneWidth = 3.2f;
                break;
            case "primary_link":
                highway = HighwayType.primary_link;
                LaneWidth = 3.5f;
                break;
            case "secondary_link":
                highway = HighwayType.secondary_link;
                LaneWidth = 3.2f;
                break;
            case "tertiary_link":
                highway = HighwayType.tertiary_link;
                LaneWidth = 3.0f;
                break;
            //Special road types
            case "living_street":
                highway = HighwayType.living_street;
                LaneWidth = 3.0f;
                break;
            case "pedestrian":
                highway = HighwayType.pedestrian;
                LaneWidth = 2.0f;
                break;
            case "track":
                highway = HighwayType.track;
                LaneWidth = 3.0f;
                break;
            case "bus_guideway":
                highway = HighwayType.bus_guideway;
                LaneWidth = 3.7f;
                break;
            case "escape":
                highway = HighwayType.escape;
                LaneWidth = 3.5f;
                break;
            case "raceway":
                highway = HighwayType.raceway;
                LaneWidth = 3.5f;
                break;
            case "road":
                highway = HighwayType.road;
                LaneWidth = 3.5f;
                break;
            //Paths
            case "footway":
                highway = HighwayType.footway;
                LaneWidth = 2.0f;
                break;
            case "bridleway":
                highway = HighwayType.bridleway;
                LaneWidth = 2.5f;
                break;
            case "steps":
                highway = HighwayType.steps;
                LaneWidth = 1.8f;
                break;
            case "path":
                highway = HighwayType.path;
                LaneWidth = 2.0f;
                break;
            //cycleway, can then go deeper in specification
            case "cycleway":
                highway = HighwayType.cycleway;
                LaneWidth = 2.0f;
                break;
            //lifecycle
            case "proposed":
                highway = HighwayType.proposed;
                LaneWidth = 3.7f;
                break;
            case "construction":
                highway = HighwayType.construction;
                LaneWidth = 3.7f;
                break;
            default:
                break;
        }
    }
}

