namespace WUIEngine.Traffic
{
    [System.Serializable]
    public class TrafficProbe
    {
        public int nodeID;

        public TrafficProbe(int nodeID)
        {
            this.nodeID = nodeID;
        }

        public static TrafficProbe[] GetTemplate()
        {
            TrafficProbe[] probes = new TrafficProbe[1];
            probes[0] = new TrafficProbe(0);
            return probes;
        }
    }
}

