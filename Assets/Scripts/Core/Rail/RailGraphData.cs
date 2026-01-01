using System;
using System.Collections.Generic;

namespace Core.Rail
{
    [Serializable]
    public class RailGraphData
    {
        public List<RailNodeData> Nodes = new();
        public List<RailEdgeData> Edges = new();
    }
}