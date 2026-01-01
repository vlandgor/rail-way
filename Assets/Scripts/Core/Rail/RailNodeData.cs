using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Rail
{
    [Serializable]
    public class RailNodeData
    {
        public int Id;
        public Vector3 Position;
        public List<int> ConnectedNodeIds = new();
    }
}